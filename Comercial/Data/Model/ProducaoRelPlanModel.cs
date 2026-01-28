using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("producao.relplan")]
public class ProducaoRelPlanModel
{
    [Key]
    [Column]
    public long id { get; set; }
    [Column]
    public string planilha { get; set; }
    [Column]
    public string ativo { get; set; }
    [Column]
    public string baia { get; set; }
    [Column]
    public string local_de_armazenamento { get; set; }
    [Column]
    public string seguranca { get; set; }
    [Column]
    public string process { get; set; }
    [Column]
    public string funcionalidade { get; set; }
    [Column]
    public string familia_produto { get; set; }
    [Column]
    public string diretor_estoque { get; set; }
    [Column]
    public string coordenador { get; set; }
    [Column]
    public string suporte_outra_unidade { get; set; }
    [Column]
    public string encarregado { get; set; }
    [Column]
    public string lider_setor { get; set; }
    [Column]
    public string producao { get; set; }
    [Column]
    public string estoque { get; set; }
    [Column]
    public string retorno { get; set; }
    [Column]
    public string est { get; set; }
    [Column]
    public string origem { get; set; }
    [Column]
    public string ficha_tecnica { get; set; }
    [Column]
    public string backup { get; set; }
    [Column]
    public string lead_time { get; set; }
    [Column]
    public string cce_sob_encomenda { get; set; }
    [Column]
    public string complexidade { get; set; }
    [Column]
    public string tipo_saldo { get; set; }
    [Column]
    public string tipo_custo { get; set; }
    [Column]
    public string GI { get; set; }
    [Column]
    public string resp_compras { get; set; }
    [Column]
    public string agrupamento { get; set; }
}
