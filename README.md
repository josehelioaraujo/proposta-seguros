# Proposta de Seguros

[![CI/CD](https://github.com/josehelioaraujo/proposta-seguros/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/josehelioaraujo/proposta-seguros/actions/workflows/ci-cd.yml)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=josehelioaraujo_proposta-seguros&metric=bugs)](https://sonarcloud.io/project/overview?id=josehelioaraujo_proposta-seguros)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=josehelioaraujo_proposta-seguros&metric=code_smells)](https://sonarcloud.io/project/overview?id=josehelioaraujo_proposta-seguros)
[![Evidências E2E](https://img.shields.io/badge/Testes%20E2E-Postman%20%7C%20Newman-orange)](https://josehelioaraujo.github.io/proposta-seguros/)

> 📊 **[Ver Relatório de Testes E2E ao vivo →](https://josehelioaraujo.github.io/proposta-seguros/)**  
> Gerado automaticamente após cada deploy via Newman + GitHub Pages.

Sistema de gerenciamento de propostas de seguro desenvolvido como teste técnico,
utilizando Arquitetura Hexagonal (Ports & Adapters), .NET 10 e PostgreSQL.

## Visão Geral

O sistema é composto por dois microserviços independentes:

| Serviço | Responsabilidade | Porta |
|---|---|---|
| **PropostaService** | Criar e gerenciar propostas de seguro | 5001 |
| **ContratacaoService** | Contratar propostas aprovadas e publicar eventos | 5002 |

---

## Diagrama Interativo

> 🔍 **[Abrir diagrama interativo e navegável →](https://gitdiagram.com/josehelioaraujo/proposta-seguros)**  
> Gerado automaticamente pelo GitDiagram — clique nos componentes para navegar pelo código.

---

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

## Tecnologias

- .NET 10 / C#
- PostgreSQL 16 + Dapper
- RabbitMQ 4 (mensageria — bônus)
- FluentValidation
- xUnit / Moq / Bogus / FluentAssertions
- Docker / Docker Compose
- Swagger / OpenAPI 3.0
- ASP.NET Health Checks

---


## Estrutura Organizacional

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
├── migrations/                           # SQL versionado V001–V005
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

## Como Executar

> Optei por VPS em vez de ambiente local para aproximar o projeto de um cenário real de produção — provisionamento, deploy via SSH e operação com Docker em servidor Linux são habilidades que fazem parte do dia a dia de desenvolvimento moderno.

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

## URLs

### VPS Hostinger

| Serviço | URL |
|---|---|
| PropostaService — Swagger | http://2.25.122.11:5001 |
| PropostaService — Health | http://2.25.122.11:5001/health |
| PropostaService — Info | http://2.25.122.11:5001/info |
| ContratacaoService — Swagger | http://2.25.122.11:5002 |
| ContratacaoService — Health | http://2.25.122.11:5002/health |
| ContratacaoService — Info | http://2.25.122.11:5002/info |
| RabbitMQ — Painel | http://2.25.122.11:15672 |

### Local

| Serviço | URL |
|---|---|
| PropostaService — Swagger | http://localhost:5001 |
| PropostaService — Health | http://localhost:5001/health |
| PropostaService — Info | http://localhost:5001/info |
| ContratacaoService — Swagger | http://localhost:5002 |
| ContratacaoService — Health | http://localhost:5002/health |
| ContratacaoService — Info | http://localhost:5002/info |
| RabbitMQ — Painel | http://localhost:15672 |

---

## Documentação

- [Enunciado do Projeto](docs/enunciado.md)
- [Postman Collection](docs/postman/)
- [Migrations SQL](migrations/)

---

## Endpoints

### PropostaService

| Método | Rota | Descrição | Status |
|--------|------|-----------|--------|
| POST | /api/Propostas | Cria uma proposta | 201 |
| GET | /api/Propostas | Lista todas as propostas | 200 |
| GET | /api/Propostas/{id} | Busca proposta por ID | 200 |
| PATCH | /api/Propostas/{id}/status | Altera status da proposta | 200 |

### ContratacaoService

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

---


<details>
<summary><strong>Migrations SQL</strong></summary>

As migrations criam os schemas e tabelas no PostgreSQL.

### Aplicar na VPS

```bash
cd /home/projetos/proposta-seguros

docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V001__create_schema_proposta.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V002__create_table_propostas.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V003__create_schema_contratacao.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V004__create_table_contratacoes.sql
docker exec -i seguros-postgres psql -U postgres -d seguros_db < migrations/V005__add_criado_em_contratacoes.sql
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

</details>

---

## Testes de Unidade

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


<details>
<summary><strong>Simulação Completa de Testes</strong></summary>

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

</details>

---

## Feature Flags

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

## Bônus

### RabbitMQ — Mensageria

O enunciado menciona mensageria como item opcional. O ContratacaoService publica
o evento PropostaContratadaEvent após cada contratação bem-sucedida.

```
Evento: PropostaContratadaEvent
Exchange: proposta.exchange (Direct)
Fila:     proposta.contratada.queue

Consumidores em produção real:
- ApoliceService   -> gera documento da apólice
- CobrancaService  -> agenda débito mensal
- NotificacaoService -> envia e-mail ao cliente
- SusepService     -> registro regulatório
```

Feature flag: Features:UsarRabbitMQ

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

### Smoke Tests E2E — Newman + GitHub Pages

Não solicitado no enunciado. Após cada deploy na VPS, a collection Postman é executada automaticamente via **Newman** (CLI oficial do Postman), validando o sistema em ambiente real de produção.

```
Deploy VPS → Health Check (aguarda VPS pronta) → Newman → GitHub Pages
```

O relatório HTML é publicado automaticamente e fica disponível em URL fixa:

**[📊 Ver Relatório de Testes E2E →](https://josehelioaraujo.github.io/proposta-seguros/)**

**Cenários executados — Fluxo 07 (PostgreSQL + RabbitMQ):**

| # | Cenário | Validações |
|---|---------|-----------|
| 07.1 | Criar Proposta | Status 201, ID retornado, CPF válido gerado automaticamente |
| 07.2 | Aprovar Proposta | Status 200, status da proposta = `Aprovada` |
| 07.3 | Contratar Proposta | Status 201, contratação criada com sucesso |
| 07.4 | Verificar Contratação | Status 200, dados persistidos no PostgreSQL |
| 07.5 | Health Check Completo | Status 200, postgres + rabbitmq = `Healthy` |

**Resultado:** 5 requests · 9 assertions · 0 falhas · ambiente real de produção (VPS Hostinger)

### Pipeline de CI/CD

Não solicitado no enunciado. Implementado para demonstrar automação completa do ciclo de desenvolvimento — do commit ao ambiente de produção, sem intervenção manual.

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

Não solicitados no enunciado. Implementados para demonstrar cobertura além dos testes unitários — validando o comportamento real da aplicação com infraestrutura real.

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


<details>
<summary><strong>Containers Docker</strong></summary>

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

</details>

---


<details>
<summary><strong>Scripts de Operação</strong></summary>

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

</details>

---


<details>
<summary><strong>Mensageria — Produtor e Consumidores</strong></summary>

Nossa aplicação atua apenas como **produtora** de eventos. O ContratacaoService
publica o evento `PropostaContratadaEvent` na fila após cada contratação
bem-sucedida e não se preocupa com o que acontece a seguir — esse é o
princípio do desacoplamento via mensageria.

### O que publicamos

```
Evento: PropostaContratadaEvent
Exchange: proposta.exchange (Direct)
Fila:     proposta.contratada.queue

Campos:
├── ContratacaoId    — identificador único da contratação
├── PropostaId       — referência à proposta contratada
├── Cpf              — CPF do segurado
├── DataContratacao  — data e hora da contratação
└── OcorridoEm       — timestamp do evento
```

### Quem consumiria em produção real

Cada consumidor é um microserviço independente que escuta a fila
e reage ao evento de forma autônoma:

| Consumidor | Responsabilidade |
|------------|-----------------|
| **ApoliceService** | Gera o documento PDF da apólice de seguro e disponibiliza para o segurado |
| **CobrancaService** | Agenda o débito mensal do prêmio na conta ou cartão do segurado |
| **NotificacaoService** | Envia e-mail e SMS de confirmação da contratação ao segurado |
| **SusepService** | Registra a contratação junto à SUSEP (órgão regulador de seguros) dentro do prazo legal de 24h |
| **AntiFraudeService** | Analisa o perfil do segurado e a contratação em busca de padrões suspeitos |
| **AuditoriaService** | Registra todos os eventos em log imutável para fins de compliance e rastreabilidade |

### Por que mensageria e não HTTP direto

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

### Como verificar as mensagens publicadas

Após rodar o fluxo 07 no Postman, acesse o painel do RabbitMQ:

```
URL:     http://2.25.122.11:15672
Usuário: guest
Senha:   guest

Queues and Streams
└── proposta.contratada.queue
    └── Messages ready: N (mensagens aguardando consumidor)
```

As mensagens ficam acumuladas pois não há consumidores implementados
neste projeto — em produção real seriam processadas imediatamente.

</details>

---


<details>
<summary><strong>Painéis Administrativos</strong></summary>

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