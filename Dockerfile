FROM node:12-alpine as js-builder
WORKDIR /app

# js build dependencies
COPY ["Schulungsportal 2/package*.json", "."]
RUN npm ci

# js
COPY ["Schulungsportal 2/js/src/", "js/src"]
RUN npm run build

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
WORKDIR /app/Schulungsportal\ 2
COPY ["Schulungsportal 2/*.csproj", "."]
RUN dotnet restore

# copy everything else and build app
COPY ["Schulungsportal 2/.", "."]
COPY --from=js-builder ["/app/wwwroot/dist/", "wwwroot/dist"] 
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS runtime
WORKDIR /app
COPY --from=build ["/app/Schulungsportal 2/out", "./"]
ENTRYPOINT ["dotnet", "Schulungsportal 2.dll"]