namespace Comercial.Data.Model.Dto;

public class PropostaFechaViewDto
{
    public long ordem { get; set; }
    public string tipo { get; set; }
    public string familia { get; set; }
    public string item { get; set; }
    public string local { get; set; }
    public string localitem { get; set; }
    public string descricao { get; set; }
    public double qtd { get; set; }
    public string dimensao { get; set; }
    public string obs { get; set; }
    public string ledml { get; set; }
    public long codquadro_preco { get; set; }
    public string sigla { get; set; }
    public string tema { get; set; }
    public string detalhe_local { get; set; }
    public double indicedimensao { get; set; }
    public double indiceled { get; set; }
    public double indiceproposta { get; set; }
    public long coddimensao { get; set; }
    public string bloco { get; set; }
    public string obs_interna { get; set; }
    public long cod_brief { get; set; }
    public long cod_linha_qdfecha { get; set; }
    public string? cadastrado_por { get; set; }
    public DateTime? data_cadastro { get; set; }
    public string? alterado_por { get; set; }
    public DateTime? data_altera { get; set; }
    public string obs_alteracao { get; set; }
    public long bloco_revisao { get; set; }
    public string altera_ok { get; set; }
    public string? confirma_alteracao_por { get; set; }
    public DateTime? confirma_alteracao_data { get; set; }
    public string sigla_serv { get; set; }
    public long coddesccoml { get; set; }
    public string obs_memorial { get; set; }
    public string link { get; set; }
    public string ordem_escolha { get; set; }
    public long idtema { get; set; }
    public long id_aprovado { get; set; }
    public double pessoas_montagem { get; set; }
    public double noites_montagem { get; set; }
    public string est { get; set; }
    public double intervalo { get; set; }
    public double fator { get; set; }
    public double peso { get; set; }
    public double cargaeletrica_led { get; set; }
    public double cargaeletrica { get; set; }
    public string baia_caminhao { get; set; }

}
