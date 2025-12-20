FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ytdlp.sln ./
COPY ytdlp.Api/ytdlp.Api.csproj ./ytdlp.Api/
COPY ytdlp.Services/ytdlp.Services.csproj ./ytdlp.Services/
COPY ytdlp.Tests/ytdlp.Tests.csproj ./ytdlp.Tests/

RUN dotnet restore
COPY . .
WORKDIR /src/ytdlp.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
USER root

# ALLE Dependencies auf einmal (inkl. unzip!)
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl ca-certificates ffmpeg unzip && \
    rm -rf /var/lib/apt/lists/*

# yt-dlp
RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp \
    -o /usr/local/bin/yt-dlp && chmod +x /usr/local/bin/yt-dlp

# Deno (funktioniert jetzt!)
RUN curl -fsSL https://deno.land/install.sh | sh && \
    cp /root/.deno/bin/deno /usr/local/bin/deno && chmod +x /usr/local/bin/deno

# User & Ordner
RUN groupadd -g 1000 appuser && \
    useradd -u 1000 -g 1000 -m appuser && \
    mkdir -p /app/downloads /app/archive /app/configs && \
    chown -R 1000:1000 /app

USER 1000:1000
WORKDIR /app
COPY --from=build --chown=1000:1000 /app/publish .

EXPOSE 8080
ENV PORT=8080 ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/api/ytdlp/config || exit 1

ENTRYPOINT ["dotnet", "ytdlp.Api.dll"]
