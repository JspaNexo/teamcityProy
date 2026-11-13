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

Desplegar una versión publicada por TeamCity en el registro local en vez de la imagen local:

```bash
IMAGEN=localhost:5000/applicationdemo TAG=1.0.5 docker stack deploy --detach=false -c docker-stack.yml applicationdemo
# PowerShell: $env:IMAGEN="localhost:5000/applicationdemo"; $env:TAG="1.0.5"; docker stack deploy ...
curl http://localhost:5000/v2/applicationdemo/tags/list                            # versiones publicadas
```

Limpiar: `docker stack rm applicationdemo` y, si ya no se usará Swarm, `docker swarm leave --force`.

Swarm no construye imágenes: en un Swarm de varios nodos la imagen debe estar en un registro
accesible para todos los nodos.

## TeamCity

La configuración está en `.teamcity/settings.kts` (Kotlin DSL):

1. **Build y Test**: compila, ejecuta pruebas (los resultados aparecen en la pestaña *Tests*) y publica el artefacto `ApplicationDemo-<versión>.zip`. Se dispara con cada commit a `main` y a `develop`. Número de build: `1.0.<contador>`.
2. **Imagen Docker Staging**: se dispara sola cuando *Build y Test* termina bien en `develop`, y cada paso tiene la condición `teamcity.build.branch equals develop`:
   1. *Docker Build Staging*: construye `localhost:5000/applicationdemo:<versión>` (`%env.APP_IMAGE_BUILD_NUMBER%`).
   2. *Docker Push Staging*: la sube al registro local de TeamCity y borra la imagen del agente.
   3. *Replace Staging Swarm Tokens*: `scripts/reemplazar-tokens-swarm.sh` genera `docker-stack.Temp.yml` con la imagen y la versión reemplazadas, y lo publica como artefacto.
3. **Despliegue Swarm Staging**: se ejecuta a mano sobre `develop`. Descarga el `docker-stack.Temp.yml` de esa versión y lo despliega con `docker stack deploy`. Requiere que el Docker del agente sea manager de un Swarm (`docker swarm init`).

El registro es el contenedor `registry:2` del entorno de TeamCity, publicado en `localhost:5000`; no necesita cuenta ni credenciales.

### Rama develop

Para que TeamCity vea `develop`, la VCS root del proyecto debe incluirla: *Edit Project → VCS Roots → (la raíz del repo) → Branch specification*, agrega `+:refs/heads/develop`.

Pasos para conectarlo:

1. Sube el repositorio a Git (GitHub, GitLab, Azure DevOps, etc.).
2. En TeamCity: *Create project → From a repository URL* con la URL del repo.
3. TeamCity detecta `.teamcity/settings.kts` y ofrece *Import settings from VCS*: acéptalo.

Requisitos del agente: .NET SDK 10 y, para la imagen, Docker.
Si tu servidor TeamCity es anterior a 2026.2, cambia `version = "2026.2"` en `settings.kts` por la versión de tu servidor.
