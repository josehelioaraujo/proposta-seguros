-- ============================================================
-- V005__add_criado_em_contratacoes.sql
-- Adiciona coluna criado_em na tabela contratacoes
-- ============================================================
ALTER TABLE contratacao.contratacoes
    ADD COLUMN IF NOT EXISTS criado_em TIMESTAMP NOT NULL DEFAULT NOW();
