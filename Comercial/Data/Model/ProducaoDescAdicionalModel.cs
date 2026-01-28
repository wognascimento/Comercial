using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("producao.tabela_desc_adicional")]
public class ProducaoDescAdicionalModel
{
    [Key]
    [Column]
    public long coduniadicional { get; set; }
    [Column]
    public long codigoproduto { get; set; }
    [Column]
    public string descricao_adicional { get; set; }
    [Column]
    public string cadastradopor { get; set; }
    [Column]
    public DateTime cadastradoem { get; set; }
    [Column]
    public string alteradopor { get; set; }
    [Column]
    public DateTime alteradoem { get; set; }
    [Column]
    public string revisao { get; set; }
    [Column]
    public string obsproducaoobrigatoria { get; set; }
    [Column]
    public string obsmontagem { get; set; }
    [Column]
    public string unidade { get; set; }
    [Column]
    public string inativo { get; set; }
}
