#!/bin/bash
echo ""
echo "[restart] Reiniciando containers..."
echo ""
docker compose down
docker compose up -d
echo ""
docker compose ps
