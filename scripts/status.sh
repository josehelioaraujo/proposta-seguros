#!/bin/bash
IP=$(curl -s ifconfig.me 2>/dev/null || echo "2.25.122.11")
echo ""
echo "[status] Containers:"
echo ""
docker compose ps
echo ""
echo "[status] URLs:"
echo "  PropostaService:    http://$IP:5001"
echo "  ContratacaoService: http://$IP:5002"
echo "  RabbitMQ Painel:    http://$IP:15672"
echo ""
