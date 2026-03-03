using Comercial.Data;
using Dapper;

namespace Comercial.Repositores;

public class QuadroPrecoRepository
{
    /*
    public async Task<InfoPrecoModel?> GetInfoPreco(string sigla)
    {
        var sql = @"SELECT * 
                    FROM info_preco_2015 
                    WHERE sigla = @sigla";

        using var connection = DbConnectionFactory.Create();
        return await connection.QueryFirstOrDefaultAsync<InfoPrecoModel>(sql, new { sigla });
    }

    public async Task<IEnumerable<MomadesAnoAtualModel>> GetAnoAtual(string sigla)
    {
        var sql = @"SELECT * 
                    FROM qry_momades_quadro_preco_ano_atual
                    WHERE sigla = @sigla";

        using var connection = DbConnectionFactory.Create();
        return await connection.QueryAsync<MomadesAnoAtualModel>(sql, new { sigla });
    }

    public async Task<IEnumerable<PrecoDetalheModel>> GetDetalhes(int codBrief, int idTema, string tipo)
    {
        var sql = @"SELECT * 
                    FROM qryPrecoDetalhes
                    WHERE codbrief = @codBrief
                    AND idtema = @idTema
                    AND tipo = @tipo";

        using var connection = DbConnectionFactory.Create();
        return await connection.QueryAsync<PrecoDetalheModel>(sql,
            new { codBrief, idTema, tipo });
    }
    */
}
