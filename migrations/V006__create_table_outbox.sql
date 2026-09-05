CREATE TABLE IF NOT EXISTS contratacao.outbox (
    id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    tipo          VARCHAR(100) NOT NULL,
    payload       JSONB        NOT NULL,
    criado_em     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    processado    BOOLEAN      NOT NULL DEFAULT FALSE,
    processado_em TIMESTAMPTZ  NULL
);

CREATE INDEX IF NOT EXISTS idx_outbox_processado
    ON contratacao.outbox (processado, criado_em)
    WHERE processado = FALSE;
