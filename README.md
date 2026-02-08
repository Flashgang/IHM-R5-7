# IHM - R5 - 7
Theo le Bastard - BUT 3 gpr ALT

---
# Architecture du projet

- IHM `html` Proxy `nginx`
- Traitement `python`
- Inst bd `C#`
- bd `mongodb` `ProjetLogsDB`

---
## Utilisation

- 1er terminal (lancer le projet)
    ```bash
    docker-compose up --build
    ```
    Puis sur site le site inserer fichier `Localhost:8080`

- 2ème terminal (veifier que les données on été inserer)
    ```bash
    docker exec -it projet-mongodb-1 mongosh 
    use ProjetLogsDB
    db.logs.countDocuments() 
    ```
    Si 4 fichier log fournie par le professeur son inserer dans la bd alors vous devez avoir `36285` lignes dans la bd.


---

### Diagramme Data Flow de niveuax 1 (DFD1)
![DFD1](https://github.com/Flashgang/IHM-R5-7/blob/main/diagramme%20dataflow%20de%20niveau%201%20(DFD1).png?raw=true)


---

### Diagramme de séquence de niveau 1 (SD1)
![SD1](https://github.com/Flashgang/IHM-R5-7/blob/main/diagramme%20de%20séquence%20de%20niveau%20(SD1).png?raw=true)

---
 

