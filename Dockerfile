# Phase de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copie des fichiers de projet et restauration des dépendances
COPY ["DungeonCrawler_Game_Service/DungeonCrawler_Game_Service.csproj", "DungeonCrawler_Game_Service/"]
COPY ["DungeonCrawler_Game_Service.Application/DungeonCrawler_Game_Service.Application.csproj", "DungeonCrawler_Game_Service.Application/"]
COPY ["DungeonCrawler_Game_Service.Domain/DungeonCrawler_Game_Service.Domain.csproj", "DungeonCrawler_Game_Service.Domain/"]
COPY ["DungeonCrawler_Game_Service.Infrastructure/DungeonCrawler_Game_Service.Infrastructure.csproj", "DungeonCrawler_Game_Service.Infrastructure/"]

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
