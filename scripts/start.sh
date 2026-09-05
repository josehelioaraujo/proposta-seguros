#!/bin/bash
echo ""
echo "[start] Iniciando containers..."
echo ""
docker compose --profile monitoring --profile ai up -d
echo ""
docker compose ps

IP=$(curl -s ifconfig.me 2>/dev/null || echo "2.25.122.11")
echo ""
echo "  PropostaService:    http://$IP:5001"
echo "  ContratacaoService: http://$IP:5002"
echo "  Prometheus:         http://$IP:9090"
echo "  Grafana:            http://$IP:3000"
echo "  Jaeger:             http://$IP:16686"
echo "  Open WebUI:         http://$IP:8080"
echo ""
