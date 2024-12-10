FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 8082
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OcrWorker/OcrWorker.csproj", "OcrWorker/"]
RUN dotnet restore "OcrWorker/OcrWorker.csproj"
COPY . .

WORKDIR "/src/OcrWorker"
RUN dotnet build "OcrWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OcrWorker.csproj" -c Release -o /app/publish

FROM base AS final
# Link libs for Tesseract
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libleptonica-dev \
        libtesseract-dev \
        tesseract-ocr \
        tesseract-ocr-eng \
        ghostscript \
    && rm -rf /var/lib/apt/lists/*
RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so
WORKDIR /app/x64
RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so
# Entrypoint
WORKDIR /app
COPY --from=publish /app/publish .

# Explicitly copy the log4net.config file if it's in the Utility folder
COPY ./Logging/log4net.config ./log4net.config

ENTRYPOINT ["dotnet", "OcrWorker.dll"]
