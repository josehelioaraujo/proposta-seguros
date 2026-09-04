#!/bin/bash
# ============================================================
# apply.sh — Aplica as flags do .env e reinicia containers
# Uso: ./scripts/apply.sh
# ============================================================

ENV_FILE=".env"

# Carrega o .env
source $ENV_FILE

echo ""
echo "============================================================"
echo "  Aplicando Feature Flags"
echo "============================================================"
echo ""
echo "  UsarBancoDados = $USAR_BANCO_DADOS"
echo "  UsarRabbitMQ   = $USAR_RABBITMQ"
echo ""

# Para todos os containers (todos os profiles)
docker compose --profile rabbitmq --profile monitoring down 2>/dev/null || docker compose down

# Sobe com profiles conforme .env — monitoring sempre ativo
if [ "$USAR_RABBITMQ" = "true" ]; then
    docker compose --profile rabbitmq --profile monitoring up -d
else
    docker compose --profile monitoring up -d
fi

echo ""
docker compose ps

IP=$(curl -s ifconfig.me 2>/dev/null || echo "2.25.122.11")
echo ""
echo "  PropostaService:    http://$IP:5001"
echo "  ContratacaoService: http://$IP:5002"
echo "  Prometheus:         http://$IP:9090"
echo "  Grafana:            http://$IP:3000  (admin/admin)"

if [ "$USAR_RABBITMQ" = "true" ]; then
    echo "  RabbitMQ Painel:    http://$IP:15672"
fi
echo ""
