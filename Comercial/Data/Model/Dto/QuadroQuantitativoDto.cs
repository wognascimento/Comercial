using System.Collections.ObjectModel;

namespace Comercial.Data.Model.Dto;

public class QuadroQuantitativoDto
{
    public long codquadro_quantitativo { get; set; }
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
    public double custo_total { get; set; }
    public double custo_item { get; set; }
    public double vlr_indice { get; set; }
    public string ledml { get; set; }
    public double vlr_led { get; set; }
    public double desconto { get; set; }
    public double custo_tot_item { get; set; }
    public double total_desc { get; set; }
    public long codbrief { get; set; }
    public string tema { get; set; }
    public long produtocliente_cod { get; set; }
    public double produtocliente_qtd { get; set; }
    public long coddesccoml { get; set; }
    public long coddimensao { get; set; }
    public string dimensaofantasia { get; set; }
    public string bloco { get; set; }
    public string obsobrigatoria { get; set; }
    public string ilustracao { get; set; }
    public long fecha_atualiza_desc { get; set; }
    public long fecha_atualiza_dimensao { get; set; }
    public long fecha_atualiza_local { get; set; }
    public long idtema { get; set; }
    public double cubagem { get; set; }
    public double m3_total { get; set; }
    public double projecao_area { get; set; }
    public double valor_desconto_area_projecao { get; set; }
    public double custo_historico { get; set; }
    public double preco_nf { get; set; }
    public double custo_historico_total { get; set; }
    public double preco_nf_total { get; set; }
    public double preco_excel { get; set; }
    public double preco_excel_total { get; set; }
    public ObservableCollection<PropostaIlustracaoModel> Ilustracoes { get; set; }
}
