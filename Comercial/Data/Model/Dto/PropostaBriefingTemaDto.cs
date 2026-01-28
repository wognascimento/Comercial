namespace Comercial.Data.Model.Dto;

public class PropostaBriefingTemaDto
{
    public long codbriefing { get; set; }
    public string sigla { get; set; }
    public string temas { get; set; }
    public long idtema { get; set; }
    public string ordem_escolha { get; set; }
    public string? faixapreco { get; set; }
    public decimal? indiceproposta { get; set; }
    public string? resp_tema { get; set; }
    public DateTime? data_conclusao { get; set; }
    public string? tot_cenografia { get; set; }
    public DateTime? data_inicio_preco { get; set; }
    public DateTime? data_conclusao_preco { get; set; }
    public bool ativo { get; set; }
}
