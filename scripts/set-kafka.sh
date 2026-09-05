#!/bin/bash
# Habilita ou desabilita o Kafka (feature flag + profile docker)
# Uso: ./set-kafka.sh --enable | --disable

set -e

ENV_FILE="/home/projetos/proposta-seguros/.env"
COMPOSE_FILE="/home/projetos/proposta-seguros/docker-compose.yml"

if [ "$1" == "--enable" ]; then
    echo "Habilitando Kafka..."
    sed -i 's/^FEATURES__USARKAFKA=.*/FEATURES__USARKAFKA=true/' "$ENV_FILE"
    sed -i 's/^FEATURES__USARRABBITMQ=.*/FEATURES__USARRABBITMQ=false/' "$ENV_FILE"

    docker compose -f "$COMPOSE_FILE" --profile kafka up -d kafka kafka-ui
    docker compose -f "$COMPOSE_FILE" restart contratacao-api
    echo "Kafka habilitado — UI disponível em http://2.25.122.11:8082"

elif [ "$1" == "--disable" ]; then
    echo "Desabilitando Kafka..."
    sed -i 's/^FEATURES__USARKAFKA=.*/FEATURES__USARKAFKA=false/' "$ENV_FILE"

    docker compose -f "$COMPOSE_FILE" --profile kafka stop kafka kafka-ui
    docker compose -f "$COMPOSE_FILE" restart contratacao-api
    echo "Kafka desabilitado"

else
    echo "Uso: $0 --enable | --disable"
    exit 1
fi
