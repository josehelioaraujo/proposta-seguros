#!/bin/bash
# set-monitoring.sh — habilita ou desabilita stack de observabilidade
# Uso: ./set-monitoring.sh --enable | --disable

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

case "$1" in
  --enable)
    echo "[monitoring] Subindo stack de observabilidade..."
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile monitoring up -d prometheus grafana jaeger loki promtail
    IP=$(hostname -I | awk '{print $1}')
    echo ""
    echo "  Prometheus : http://$IP:9090"
    echo "  Grafana    : http://$IP:3000  (admin/admin)"
    echo "  Jaeger     : http://$IP:16686"
    echo "  Loki       : http://$IP:3100"
    echo ""
    ;;
  --disable)
    echo "[monitoring] Parando stack de observabilidade..."
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile monitoring stop prometheus grafana jaeger loki promtail
    docker compose -f "$ROOT_DIR/docker-compose.yml" --profile monitoring rm -f prometheus grafana jaeger loki promtail
    echo "[monitoring] Servicos parados."
    ;;
  *)
    echo "Uso: $0 --enable | --disable"
    exit 1
    ;;
esac
