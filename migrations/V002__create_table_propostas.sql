CREATE TABLE IF NOT EXISTS proposta.propostas (
    id              UUID          PRIMARY KEY,
    nome_cliente    VARCHAR(200)  NOT NULL,
    cpf             VARCHAR(20)   NOT NULL,
    tipo_seguro     INT           NOT NULL,
    valor           DECIMAL(10,2) NOT NULL,
    status          INT           NOT NULL DEFAULT 1,
    criado_em       TIMESTAMP     NOT NULL DEFAULT NOW(),
    atualizado_em   TIMESTAMP     NULL
);

CREATE INDEX IF NOT EXISTS idx_propostas_cpf_tipo
    ON proposta.propostas (cpf, tipo_seguro);
