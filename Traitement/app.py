import os
import io
import json
import hashlib
import asyncio
import requests
import pandas as pd
from fastapi import FastAPI, UploadFile, File, BackgroundTasks
from fastapi.responses import StreamingResponse

app = FastAPI()

CSHARP_API_URL = os.getenv("CSHARP_API_URL", "http://instbdc:80/api/logs")
current_progress = 0

def calculate_hash(row_str):
    return hashlib.sha256(row_str.encode('utf-8')).hexdigest()

async def process_csv_logic(file_content: bytes):
    global current_progress
    current_progress = 0
    
    try:
        # 1. Lecture rapide
        df = pd.read_csv(
            io.BytesIO(file_content), 
            sep=',', 
            quotechar='"', 
            on_bad_lines='warn', 
            engine='python'
        )
        
        total_lines = len(df)
        if total_lines == 0:
            current_progress = 100
            return

        # 2. Configuration des lots (Batch)
        BATCH_SIZE = 500 
        batch_payload = []
        
        # On itère sur les lignes
        for index, row in df.iterrows():
            try:
                # Nettoyage des données (NaN -> None)
                log_data = row.to_dict()
                log_data = {k: (None if pd.isna(v) else v) for k, v in log_data.items()}
                
                # Calcul du hash
                row_string = json.dumps(log_data, sort_keys=True)
                log_hash = calculate_hash(row_string)
                
                # Ajout au paquet
                batch_payload.append({
                    "hash": log_hash,
                    "data": log_data
                })

                # 3. Si le paquet est plein, on l'envoie
                if len(batch_payload) >= BATCH_SIZE:
                    requests.post(f"{CSHARP_API_URL}/bulk", json=batch_payload)
                    batch_payload = [] # On vide le paquet
                    
                    # Mise à jour progression
                    current_progress = int(((index + 1) / total_lines) * 100)
                    await asyncio.sleep(0.001)

            except Exception as e:
                print(f"[Python] Erreur ligne {index}: {e}")

        # 4. Envoi du dernier paquet (s'il reste des données)
        if len(batch_payload) > 0:
            requests.post(f"{CSHARP_API_URL}/bulk", json=batch_payload)

    except Exception as e:
        print(f"Erreur globale traitement: {e}")
    finally:
        current_progress = 100

@app.post("/upload")
async def upload_endpoint(background_tasks: BackgroundTasks, file: UploadFile = File(...)):
    content = await file.read()
    background_tasks.add_task(process_csv_logic, content)
    return {"status": "started"}

@app.get("/progress")
async def progress_endpoint():
    async def event_generator():
        while True:
            yield f"data: {current_progress}\n\n"
            if current_progress >= 100:
                break
            await asyncio.sleep(0.5)
    return StreamingResponse(event_generator(), media_type="text/event-stream")