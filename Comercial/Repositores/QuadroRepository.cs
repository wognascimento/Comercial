using Comercial.Data;
using Comercial.Data.Model;
using Comercial.Data.Model.Dto;
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

public class TemaDto
{
    public int codproposta { get; set; }
    public int idtema { get; set; }
    public int idtema_ordem { get; set; }
    public string sigla { get; set; }
    public string tema_escolhido { get; set; }                
}

public class TipoDto
{
    public string Tipo { get; set; }
}

public class QuadroPrecoDetalheDto
{
    public string sigla { get; set; }
    public string tema { get; set; }
    public string tipo { get; set; }
    public string bloco { get; set; }
    public long coddimensao { get; set; }
    public string familia { get; set; }
    public string item { get; set; }
    public string localitem { get; set; }
    public string descricao { get; set; }
    public double qtd { get; set; }
    public double inicio { get; set; }
    public double propostas { get; set; }
    public double fecha { get; set; }
    public double qtdanterior { get; set; }
    public string dimensao { get; set; }
    public string obs { get; set; }
    public string obsinterna { get; set; }
    public double custounitarioapurado { get; set; }
    public double custounitarioestimado { get; set; }
    public double custo_material_unitario { get; set; }
    public double preco_excel { get; set; }
    public double desconto_area_projecao { get; set; }
    public long majoracao { get; set; }
    public double vlr_indice { get; set; }
    public double m3_unitario { get; set; }
    public double cubagem_estimada { get; set; }
    public double n_pess_homologada { get; set; }
    public string novo { get; set; }
    public double preco_nf { get; set; }
    public double preco_nf_total { get; set; }
    public double custo_historico_total { get; set; }
    public double peso { get; set; }
    public double valor_unitario { get; set; }
    public double custo_tot_item { get; set; }
    public double desconto { get; set; }
    public double total { get; set; }
    public double vlrunidsugerido { get; set; }
    public double custo_item { get; set; }
    public string ledml { get; set; }
    public double vlr_led { get; set; }
    public double total_desc { get; set; }
    public string atualizar_quadros { get; set; }
    public string anotacoes { get; set; }
    public double custo_total { get; set; }
    public double custo_historico { get; set; }
    public double projecao_area { get; set; }
    public double valor_desconto_area_projecao { get; set; }
    public string old { get; set; }
    public long codbrief { get; set; }
    public int qtd_bloco { get; set; }
    public double dsl17 { get; set; }
    public double dsl18 { get; set; }
    public double horas_estimadas { get; set; }
    public long ordem { get; set; }
    public string i { get; set; }
    public long idtema { get; set; }
    public long idtema_ordem { get; set; }
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

    public async Task<IEnumerable<TemaDto>> GetTemasDetalheAsync(long codProposta)
    {
        using var conn = DbConnectionFactory.Create();

        string sql = @"
            SELECT tema_escolhido,
                   idtema,
                   codproposta,
                   idtema_ordem,
                   sigla
            FROM comercial.view_proposta_concluidas
            WHERE codproposta = @cod
            ORDER BY ordem_escolha, idtema_ordem";

        return await conn.QueryAsync<TemaDto>(sql, new { cod = codProposta });
    }

    public async Task<IEnumerable<TipoDto>> GetTiposAsync(int cod, int idTemaOrdem)
    {
        using var conn = DbConnectionFactory.Create();

        string sql = @"
            SELECT tipo
            FROM comercial.view_tipo_excel
            WHERE codbrief = @cod
            AND idtema_ordem = @ordem";

        return await conn.QueryAsync<TipoDto>(sql, new { cod, ordem = idTemaOrdem });
    }

    public async Task<IEnumerable<QuadroPrecoDetalheDto>> GetQuadroAsync(int codbrief)
    {
        using var conn = DbConnectionFactory.Create();

        string sql = @"
            SELECT *
            FROM comercial.view_quadro_preco_excel
            WHERE codbrief = @codbrief
            ORDER BY tema, ordem, item";

        return await conn.QueryAsync<QuadroPrecoDetalheDto>(sql, new { codbrief });
    }

    public async Task<IEnumerable<QuadroPrecoDetalheDto>> GetQuadroAsync(int cod, int idTemaOrdem, string tipo)
    {
        using var conn = DbConnectionFactory.Create();

        string sql = @"
            SELECT *
            FROM comercial.view_quadro_preco_excel
            WHERE tipo = @tipo
              AND codbrief = @cod
              AND idtema_ordem = @ordem
            ORDER BY ordem, item";

        return await conn.QueryAsync<QuadroPrecoDetalheDto>(sql, new { cod, ordem = idTemaOrdem, tipo });
    }

    public async Task<IEnumerable<ComercialFechaModel>> GetFechaAsync(string sigla, int anoAnterior)
    {
        using var conn = DbConnectionFactory.Create();
        string sql = @"SELECT * FROM comercial.tblfecha WHERE sigla = @sigla AND ano = @anoAnterior";
        return await conn.QueryAsync<ComercialFechaModel>(sql, new { sigla, anoAnterior });
    }

    public async Task<IEnumerable<PropostaBriefingTemaDto>> GetBriefingTemaAsync(int codBrief, int idTema)
    {
        using var conn = DbConnectionFactory.Create();
        string sql = @"SELECT * FROM comercial.proposta_temas_briefing WHERE codbriefing = @codBrief AND idtema = @idTema ORDER BY ordem_escolha";
        return await conn.QueryAsync<PropostaBriefingTemaDto>(sql, new { codBrief, idTema });     
    }

    public async Task<IEnumerable<PropostaPrecoBlocoDto>> GetPrecoBlocosAsync(long codBrief, long idTema)
    {
        using var conn = DbConnectionFactory.Create();
        string sql = @"SELECT * FROM comercial.proposta_preco_bloco WHERE codbrief = @codBrief AND idtema = @idTema AND tipo = 'Proposta' ORDER BY tema, item, tipo";
        return await conn.QueryAsync<PropostaPrecoBlocoDto>(sql, new { codBrief, idTema });     
    }

    public async Task<IEnumerable<PropostaPrecoTipoDto>> GetPrecoTiposAsync(long codBrief, long idTema)
    {
        using var conn = DbConnectionFactory.Create();
        string sql = @"SELECT * FROM comercial.proposta_preco_tipo WHERE codbrief = @codBrief AND idtema = @idTema ORDER BY tema, ordem, tipo";
        return await conn.QueryAsync<PropostaPrecoTipoDto>(sql, new { codBrief, idTema });     
    }
}

