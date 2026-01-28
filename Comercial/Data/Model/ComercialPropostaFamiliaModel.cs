using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("comercial.proposta_familia")]
public class ComercialPropostaFamiliaModel
{
    [Key]
    [Column("id")]
    public int id { get; set; }
    [Column("familia")]
    [Required]
    public string familia { get; set; }
    [Column("resp_descricao")]
    public string resp_descricao { get; set; }
    [Column("resp_dimensao")]
    public string resp_dimensao { get; set; }
    [Column("resp_altera_descricao")]
    public string resp_altera_descricao { get; set; }
    [Column("resp_altera_dimensao")]
    public string resp_altera_dimensao { get; set; }
}
