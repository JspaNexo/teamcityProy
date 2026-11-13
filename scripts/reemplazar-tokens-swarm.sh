#!/bin/bash
# Genera el archivo Swarm final reemplazando los tokens ${IMAGEN} y ${TAG} por sus valores,
# para desplegar exactamente la imagen que se acaba de publicar.
#
# Uso: IMAGEN=localhost:5000/applicationdemo TAG=1.0.7 bash scripts/reemplazar-tokens-swarm.sh [entrada] [salida]
set -euo pipefail

INPUT_FILE="${1:-docker-stack.yml}"
FINAL_FILE="${2:-docker-stack.Temp.yml}"

# Sin estas variables el stack usaría sus valores por defecto (imagen local, latest): mejor fallar
: "${IMAGEN:?Falta la variable de entorno IMAGEN}"
: "${TAG:?Falta la variable de entorno TAG}"

# docker stack config reemplaza las variables y además valida el archivo con el mismo cargador que usa el deploy
docker stack config -c "$INPUT_FILE" > "$FINAL_FILE"

echo "Generado $FINAL_FILE con la imagen $IMAGEN:$TAG"
cat "$FINAL_FILE"
