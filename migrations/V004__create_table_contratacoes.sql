CREATE TABLE IF NOT EXISTS contratacao.contratacoes (
    id               UUID         PRIMARY KEY,
    proposta_id      UUID         NOT NULL,
    cpf              VARCHAR(20)  NOT NULL,
    data_contratacao TIMESTAMP    NOT NULL DEFAULT NOW(),
    criado_em        TIMESTAMP    NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_contratacoes_proposta_id
    ON contratacao.contratacoes (proposta_id);
