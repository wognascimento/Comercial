using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Comercial.Data.Model;

[Table("comercial.proposta_dimensaodescricaocomercial")]
public class ComercialPropostaDimensaoDescricaoComercialModel
{
    // chave primária (coddimensao)
    [Key] // Dapper.Contrib
    [Column]
    public long coddimensao { get; set; }

    // foreign key (não nula)
    [Required]
    [Column]
    public long coddesccoml { get; set; }

    // obrigatório
    [Required]
    [Column]
    public string dimensao { get; set; }

    // strings opcionais
    [Column]
    public string? dimensaofantasia { get; set; }
    [Column]
    public string? nomefantasia { get; set; }
    [Column]
    public string? observacao { get; set; }
    [Column]
    public string? obsobrigatoria { get; set; }

    // char/varchar curta (representado como string)
    [Column]
    public string? travaled { get; set; } // coluna travaled

    // numericos
    [Column]
    public float? indicedimensao { get; set; } // indicedimensao (real)
    [Column]
    public float? indiceled { get; set; } // indiceled (real)
    [Column]
    public double? indicedesconto { get; set; } // indicedesconto (double precision)
    [Column]
    public double? indicedescontopreco { get; set; } // indicedescontopreco
    [Column]
    public float? cargaeletrica { get; set; } // cargaeletrica (real)
    [Column]
    public float? areareal { get; set; } // areareal
    [Column]
    public double? cubagem { get; set; } // cubagem (double precision)
    [Column]
    public float? peso { get; set; } // peso (real)
    [Column]
    public float? custounitarioapurado { get; set; } // custounitarioapurado (real)
    [Column]
    public double? custounitarioestimado { get; set; } // custounitarioestimado (double precision)
    [Column]
    public float? preco { get; set; } // preco (real)

    // auditoria / controle
    [Column]
    public string? cadastradopor { get; set; } // cadastradopor (NOT NULL)

    //[Computed] // gerado pelo DB (DEFAULT now()), Dapper.Contrib não tenta setar em INSERT/UPDATE
    [Column]
    public DateTime datacadastro { get; set; } // datacadastro timestamptz NOT NULL

    // mais campos opcionais
    [Column]
    public string? insumo_concluido { get; set; } // insumo_concluido
    [Column]
    public string? insumo_concluido_por { get; set; } // insumo_concluido_por
    [Column]
    public DateTime? insumo_concluido_data { get; set; } // insumo_concluido_data

    [Column]
    public string? concatenar { get; set; } // concatenar
    [Column]
    public string? ativo { get; set; } // ativo (char(1))
    [Column]
    public string? relatorio_estabilidade { get; set; } // relatorio_estabilidade (text)
    [Column]
    public string? verificador { get; set; } // verificador

    [Column]
    public float? cargaeletrica_led { get; set; } // cargaeletrica_led
    [Column]
    public string? aux { get; set; } // aux

    // projeções e custos
    [Column]
    public double? projecao_area { get; set; } // projecao_area
    [Column]
    public double? valor_desconto_area_projecao { get; set; } // valor_desconto_area_projecao
    [Column]
    public double? custo_historico { get; set; } // custo_historico
    [Column]
    public double? preco_nf { get; set; } // preco_nf
    [Column]
    public double? cubagem_estimada { get; set; } // cubagem_estimada
    [Column]
    public double? horas_estimadas { get; set; } // horas_estimadas

    // metragem / medidas
    [Column]
    public double? a_metragem { get; set; } // a_metragem
    [Column]
    public double? v_metragem { get; set; } // v_metragem
    [Column]
    public double? h_metragem { get; set; } // h_metragem
    [Column]
    public double? h_tot_metragem { get; set; } // h_tot_metragem
    [Column]
    public double? c_metragem { get; set; } // c_metragem
    [Column]
    public double? l_metragem { get; set; } // l_metragem
    [Column]
    public double? d_metragem { get; set; } // d_metragem

    // montagem / tempo
    [Column]
    public double? pessoas_montagem { get; set; } // pessoas_montagem
    [Column]
    public double? noites_montagem { get; set; } // noites_montagem

    // demais
    [Column]
    public double? intervalo { get; set; } // intervalo
    [Column]
    public double? fator { get; set; } // fator
    [Column]
    public float? custo_unitario_apurado_anterior { get; set; } // custo_unitario_apurado_anterior
    [Column]
    public float? custo_estimado_anterior { get; set; } // custo_estimado_anterior

    [Column]
    public string? sub_classificacao { get; set; } // sub_classificacao
    [Column]
    public string? categoria { get; set; } // categoria (text)
    [Column]
    public string? cat_online { get; set; } // cat_online (text)
    [Column]
    public string? cat_offline { get; set; } // cat_offline (text)
    [Column]
    public string? foto { get; set; } // foto (text)
    [Column]
    public string? layout { get; set; } // layout (text)
    [Column]
    public double? cat_preco { get; set; } // cat_preco (double precision)
    [Column]
    public string? descricao_licitacao { get; set; } // descricao_licitacao (text)
}
