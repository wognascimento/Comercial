using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("producao.tblcomplementoadicional")]
public class ProducaoComplementoAdicionalModel
{
    [Key]
    [Column]
    public long codcompladicional { get; set; }
    [Column]
    public string complementoadicional { get; set; }
    [Column]
    public string status { get; set; }
    [Column]
    public double estoque_inicial { get; set; }
    [Column]
    public string desc_process { get; set; }
    [Column]
    public double estoque_inicial_processado { get; set; }
    [Column]
    public double altura { get; set; }
    [Column]
    public double largura { get; set; }
    [Column]
    public double profundidade { get; set; }
    [Column]
    public int vida_util { get; set; }
    [Column]
    public double diametro { get; set; }
    [Column]
    public double peso { get; set; }
    [Column]
    public string unidade { get; set; }
    [Column]
    public string cadastradopor { get; set; }
    [Column]
    public DateTime cadastradoem { get; set; }
    [Column]
    public string alterado_por { get; set; }
    [Column]
    public DateTime alterado_em { get; set; }
    [Column]
    public int custo_real { get; set; }
    [Column]
    public string prodcontrolado { get; set; }
    [Column]
    public double volume { get; set; }
    [Column]
    public double area { get; set; }
    [Column]
    public double precolocacao { get; set; }
    [Column]
    public string descricaofiscal { get; set; }
    [Column]
    public string descricaoespanhol { get; set; }
    [Column]
    public double estoque_min { get; set; }
    [Column]
    public long v_unit { get; set; }
    [Column]
    public double v_unit_dolar { get; set; }
    [Column]
    public string ncm { get; set; }
    [Column]
    public string tipo { get; set; }
    [Column]
    public double custoestimado { get; set; }
    [Column]
    public double indicecorrecao { get; set; }
    [Column]
    public string nf { get; set; }
    [Column]
    public double pesobruto { get; set; }
    [Column]
    public long coduniadicional { get; set; }
    [Column]
    public string codfornecedor { get; set; }
    [Column]
    public string foralinhafornecedor { get; set; }
    [Column]
    public string origemcusto { get; set; }
    [Column]
    public DateTime datafichatecnica { get; set; }
    [Column]
    public string respfichatenica { get; set; }
    [Column]
    public DateTime datainiciofichatecnica { get; set; }
    [Column]
    public string respcusto { get; set; }
    [Column]
    public DateTime datacusto { get; set; }
    [Column]
    public string contabil { get; set; }
    [Column]
    public string produto_novo { get; set; }
    [Column]
    public string acompanhamento { get; set; }
    [Column]
    public string responsavel_acompanha { get; set; }
    [Column]
    public DateTime concluido_acompanha { get; set; }
    [Column]
    public string obs_acompanhamento { get; set; }
    [Column]
    public string importado { get; set; }
    [Column]
    public string contabil_pldc { get; set; }
    [Column]
    public string narrativa { get; set; }
    [Column]
    public string alx { get; set; }
    [Column]
    public string inativo { get; set; }
    [Column]
    public int qtd_etiqueta { get; set; }
    [Column]
    public string fracao { get; set; }
    [Column]
    public string dividir_qtd_volume { get; set; }
    [Column]
    public string conta_aplica_contabil { get; set; }
    [Column]
    public string centro_custo_contabil { get; set; }
    [Column]
    public string especial { get; set; }
    [Column]
    public string foto { get; set; }
    [Column]
    public double custodescadicional_custo { get; set; }
    [Column]
    public int custodescadicional_codcompladicional { get; set; }
    [Column]
    public string tamanho_construcao { get; set; }
    [Column]
    public string diverso { get; set; }
    [Column]
    public string dificuldade { get; set; }
    [Column]
    public double saldo_patrimonial_ano_anterior { get; set; }
    [Column]
    public double saldo_disponivel_ano_anterior { get; set; }
    [Column]
    public string custo_despesa { get; set; }
    [Column]
    public string link_foto { get; set; }
    [Column]
    public double preco_shopping { get; set; }
    [Column]
    public string exportado_folhamatic { get; set; }
    [Column]
    public double saldo_estoque { get; set; }
    [Column]
    public double m3 { get; set; }
    [Column]
    public double pl { get; set; }
    [Column]
    public double pb { get; set; }
}
