FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082

# Install libs for Tesseract
RUN apt-get update && apt-get install -y libc6 libgdiplus libc6-dev
RUN apt-get -y update && apt-get -y install ghostscript
RUN apt-get update
RUN apt-get install -y git cmake build-essential
RUN mkdir leptonica
RUN git clone https://github.com/DanBloomberg/leptonica.git /leptonica

WORKDIR /leptonica
RUN mkdir build
WORKDIR /leptonica/build
RUN cmake ..

RUN mkdir glibc
RUN git clone git://sourceware.org/git/glibc.git /glibc

WORKDIR /glibc
RUN mkdir build
WORKDIR /glibc/build
RUN make ..

WORKDIR /app
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
    && rm -rf /var/lib/apt/lists/*
RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so
WORKDIR /app/x64
RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so
# Entrypoint
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OcrWorker.dll"]
