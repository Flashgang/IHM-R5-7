FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# On copie tout le dossier InsBD dans l'image
COPY . .

# On restaure les dépendances et on compile
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .

# On expose le port 80 (interne au conteneur)
EXPOSE 80

# Commande de lancement (remplace "NomDeTonProjet.dll" par le vrai nom de ton fichier .csproj sans l'extension)
# Si tu ne sais pas, regarde dans ton dossier InsBD si tu as un fichier .csproj
ENTRYPOINT ["dotnet", "InsBD.dll"]