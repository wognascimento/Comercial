using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("producao.qry3descricoes")]
public class ProducaoDescricaoModel
{
    [Key]
    [Column]
    public long codcompladicional { get; set; }
    [Column]
    public string planilha { get; set; }
    [Column]
    public string descricao { get; set; }
    [Column]
    public string descricao_adicional { get; set; }
    [Column]
    public string complementoadicional { get; set; }
    [Column]
    public string unidade { get; set; }
    [Column]
    public string inativo { get; set; }
    [Column]
    public string prodcontrolado { get; set; }
    [Column]
    public int vida_util { get; set; }
    [Column]
    public string diverso { get; set; }
    [Column]
    public double custo { get; set; }
    [Column]
    public long coduniadicional { get; set; }
    [Column]
    public string descricaofiscal { get; set; }
    [Column]
    public string descricaoespanhol { get; set; }
    [Column]
    public string familia { get; set; }
    [Column]
    public string descricao_completa { get; set; }
    [Column]
    public long codigo { get; set; }
    [Column]
    public double saldo_estoque { get; set; }
    [Column]
    public string classe_compra { get; set; }
    [Column]
    public double estoque_min { get; set; }
    [Column]
    public double m3 { get; set; }
    [Column]
    public double pl { get; set; }
    [Column]
    public double pb { get; set; }
    [Column]
    public double estoque_inicial { get; set; }
    [Column]
    public string ncm { get; set; }
    [Column]
    public double peso { get; set; }
    [Column]
    public string produto_novo { get; set; }
}
