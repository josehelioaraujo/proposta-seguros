#!/bin/bash
# ============================================================
# apply.sh — Aplica as flags do .env e reinicia containers
# Uso: ./scripts/apply.sh
# ============================================================

ENV_FILE=".env"
source $ENV_FILE

echo ""
echo "============================================================"
echo "  Aplicando Feature Flags"
echo "============================================================"
echo ""
echo "  UsarBancoDados = $USAR_BANCO_DADOS"
echo "  UsarRabbitMQ   = $USAR_RABBITMQ"
echo ""

docker compose --profile rabbitmq --profile monitoring --profile ai down 2>/dev/null || docker compose down

if [ "$USAR_RABBITMQ" = "true" ]; then
    docker compose --profile rabbitmq --profile monitoring --profile ai up -d
else
    docker compose --profile monitoring --profile ai up -d
fi

echo ""
docker compose ps

IP=$(curl -s ifconfig.me 2>/dev/null || echo "2.25.122.11")
echo ""
echo "  PropostaService:    http://$IP:5001"
echo "  ContratacaoService: http://$IP:5002"
echo "  Prometheus:         http://$IP:9090"
echo "  Grafana:            http://$IP:3000"
echo "  Jaeger:             http://$IP:16686"
echo "  Open WebUI:         http://$IP:8080"

if [ "$USAR_RABBITMQ" = "true" ]; then
    echo "  RabbitMQ Painel:    http://$IP:15672"
fi
echo ""
