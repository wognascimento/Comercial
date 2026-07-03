CREATE TABLE IF NOT EXISTS comercial.proposta_descricaodimensao_imagem
(
    id_imagem serial NOT NULL,
    coddimensao integer NOT NULL,
    nome_arquivo character varying(255) COLLATE pg_catalog."default" NOT NULL,
    content_type character varying(100) COLLATE pg_catalog."default" NOT NULL,
    caminho_arquivo text COLLATE pg_catalog."default",
    caminho_thumbnail text COLLATE pg_catalog."default",
    capa boolean NOT NULL DEFAULT false,
    cadastrado_por character varying(50) COLLATE pg_catalog."default",
    cadastrado_em timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    alterado_por character varying(50) COLLATE pg_catalog."default",
    alterado_em timestamp without time zone,
    CONSTRAINT proposta_descricaodimensao_imagem_pkey PRIMARY KEY (id_imagem)
);

ALTER TABLE comercial.proposta_descricaodimensao_imagem
    ADD COLUMN IF NOT EXISTS caminho_arquivo text COLLATE pg_catalog."default";

ALTER TABLE comercial.proposta_descricaodimensao_imagem
    ADD COLUMN IF NOT EXISTS caminho_thumbnail text COLLATE pg_catalog."default";

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'comercial'
          AND table_name = 'proposta_descricaodimensao_imagem'
          AND column_name = 'imagem'
    ) THEN
        ALTER TABLE comercial.proposta_descricaodimensao_imagem
            ALTER COLUMN imagem DROP NOT NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_proposta_descricaodimensao_imagem_coddimensao
    ON comercial.proposta_descricaodimensao_imagem (coddimensao);

CREATE UNIQUE INDEX IF NOT EXISTS ux_proposta_descricaodimensao_imagem_capa
    ON comercial.proposta_descricaodimensao_imagem (coddimensao)
    WHERE capa = true;
