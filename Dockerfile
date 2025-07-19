# Usa .NET 9.0 PREVIEW
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
WORKDIR /app

# Instala Chrome en la imagen base
RUN apt-get update && \
    apt-get install -y wget gnupg unzip && \
    wget https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb && \
    apt-get install -y ./google-chrome-stable_current_amd64.deb && \
    rm google-chrome-stable_current_amd64.deb

# Imagen de build con SDK 9.0 PREVIEW
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "TarjetaCheckerWeb.dll"]
