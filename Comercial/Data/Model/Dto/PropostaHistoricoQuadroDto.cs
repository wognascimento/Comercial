namespace Comercial.Data.Model.Dto;

public class PropostaHistoricoQuadroDto
{
    public long ano { get; set; }
    public long ordem { get; set; }
    public string sigla { get; set; }
    public string tipo { get; set; }
    public string familia { get; set; }
    public string item { get; set; }
    public string localitem { get; set; }
    public string local { get; set; }
    public string localdetalhe { get; set; }
    public string descricao { get; set; }
    public string descricaocomercial { get; set; }
    public string nomefantasia { get; set; }
    public double qtd { get; set; }
    public double qtdanterior { get; set; }
    public string dimensao { get; set; }
    public string obs { get; set; }
    public string obsinterna { get; set; }
    public double custounitarioapurado { get; set; }
    public double custounitarioestimado { get; set; }
    public double custo_item { get; set; }
    public double vlr_indice { get; set; }
    public string ledml { get; set; }
    public double vlr_led { get; set; }
    public double desconto { get; set; }
    public double custo_tot_item { get; set; }
    public double total_desc { get; set; }
    public long codquadro_quantitativo { get; set; }
    public long codbrief { get; set; }
    public string tema { get; set; }
    public long produtocliente_cod { get; set; }
    public double produtocliente_qtd { get; set; }
    public long coddesccoml { get; set; }
    public long coddimensao { get; set; }
    public string dimensaofantasia { get; set; }
    public string bloco { get; set; }
    public double valor_unitario { get; set; }
    public double correcao_20 { get; set; }
    public double total { get; set; }
    public long codquadro_preco { get; set; }
    public long idtema { get; set; }
}
