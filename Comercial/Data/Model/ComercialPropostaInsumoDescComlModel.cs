using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("comercial.proposta_insumodesccoml")]
public class ComercialPropostaInsumoDescComlModel
{

    [Key]
    [Column]
    public long codinsumo { get; set; }
    [Column]
    public long coddimensao { get; set; }
    [Column]
    public long codcompladicional { get; set; }
    [Column]
    public double qtd { get; set; }
    [Column]
    public string origem { get; set; }
    [Column]
    public string homologacao { get; set; }
    [Column]
    public string id { get; set; }
    [Column]
    public string consulta_estoque { get; set; }

}
