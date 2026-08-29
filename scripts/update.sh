#!/bin/bash
echo ""
echo "[update] Atualizando projeto via git..."
echo ""
git pull origin main
echo ""
echo "[update] Rebuild dos containers..."
echo ""
docker compose down
docker compose up -d --build
echo ""
echo "[update] Projeto atualizado!"
echo ""
docker compose ps
