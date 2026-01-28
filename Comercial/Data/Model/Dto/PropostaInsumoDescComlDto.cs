namespace Comercial.Data.Model.Dto;

public class PropostaInsumoDescComlDto
{
    public long codinsumo { get; set; }
    public long coddimensao { get; set; }
    public long codcompladicional { get; set; }
    public string consulta_estoque { get; set; }
    public string planilha { get; set; }
    public string descricao_completa { get; set; }
    public string unidade { get; set; }
    public double qtd { get; set; }
    public double vlr_unitario { get; set; }
    public double vlr_total { get; set; }
    public double indiceproposta { get; set; }
    public string led { get; set; }
    public string id { get; set; }
}
