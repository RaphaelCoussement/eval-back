# Phase de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Récupération du token passé par le pipeline (--build-arg)
ARG NUGET_AUTH_TOKEN
# Injection en variable d'environnement pour que le nuget.config puisse le lire
ENV NUGET_AUTH_TOKEN=$NUGET_AUTH_TOKEN

# Copie du fichier de configuration NuGet à la racine du contexte
COPY ["NuGet.config", "."]
# ------------------------------------

# Copie des fichiers de projet
COPY ["DungeonCrawler_Game_Service/DungeonCrawler_Game_Service.csproj", "DungeonCrawler_Game_Service/"]
COPY ["DungeonCrawler_Game_Service.Application/DungeonCrawler_Game_Service.Application.csproj", "DungeonCrawler_Game_Service.Application/"]
COPY ["DungeonCrawler_Game_Service.Domain/DungeonCrawler_Game_Service.Domain.csproj", "DungeonCrawler_Game_Service.Domain/"]
COPY ["DungeonCrawler_Game_Service.Infrastructure/DungeonCrawler_Game_Service.Infrastructure.csproj", "DungeonCrawler_Game_Service.Infrastructure/"]

# Restauration des dépendances (utilisera le nuget.config et le token)
RUN dotnet restore "DungeonCrawler_Game_Service/DungeonCrawler_Game_Service.csproj"

# Copie du reste des fichiers et build en Release
COPY . .
WORKDIR "/src/DungeonCrawler_Game_Service"
RUN dotnet build "DungeonCrawler_Game_Service.csproj" -c Release -o /app/build

# Phase de publication
RUN dotnet publish "DungeonCrawler_Game_Service.csproj" -c Release -o /app/publish --no-restore

# Phase de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Configuration pour la production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Point d'entrée
ENTRYPOINT ["dotnet", "DungeonCrawler_Game_Service.dll"]