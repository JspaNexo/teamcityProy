# ApplicationDemo

API mínima en .NET 10 para practicar CI/CD con TeamCity.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/` | Nombre, versión, entorno e instancia (contenedor) que respondió |
| GET | `/health` | Health check |
| GET | `/api/productos` | Lista de productos (en memoria) |
| GET | `/api/productos/{id}` | Producto por id (404 si no existe) |
| POST | `/api/productos` | Crea un producto `{ "nombre": "...", "precio": 10.5 }` |
| GET | `/api/total?precio=10&cantidad=3&descuento=10` | Calcula total con descuento |

## Local

```bash
dotnet test ApplicationDemo.slnx          # pruebas unitarias + integración
dotnet run --project ApplicationDemo      # levanta la API
docker compose up --build                 # API en http://localhost:8080
```

## Docker Swarm

`docker-stack.yml` despliega la API con 3 réplicas detrás del balanceador de Swarm, actualización gradual
(`start-first`) con rollback automático si una réplica nueva no pasa el healthcheck, y límites de CPU/memoria.

```bash
docker swarm init                                                    # una sola vez: convierte el Docker local en un Swarm de un nodo
docker build -t applicationdemo:latest -f ApplicationDemo/Dockerfile .
docker stack deploy --detach=false -c docker-stack.yml applicationdemo

docker service ls                                                    # réplicas listas: 3/3
docker service ps applicationdemo_api                                # dónde corre cada réplica y su estado
curl http://localhost:8080/                                          # "instancia" cambia entre llamadas: balanceo entre réplicas
docker service scale applicationdemo_api=5                           # escalar
```

Actualizar a otra versión (rolling update) y volver atrás:

```bash
TAG=1.0.5 docker stack deploy --detach=false -c docker-stack.yml applicationdemo   # PowerShell: $env:TAG="1.0.5"; docker stack deploy ...
docker service rollback applicationdemo_api
```

Limpiar: `docker stack rm applicationdemo` y, si ya no se usará Swarm, `docker swarm leave --force`.

Swarm no construye imágenes: en un Swarm de varios nodos la imagen debe estar en un registro
(Docker Hub, GHCR, etc.) para que todos los nodos puedan descargarla.

## TeamCity

La configuración está en `.teamcity/settings.kts` (Kotlin DSL):

1. **Build y Test**: compila, ejecuta pruebas (los resultados aparecen en la pestaña *Tests*) y publica el artefacto `ApplicationDemo-<versión>.zip`. Se dispara con cada commit a la rama por defecto. Número de build: `1.0.<contador>`.
2. **Imagen Docker**: construye `applicationdemo:<versión>` y `applicationdemo:latest`. Corre después de *Build y Test* y solo en agentes con Docker.
3. **Despliegue Swarm**: despliega `applicationdemo:<versión>` con `docker-stack.yml`. Se ejecuta a mano (TeamCity corre antes las dos anteriores) y requiere que el Docker del agente sea manager de un Swarm (`docker swarm init`).

Pasos para conectarlo:

1. Sube el repositorio a Git (GitHub, GitLab, Azure DevOps, etc.).
2. En TeamCity: *Create project → From a repository URL* con la URL del repo.
3. TeamCity detecta `.teamcity/settings.kts` y ofrece *Import settings from VCS*: acéptalo.

Requisitos del agente: .NET SDK 10 y, para la imagen, Docker.
Si tu servidor TeamCity es anterior a 2026.2, cambia `version = "2026.2"` en `settings.kts` por la versión de tu servidor.
