using System.ComponentModel.DataAnnotations;

namespace Comercial.Data.Model;

public class PropostaIlustracaoModel
{
    [Required]
    public string? sigla { get; set; }
    [Required]
    public string? tema { get; set; }
    public DateTime? data_pedido { get; set; }
    [Required]
    public string? tipo { get; set; }
    [Required]
    public string? qtd { get; set; }
    public string? resp { get; set; }
    public DateTime? data_conclusao { get; set; }
    public string? inserido_por { get; set; }
    public string? obs { get; set; }
    public long? codilustracao { get; set; }
    public long? codquadro_quantitativo { get; set; }
    public string? controle_pedidos { get; set; }
    public string? link { get; set; }
    public string? proposta { get; set; }
    [Required]
    public string? item { get; set; }
    [Required]
    public long? codbriefing { get; set; }
    public string? tipo_quadro { get; set; }
    public long? codpreco { get; set; }
    public string? cancelado { get; set; }
    public string? cancelado_por { get; set; }
    public DateTime? cancelado_data { get; set; }
    public string? cancelado_obs { get; set; }
    public DateTime? data_inicio { get; set; }
    public string? alterado_por { get; set; }
    public DateTime? alterado_em { get; set; }
    public string? resp_layout { get; set; }
    public DateTime? data_inicio_layout { get; set; }
    public DateTime? data_fim_layout { get; set; }
    public string? obs_layout { get; set; }
    public string? resp_planta_layout { get; set; }
    public DateTime? data_inicio_planta_layout { get; set; }
    public DateTime? data_fim_planta_layout { get; set; }
    public string? obs_planta_layout { get; set; }
    [Required]
    public long? idtema { get; set; }
}
