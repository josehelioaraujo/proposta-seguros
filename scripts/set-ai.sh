#!/bin/bash
# set-ai.sh — habilita ou desabilita Open WebUI
# Uso: ./set-ai.sh --enable | --disable

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

case "$1" in
  --enable)
    echo "[ai] Subindo Open WebUI..."
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile ai up -d open-webui
    IP=$(hostname -I | awk '{print $1}')
    echo "[ai] Open WebUI: http://$IP:8080"
    ;;
  --disable)
    echo "[ai] Parando Open WebUI..."
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile ai stop open-webui
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile ai rm -f open-webui
    echo "[ai] Servico parado."
    ;;
  *)
    echo "Uso: $0 --enable | --disable"
    exit 1
    ;;
esac
