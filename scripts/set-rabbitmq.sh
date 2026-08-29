#!/bin/bash
# ============================================================
# set-rabbitmq.sh — Habilita ou desabilita RabbitMQ
# Uso: ./scripts/set-rabbitmq.sh --enable | --disable
# ============================================================

ENV_FILE=".env"

usage() {
    echo "Uso: ./scripts/set-rabbitmq.sh --enable | --disable"
    echo ""
    echo "  --enable   Liga RabbitMQ"
    echo "  --disable  Desliga RabbitMQ"
    exit 1
}

[ $# -eq 0 ] && usage

case $1 in
    --enable)
        VALOR=true
        echo ""
        echo "[set-rabbitmq] RabbitMQ HABILITADO"
        ;;
    --disable)
        VALOR=false
        echo ""
        echo "[set-rabbitmq] RabbitMQ DESABILITADO"
        ;;
    *)
        usage
        ;;
esac

# Atualiza o .env
sed -i "s/USAR_RABBITMQ=.*/USAR_RABBITMQ=$VALOR/" $ENV_FILE
echo "[set-rabbitmq] .env atualizado!"

# Mostra estado atual do .env
echo ""
echo "[set-rabbitmq] Flags atuais:"
cat $ENV_FILE | grep -v "^#" | grep -v "^$"
echo ""

# Pergunta se quer aplicar agora
read -p "Reiniciar containers agora? (s/n): " RESPOSTA
if [ "$RESPOSTA" = "s" ]; then
    docker compose --profile rabbitmq down 2>/dev/null || docker compose down

    if [ "$VALOR" = "true" ]; then
        docker compose --profile rabbitmq up -d
        IP=$(curl -s ifconfig.me 2>/dev/null || echo "2.25.122.11")
        echo ""
        echo "  RabbitMQ Painel: http://$IP:15672"
        echo "  usuario: guest / senha: guest"
    else
        docker compose up -d
    fi
    echo ""
    docker compose ps
fi
