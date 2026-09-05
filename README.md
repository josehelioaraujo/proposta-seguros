# Proposta de Seguros

[![CI/CD](https://github.com/josehelioaraujo/proposta-seguros/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/josehelioaraujo/proposta-seguros/actions/workflows/ci-cd.yml)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=josehelioaraujo_proposta-seguros&metric=alert_status)](https://sonarcloud.io/project/overview?id=josehelioaraujo_proposta-seguros)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=josehelioaraujo_proposta-seguros&metric=bugs)](https://sonarcloud.io/project/overview?id=josehelioaraujo_proposta-seguros)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=josehelioaraujo_proposta-seguros&metric=code_smells)](https://sonarcloud.io/project/overview?id=josehelioaraujo_proposta-seguros)


## Introdução

<details>
<summary><strong>Sistema de propostas de seguro com Arquitetura Hexagonal, Kafka, RabbitMQ, Outbox Pattern, Observabilidade completa e MCP Server</strong></summary>

Sistema de gerenciamento de propostas de seguro desenvolvido com **Arquitetura Hexagonal (Ports & Adapters)**, **.NET 10** e **PostgreSQL**, composto por dois microserviços independentes que se comunicam via HTTP REST e mensageria assíncrona com Kafka e RabbitMQ.

O **PropostaService** gerencia o ciclo de vida das propostas — criação, consulta e alteração de status (Em Análise → Aprovada / Rejeitada). O **ContratacaoService** efetua a contratação de propostas aprovadas, consultando o PropostaService via HTTP, persistindo a contratação e publicando o evento `PropostaContratadaEvent` no RabbitMQ para consumo por serviços downstream (apólice, cobrança, notificação, SUSEP).

Ambos os serviços operam em dois modos intercambiáveis via feature flag — **InMemory** para desenvolvimento e **PostgreSQL via Dapper** para produção — sem alteração de código, demonstrando o padrão Ports & Adapters na prática.

O projeto inclui pipeline CI/CD completo, testes de integração com Testcontainers, smoke tests E2E via Newman, deploy automatizado em VPS real, stack completa de observabilidade (Prometheus, Grafana, Jaeger, Loki) e **MCP Server** expondo as operações das APIs como tools para agentes de IA.

---

</details>
 

## ⚙️ Tecnologias

<details>
<summary><strong>Ver stack completa de tecnologias</strong></summary>

- .NET 10 / C#
- PostgreSQL 16 + Dapper
- Apache Kafka 3.9 (KRaft — event streaming, sem Zookeeper)
- RabbitMQ 4 (message broker)
- Outbox Pattern (garantia de entrega entre banco e mensageria)
- DbUp (migrations automáticas no startup)
- FluentValidation
- xUnit / Moq / Bogus / FluentAssertions / Testcontainers
- Docker / Docker Compose
- Swagger / OpenAPI 3.0
- ASP.NET Health Checks
- Prometheus + Grafana (métricas e dashboards)
- Jaeger + OpenTelemetry (distributed tracing)
- Loki + Promtail (agregação de logs)
- ModelContextProtocol.AspNetCore (MCP Server)

</details>

## 📚 Documentação Rápida

| | |
|---|---|
| 📋 [Changelog](Changelog.md) | Histórico de versões e evolução do projeto |
| 📮 [Postman Collection](docs/postman/) | Cenários de teste prontos para importar |
| 🗃️ [Migrations SQL](migrations/) | Scripts de banco versionados V001–V006 |
| 📄 [Enunciado](docs/enunciado.md) | Especificação original do projeto |

---

 

## Visão Geral

O sistema é composto por dois microserviços independentes:

| Serviço | Responsabilidade | Porta |
|---|---|---|
| **PropostaService** | Criar e gerenciar propostas de seguro | 5001 |
| **ContratacaoService** | Contratar propostas aprovadas e publicar eventos | 5002 |

---

## 🔀 Integração com Mensagerias

O sistema suporta **Kafka** e **RabbitMQ** de forma intercambiável via feature flags — sem alteração de código. A troca de broker é feita apenas no `.env` da VPS ou via pipeline CI/CD, demonstrando o padrão **12-Factor App** e a aderência real à Arquitetura Hexagonal.

<details>
<summary><strong>Ver detalhes — Arquitetura, Outbox Pattern, Kafka, RabbitMQ, Testes</strong></summary>


O sistema suporta **Kafka** e **RabbitMQ** de forma intercambiável via feature flags — sem alteração de código. A troca de broker é feita apenas no `.env` da VPS ou via pipeline CI/CD, demonstrando o padrão **12-Factor App** e a aderência real à Arquitetura Hexagonal.

---

### Arquitetura de Mensageria

```
ContratarPropostaUseCase
  └── BEGIN TRANSACTION (PostgreSQL)
        ├── INSERT contratacoes          ← dados da contratação
        └── INSERT contratacao.outbox    ← evento pendente de publicação
              COMMIT (atômico)

OutboxPublisherWorker (BackgroundService — a cada 5s)
  └── SELECT outbox WHERE processado = false
        ├── KafkaEventPublisher  → tópico: proposta-contratada
        │   ou
        └── RabbitMqEventPublisher → exchange: proposta.exchange
        └── UPDATE outbox SET processado = true
```

> **Outbox Pattern** garante atomicidade entre banco e mensageria — se a API cair após gravar a contratação mas antes de publicar, o worker retoma na próxima rodada. Zero perda de mensagem.

---

### Feature Flags de Mensageria

| Flag | Valor | Comportamento |
|------|-------|--------------|
| `Features:UsarKafka` | `true` | Publica no Kafka (KRaft, sem Zookeeper) |
| `Features:UsarRabbitMQ` | `true` | Publica no RabbitMQ |
| Ambas | `false` | `NullEventPublisher` — API não falha |

> **Regra:** apenas uma flag pode estar ativa por vez. O `IEventPublisher` é injetado via DI conforme a flag — o `OutboxPublisherWorker` não conhece o broker.
 
---

### Trocar de Broker — Zero Código

**Via scripts na VPS:**

```bash
./scripts/set-kafka.sh --enable    # ativa Kafka, desativa RabbitMQ
./scripts/set-kafka.sh --disable   # desativa Kafka

./scripts/set-rabbitmq.sh --enable  # ativa RabbitMQ, desativa Kafka
./scripts/set-rabbitmq.sh --disable # desativa RabbitMQ
```

**Via GitHub Actions (workflow_dispatch):**

```
Actions → CI/CD → Run workflow
  ├── Broker: kafka / rabbitmq / nenhum
  └── Usar banco: true / false
```

A esteira aplica as flags automaticamente na VPS, reinicia os containers e executa os Smoke Tests E2E para validar.

---

### Kafka — Apache Kafka KRaft

Kafka rodando em modo **KRaft** (sem Zookeeper) — arquitetura moderna, 1 container apenas.

```
Broker:  seguros-kafka:9092
Tópico:  proposta-contratada
Modo:    KRaft (Apache Kafka 3.9.0)
```

**Configuração no `appsettings.json`:**

```json
{
  "Features": { "UsarKafka": true },
  "Kafka": {
    "BootstrapServers": "kafka:9092",
    "Topicos": {
      "PropostaContratadaEvent": "proposta-contratada"
    }
  }
}
```

**Kafka UI — Dashboard:**

```
URL: http://2.25.122.11:8082
```

O que visualizar no Kafka UI:

```
Topics → proposta-contratada
  ├── Messages     — mensagens publicadas com payload JSON
  ├── Consumers    — consumer groups ativos
  └── Overview     — partições, offsets, throughput
```

---

### RabbitMQ — Message Broker

```
Exchange:    proposta.exchange (Direct)
Fila:        proposta.contratada.queue
Routing Key: proposta.contratada
```

**RabbitMQ Management — Dashboard:**

```
URL:     http://2.25.122.11:15672
Usuário: guest / Senha: guest
```

Para visualizar mensagens publicadas:

```
Queues → proposta.contratada.queue
  → Get messages → Ackmode: Nack → Get Message(s)
```

Mensagem publicada:

```json
{
  "ContratacaoId":   "48988e73-6c65-46f6-b10d-8b8352111df2",
  "PropostaId":      "c13f6050-1426-48f9-83ae-41d7133fa7ba",
  "Cpf":             "529.982.247-25",
  "DataContratacao": "2026-09-05T14:30:00",
  "OcorridoEm":      "2026-09-05T14:30:00"
}
```

---

### Outbox Pattern — Garantia de Entrega

```
SEM Outbox:
  ├── INSERT contratacao  ✅
  ├── API cai             💥
  └── Evento perdido      ❌

COM Outbox:
  ├── INSERT contratacao + INSERT outbox  ✅ (mesma transação)
  ├── API cai             💥
  └── OutboxPublisherWorker retoma em 5s  ✅
```

Tabela `contratacao.outbox`:

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | UUID | Identificador do evento |
| `tipo` | VARCHAR | Nome do evento (`PropostaContratadaEvent`) |
| `payload` | JSONB | Corpo do evento serializado |
| `processado` | BOOLEAN | `false` = pendente, `true` = publicado |
| `criado_em` | TIMESTAMPTZ | Quando foi gravado |
| `processado_em` | TIMESTAMPTZ | Quando foi publicado |

---

### Migrations — Automáticas no Startup

As migrations SQL são aplicadas automaticamente no startup das APIs via **DbUp** quando `UsarBancoDados=true` — sem intervenção manual.

| Migration | Descrição |
|-----------|-----------|
| V001 | CREATE SCHEMA proposta |
| V002 | CREATE TABLE proposta.propostas |
| V003 | CREATE SCHEMA contratacao |
| V004 | CREATE TABLE contratacao.contratacoes |
| V005 | ALTER TABLE contratacoes ADD COLUMN criado_em |
| V006 | CREATE TABLE contratacao.outbox (Outbox Pattern) |

---

### Testes de Integração — Kafka e RabbitMQ

Os testes sobem **infraestrutura real** via Testcontainers e validam o fluxo completo de publicação:

```
KafkaFixture (Testcontainers)
  ├── docker run postgres:16-alpine   ← banco limpo
  ├── docker run cp-kafka:7.4.0       ← kafka limpo
  ├── aplica migrations V001-V006
  └── sobe ContratacaoService apontando para esses containers

ContratarProposta_DevePublicarEventoNoKafka
  ├── registra proposta aprovada no FakePropostaClient
  ├── POST /api/Contratacoes → 201 Created
  ├── aguarda OutboxPublisherWorker (8s)
  ├── consome do tópico proposta-contratada
  └── verifica payload contém PropostaId ✅
```

> Sem Docker local os testes são pulados automaticamente (`Skip`) — sem falha.

---

### Destaques Técnicos


**Agnóstico ao broker — Strategy via DI:**

```csharp
if (usarKafka)
    services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
else if (usarRabbitMQ)
    services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
else
    services.AddSingleton<IEventPublisher, NullEventPublisher>();
```

**GitOps — Feature flags via CI/CD:**

```yaml
workflow_dispatch:
  inputs:
    broker:
      type: choice
      options: [kafka, rabbitmq, nenhum]
    usar_banco:
      type: choice
      options: ['true', 'false']
```



### Consumidores — O que acontece após a publicação

O `ContratacaoService` atua como **produtor** — publica o evento e retorna 201 sem se preocupar com o que acontece a seguir. Esse é o princípio do desacoplamento via mensageria.

| Consumidor | Responsabilidade |
|------------|--------------------|
| **ApoliceService** | Gera o documento PDF da apólice e disponibiliza ao segurado |
| **CobrancaService** | Agenda o débito mensal do prêmio na conta ou cartão |
| **NotificacaoService** | Envia e-mail/WhatsApp de confirmação ao segurado |
| **SusepService** | Registra a contratação junto à SUSEP dentro do prazo legal de 24h |
| **AntiFraudeService** | Analisa padrões suspeitos no perfil e na contratação |
| **AuditoriaService** | Registra eventos em log imutável para compliance |

> No projeto atual os consumidores não estão implementados — as mensagens ficam acumuladas no broker aguardando. Em produção real seriam processadas imediatamente.

**Por que mensageria e não HTTP direto:**

```
Sem mensageria (HTTP síncrono):
└── ContratacaoService chama cada serviço diretamente
    ├── Se NotificacaoService cair  → contratação falha
    ├── Se CobrancaService lento    → resposta demora
    └── Acoplamento alto entre serviços

Com mensageria (assíncrono):
└── ContratacaoService publica o evento e retorna 201
    ├── Cada consumidor processa no seu próprio ritmo
    ├── Se um cair, a mensagem fica na fila até ele voltar
    ├── Escala independente por serviço
    └── Desacoplamento total
```

---


</details>


## 🏗️ Arquitetura do Sistema

<details>
<summary><strong>Ver diagrama, fluxo de funcionamento e Arquitetura Hexagonal</strong></summary>

## Diagrama Arquitetural

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

<details>
<summary><strong>Fluxo de Funcionamento</strong></summary>

<br>

O cliente (Swagger, Postman ou aplicação front-end) interage com dois serviços independentes via HTTP REST.

**PropostaService** recebe a solicitação de seguro, valida o CPF e as regras de negócio por tipo de produto (Strategy Pattern) e persiste a proposta com status `EmAnalise`. O repositório é intercambiável via feature flag: `InMemory` para desenvolvimento, `PostgreSQL via Dapper` para produção.

**ContratacaoService** recebe a requisição de contratação, consulta o PropostaService via HTTP para confirmar que a proposta existe e está aprovada, persiste a contratação e publica o evento `PropostaContratadaEvent` no RabbitMQ. A publicação é opcional (feature flag) e, caso o RabbitMQ esteja indisponível, o `NullEventPublisher` garante que a API não falhe — a contratação é salva normalmente.

Ambos os serviços compartilham o mesmo banco `seguros_db`, segregados por schemas (`proposta` e `contratacao`).

</details>

<details>
<summary><strong>O que é Arquitetura Hexagonal — e como nossa implementação é aderente</strong></summary>

<br>

A Arquitetura Hexagonal (Ports & Adapters), proposta por Alistair Cockburn, organiza a aplicação em três zonas concêntricas:

- **Núcleo (Domain + Application)** — contém as regras de negócio e os casos de uso. É completamente isolado: não referencia banco de dados, HTTP, fila ou qualquer framework externo.
- **Ports** — interfaces definidas pelo núcleo que descrevem o que ele precisa do mundo externo (ex: "preciso persistir uma proposta", "preciso publicar um evento").
- **Adapters** — implementações concretas dos Ports, vivendo na camada de Infrastructure. São substituíveis sem alterar o núcleo.

O princípio central é que **a infraestrutura depende do domínio — nunca o contrário**.

---

**Como nossas camadas mapeiam para a Arquitetura Hexagonal:**

| Camada do projeto | Papel na Hexagonal | O que contém |
|-------------------|--------------------|--------------|
| `*.Domain` | Núcleo — entidades e Ports de saída | `Proposta`, `Contratacao`, `IPropostaRepository`, `IContratacaoRepository`, `IEventPublisher`, `IRegraSeguro` |
| `*.Application` | Núcleo — casos de uso | `CriarPropostaUseCase`, `ContratarPropostaUseCase`, `Result<T>`, DTOs |
| `*.Infrastructure` | Adapters de saída | `DapperPropostaRepository`, `InMemoryPropostaRepository`, `RabbitMqEventPublisher`, `NullEventPublisher`, `HttpPropostaServiceClient` |
| `*.Api` | Adapter de entrada | Controllers REST, configuração de DI, injeção do Adapter correto via feature flag |

**Evidência prática da aderência:** os 13 testes unitários rodam sem banco de dados, sem Docker e sem RabbitMQ — porque os Use Cases dependem apenas das interfaces do Domain, e os testes injetam mocks no lugar dos Adapters reais. Trocar PostgreSQL por InMemory exige zero alteração de código — apenas uma flag de configuração.

---

**Tipos de seguro suportados** — cada tipo possui sua própria implementação de `IRegraSeguro` (Strategy Pattern):

| Código | Tipo |
|--------|------|
| 1 | SeguroFGTSProtegido |
| 2 | SeguroVidaFamiliar |
| 3 | SeguroCartaoProtegido |
| 4 | ProtecaoCreditoTrabalhador |
| 5 | SeguroContaCelularProtegidos |

</details>

---

</details>


## 📁 Estrutura do Projeto

<details>
<summary><strong>Ver estrutura de pastas e principais classes</strong></summary>


```
proposta-seguros/
├── src/
│   ├── PropostaService/
│   │   ├── PropostaService.Api/          # Controllers, DI, Program.cs
│   │   ├── PropostaService.Application/  # Use Cases, DTOs, interfaces de entrada
│   │   ├── PropostaService.Domain/       # Entidades, Ports (interfaces), regras
│   │   └── PropostaService.Infrastructure/ # Adapters: Dapper, InMemory, HTTP
│   └── ContratacaoService/
│       ├── ContratacaoService.Api/
│       ├── ContratacaoService.Application/
│       ├── ContratacaoService.Domain/
│       └── ContratacaoService.Infrastructure/
├── tests/
│   ├── PropostaService.Tests/            # 13 testes unitários
│   └── ContratacaoService.Tests/
├── migrations/                           # SQL versionado V001–V006 (DbUp automático)
├── scripts/                              # Shell scripts de operação VPS
├── docs/
│   ├── postman/                          # Collection + environments
│   └── enunciado.md
├── docker-compose.yml
├── .env.example
└── proposta-seguros.sln
```

### Principais classes e responsabilidades

| Camada | Classe / Interface | Responsabilidade |
|--------|--------------------|-----------------|
| Domain | `Proposta` | Entidade principal — encapsula status e regras de transição |
| Domain | `IPropostaRepository` | Port de saída — contrato de persistência |
| Domain | `IRegraSeguro` | Port — Strategy de validação por tipo de seguro |
| Domain | `SeguroFactory` | Factory — retorna a implementação correta de `IRegraSeguro` |
| Application | `CriarPropostaUseCase` | Orquestra criação e validação da proposta |
| Application | `ContratarPropostaUseCase` | Orquestra contratação, consulta ao PropostaService e publicação de evento |
| Application | `Result<T>` | Encapsula sucesso ou falha sem lançar exceções de controle de fluxo |
| Infrastructure | `DapperPropostaRepository` | Adapter — implementa `IPropostaRepository` com SQL explícito via Dapper |
| Infrastructure | `InMemoryPropostaRepository` | Adapter — implementa `IPropostaRepository` com `List<T>` em memória |
| Infrastructure | `RabbitMqEventPublisher` | Adapter — publica `PropostaContratadaEvent` no RabbitMQ |
| Infrastructure | `NullEventPublisher` | Null Object — substitui RabbitMQ quando desabilitado, sem falhar |
| Infrastructure | `HttpPropostaServiceClient` | Adapter — consulta o PropostaService via HTTP |
| Api | `PropostasController` | Entrada HTTP — delega para Use Cases, mapeia para status codes |

---

</details>


## 🚀 Como Executar

<details>
<summary><strong>Ver opções de execução — VPS, Docker local ou sem Docker</strong></summary>


> Optei por VPS da Hostinger,em vez de ambiente local para aproximar o projeto de um cenário real de produção — provisionamento, deploy via SSH e operação com Docker em servidor Linux são habilidades que fazem parte do dia a dia de desenvolvimento moderno.

### Opção 1 — Docker na VPS (recomendado)

```bash
ssh root@2.25.122.11
cd /home/projetos/proposta-seguros
./scripts/apply.sh
```

### Opção 2 — Docker local

```bash
git clone https://github.com/josehelioaraujo/proposta-seguros.git
cd proposta-seguros
cp .env.example .env
docker compose up --build -d
```

### Opção 3 — Local sem Docker

```bash
# Terminal 1 — PropostaService
cd src/PropostaService/PropostaService.Api
dotnet run

# Terminal 2 — ContratacaoService
cd src/ContratacaoService/ContratacaoService.Api
dotnet run
```

---

</details>


## 🌐 URLs, Endpoints e Acessos

<details>
<summary><strong>Ver URLs, endpoints das APIs e health checks</strong></summary>

### VPS Hostinger

| Serviço | URL |
|---|---|
| PropostaService — Swagger | http://2.25.122.11:5001 |
| ContratacaoService — Swagger | http://2.25.122.11:5002 |
| Kafka UI | http://2.25.122.11:8082 |
| RabbitMQ — Painel | http://2.25.122.11:15672 |
| Grafana | http://2.25.122.11:3000 |
| Prometheus | http://2.25.122.11:9090 |
| Jaeger | http://2.25.122.11:16686 |
| Adminer | http://2.25.122.11:5050 |

### Local

| Serviço | URL |
|---|---|
| PropostaService — Swagger | http://localhost:5001 |
| ContratacaoService — Swagger | http://localhost:5002 |
| RabbitMQ — Painel | http://localhost:15672 |

### Endpoints — PropostaService

| Método | Rota | Descrição | Status |
|--------|------|-----------|--------|
| POST | /api/Propostas | Cria uma proposta | 201 |
| GET | /api/Propostas | Lista todas as propostas | 200 |
| GET | /api/Propostas/{id} | Busca proposta por ID | 200 |
| PATCH | /api/Propostas/{id}/status | Altera status da proposta | 200 |

### Endpoints — ContratacaoService

| Método | Rota | Descrição | Status |
|--------|------|-----------|--------|
| POST | /api/Contratacoes | Contrata uma proposta aprovada | 201 |
| GET | /api/Contratacoes/{id} | Busca contratação por ID | 200 |

### Health Checks

| Endpoint | PropostaService | ContratacaoService |
|----------|-----------------|-------------------|
| /health | :5001/health | :5002/health |
| /health/live | :5001/health/live | :5002/health/live |
| /health/ready | :5001/health/ready | :5002/health/ready |

</details>


## 🗃️ Migrations SQL

<details>
<summary><strong>Ver scripts de banco e estrutura criada</strong></summary>


As migrations são aplicadas **automaticamente no startup** das APIs via **DbUp** quando `UsarBancoDados=true` — sem intervenção manual.

> Para aplicar manualmente na VPS:

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

</details>


## 🧪 Testes

<details>
<summary><strong>Ver como executar os testes unitários e de integração</strong></summary>


```bash
dotnet test .\proposta-seguros.sln
```

```
Resultado esperado:
total: 13 | falhou: 0 | bem-sucedido: 13
```

---

</details>


## 📮 Postman Collection

<details>
<summary><strong>Ver como importar e usar a collection</strong></summary>


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

| Pasta | Descrição |
|-------|-----------|
| 01 — Health Checks | Verifica saúde dos dois serviços |
| 02 — PropostaService | Todos os cenários de proposta |
| 03 — ContratacaoService | Todos os cenários de contratação |
| 04 — Fluxo Completo | Fluxo end-to-end com dados fixos |
| 05 — Fluxo VPS InMemory | Dados aleatórios — sem banco |
| 06 — Fluxo VPS PostgreSQL | Dados aleatórios — com banco |
| 07 — Fluxo VPS PostgreSQL + RabbitMQ | Dados aleatórios — banco + fila |

---

</details>


## 🎭 Simulação Completa de Testes

<details>
<summary><strong>Ver cenários InMemory, PostgreSQL e Kafka/RabbitMQ</strong></summary>


### Cenário 1 — InMemory

```bash
# Na VPS
./scripts/set-banco.sh --disable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "05 — Fluxo VPS InMemory"
Ação:    Run folder -> Start run

Resultado esperado:
- 7/7 testes passando
- Dados gerados automaticamente
- Fluxo: criar -> aprovar -> contratar -> verificar
```

### Cenário 2 — PostgreSQL

```bash
# Aplica migrations
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V001__create_schema_proposta.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V002__create_table_propostas.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V003__create_schema_contratacao.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V004__create_table_contratacoes.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V005__add_criado_em_contratacoes.sql

# Liga PostgreSQL
./scripts/set-banco.sh --enable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "06 — Fluxo VPS PostgreSQL"
Ação:    Run folder -> Start run

Resultado esperado:
- 7/7 testes passando
- Dados persistidos no PostgreSQL
- Health check mostra postgres: Healthy
```

### Cenário 3 — PostgreSQL + RabbitMQ

```bash
# Liga RabbitMQ
./scripts/set-rabbitmq.sh --enable
```

```
Postman: ambiente "VPS Hostinger"
Pasta:   "07 — Fluxo VPS PostgreSQL + RabbitMQ"
Ação:    Run folder -> Start run

Resultado esperado:
- 7/7 testes passando
- Evento PropostaContratadaEvent publicado na fila
- Health check mostra postgres + rabbitmq: Healthy

Verifica a fila:
URL:     http://2.25.122.11:15672
Usuário: guest / Senha: guest
Fila:    proposta.contratada.queue
```

---

</details>


## 🚩 Feature Flags

<details>
<summary><strong>Ver flags disponíveis e scripts de operação</strong></summary>


| Flag | false (padrão) | true |
|------|----------------|------|
| Features:UsarBancoDados | InMemory | PostgreSQL via Dapper |
| Features:UsarRabbitMQ | Sem mensageria | Publica PropostaContratadaEvent |

### Scripts de operação (VPS)

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

---

</details>


## 📊 Observabilidade

<details>
<summary><strong>Ver Prometheus, Grafana, Jaeger, Loki e URLs</strong></summary>


Stack completa de observabilidade rodando em produção — métricas, logs e traces distribuídos, integrada e acessível via browser.

<details>
<summary><strong>📊 Prometheus — Coleta de Métricas</strong></summary>

<br>

Coleta métricas das duas APIs a cada 15 segundos via scrape no endpoint `/metrics`. Armazena séries temporais e responde queries PromQL.

**Métricas de negócio instrumentadas:**

| Métrica | Descrição |
|---|---|
| `propostas_criadas_total` | Total de propostas criadas |
| `propostas_aprovadas_total` | Total de propostas aprovadas |
| `propostas_rejeitadas_total` | Total de propostas rejeitadas |
| `contratacoes_realizadas_total` | Total de contratações realizadas |
| `rabbitmq_eventos_publicados_total` | Total de eventos publicados no RabbitMQ |

**Métricas de infra — automáticas via `prometheus-net`:**
- Requisições HTTP por segundo por endpoint e status code
- Latência HTTP (histograma — p50, p90, p99)

**Exemplos de queries PromQL:**

```promql
propostas_criadas_total
propostas_aprovadas_total / clamp_min(propostas_criadas_total, 1) * 100
rate(http_requests_received_total[1m])
histogram_quantile(0.99, rate(http_request_duration_seconds_bucket[1m]))
```

</details>

<details>
<summary><strong>📈 Grafana — Dashboards</strong></summary>

<br>

Dashboard **Proposta de Seguros** provisionado automaticamente com 10 painéis — métricas de negócio, latência HTTP, eventos RabbitMQ, taxa de aprovação e logs em tempo real.

Datasources provisionados automaticamente: Prometheus, Loki e Jaeger.

| URL | Descrição |
|---|---|
| https://grafana.2.25.122.11.nip.io | Acesso HTTPS (certificado Let's Encrypt) |
| http://2.25.122.11:3000 | Acesso direto HTTP |

> HTTPS via Nginx + Let's Encrypt com certificado automático pelo domínio `nip.io`.

</details>

<details>
<summary><strong>🔍 Jaeger — Distributed Tracing</strong></summary>

<br>

Rastreia o caminho completo de uma requisição entre os dois microserviços via OpenTelemetry. Zero alteração nos Use Cases — instrumentação automática de chamadas HTTP e HttpClient.

**Exemplo de trace — `POST /api/Contratacoes`:**

```
contratacao-api: POST /api/Contratacoes          [342ms]
  ├── Validação FluentValidation                  [2ms]
  ├── Repository.GetByPropostaIdAsync             [8ms]
  ├── HTTP GET proposta-api/api/Propostas/{id}   [298ms]
  │     └── proposta-api: GET /api/Propostas/{id} [295ms]
  └── Repository.AddAsync                         [9ms]
```

| URL | Descrição |
|---|---|
| http://2.25.122.11:16686 | UI do Jaeger |

</details>

<details>
<summary><strong>📋 Loki — Agregação de Logs</strong></summary>

<br>

Agrega os logs dos containers Docker via Promtail e disponibiliza para consulta via LogQL no Grafana. Zero alteração no código — coleta automática via docker socket.

```logql
{service="proposta-api"}
{service="contratacao-api"} |= "error"
```

Os painéis de logs estão integrados no dashboard do Grafana.

</details>

<details>
<summary><strong>🔗 Resumo — URLs de Observabilidade</strong></summary>

<br>

| Ferramenta | URL | Descrição |
|---|---|---|
| **Grafana** | https://grafana.2.25.122.11.nip.io | Dashboards — métricas, logs e traces |
| **Grafana** | http://2.25.122.11:3000 | Acesso direto HTTP |
| **Prometheus** | http://2.25.122.11:9090 | Queries PromQL e status dos targets |
| **Jaeger** | http://2.25.122.11:16686 | Distributed tracing |
| **Loki** | http://2.25.122.11:3100 | API de logs (acesso via Grafana) |
| **PropostaService /metrics** | http://2.25.122.11:5001/metrics | Endpoint raw Prometheus |
| **ContratacaoService /metrics** | http://2.25.122.11:5002/metrics | Endpoint raw Prometheus |

```bash
./scripts/set-monitoring.sh --enable   # sobe Prometheus, Grafana, Jaeger, Loki, Promtail
./scripts/set-monitoring.sh --disable  # para e remove os containers
```

</details>

---

</details>


## 🤖 MCP Server — Integração com IA

<details>
<summary><strong>Ver tools disponíveis e como conectar ao Claude</strong></summary>


Ambas as APIs expõem um **MCP Server** (Model Context Protocol), permitindo que agentes de IA como o Claude interajam diretamente com o sistema em linguagem natural — sem precisar construir requisições HTTP manualmente.

<details>
<summary><strong>🤖 Tools disponíveis</strong></summary>

<br>

**PropostaService** — `http://2.25.122.11:5001/mcp`

| Tool | Descrição |
|---|---|
| `criar_proposta` | Cria uma nova proposta de seguro |
| `listar_propostas` | Lista todas as propostas cadastradas |
| `obter_proposta` | Obtém uma proposta pelo ID |
| `alterar_status_proposta` | Altera o status de uma proposta (EmAnalise / Aprovada / Rejeitada) |

**ContratacaoService** — `http://2.25.122.11:5002/mcp`

| Tool | Descrição |
|---|---|
| `contratar_proposta` | Contrata uma proposta aprovada |
| `obter_contratacao` | Obtém uma contratação pelo ID |

</details>

<details>
<summary><strong>🧪 Testando com MCP Inspector</strong></summary>

<br>

O MCP Inspector é uma UI visual para explorar e executar as tools — equivalente ao Swagger para MCP.

```bash
# Requer Node.js instalado
npx @modelcontextprotocol/inspector http://2.25.122.11:5001/mcp
npx @modelcontextprotocol/inspector http://2.25.122.11:5002/mcp
```

Abre no browser em `http://localhost:6274` — conecta, seleciona a tool, preenche os parâmetros e executa.

</details>

<details>
<summary><strong>🔌 Conectando ao Claude Desktop</strong></summary>

<br>

1. Abre o Claude Desktop → **Configurações → Conectores → Adicionar conector**
2. Adiciona os dois servidores:

| Nome | URL |
|---|---|
| Proposta Seguros | `http://2.25.122.11:5001/mcp` |
| Contratacao Seguros | `http://2.25.122.11:5002/mcp` |

3. Habilita os conectores na conversa
4. Exemplos de uso em linguagem natural:

```
"Crie uma proposta para João Silva, CPF 123.456.789-09, tipo 2 (SeguroVidaFamiliar), valor 200"
"Liste todas as propostas"
"Aprove a proposta ID xxxx-xxxx"
"Contrate a proposta ID xxxx-xxxx para o CPF 123.456.789-09"
```

</details>

<details>
<summary><strong>⚙️ Implementação</strong></summary>

<br>

O MCP Server é implementado como um **Adapter de entrada** na camada `Api` — padrão Ports & Adapters aplicado à comunicação com agentes de IA, da mesma forma que os Controllers REST são Adapters de entrada HTTP.

```
Agente IA → MCP /mcp → [PropostasMcpAdapter] → Use Cases → Domain
HTTP REST → GET/POST → [PropostasController] → Use Cases → Domain
```

Pacote utilizado: `ModelContextProtocol.AspNetCore`

Registro no `Program.cs`:
```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<PropostasMcpAdapter>();

app.MapMcp("/mcp");
```

</details>

</details>


## ⭐ Destaques de Implementação

<details>
<summary><strong>Ver padrões e decisões técnicas do projeto</strong></summary>


### Mensageria — Outbox Pattern

O `ContratacaoService` publica o evento `PropostaContratadaEvent` após cada contratação bem-sucedida. O broker ativo é definido por feature flag — Kafka ou RabbitMQ — sem alteração de código.

```
Evento: PropostaContratadaEvent

Consumidores em produção real:
- ApoliceService      -> gera documento da apólice
- CobrancaService     -> agenda débito mensal
- NotificacaoService  -> envia e-mail/WhatsApp ao cliente
- SusepService        -> registro regulatório
```

Feature flags: `Features:UsarKafka` / `Features:UsarRabbitMQ`

### Health Checks — ASP.NET Nativo

Implementados usando o sistema nativo do ASP.NET Core sem bibliotecas externas,
com três níveis de verificação por serviço.

```
PropostaService    -> self + postgres (se habilitado)
ContratacaoService -> self + postgres + proposta-service + rabbitmq
```

### PostgreSQL + Dapper com Feature Flag

O sistema opera em dois modos sem alteração de código:

```
UsarBancoDados: false -> InMemory (List<T>) — desenvolvimento
UsarBancoDados: true  -> PostgreSQL via Dapper — produção
```

Demonstra o padrão Ports & Adapters na prática: o mesmo Use Case funciona
com qualquer repositório injetado via DI.

### Logging Estruturado

Logging implementado em todos os Use Cases com níveis configurados por ambiente
via appsettings.json, preparado para integração futura com Prometheus e Grafana.

### Docker — Multi-stage Build

Imagens construídas em duas etapas: SDK para compilar, runtime para executar.
Resultado: imagem final de aproximadamente 200MB ao invés de 900MB, sem código
fonte exposto e sem ferramentas de build em produção.

### Deploy em VPS

Sistema em execução em VPS real na Hostinger (Ubuntu 24.04, Docker 29.7.2),
acessível publicamente para avaliação sem necessidade de setup local.

```
PropostaService:    http://2.25.122.11:5001
ContratacaoService: http://2.25.122.11:5002
```

### Pipeline de CI/CD

Foi implementado para demonstrar automação completa do ciclo de desenvolvimento — do commit ao ambiente de produção, sem intervenção manual.

O pipeline é composto por cinco etapas executadas em sequência:

```
push em src/** → Testes de Unidade → Integração PropostaService → Integração RabbitMQ → SonarCloud → Deploy VPS
                       ❌ para tudo         ❌ para aqui                ❌ para aqui        ❌ para aqui
```

- **Testes de Unidade** — executa os 13 testes automaticamente a cada push. Se algum falhar, o pipeline é interrompido e o deploy não acontece.
- **Testes de Integração — PropostaService** — sobe a API em memória via `WebApplicationFactory` e testa os modos InMemory e PostgreSQL com banco real via Testcontainers.
- **Testes de Integração — RabbitMQ** — sobe PostgreSQL e RabbitMQ reais via Testcontainers e valida que o ContratacaoService conecta e opera corretamente.
- **SonarCloud** — analisa qualidade de código, bugs, code smells e duplicações. Só prossegue se todos os testes passarem.
- **Deploy automático na VPS** — conecta via SSH, faz `git pull`, reconstrói as imagens Docker e sobe os containers. Só executa se testes e análise passarem.

O pipeline **não dispara** em commits de documentação (`README`, `docs/`, `scripts/`) — apenas alterações em `src/**` ou `tests/**` acionam a execução, evitando builds desnecessários.

Cada deploy injeta automaticamente a versão, o commit SHA e a data de build nas imagens Docker, rastreáveis via endpoint:

```
GET http://2.25.122.11:5001/info
GET http://2.25.122.11:5002/info
```

```json
{
  "service":     "PropostaService",
  "version":     "1.0.11",
  "commit":      "56d4384",
  "builtAt":     "2026-08-30T18:27:11Z",
  "serverTime":  "2026-08-30T18:28:00Z",
  "serverName":  "bba1df1c6759",
  "environment": "Production"
}
```

### Testes de Integração

Foram implementados para demonstrar cobertura além dos testes unitários — validando o comportamento real da aplicação com infraestrutura real.

Utilizam `WebApplicationFactory` (ASP.NET Core) e `Testcontainers` — containers Docker reais sobem e são destruídos automaticamente durante a execução dos testes, sem dependência de ambiente externo.

Os testes são controlados pela variável `RUN_INTEGRATION_TESTS`:
- **`true`** (padrão no pipeline) — todos os testes executam
- **`false`** — testes de integração são pulados (`Skipped`), sem falha
- **Sem Docker local** — testes com Testcontainers são pulados automaticamente

**Cenários cobertos:**

| Projeto | Modo | Cenário | Resultado esperado |
|---------|------|---------|-------------------|
| PropostaService | InMemory | `POST /api/Propostas` com dados válidos | 201 Created |
| PropostaService | InMemory | `GET /api/Propostas` | 200 OK |
| PropostaService | InMemory | `GET /health` | Healthy |
| PropostaService | InMemory | `POST /api/Propostas` com CPF inválido | 400 Bad Request |
| PropostaService | PostgreSQL | `POST` + `GET /{id}` — cria e persiste | 201 + 200 |
| PropostaService | PostgreSQL | Proposta duplicada (mesmo CPF + tipo) | 409 Conflict |
| PropostaService | PostgreSQL | `GET /health` com banco real | Healthy |
| ContratacaoService | RabbitMQ | Conexão direta ao broker | IsOpen = true |
| ContratacaoService | RabbitMQ | `GET /health/live` com RabbitMQ real | Healthy |
| ContratacaoService | RabbitMQ | `POST /api/Contratacoes` sem PropostaService | API responde (sem travar) |

---

</details>


## 🐳 Containers Docker

<details>
<summary><strong>Ver containers, imagens e configurações</strong></summary>


| Container | Imagem | Porta | Descrição |
|-----------|--------|-------|-----------|
| proposta-api | proposta-seguros-proposta-api | 5001 | API de gerenciamento de propostas de seguro |
| contratacao-api | proposta-seguros-contratacao-api | 5002 | API de contratação de propostas aprovadas |
| seguros-postgres | postgres:16-alpine | 5432 | Banco de dados PostgreSQL compartilhado |
| seguros-rabbitmq | rabbitmq:4-management-alpine | 5672 / 15672 | Mensageria — sobe apenas com profile rabbitmq |

### Observações

```
proposta-api e contratacao-api
└── Multi-stage build: sdk:10.0 (build) + aspnet:10.0 (runtime)
└── Imagem final: ~200MB (sem SDK, sem código fonte)
└── Ambiente: Production

seguros-postgres
└── Volume persistente: postgres_data
└── Healthcheck: pg_isready a cada 10s
└── APIs só sobem após postgres estar Healthy

seguros-rabbitmq
└── Sobe apenas quando USAR_RABBITMQ=true
└── Painel de gerenciamento: http://2.25.122.11:15672
└── usuário: guest / senha: guest
```

---

</details>


## 🛠️ Scripts de Operação

<details>
<summary><strong>Ver todos os scripts disponíveis na VPS</strong></summary>


Todos os scripts ficam na pasta `scripts/` e devem ser executados
a partir da raiz do projeto na VPS.

### Gerenciamento de containers

| Script | Descrição |
|--------|-----------| 
| `./scripts/start.sh` | Inicia os containers |
| `./scripts/stop.sh` | Para os containers |
| `./scripts/restart.sh` | Reinicia os containers |
| `./scripts/status.sh` | Exibe status dos containers e URLs |
| `./scripts/update.sh` | git pull + rebuild + reinicia |

### Feature flags

| Script | Descrição |
|--------|-----------|
| `./scripts/set-banco.sh --enable` | Liga PostgreSQL (Dapper) |
| `./scripts/set-banco.sh --disable` | Volta para InMemory |
| `./scripts/set-rabbitmq.sh --enable` | Liga RabbitMQ e sobe o container |
| `./scripts/set-rabbitmq.sh --disable` | Desliga RabbitMQ |
| `./scripts/apply.sh` | Aplica as flags do .env e reinicia |

### Logs

| Script | Descrição |
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

# Atualiza para última versão
./scripts/update.sh

# Alterna para PostgreSQL
./scripts/set-banco.sh --enable

# Acompanha logs em tempo real
./scripts/logs.sh --proposta
```

### Arquivo .env

O arquivo `.env` na raiz do projeto persiste as feature flags entre reinicializações:

```bash
# Ver flags atuais
cat .env

# Conteúdo esperado:
USAR_BANCO_DADOS=false
USAR_RABBITMQ=false
```

Os scripts `set-banco.sh` e `set-rabbitmq.sh` atualizam o `.env` automaticamente.

---


---


</details>


## 🖥️ Painéis Administrativos

<details>
<summary><strong>Ver Adminer, RabbitMQ Management e Kafka UI</strong></summary>


### Adminer — PostgreSQL

Interface web para visualizar e consultar o banco de dados PostgreSQL.

```bash
# Sobe o Adminer
./scripts/set-adminer.sh --enable

# Para o Adminer
./scripts/set-adminer.sh --disable
```

```
URL:      http://2.25.122.11:5050
Sistema:  PostgreSQL
Servidor: postgres
Usuário:  postgres
Senha:    postgres
Banco:    seguros_db
```

Tabelas disponíveis:

```
seguros_db
├── proposta.propostas       — propostas de seguro criadas
└── contratacao.contratacoes — contratações realizadas
```

---

### RabbitMQ Management — Mensageria

Interface web nativa do RabbitMQ para monitorar filas e mensagens.

```
URL:     http://2.25.122.11:15672
Usuário: guest
Senha:   guest
```

O que monitorar após rodar o fluxo com RabbitMQ habilitado:

```
Queues and Streams
└── proposta.contratada.queue
    ├── Messages ready   — mensagens aguardando consumidor
    ├── Message rates    — taxa de publicação
    └── Get messages     — visualiza o conteúdo do evento
```

Para ver o conteúdo de uma mensagem:

```
Queues → proposta.contratada.queue
→ Get messages → Ackmode: Nack → Get Message(s)
```

Conteúdo esperado:

```json
{
  "ContratacaoId":   "48988e73-6c65-46f6-b10d-8b8352111df2",
  "PropostaId":      "c13f6050-1426-48f9-83ae-41d7133fa7ba",
  "Cpf":             "120.147.173-70",
  "DataContratacao": "2026-08-30T00:54:47",
  "OcorridoEm":      "2026-08-30T00:54:47"
}
```
</details>