#!/bin/bash
# set-monitoring.sh — habilita ou desabilita Prometheus + Grafana
# Uso: ./set-monitoring.sh --enable | --disable

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

case "$1" in
  --enable)
    echo "[monitoring] Subindo Prometheus + Grafana..."
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile monitoring up -d prometheus grafana
    IP=$(hostname -I | awk '{print $1}')
    echo "[monitoring] Prometheus : http://$IP:9090"
    echo "[monitoring] Grafana    : http://$IP:3000  (admin/admin)"
    ;;
  --disable)
    echo "[monitoring] Parando Prometheus + Grafana..."
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile monitoring stop prometheus grafana
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile monitoring rm -f prometheus grafana
    echo "[monitoring] Servicos parados."
    ;;
  *)
    echo "Uso: $0 --enable | --disable"
    exit 1
    ;;
esac
