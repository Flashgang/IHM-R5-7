# Image Python
FROM python:3.9-slim

WORKDIR /app

# Installation des dépendances système (parfois nécessaire pour Pandas/Numpy)
RUN apt-get update && apt-get install -y gcc

# script
COPY app.py .

# Installation des librairies Python 
RUN pip install --no-cache-dir fastapi uvicorn python-multipart requests pandas

# Commande de lancement (sur le port 8000 interne)
CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8000"]