FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# curl solo para el HEALTHCHECK (la imagen base no lo incluye); se instala como root antes de bajar privilegios
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ApplicationDemo/ApplicationDemo.csproj", "ApplicationDemo/"]
RUN dotnet restore "ApplicationDemo/ApplicationDemo.csproj"
COPY . .
WORKDIR "/src/ApplicationDemo"

# dotnet publish compila y publica en un solo paso
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Versión que devuelve GET /. TeamCity pasa el número de build (--build-arg VERSION=1.0.7);
# se declara después del restore para no invalidar esa capa en cada versión
ARG VERSION=1.0.0
RUN dotnet publish "./ApplicationDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -p:Version=$VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Docker Compose y Swarm usan este chequeo; Swarm no da por sana una réplica (ni sigue un rolling update) hasta que pase
HEALTHCHECK --interval=10s --timeout=3s --start-period=15s --retries=3 \
    CMD curl -fsS http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "ApplicationDemo.dll"]
