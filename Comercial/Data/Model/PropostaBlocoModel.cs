using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("comercial.proposta_descricaocomercial")]
public class PropostaBlocoModel
{
    [Key]
    public int ordem { get; set; }
    public string bloco { get; set; }
    public int bloco_revisao { get; set; }
}
