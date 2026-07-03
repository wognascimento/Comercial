namespace Comercial.Data.Model.Dto;

public class PropostaDescricaoDimensaoConsultaDto
{
    public int coddimensao { get; set; }
    public string? familia { get; set; }
    public string? descricaocomercial { get; set; }
    public string? dimensao { get; set; }
    public string? descricao_licitacao { get; set; }
    public float? preco { get; set; }
    public bool produto_woocommerce { get; set; }
}
