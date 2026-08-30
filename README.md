# Proposta de Seguros

Sistema de gerenciamento de propostas de seguro desenvolvido como teste tÃ©cnico,
utilizando Arquitetura Hexagonal (Ports & Adapters), .NET 10 e PostgreSQL.

## VisÃ£o Geral

O sistema Ã© composto por dois microserviÃ§os independentes:

| ServiÃ§o | Responsabilidade | Porta |
|---|---|---|
| **PropostaService** | Criar e gerenciar propostas de seguro | 5001 |
| **ContratacaoService** | Contratar propostas aprovadas e publicar eventos | 5002 |

---

## Arquitetura

```mermaid
flowchart TD
    CLIENT([Cliente / Swagger / Postman])

    subgraph PS [PropostaService :5001]
        PA[REST Controller] --> PUC1[CriarPropostaUseCase]
        PA --> PUC2[AlterarStatusUseCase]
        PUC1 --> PD[Proposta]
        PUC2 --> PD
        PD --> PREP[IPropostaRepository]
        PREP --> INMEM[(InMemory)]
        PREP --> DAPPER[(PostgreSQL Dapper)]
    end

    subgraph CS [ContratacaoService :5002]
        CA[REST Controller] --> CUC[ContratarPropostaUseCase]
        CUC --> CD[Contratacao]
        CUC --> CREP[IContratacaoRepository]
        CUC --> CHTTP[HttpPropostaServiceClient]
        CUC --> CPUB[IEventPublisher]
        CREP --> INMEM2[(InMemory)]
        CREP --> DAPPER2[(PostgreSQL Dapper)]
        CPUB --> MQ[(RabbitMQ)]
    end

    DB[(PostgreSQL seguros_db)]
    CLIENT -->|POST /api/Propostas| PA
    CLIENT -->|POST /api/Contratacoes| CA
    CHTTP -->|GET /api/Propostas/id| PA
    DAPPER --> DB
    DAPPER2 --> DB
```

---

## Tecnologias

- .NET 10 / C#
- PostgreSQL 16 + Dapper
- RabbitMQ 4 (mensageria â€” bonus)
- FluentValidation
- xUnit / Moq / Bogus / FluentAssertions
- Docker / Docker Compose
- Swagger / OpenAPI 3.0
- ASP.NET Health Checks

---

## Tipos de Seguro (BMG)

| Codigo | Tipo | Valor Minimo |
|--------|------|--------------|
| 1 | SeguroFGTSProtegido | R$ 50,00 |
| 2 | SeguroVidaFamiliar | R$ 30,00 |
| 3 | SeguroCartaoProtegido | R$ 15,00 |
| 4 | ProtecaoCreditoTrabalhador | R$ 25,00 |
| 5 | SeguroContaCelularProtegidos | R$ 10,00 |

---

## Como Executar

### Opcao 1 â€” Docker na VPS (recomendado)

```bash
ssh root@2.25.122.11
cd /home/projetos/proposta-seguros
./scripts/apply.sh
```

### Opcao 2 â€” Docker local

```bash
git clone https://github.com/josehelioaraujo/proposta-seguros.git
cd proposta-seguros
cp .env.example .env
docker compose up --build -d
```

### Opcao 3 â€” Local sem Docker

```bash
# Terminal 1 â€” PropostaService
cd src/PropostaService/PropostaService.Api
dotnet run

# Terminal 2 â€” ContratacaoService
cd src/ContratacaoService/ContratacaoService.Api
dotnet run
```

---

## URLs

### VPS Hostinger

| Servico | URL |
|---|---|
| PropostaService â€” Swagger | http://2.25.122.11:5001 |
| ContratacaoService â€” Swagger | http://2.25.122.11:5002 |
| RabbitMQ â€” Painel | http://2.25.122.11:15672 |

### Local

| Servico | URL |
|---|---|
| PropostaService â€” Swagger | http://localhost:5001 |
| ContratacaoService â€” Swagger | http://localhost:5002 |
| RabbitMQ â€” Painel | http://localhost:15672 |

---

## Endpoints

### PropostaService

| Metodo | Rota | Descricao | Status |
|--------|------|-----------|--------|
| POST | /api/Propostas | Cria uma proposta | 201 |
| GET | /api/Propostas | Lista todas as propostas | 200 |
| GET | /api/Propostas/{id} | Busca proposta por ID | 200 |
| PATCH | /api/Propostas/{id}/status | Altera status da proposta | 200 |

### ContratacaoService

| Metodo | Rota | Descricao | Status |
|--------|------|-----------|--------|
| POST | /api/Contratacoes | Contrata uma proposta aprovada | 201 |
| GET | /api/Contratacoes/{id} | Busca contratacao por ID | 200 |

### Health Checks

| Endpoint | PropostaService | ContratacaoService |
|----------|-----------------|-------------------|
| /health | :5001/health | :5002/health |
| /health/live | :5001/health/live | :5002/health/live |
| /health/ready | :5001/health/ready | :5002/health/ready |

---

## Migrations SQL

As migrations criam os schemas e tabelas no PostgreSQL.

### Aplicar na VPS

```bash
cd /home/projetos/proposta-seguros

# Aplica todas as migrations em ordem
for f in migrations/V*.sql; do
    echo "Aplicando $f..."
    docker exec -i seguros-postgres psql -U postgres -d seguros_db < $f
done
```

### Estrutura criada

```
seguros_db
â”œâ”€â”€ schema: proposta
â”‚   â””â”€â”€ tabela: propostas
â”‚       â”œâ”€â”€ id, nome_cliente, cpf
â”‚       â”œâ”€â”€ tipo_seguro, valor, status
â”‚       â””â”€â”€ criado_em, atualizado_em
â”‚
â””â”€â”€ schema: contratacao
    â””â”€â”€ tabela: contratacoes
        â”œâ”€â”€ id, proposta_id
        â”œâ”€â”€ cpf, data_contratacao
```

---

## Testes Unitarios

```bash
dotnet test .\proposta-seguros.sln
```

```
Resultado esperado:
total: 13 | falhou: 0 | bem-sucedido: 13
```

---

## Testando via Postman

### 1. Importar os arquivos

```
Postman -> Import -> seleciona os 3 arquivos da pasta docs/postman/:
â”œâ”€â”€ proposta-seguros.postman_collection.json
â”œâ”€â”€ env-local.json
â””â”€â”€ env-vps.json
```

### 2. Selecionar o ambiente

```
Canto superior direito do Postman:
â”œâ”€â”€ "Local"          -> http://localhost:500x
â””â”€â”€ "VPS Hostinger"  -> http://2.25.122.11:500x
```

### 3. Estrutura da Collection

| Pasta | Descricao |
|-------|-----------|
| 01 â€” Health Checks | Verifica saude dos dois servicos |
| 02 â€” PropostaService | Todos os cenarios de proposta |
| 03 â€” ContratacaoService | Todos os cenarios de contratacao |
| 04 â€” Fluxo Completo | Fluxo end-to-end com dados fixos |
| 05 â€” Fluxo VPS InMemory | Dados aleatorios â€” sem banco |
| 06 â€” Fluxo VPS PostgreSQL | Dados aleatorios â€” com banco |
| 07 â€” Fluxo VPS PostgreSQL + RabbitMQ | Dados aleatorios â€” banco + fila |

---

## Simulacao Completa de Testes

### Cenario 1 â€” InMemory

```bash
# Na VPS
./scripts/set-banco.sh --disable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "05 â€” Fluxo VPS InMemory"
Acao:    Run folder -> Start run

Resultado esperado:
- 7/7 testes passando
- Dados gerados automaticamente
- Fluxo: criar -> aprovar -> contratar -> verificar
```

### Cenario 2 â€” PostgreSQL

```bash
# Aplica migrations
# Aplica todas as migrations em ordem
for f in migrations/V*.sql; do
    echo "Aplicando $f..."
    docker exec -i seguros-postgres psql -U postgres -d seguros_db < $f
done

# Liga PostgreSQL
./scripts/set-banco.sh --enable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "06 â€” Fluxo VPS PostgreSQL"
Acao:    Run folder -> Start run

Resultado esperado:
- 7/7 testes passando
- Dados persistidos no PostgreSQL
- Health check mostra postgres: Healthy
```

### Cenario 3 â€” PostgreSQL + RabbitMQ

```bash
# Liga RabbitMQ
./scripts/set-rabbitmq.sh --enable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "07 â€” Fluxo VPS PostgreSQL + RabbitMQ"
Acao:    Run folder -> Start run

Resultado esperado:
- 7/7 testes passando
- Evento PropostaContratadaEvent publicado na fila
- Health check mostra postgres + rabbitmq: Healthy

Verifica a fila:
URL:     http://2.25.122.11:15672
Usuario: guest / Senha: guest
Fila:    proposta.contratada.queue
```

---

## Feature Flags

| Flag | false (padrao) | true |
|------|----------------|------|
| Features:UsarBancoDados | InMemory | PostgreSQL via Dapper |
| Features:UsarRabbitMQ | Sem mensageria | Publica PropostaContratadaEvent |

### Scripts de operacao (VPS)

```bash
./scripts/apply.sh                    # aplica flags do .env e sobe containers
./scripts/set-banco.sh --enable       # liga PostgreSQL
./scripts/set-banco.sh --disable      # volta para InMemory
./scripts/set-rabbitmq.sh --enable    # liga RabbitMQ
./scripts/set-rabbitmq.sh --disable   # desliga RabbitMQ
./scripts/update.sh                   # git pull + rebuild
./scripts/status.sh                   # status dos containers e URLs
./scripts/logs.sh --proposta          # logs PropostaService
./scripts/logs.sh --contratacao       # logs ContratacaoService
./scripts/logs.sh --all               # todos os logs
```

---

## Funcionalidades Bonus

Os itens abaixo nao foram solicitados no enunciado. Foram implementados como
demonstracao de boas praticas e conhecimento tecnico adicional. Para nao impactar
o fluxo principal, cada funcionalidade bonus e controlada por feature flags â€”
desabilitadas por padrao e habilitadas sob demanda.

### RabbitMQ â€” Mensageria

O enunciado menciona mensageria como item opcional. O ContratacaoService publica
o evento PropostaContratadaEvent apos cada contratacao bem-sucedida.

```
Evento: PropostaContratadaEvent
Exchange: proposta.exchange (Direct)
Fila:     proposta.contratada.queue

Consumidores em producao real:
- ApoliceService   -> gera documento da apolice
- CobrancaService  -> agenda debito mensal
- NotificacaoService -> envia email ao cliente
- SusepService     -> registro regulatorio
```

Feature flag: Features:UsarRabbitMQ

### Health Checks â€” ASP.NET Nativo

Implementados usando o sistema nativo do ASP.NET Core sem bibliotecas externas,
com tres niveis de verificacao por servico.

```
PropostaService    -> self + postgres (se habilitado)
ContratacaoService -> self + postgres + proposta-service + rabbitmq
```

### PostgreSQL + Dapper com Feature Flag

O sistema opera em dois modos sem alteracao de codigo:

```
UsarBancoDados: false -> InMemory (List<T>) â€” desenvolvimento
UsarBancoDados: true  -> PostgreSQL via Dapper â€” producao
```

Demonstra o padrao Ports & Adapters na pratica: o mesmo Use Case funciona
com qualquer repositorio injetado via DI.

### Logging Estruturado

Logging implementado em todos os Use Cases com niveis configurados por ambiente
via appsettings.json, preparado para integracao futura com Prometheus e Grafana.

### Docker â€” Multi-stage Build

Imagens construidas em duas etapas: SDK para compilar, runtime para executar.
Resultado: imagem final de aproximadamente 200MB ao inves de 900MB, sem codigo
fonte exposto e sem ferramentas de build em producao.

### Deploy em VPS

Sistema em execucao em VPS real na Hostinger (Ubuntu 24.04, Docker 29.7.2),
acessivel publicamente para avaliacao sem necessidade de setup local.

```
PropostaService:    http://2.25.122.11:5001
ContratacaoService: http://2.25.122.11:5002
```

---

## Documentacao

- [Enunciado do Projeto](docs/enunciado.md)
- [Postman Collection](docs/postman/)
- [Migrations SQL](migrations/)

