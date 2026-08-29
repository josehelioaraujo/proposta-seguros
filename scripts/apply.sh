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

# Para containers
docker compose --profile rabbitmq down 2>/dev/null || docker compose down

# Sobe com flags do .env
if [ "$USAR_RABBITMQ" = "true" ]; then
    docker compose --profile rabbitmq up -d
else
    docker compose up -d
fi

echo ""
docker compose ps

IP=$(curl -s ifconfig.me 2>/dev/null || echo "2.25.122.11")
echo ""
echo "  PropostaService:    http://$IP:5001"
echo "  ContratacaoService: http://$IP:5002"

if [ "$USAR_RABBITMQ" = "true" ]; then
    echo "  RabbitMQ Painel:    http://$IP:15672"
fi
echo ""
