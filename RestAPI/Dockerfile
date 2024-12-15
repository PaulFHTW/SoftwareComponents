# FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:35792ea4ad1db051981f62b313f1be3b46b1f45cadbaa3c288cd0d3056eefb83 AS build-env
# WORKDIR /RestAPI

# Copy everything
# COPY ./RestAPI/ .
# Restore as distinct layers
# RUN dotnet restore
# Build and publish a release
# RUN dotnet publish -c Release -o /RestAPI/out

# Build runtime image
# FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809@sha256:6c4df091e4e531bb93bdbfe7e7f0998e7ced344f54426b7e874116a3dc3233ff
# COPY --from=build-env /RestAPI/out .

# ENTRYPOINT ["dotnet", "RestAPI.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./RestAPI/RestAPI.csproj", "./RestAPI/"]
RUN dotnet restore "./RestAPI/RestAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./RestAPI/RestAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RestAPI/RestAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Explicitly copy the log4net.config file if it's in the Utility folder
COPY ./Logging/log4net.config ./log4net.config

# Copy the generated NPaperless.xml file from the build folder to /app/
COPY --from=build /src/RestAPI/docs/NPaperless.xml /app/

ENTRYPOINT ["dotnet", "RestAPI.dll"]