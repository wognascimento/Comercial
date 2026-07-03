CREATE TABLE IF NOT EXISTS comercial.produto_woocommerce
(
    woocommerce_id integer NOT NULL,
    sku character varying(50) COLLATE pg_catalog."default" NOT NULL,
    data_sync timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT produto_woocommerce_pkey PRIMARY KEY (woocommerce_id)
);
