using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data.Model;

[Table("producao.produtos")]
public class ProducaoProdutoModel
{
    [Key]
    [Column]
    public long codigo { get; set; }
    [Column]
    public string planilha { get; set; }
    [Column]
    public string descricao { get; set; }
    [Column]
    public string cadastrado_por { get; set; }
    [Column]
    public DateTime datacadastro { get; set; }
    [Column]
    public string familia { get; set; }
    [Column]
    public string classe_solict_compra { get; set; }
    [Column]
    public string alterado_por { get; set; }
    [Column]
    public DateTime data_altera { get; set; }
    [Column]
    public string inativo { get; set; }
}
