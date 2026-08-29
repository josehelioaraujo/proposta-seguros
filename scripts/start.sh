#!/bin/bash
echo ""
echo "[start] Iniciando containers..."
echo ""
docker compose up -d
echo ""
docker compose ps
