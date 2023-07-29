FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
WORKDIR /app/Schulungsportal\ 2
COPY ["Schulungsportal 2/*.csproj", "."]
RUN dotnet restore

# copy everything else and build app
COPY ["Schulungsportal 2/.", "."]
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS runtime
WORKDIR /app
COPY --from=build ["/app/Schulungsportal 2/out", "./"]
ENTRYPOINT ["dotnet", "Schulungsportal 2.dll"]