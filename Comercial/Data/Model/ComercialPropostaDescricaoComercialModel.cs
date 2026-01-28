using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("comercial.proposta_descricaocomercial")]
public class ComercialPropostaDescricaoComercialModel
{
    [Key]
    [Column("coddesccoml")]
    public long coddesccoml { get; set; }
    [Column("familia")]
    [Required]
    public string familia { get; set; }
    [Column("descricaocomercial")]
    [Required]
    public string descricaocomercial { get; set; }
    [Column("alex")]
    public string alex { get; set; }
    [Column("ativo")]
    public string ativo { get; set; }
    [Column("planilha_predominante")]
    public string planilha_predominante { get; set; }
    [Column("id_familia")]
    [Required]
    public int id_familia { get; set; }
}
