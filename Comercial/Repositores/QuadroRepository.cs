using Comercial.Data;
using Dapper;
using System.Collections.ObjectModel;

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

public class DetalhePreco
{
    public int codbrief { get; set; }
    public string tema { get; set; }
    public int ordem { get; set; }
    public string tipo { get; set; }
    public string primeirodeitem { get; set; }
    public string bloco { get; set; }
    public double somadetotal { get; set; }
    public long idtema { get; set; }
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

    public async Task<IEnumerable<ItemTabelaModel>> GetItensTabelaAsync(long codBrief, long idTema, string tipo)
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

    public async Task<IEnumerable<TemaModel>> GetTemasPropostaAsync(long codBrief)
    {
        using var conn = DbConnectionFactory.Create();

        string sql = @"
            SELECT idtema AS IdTema,
                   tema_escolhido AS TemaEscolhido,
                   ordem_escolha AS OrdemEscolha
            FROM comercial.propostas
            WHERE codproposta = @codBrief AND cancelado = '0'
            ORDER BY ordem_escolha";

        return await conn.QueryAsync<TemaModel>(sql, new { codBrief });
    }

    public async Task<ObservableCollection<DetalhePreco>> GetDetalhesPrecoAsync(long codBrief, long idtema, string tipo)
    {
        using var conn = DbConnectionFactory.Create();

        string sql = @"
            SELECT
                codbrief,
                tema,
                ordem,
                tipo,
                FIRST (item) as primeirodeitem,
                CONCAT_WS('', bloco, agrupamento) as bloco,
                SUM(preco_excel_total) as somadetotal,
                idtema
            FROM
                comercial.view_quadro_preco
            WHERE codbrief = @codBrief AND idtema= @idtema AND tipo = @tipo
            GROUP BY
                codbrief,
                tema,
                ordem,
                tipo,
                CONCAT_WS('', bloco, agrupamento),
                idtema
            ORDER BY
                codbrief,
                tema,
                ordem,
                FIRST (item),
                CONCAT_WS('', bloco, agrupamento);";

        return new ObservableCollection<DetalhePreco>( await conn.QueryAsync<DetalhePreco>(sql, new { codBrief, idtema, tipo }) );
    }
}
