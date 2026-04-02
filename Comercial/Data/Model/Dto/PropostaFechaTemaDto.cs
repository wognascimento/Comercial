namespace Comercial.Data.Model.Dto;

public class PropostaFechaTemaDto
{
    public long cod_brief { get; set; }
    public long idtema { get; set; }
    public string ordem_escolha { get; set; }
    public string tema { get; set; }
    public string faixapreco { get; set; }
    public string? resp_conclusao_preco { get; set; }
    public DateTime? data_tema_fecha { get; set; }
    public int indice { get; set; }
    public string resp_tema { get; set; }
    public string? texto { get; set; }
}
