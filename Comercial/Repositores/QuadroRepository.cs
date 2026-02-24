using Comercial.Data;
using Dapper;

namespace Comercial.Repositores;

public class TemaModel
{
    public long IdTema { get; set; }
    public string TemaEscolhido { get; set; }
    public int OrdemEscolha { get; set; }
}

public class ItemTabelaModel
{
    public string Bloco { get; set; }
    public string Item { get; set; }
    public string LocalItem { get; set; }
    public string Descricao { get; set; }
    public string Qtd { get; set; }
    public string Dimensao { get; set; }
    public string Obs { get; set; }
}

public class QuadroRepository
{
    public async Task<IEnumerable<TemaModel>> GetTemasAsync(long codBrief)
    {
        using var conn = DbConnectionFactory.Create();

        string sql = @"
            SELECT idtema AS IdTema,
                   tema_escolhido AS TemaEscolhido,
                   ordem_escolha AS OrdemEscolha
            FROM comercial.qryword_temaquadrocriado
            WHERE codbrief = @codBrief
            ORDER BY ordem_escolha";

        return await conn.QueryAsync<TemaModel>(sql, new { codBrief });
    }

    public async Task<IEnumerable<ItemTabelaModel>> GetItensTabelaAsync(
        long codBrief, long idTema, string tipo)
    {
        using var conn = DbConnectionFactory.Create();

        string sql = @"
            SELECT bloco,
                   item,
                   localitem,
                   descricao,
                   qtd,
                   dimensao,
                   obs
            FROM comercial.qryword_concatenardescricao
            WHERE codbrief = @codBrief
              AND idtema = @idTema
              AND tipo = @tipo
            ORDER BY item";

        return await conn.QueryAsync<ItemTabelaModel>(sql,
            new { codBrief, idTema, tipo });
    }
}
