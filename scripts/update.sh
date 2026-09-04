#!/bin/bash
echo ""
echo "[update] Atualizando projeto via git..."
echo ""
git pull origin main
echo ""
echo "[update] Rebuild dos containers..."
echo ""
docker compose --profile rabbitmq --profile monitoring down 2>/dev/null || docker compose down
docker compose --profile monitoring up -d --build
echo ""
echo "[update] Projeto atualizado!"
echo ""
docker compose ps
