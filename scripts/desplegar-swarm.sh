#!/bin/bash
# Despliega en Docker Swarm el archivo generado por scripts/reemplazar-tokens-swarm.sh
# y numera el build de TeamCity con la versión desplegada.
#
# Uso: bash scripts/desplegar-swarm.sh [archivo-stack] [nombre-stack]
set -euo pipefail

STACK_FILE="${1:-docker-stack.Temp.yml}"
STACK_NAME="${2:-applicationdemo}"

if [ ! -f "$STACK_FILE" ]; then
    echo "No existe $STACK_FILE: ¿hay algún build correcto de Imagen Docker Staging en develop?" >&2
    exit 1
fi

IMAGEN=$(grep -m1 -oE 'image: *[^ ]+' "$STACK_FILE" | sed -E 's/image: *//' || true)

if [ -z "$IMAGEN" ]; then
    echo "$STACK_FILE no define ninguna imagen" >&2
    exit 1
fi

VERSION="${IMAGEN##*:}"

# Mensaje de servicio de TeamCity: el build muestra la versión desplegada (ej. 1.0.2) en vez de un contador
echo "##teamcity[buildNumber '$VERSION']"
echo "Desplegando $IMAGEN en el stack $STACK_NAME"

# --detach=false espera a que las réplicas converjan: si el rolling update falla, el build falla
docker stack deploy --detach=false -c "$STACK_FILE" "$STACK_NAME"
