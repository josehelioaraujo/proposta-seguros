#!/bin/bash
# ============================================================
# set-adminer.sh — Habilita ou desabilita o Adminer
# Uso: ./scripts/set-adminer.sh --enable | --disable
# ============================================================

usage() {
    echo "Uso: ./scripts/set-adminer.sh --enable | --disable"
    echo ""
    echo "  --enable   Sobe o Adminer (painel PostgreSQL)"
    echo "  --disable  Para o Adminer"
    exit 1
}

[ $# -eq 0 ] && usage

case $1 in
    --enable)
        echo ""
        echo "[set-adminer] Subindo Adminer..."
        docker compose --profile tools up -d adminer
        IP=$(curl -s ifconfig.me 2>/dev/null || echo "2.25.122.11")
        echo ""
        echo "[set-adminer] Adminer disponivel em:"
        echo "  URL:      http://$IP:5050"
        echo "  Sistema:  PostgreSQL"
        echo "  Servidor: postgres"
        echo "  Usuario:  postgres"
        echo "  Senha:    postgres"
        echo "  Banco:    seguros_db"
        echo ""
        ;;
    --disable)
        echo ""
        echo "[set-adminer] Parando Adminer..."
        docker compose --profile tools stop adminer
        docker compose --profile tools rm -f adminer
        echo "[set-adminer] Adminer parado!"
        echo ""
        ;;
    *)
        usage
        ;;
esac
