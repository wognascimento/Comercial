namespace Comercial.Data.Model;

public class PropostaQuadroFechaModel
{
    public string sigla { get; set; }
    public string tema { get; set; }
    public string tipo { get; set; }
    public long coddimensao { get; set; }
    public string local { get; set; }
    public string item { get; set; }
    public double qtd { get; set; }
    public string obs { get; set; }
    public string? alterado_por { get; set; }
    public DateTime? data_altera { get; set; }
    public string cadastrado_por { get; set; }
    public DateTime data_cadastro { get; set; }
    public string detalhe_local { get; set; }
    public string ledml { get; set; }
    public long cod_brief { get; set; }
    public string obs_interna { get; set; }
    public string bloco { get; set; }
    public long codquadro_preco { get; set; }
    public long cod_linha_qdfecha { get; set; }
    public string alterado { get; set; }
    public string obs_alteracao { get; set; }
    public long bloco_revisao { get; set; }
    public string altera_ok { get; set; }
    public string confirma_alteracao_por { get; set; }
    public DateTime confirma_alteracao_data { get; set; }
    public DateTime data_revisado { get; set; }
    public string obs_memorial { get; set; }
    public string status { get; set; }
    public string liberado { get; set; }
    public string ok { get; set; }
    public string resp_revisao { get; set; }
    public DateTime prazo_revisao { get; set; }
    public string obs_revisao { get; set; }
    public string revisado_por { get; set; }
    public DateTime data_revisado_por { get; set; }
    public long produtocliente_cod { get; set; }
    public double produtocliente_qtd { get; set; }
    public string sigla_serv { get; set; }
    public string motivo_alt_pos_revisao { get; set; }
    public string ok_revisao_alterada { get; set; }
    public string revisao_alt_por { get; set; }
    public DateTime data_alt_revisao { get; set; }
    public string link { get; set; }
    public long id_aprovado { get; set; }
    public long idtema { get; set; }
    public DateTime controle_alteracao_data { get; set; }
    public string controle_alteracao_por { get; set; }
    public string pendencia { get; set; }
    public string? item_novo { get; set; }

}
