FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server/Server.csproj", "Server/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "./Server/Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet publish "./Server.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    -r linux-x64 \
    --self-contained true \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:PublishReadyToRun=true \
    /p:UseAppHost=true \
    /p:DebugType=none \
    /p:DebugSymbols=false

FROM debian:bookworm-slim AS final
WORKDIR /app
RUN apt-get update && apt-get install -y libicu72 && rm -rf /var/lib/apt/lists/*
RUN apt-get update && apt-get install -y \
    libssl3 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
COPY Server/MainServer/Database/server.db ./server.db
COPY Server/Libs/FFmpeg ./
COPY Server/Certs ./Certs
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["./Server"]
