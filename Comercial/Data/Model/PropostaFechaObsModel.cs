namespace Comercial.Data.Model;

public class PropostaFechaObsModel
{
    public long id { get; set; }
    public string obs { get; set; }
    public string inserido_por { get; set; }
    public DateTime inserido_em { get; set; }
    public string tema { get; set; }
    public long briefing { get; set; }
    public long idtema { get; set; }
}
