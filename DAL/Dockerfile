# Staging: Base Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Staging: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiert nur die .csproj-Datei aus dem entsprechenden Ordner
COPY ["./DAL/DAL.csproj", "./DAL/"]
RUN dotnet restore "./DAL/DAL.csproj"

# Kopiere alle anderen Dateien
COPY . ./

# Build der Anwendung
RUN dotnet build "./DAL/DAL.csproj" -c Release -o /app/build

# Staging: Publish Stage
FROM build AS publish
RUN dotnet publish "./DAL/DAL.csproj" -c Release -o /app/publish

# Staging: Final Stage
FROM base AS final
WORKDIR /app

# Kopiere ver�ffentlichte Dateien aus dem vorherigen Staging
COPY --from=publish /app/publish .

# Start der Anwendung
ENTRYPOINT ["dotnet", "DAL.dll"]
