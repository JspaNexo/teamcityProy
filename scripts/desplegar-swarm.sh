#!/bin/bash
# Despliega en Docker Swarm el archivo generado por scripts/reemplazar-tokens-swarm.sh
# y numera el build de TeamCity con la versión desplegada.
#
# Uso: bash scripts/desplegar-swarm.sh [archivo-stack] [nombre-stack]
set -euo pipefail

STACK_FILE="${1:-docker-stack.Temp.yml}"
STACK_NAME="${2:-applicationdemo}"

# En TeamCity el archivo llega como artefacto de Imagen Docker Staging (si no hay ninguno, falla antes la dependencia);
# esta comprobación sirve al ejecutar el script a mano
if [ ! -f "$STACK_FILE" ]; then
    echo "No existe $STACK_FILE: genéralo con scripts/reemplazar-tokens-swarm.sh" >&2
    exit 1
fi

IMAGEN=$(grep -m1 -oE 'image: *[^ ]+' "$STACK_FILE" | sed -E 's/image: *//; s/["'\'']//g' || true)

if [ -z "$IMAGEN" ]; then
    echo "$STACK_FILE no define ninguna imagen" >&2
    exit 1
fi

# La versión es la etiqueta: lo que sigue al último ':' del nombre, sin el digest (@sha256:...)
# y sin confundirla con el puerto del registro (localhost:5000/applicationdemo no tiene etiqueta)
REFERENCIA="${IMAGEN%%@*}"
NOMBRE="${REFERENCIA##*/}"

if [[ "$NOMBRE" != *:* ]]; then
    echo "La imagen $IMAGEN no tiene etiqueta de versión: no se sabe qué versión se despliega" >&2
    exit 1
fi

VERSION="${NOMBRE##*:}"
REPOSITORIO="${REFERENCIA%:*}"

# Si la imagen viene de un registro (primer segmento con '.' o ':', o localhost), comprueba que la etiqueta exista:
# si el registro perdió las imágenes, Swarm rechazaría las réplicas en vez de fallar al instante
REGISTRO="${REPOSITORIO%%/*}"

if [[ "$REPOSITORIO" == */* && ( "$REGISTRO" == *.* || "$REGISTRO" == *:* || "$REGISTRO" == localhost ) ]]; then
    RUTA="${REPOSITORIO#*/}"
    URL="http://$REGISTRO/v2/$RUTA/manifests/$VERSION"

    if ! curl -fsS -o /dev/null -I \
        -H "Accept: application/vnd.oci.image.index.v1+json" \
        -H "Accept: application/vnd.oci.image.manifest.v1+json" \
        -H "Accept: application/vnd.docker.distribution.manifest.list.v2+json" \
        -H "Accept: application/vnd.docker.distribution.manifest.v2+json" \
        "$URL"; then
        echo "La imagen $REPOSITORIO:$VERSION no está en el registro $REGISTRO ($URL)" >&2
        exit 1
    fi
fi

# Mensaje de servicio de TeamCity: el build muestra la versión desplegada (ej. 1.0.2) en vez de un contador
echo "##teamcity[buildNumber '$VERSION']"
echo "Desplegando $IMAGEN en el stack $STACK_NAME"

# --detach=false espera a que las réplicas converjan: si el rolling update falla, el build falla
docker stack deploy --detach=false -c "$STACK_FILE" "$STACK_NAME"
