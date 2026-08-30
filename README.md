# Proposta de Seguros

Sistema de gerenciamento de propostas de seguro desenvolvido como teste técnico,
utilizando Arquitetura Hexagonal (Ports & Adapters), .NET 10 e PostgreSQL.

## Visão Geral

O sistema é composto por dois microserviços independentes:

| Serviço | Responsabilidade | Porta |
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
- RabbitMQ 4 (mensageria — bonus)
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

### Opcao 1 — Docker na VPS (recomendado)

```bash
ssh root@2.25.122.11
cd /home/projetos/proposta-seguros
./scripts/apply.sh
```

### Opcao 2 — Docker local

```bash
git clone https://github.com/josehelioaraujo/proposta-seguros.git
cd proposta-seguros
cp .env.example .env
docker compose up --build -d
```

### Opcao 3 — Local sem Docker

```bash
# Terminal 1 — PropostaService
cd src/PropostaService/PropostaService.Api
dotnet run

# Terminal 2 — ContratacaoService
cd src/ContratacaoService/ContratacaoService.Api
dotnet run
```

---

## URLs

### VPS Hostinger

| Servico | URL |
|---|---|
| PropostaService — Swagger | http://2.25.122.11:5001 |
| ContratacaoService — Swagger | http://2.25.122.11:5002 |
| RabbitMQ — Painel | http://2.25.122.11:15672 |

### Local

| Servico | URL |
|---|---|
| PropostaService — Swagger | http://localhost:5001 |
| ContratacaoService — Swagger | http://localhost:5002 |
| RabbitMQ — Painel | http://localhost:15672 |

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

docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V001__create_schema_proposta.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V002__create_table_propostas.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V003__create_schema_contratacao.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V004__create_table_contratacoes.sql
```

### Estrutura criada

```
seguros_db
├── schema: proposta
│   └── tabela: propostas
│       ├── id, nome_cliente, cpf
│       ├── tipo_seguro, valor, status
│       └── criado_em, atualizado_em
│
└── schema: contratacao
    └── tabela: contratacoes
        ├── id, proposta_id
        ├── cpf, data_contratacao
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
├── proposta-seguros.postman_collection.json
├── env-local.json
└── env-vps.json
```

### 2. Selecionar o ambiente

```
Canto superior direito do Postman:
├── "Local"          -> http://localhost:500x
└── "VPS Hostinger"  -> http://2.25.122.11:500x
```

### 3. Estrutura da Collection

| Pasta | Descricao |
|-------|-----------|
| 01 — Health Checks | Verifica saude dos dois servicos |
| 02 — PropostaService | Todos os cenarios de proposta |
| 03 — ContratacaoService | Todos os cenarios de contratacao |
| 04 — Fluxo Completo | Fluxo end-to-end com dados fixos |
| 05 — Fluxo VPS InMemory | Dados aleatorios — sem banco |
| 06 — Fluxo VPS PostgreSQL | Dados aleatorios — com banco |
| 07 — Fluxo VPS PostgreSQL + RabbitMQ | Dados aleatorios — banco + fila |

---

## Simulacao Completa de Testes

### Cenario 1 — InMemory

```bash
# Na VPS
./scripts/set-banco.sh --disable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "05 — Fluxo VPS InMemory"
Acao:    Run folder -> Start run

Resultado esperado:
- 7/7 testes passando
- Dados gerados automaticamente
- Fluxo: criar -> aprovar -> contratar -> verificar
```

### Cenario 2 — PostgreSQL

```bash
# Aplica migrations
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V001__create_schema_proposta.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V002__create_table_propostas.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V003__create_schema_contratacao.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V004__create_table_contratacoes.sql

# Liga PostgreSQL
./scripts/set-banco.sh --enable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "06 — Fluxo VPS PostgreSQL"
Acao:    Run folder -> Start run

Resultado esperado:
- 7/7 testes passando
- Dados persistidos no PostgreSQL
- Health check mostra postgres: Healthy
```

### Cenario 3 — PostgreSQL + RabbitMQ

```bash
# Liga RabbitMQ
./scripts/set-rabbitmq.sh --enable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "07 — Fluxo VPS PostgreSQL + RabbitMQ"
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
o fluxo principal, cada funcionalidade bonus e controlada por feature flags —
desabilitadas por padrao e habilitadas sob demanda.

### RabbitMQ — Mensageria

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

### Health Checks — ASP.NET Nativo

Implementados usando o sistema nativo do ASP.NET Core sem bibliotecas externas,
com tres niveis de verificacao por servico.

```
PropostaService    -> self + postgres (se habilitado)
ContratacaoService -> self + postgres + proposta-service + rabbitmq
```

### PostgreSQL + Dapper com Feature Flag

O sistema opera em dois modos sem alteracao de codigo:

```
UsarBancoDados: false -> InMemory (List<T>) — desenvolvimento
UsarBancoDados: true  -> PostgreSQL via Dapper — producao
```

Demonstra o padrao Ports & Adapters na pratica: o mesmo Use Case funciona
com qualquer repositorio injetado via DI.

### Logging Estruturado

Logging implementado em todos os Use Cases com niveis configurados por ambiente
via appsettings.json, preparado para integracao futura com Prometheus e Grafana.

### Docker — Multi-stage Build

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

---

## Containers Docker

| Container | Imagem | Porta | Descricao |
|-----------|--------|-------|-----------|
| proposta-api | proposta-seguros-proposta-api | 5001 | API de gerenciamento de propostas de seguro |
| contratacao-api | proposta-seguros-contratacao-api | 5002 | API de contratacao de propostas aprovadas |
| seguros-postgres | postgres:16-alpine | 5432 | Banco de dados PostgreSQL compartilhado |
| seguros-rabbitmq | rabbitmq:4-management-alpine | 5672 / 15672 | Mensageria — sobe apenas com profile rabbitmq |

### Observacoes

```
proposta-api e contratacao-api
└── Multi-stage build: sdk:10.0 (build) + aspnet:10.0 (runtime)
└── Imagem final: ~200MB (sem SDK, sem codigo fonte)
└── Ambiente: Production

seguros-postgres
└── Volume persistente: postgres_data
└── Healthcheck: pg_isready a cada 10s
└── APIs so sobem apos postgres estar Healthy

seguros-rabbitmq
└── Sobe apenas quando USAR_RABBITMQ=true
└── Painel de gerenciamento: http://2.25.122.11:15672
└── usuario: guest / senha: guest
```

---

## Scripts de Operacao

Todos os scripts ficam na pasta `scripts/` e devem ser executados
a partir da raiz do projeto na VPS.

### Gerenciamento de containers

| Script | Descricao |
|--------|-----------|
| `./scripts/start.sh` | Inicia os containers |
| `./scripts/stop.sh` | Para os containers |
| `./scripts/restart.sh` | Reinicia os containers |
| `./scripts/status.sh` | Exibe status dos containers e URLs |
| `./scripts/update.sh` | git pull + rebuild + reinicia |

### Feature flags

| Script | Descricao |
|--------|-----------|
| `./scripts/set-banco.sh --enable` | Liga PostgreSQL (Dapper) |
| `./scripts/set-banco.sh --disable` | Volta para InMemory |
| `./scripts/set-rabbitmq.sh --enable` | Liga RabbitMQ e sobe o container |
| `./scripts/set-rabbitmq.sh --disable` | Desliga RabbitMQ |
| `./scripts/apply.sh` | Aplica as flags do .env e reinicia |

### Logs

| Script | Descricao |
|--------|-----------|
| `./scripts/logs.sh --proposta` | Logs do PropostaService em tempo real |
| `./scripts/logs.sh --contratacao` | Logs do ContratacaoService em tempo real |
| `./scripts/logs.sh --postgres` | Logs do PostgreSQL em tempo real |
| `./scripts/logs.sh --all` | Logs de todos os containers |

### Como usar

```bash
# Conecta na VPS
ssh root@2.25.122.11
cd /home/projetos/proposta-seguros

# Verifica status atual
./scripts/status.sh

# Atualiza para ultima versao
./scripts/update.sh

# Alterna para PostgreSQL
./scripts/set-banco.sh --enable

# Acompanha logs em tempo real
./scripts/logs.sh --proposta
```

### Arquivo .env

O arquivo `.env` na raiz do projeto persiste as feature flags entre reinicializacoes:

```bash
# Ver flags atuais
cat .env

# Conteudo esperado:
USAR_BANCO_DADOS=false
USAR_RABBITMQ=false
```

Os scripts `set-banco.sh` e `set-rabbitmq.sh` atualizam o `.env` automaticamente.

---

## Mensageria — Produtor e Consumidores

Nossa aplicacao atua apenas como **produtora** de eventos. O ContratacaoService
publica o evento `PropostaContratadaEvent` na fila apos cada contratacao
bem-sucedida e nao se preocupa com o que acontece a seguir — esse e o
principio do desacoplamento via mensageria.

### O que publicamos

```
Evento: PropostaContratadaEvent
Exchange: proposta.exchange (Direct)
Fila:     proposta.contratada.queue

Campos:
├── ContratacaoId    — identificador unico da contratacao
├── PropostaId       — referencia a proposta contratada
├── Cpf              — CPF do segurado
├── DataContratacao  — data e hora da contratacao
└── OcorridoEm       — timestamp do evento
```

### Quem consumiria em producao real

Cada consumidor e um microservico independente que escuta a fila
e reage ao evento de forma autonoma:

| Consumidor | Responsabilidade |
|------------|-----------------|
| **ApoliceService** | Gera o documento PDF da apolice de seguro e disponibiliza para o segurado |
| **CobrancaService** | Agenda o debito mensal do premio na conta ou cartao do segurado |
| **NotificacaoService** | Envia email e SMS de confirmacao da contratacao ao segurado |
| **SusepService** | Registra a contratacao junto a SUSEP (orgao regulador de seguros) dentro do prazo legal de 24h |
| **AntiFraudeService** | Analisa o perfil do segurado e a contratacao em busca de padroes suspeitos |
| **AuditoriaService** | Registra todos os eventos em log imutavel para fins de compliance e rastreabilidade |

### Por que mensageria e nao HTTP direto

```
Sem mensageria (HTTP sincrono):
└── ContratacaoService chama cada servico diretamente
    ├── Se NotificacaoService cair  → contratacao falha
    ├── Se CobrancaService lento    → resposta demora
    └── Acoplamento alto entre servicos

Com mensageria (assincrono):
└── ContratacaoService publica o evento e retorna 201
    ├── Cada consumidor processa no seu proprio ritmo
    ├── Se um cair, a mensagem fica na fila ate ele voltar
    ├── Escala independente por servico
    └── Desacoplamento total
```

### Como verificar as mensagens publicadas

Apos rodar o fluxo 07 no Postman, acesse o painel do RabbitMQ:

```
URL:     http://2.25.122.11:15672
Usuario: guest
Senha:   guest

Queues and Streams
└── proposta.contratada.queue
    └── Messages ready: N (mensagens aguardando consumidor)
```

As mensagens ficam acumuladas pois nao ha consumidores implementados
neste projeto — em producao real seriam processadas imediatamente.
