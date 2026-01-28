namespace Comercial.Data.Model;

public class PropostaQuadroQuantitativoModel
{
    public long? codquadro_quantitativo { get; set; }
    public long? codbrief { get; set; }
    public string? sigla { get; set; }
    public string? tema { get; set; }
    public string? tipo { get; set; }
    public string? item { get; set; }
    public string? local { get; set; }
    public string? localdetalhe { get; set; }
    public long? coddimensao { get; set; }
    public double? qtd { get; set; }
    public double? qtdanterior { get; set; }
    public string? obs { get; set; }
    public string? obsinterna { get; set; }
    public string? ledml { get; set; }
    public double? desconto { get; set; }
    public string? bloco { get; set; }
    public long? produtocliente_cod { get; set; }
    public double? produtocliente_qtd { get; set; }
    public string? tot_cenografia { get; set; }
    public string? cadastradopor { get; set; }
    public DateTime? datacadastro { get; set; }
    public string? alteradopor { get; set; }
    public DateTime? dataaltera { get; set; }
    public double? custo_item { get; set; }
    public double? vlr_indice { get; set; }
    public double? vlr_led { get; set; }
    public double? custo_tot_item { get; set; }
    public double? total_desc { get; set; }
    public string? ilustracao { get; set; }
    public long? fecha_atualiza_desc { get; set; }
    public long? fecha_atualiza_dimensao { get; set; }
    public long? fecha_atualiza_local { get; set; }
    public long? idtema { get; set; }

}
