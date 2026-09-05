# Changelog

Todas as mudanças relevantes deste projeto estão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).

---

## [1.0.44] — 2026-09-05

### Adicionado
- **Outbox Pattern** — evento gravado na mesma transação PostgreSQL da contratação, garantindo atomicidade entre banco e mensageria. Zero perda de evento mesmo em falha da API
- **Apache Kafka KRaft** — broker Kafka sem Zookeeper (1 container), profile `kafka` no Docker Compose
- **Kafka UI** (Provectus) — dashboard web para visualizar tópicos e mensagens (porta 8082)
- **KafkaEventPublisher** — publica `PropostaContratadaEvent` no tópico `proposta-contratada` com `Acks.All` e idempotência habilitada
- **OutboxPublisherWorker** — `BackgroundService` que lê o outbox a cada 5s e publica no broker ativo
- **NullOutboxRepository** — permite operação sem banco (modo InMemory) sem quebrar o DI
- **DbUp-PostgreSQL** — migrations aplicadas automaticamente no startup das APIs quando `UsarBancoDados=true`
- **Migration V006** — tabela `contratacao.outbox` com índice parcial `WHERE processado = false`
- **Testes de integração Kafka** — `KafkaFixture` sobe Kafka + PostgreSQL reais via Testcontainers e valida publicação E2E do evento no tópico
- **FakePropostaServiceClient** — permite testes isolados do `ContratacaoService` sem dependência HTTP do `PropostaService`
- **workflow_dispatch** no CI/CD — disparo manual da esteira com seleção de broker (kafka/rabbitmq/nenhum) e banco via UI do GitHub Actions
- **Feature flag `UsarKafka`** — mensageria agnóstica: Kafka ou RabbitMQ intercambiáveis via `.env`, zero alteração de código

### Alterado
- `ContratarPropostaUseCase` — grava contratação + evento outbox na mesma transação PostgreSQL
- `IContratacaoRepository` — sobrecarga `AddAsync(contratacao, IDbTransaction)` para suporte a transações
- `OutboxPublisherWorker` usa `IServiceScopeFactory` para resolver `IOutboxRepository` (Scoped) a partir do Hosted Service (Singleton) — padrão oficial Microsoft
- Job CI/CD `Testes de Integração RabbitMQ` renomeado para `Testes de Integração Mensageria` — agnóstico ao broker
- Deploy CI/CD sempre inclui profile `kafka` e aplica feature flags por default (Kafka + banco habilitados)
- `start.sh` atualizado com profile `kafka`
- `docker-compose.yml` atualizado com Kafka KRaft, Kafka UI e volume `kafka_data`
- `appsettings.json` ContratacaoService atualizado com seção `Kafka` e flag `UsarKafka`

### Corrigido
- Conflito de lifetime: `OutboxPublisherWorker` (Singleton) consumindo `IOutboxRepository` (Scoped) — resolvido com `IServiceScopeFactory`
- Kafka KRaft falhava com `advertised.listeners = 0.0.0.0` — corrigido com `hostname: seguros-kafka` explícito no Docker Compose
- Versões incompatíveis de Testcontainers (PostgreSql 4.4.0 vs Kafka 4.14.0) — alinhadas para 4.14.0

---

## [1.0.36] — 2026-08-31

### Adicionado
- **Stack de Observabilidade completa** — Prometheus, Grafana, Jaeger (OpenTelemetry), Loki, Promtail
- **Grafana HTTPS** via Nginx + Let's Encrypt (`grafana.2.25.122.11.nip.io`)
- **MCP Server** em ambas as APIs — 6 tools para integração com agentes de IA (Claude)
- **Testes de integração** com Testcontainers — PostgreSQL e RabbitMQ reais no CI/CD
- **Smoke Tests E2E** via Newman + relatório publicado no GitHub Pages
- **SonarCloud** — análise de qualidade de código integrada ao pipeline
- **Métricas de negócio** — `propostas_criadas_total`, `contratacoes_realizadas_total`, `rabbitmq_eventos_publicados_total`
- **Distributed tracing** — rastreamento completo entre PropostaService e ContratacaoService via Jaeger
- **HTTPS para MCP** via Nginx (`proposta.2.25.122.11.nip.io`, `contratacao.2.25.122.11.nip.io`)
- **Ollama** instalado na VPS como serviço systemd — modelos llama3.2, mistral, nomic-embed-text
- **Open WebUI** — interface visual para Ollama (porta 8080)

### Alterado
- Pipeline CI/CD expandido para 6 jobs: Unidade → Integração → Mensageria → SonarCloud → Deploy → Smoke Tests
- Deploy injeta `BUILD_VERSION`, `BUILD_COMMIT` e `BUILD_DATE` nas imagens Docker

---

## [1.0.11] — 2026-08-30

### Adicionado
- **RabbitMQ** — publicação do evento `PropostaContratadaEvent` após contratação
- **NullEventPublisher** — Null Object Pattern, garante que a API não falha quando RabbitMQ está indisponível
- **Feature flag `UsarRabbitMQ`** — mensageria opcional via configuração
- **Pipeline CI/CD** — GitHub Actions com testes, build e deploy automático via SSH
- **Deploy em VPS** — Hostinger Ubuntu 24.04, Docker Compose, IP público
- **Multi-stage Dockerfile** — imagem final ~200MB (sdk + aspnet separados)
- **Scripts de operação** — `start.sh`, `stop.sh`, `update.sh`, `set-banco.sh`, `set-rabbitmq.sh`
- **Postman Collection** — 7 pastas cobrindo todos os cenários (InMemory, PostgreSQL, RabbitMQ)

### Alterado
- `ContratarPropostaUseCase` — publica evento após persistência bem-sucedida

---

## [1.0.1] — 2026-08-29

### Adicionado
- **PropostaService** — CRUD de propostas com Arquitetura Hexagonal (Ports & Adapters)
- **ContratacaoService** — contratação de propostas aprovadas com consulta HTTP ao PropostaService
- **Strategy Pattern** — 5 tipos de seguro com regras independentes (`IRegraSeguro`)
- **Result Pattern** — `Result<T>` sem exceções de controle de fluxo
- **Feature flag `UsarBancoDados`** — InMemory (dev) ou PostgreSQL via Dapper (prod)
- **Migrations SQL** — V001 a V005, schemas separados por serviço
- **FluentValidation** — validação de CPF e regras de negócio
- **Health Checks** — `/health`, `/health/live`, `/health/ready` em ambos os serviços
- **Swagger/OpenAPI** — documentação interativa das APIs
- **13 testes unitários** — cobertura dos Use Cases sem dependência de infraestrutura

---

[1.0.44]: https://github.com/josehelioaraujo/proposta-seguros/compare/v1.0.36...HEAD
[1.0.36]: https://github.com/josehelioaraujo/proposta-seguros/compare/v1.0.11...v1.0.36
[1.0.11]: https://github.com/josehelioaraujo/proposta-seguros/compare/v1.0.1...v1.0.11
[1.0.1]: https://github.com/josehelioaraujo/proposta-seguros/releases/tag/v1.0.1