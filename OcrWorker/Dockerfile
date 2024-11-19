FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082

# Install libs for Tesseract
RUN apt-get update
RUN apt-get install -y git cmake build-essential
RUN mkdir leptonica
RUN git clone https://github.com/DanBloomberg/leptonica.git /leptonica

WORKDIR /leptonica
RUN mkdir build
WORKDIR /leptonica/build
RUN cmake ..

RUN apt-get install -y libleptonica-dev libtesseract-dev

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
WORKDIR /app/x64
RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 libleptonica-1.82.0.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5.2.0 libtesseract41.so
# Entrypoint
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OcrWorker.dll"]
