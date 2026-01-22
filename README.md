Theo le Bastard - BUT 3 gpr ALT

IHM (html) Proxy (nginx)
Traitement (python)
Ins bd (C#)
bd (mongodb)(ProjetLogsDB)

1er terminal
docker-compose up --build
(sur site inserer fichier)

2ème terminal
docker exec -it projet-mongodb-1 mongosh 
use ProjetLogsDB
db.logs.countDocuments() (si inserer 4 fichier log fournie par le prof alors 36285 lignes)
