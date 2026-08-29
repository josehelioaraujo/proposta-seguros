#!/bin/bash
# ============================================================
# set-banco.sh — Habilita ou desabilita PostgreSQL
# Uso: ./scripts/set-banco.sh --enable | --disable
# ============================================================

ENV_FILE=".env"

usage() {
    echo "Uso: ./scripts/set-banco.sh --enable | --disable"
    echo ""
    echo "  --enable   Liga PostgreSQL (Dapper)"
    echo "  --disable  Liga InMemory (sem banco)"
    exit 1
}

[ $# -eq 0 ] && usage

case $1 in
    --enable)
        VALOR=true
        echo ""
        echo "[set-banco] PostgreSQL HABILITADO"
        ;;
    --disable)
        VALOR=false
        echo ""
        echo "[set-banco] InMemory HABILITADO"
        ;;
    *)
        usage
        ;;
esac

# Atualiza o .env
sed -i "s/USAR_BANCO_DADOS=.*/USAR_BANCO_DADOS=$VALOR/" $ENV_FILE
echo "[set-banco] .env atualizado!"

# Mostra estado atual do .env
echo ""
echo "[set-banco] Flags atuais:"
cat $ENV_FILE | grep -v "^#" | grep -v "^$"
echo ""

# Pergunta se quer aplicar agora
read -p "Reiniciar containers agora? (s/n): " RESPOSTA
if [ "$RESPOSTA" = "s" ]; then
    docker compose down
    docker compose up -d
    echo ""
    docker compose ps
fi
