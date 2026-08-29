#!/bin/bash
usage() {
    echo "Uso: ./scripts/logs.sh --proposta | --contratacao | --postgres | --all"
    exit 1
}

[ $# -eq 0 ] && usage

case $1 in
    --proposta)    docker compose logs -f proposta-api ;;
    --contratacao) docker compose logs -f contratacao-api ;;
    --postgres)    docker compose logs -f postgres ;;
    --all)         docker compose logs -f ;;
    *)             usage ;;
esac
