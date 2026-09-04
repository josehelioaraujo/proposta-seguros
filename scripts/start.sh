#!/bin/bash
echo ""
echo "[start] Iniciando containers..."
echo ""
docker compose --profile monitoring up -d
echo ""
docker compose ps

IP=$(curl -s ifconfig.me 2>/dev/null || echo "2.25.122.11")
echo ""
echo "  PropostaService:    http://$IP:5001"
echo "  ContratacaoService: http://$IP:5002"
echo "  Prometheus:         http://$IP:9090"
echo "  Grafana:            http://$IP:3000  (admin/admin)"
echo ""
