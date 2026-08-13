using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using Dapper;
using Npgsql;
using NpgsqlTypes;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Comercial.Views.Consulta;

public partial class TodasDescricoes : UserControl
{
    private readonly TodasDescricoesViewModel viewModel = new();
    private readonly Dictionary<int, ValoresEditaveisTodasDescricoes> valoresOriginais = [];

    public TodasDescricoes()
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += TodasDescricoes_Loaded;
    }

    private async void TodasDescricoes_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= TodasDescricoes_Loaded;

        try
        {
            descricoesGrid.IsBusy = true;
            await viewModel.CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível carregar as descrições.\n\n{ex.Message}",
                "Todas as descrições",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            descricoesGrid.IsBusy = false;
        }
    }

    private async void DescricoesGrid_RowEditEnded(
        object sender,
        GridViewRowEditEndedEventArgs e)
    {
        if (e.EditAction != GridViewEditAction.Commit ||
            e.NewData is not PropostaDescricaoDimensaoConsultaDto item)
        {
            return;
        }

        try
        {
            descricoesGrid.IsBusy = true;

            if (!valoresOriginais.TryGetValue(item.coddimensao, out var original))
                original = ValoresEditaveisTodasDescricoes.From(item);

            var resultado = await viewModel.SalvarAlteracoesAsync(item, original);

            if (resultado.DescricaoLicitacaoFalhou)
                item.descricao_licitacao = original.DescricaoLicitacao;

            if (resultado.PrecoFalhou)
                item.preco = original.Preco;

            if (resultado.TemFalhas)
            {
                descricoesGrid.Items.Refresh();
                MessageBox.Show(
                    resultado.Mensagem,
                    "Todas as descrições",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível salvar a descrição.\n\n{ex.Message}",
                "Todas as descrições",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            await viewModel.RecarregarAsync(item);
            descricoesGrid.Items.Refresh();
        }
        finally
        {
            valoresOriginais.Remove(item.coddimensao);
            descricoesGrid.IsBusy = false;
        }
    }

    private void DescricoesGrid_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
    {
        if (e.Cell?.DataContext is not PropostaDescricaoDimensaoConsultaDto item)
            return;

        var columnName = e.Cell.Column?.UniqueName;
        if (columnName is not "descricao_licitacao" and not "preco")
            return;

        valoresOriginais.TryAdd(item.coddimensao, ValoresEditaveisTodasDescricoes.From(item));
    }

    private async void SincronizarWooCommerce_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            descricoesGrid.IsBusy = true;
            sincronizarWooCommerceButton.IsEnabled = false;
            statusSincronizacaoText.Text = "Buscando produtos na loja virtual...";

            var total = await viewModel.SincronizarProdutosWooCommerceAsync();
            await viewModel.CarregarAsync();

            statusSincronizacaoText.Text = $"Sincronizados: {total:N0} produtos.";
        }
        catch (Exception ex)
        {
            statusSincronizacaoText.Text = string.Empty;
            MessageBox.Show(
                $"Não foi possível sincronizar produtos da loja virtual.\n\n{ex.Message}",
                "Todas as descrições",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            sincronizarWooCommerceButton.IsEnabled = true;
            descricoesGrid.IsBusy = false;
        }
    }

    private void DescricoesGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var row = FindVisualParent<GridViewRow>(e.OriginalSource as DependencyObject);
        if (row?.Item is PropostaDescricaoDimensaoConsultaDto descricao)
            descricoesGrid.SelectedItem = descricao;
    }

    private void AdicionarImagem_Click(object sender, RoutedEventArgs e)
    {
        if (descricoesGrid.SelectedItem is not PropostaDescricaoDimensaoConsultaDto descricao)
        {
            MessageBox.Show(
                "Selecione uma descrição para adicionar imagem.",
                "Todas as descrições",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var titulo = string.IsNullOrWhiteSpace(descricao.descricaocomercial)
            ? $"Dimensão {descricao.coddimensao}"
            : descricao.descricaocomercial;

        var window = new TodasDescricoesImagensWindow(descricao.coddimensao, titulo)
        {
            Owner = Window.GetWindow(this)
        };

        window.ShowDialog();
    }

    private static T? FindVisualParent<T>(DependencyObject? element)
        where T : DependencyObject
    {
        while (element is not null)
        {
            if (element is T typed)
                return typed;

            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }
}

public class TodasDescricoesViewModel
{
    private readonly DataBaseSettings baseSettings = DataBaseSettings.Instance;

    public ObservableCollection<PropostaDescricaoDimensaoConsultaDto> Descricoes { get; } = [];

    public async Task CarregarAsync()
    {
        await InserirPrecosBaseAusentesAsync();

        const string sql = """
            SELECT
                coddimensao,
                familia,
                descricaocomercial,
                dimensao,
                descricao_licitacao,
                (
                    SELECT base.preco_base
                    FROM comercial.proposta_base_preco_zefe base
                    WHERE base.coddimensao = descricao.coddimensao
                    LIMIT 1
                ) AS preco,
                EXISTS (
                    SELECT 1
                    FROM comercial.produto_woocommerce woo
                    WHERE btrim(woo.sku) = descricao.coddimensao::text
                ) AS produto_woocommerce
            FROM comercial.proposta_descricaodimensao descricao
            ORDER BY familia, descricaocomercial, dimensao;
            """;

        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        var dados = await connection.QueryAsync<PropostaDescricaoDimensaoConsultaDto>(sql);

        Descricoes.Clear();
        foreach (var item in dados)
            Descricoes.Add(item);
    }

    private async Task InserirPrecosBaseAusentesAsync()
    {
        const string sql = """
            INSERT INTO comercial.proposta_base_preco_zefe
                (coddimensao, preco_base, faixa_tema)
            SELECT
                descricao.coddimensao,
                0,
                4
            FROM comercial.proposta_descricaodimensao descricao
            WHERE NOT EXISTS (
                SELECT 1
                FROM comercial.proposta_base_preco_zefe base
                WHERE base.coddimensao = descricao.coddimensao
            );
            """;

        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        await connection.ExecuteAsync(sql);
    }

    public async Task<ResultadoSalvarTodasDescricoes> SalvarAlteracoesAsync(
        PropostaDescricaoDimensaoConsultaDto item,
        ValoresEditaveisTodasDescricoes original)
    {
        var resultado = new ResultadoSalvarTodasDescricoes();
        var descricaoAlterada = !string.Equals(
            item.descricao_licitacao,
            original.DescricaoLicitacao,
            StringComparison.Ordinal);
        var precoAlterado = item.preco != original.Preco;

        if (!descricaoAlterada && !precoAlterado)
            return resultado;

        const string atualizarDescricaoSql = """
            UPDATE comercial.proposta_dimensaodescricaocomercial
            SET descricao_licitacao = @descricao_licitacao
            WHERE coddimensao = @coddimensao;
            """;
        const string atualizarPrecoSql = """
            UPDATE comercial.proposta_base_preco_zefe
            SET preco_base = @preco
            WHERE coddimensao = @coddimensao;
            """;
        const string inserirPrecoSql = """
            INSERT INTO comercial.proposta_base_preco_zefe
                (coddimensao, preco_base, faixa_tema)
            VALUES
                (@coddimensao, @preco, 4);
            """;

        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        await connection.OpenAsync();

        if (descricaoAlterada)
        {
            try
            {
                var linhas = await connection.ExecuteAsync(atualizarDescricaoSql, item);
                if (linhas != 1)
                    throw new InvalidOperationException("A dimensão não foi localizada para atualização.");
            }
            catch (Exception ex)
            {
                resultado.DescricaoLicitacaoFalhou = true;
                resultado.Erros.Add($"DESCRIÇÃO LICITAÇÃO não foi alterada: {ex.Message}");
            }
        }

        if (precoAlterado)
        {
            try
            {
                var linhas = await connection.ExecuteAsync(atualizarPrecoSql, item);
                if (linhas == 0)
                    await connection.ExecuteAsync(inserirPrecoSql, item);
            }
            catch (Exception ex)
            {
                resultado.PrecoFalhou = true;
                resultado.Erros.Add($"PREÇO não foi alterado: {ex.Message}");
            }
        }

        return resultado;
    }

    public async Task RecarregarAsync(PropostaDescricaoDimensaoConsultaDto item)
    {
        const string sql = """
            SELECT
                coddimensao,
                familia,
                descricaocomercial,
                dimensao,
                descricao_licitacao,
                (
                    SELECT base.preco_base
                    FROM comercial.proposta_base_preco_zefe base
                    WHERE base.coddimensao = descricao.coddimensao
                    LIMIT 1
                ) AS preco,
                EXISTS (
                    SELECT 1
                    FROM comercial.produto_woocommerce woo
                    WHERE btrim(woo.sku) = descricao.coddimensao::text
                ) AS produto_woocommerce
            FROM comercial.proposta_descricaodimensao descricao
            WHERE descricao.coddimensao = @coddimensao;
            """;

        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        var original = await connection.QuerySingleOrDefaultAsync<PropostaDescricaoDimensaoConsultaDto>(
            sql,
            new { item.coddimensao });

        if (original is null)
        {
            Descricoes.Remove(item);
            return;
        }

        item.familia = original.familia;
        item.descricaocomercial = original.descricaocomercial;
        item.dimensao = original.dimensao;
        item.descricao_licitacao = original.descricao_licitacao;
        item.preco = original.preco;
        item.produto_woocommerce = original.produto_woocommerce;
    }

    public async Task<int> SincronizarProdutosWooCommerceAsync()
    {
        var siteUrl = ReadSetting("WooCommerceUrl", "COMERCIAL_WOO_URL", "https://lojapro.cipolatti.com.br").TrimEnd('/');
        var consumerKey = ReadSetting("WooCommerceConsumerKey", "COMERCIAL_WOO_CONSUMER_KEY", string.Empty);
        var consumerSecret = ReadSetting("WooCommerceConsumerSecret", "COMERCIAL_WOO_CONSUMER_SECRET", string.Empty);

        if (string.IsNullOrWhiteSpace(siteUrl))
            throw new InvalidOperationException("Configure a chave WooCommerceUrl no App.config.");

        if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret))
            throw new InvalidOperationException("Configure WooCommerceConsumerKey e WooCommerceConsumerSecret no App.config ou nas variáveis de ambiente.");

        var produtos = await BuscarProdutosWooCommerceAsync(siteUrl, consumerKey, consumerSecret);
        var produtosValidos = produtos
            .Where(produto => produto.Id > 0 && !string.IsNullOrWhiteSpace(produto.Sku))
            .GroupBy(produto => produto.Id)
            .Select(grupo => grupo.Last())
            .ToList();

        if (produtosValidos.Count == 0)
            throw new InvalidOperationException("A API retornou produtos, mas nenhum produto possui SKU válido.");

        await GravarProdutosWooCommerceAsync(produtosValidos);
        return produtosValidos.Count;
    }

    private async Task<List<WooProduct>> BuscarProdutosWooCommerceAsync(
        string siteUrl,
        string consumerKey,
        string consumerSecret)
    {
        try
        {
            return await BuscarProdutosWooCommerceAsync(
                siteUrl,
                consumerKey,
                consumerSecret,
                usarCredenciaisNaUrl: false);
        }
        catch (WooCommerceUnauthorizedException)
        {
            return await BuscarProdutosWooCommerceAsync(
                siteUrl,
                consumerKey,
                consumerSecret,
                usarCredenciaisNaUrl: true);
        }
    }

    private async Task<List<WooProduct>> BuscarProdutosWooCommerceAsync(
        string siteUrl,
        string consumerKey,
        string consumerSecret,
        bool usarCredenciaisNaUrl)
    {
        using var http = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(3)
        };

        if (!usarCredenciaisNaUrl)
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        var produtos = new List<WooProduct>();
        const int perPage = 100;

        for (var page = 1; ; page++)
        {
            var url = CriarUrlProdutosWooCommerce(
                siteUrl,
                page,
                perPage,
                consumerKey,
                consumerSecret,
                usarCredenciaisNaUrl);

            using var response = await http.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !usarCredenciaisNaUrl)
                throw new WooCommerceUnauthorizedException();

            if (!response.IsSuccessStatusCode)
            {
                var detalhe = content.Contains("consumer_secret", StringComparison.OrdinalIgnoreCase) ||
                              content.Contains("consumer_key", StringComparison.OrdinalIgnoreCase)
                    ? "Resposta omitida por conter dados sensíveis."
                    : content;

                throw new InvalidOperationException($"WooCommerce retornou {(int)response.StatusCode} - {response.ReasonPhrase}. {detalhe}");
            }

            var lista = JsonSerializer.Deserialize<List<WooProduct>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            if (lista.Count == 0)
                break;

            produtos.AddRange(lista);

            if (lista.Count < perPage)
                break;
        }

        return produtos;
    }

    private static string CriarUrlProdutosWooCommerce(
        string siteUrl,
        int page,
        int perPage,
        string consumerKey,
        string consumerSecret,
        bool usarCredenciaisNaUrl)
    {
        var url = $"{siteUrl}/wp-json/wc/v3/products?per_page={perPage}&page={page}";

        if (!usarCredenciaisNaUrl)
            return url;

        return $"{url}&consumer_key={Uri.EscapeDataString(consumerKey)}&consumer_secret={Uri.EscapeDataString(consumerSecret)}";
    }

    private async Task GravarProdutosWooCommerceAsync(IReadOnlyCollection<WooProduct> produtos)
    {
        const string criarTabelaTemporariaSql = """
            CREATE TEMP TABLE tmp_produto_woocommerce
            (
                woocommerce_id integer NOT NULL,
                sku character varying(50) NOT NULL
            ) ON COMMIT DROP;
            """;

        const string upsertSql = """
            INSERT INTO comercial.produto_woocommerce
                (woocommerce_id, sku, data_sync)
            SELECT
                woocommerce_id,
                sku,
                CURRENT_TIMESTAMP
            FROM tmp_produto_woocommerce
            ON CONFLICT (woocommerce_id)
            DO UPDATE SET
                sku = EXCLUDED.sku,
                data_sync = CURRENT_TIMESTAMP;

            DELETE FROM comercial.produto_woocommerce produto
            WHERE NOT EXISTS (
                SELECT 1
                FROM tmp_produto_woocommerce atual
                WHERE atual.woocommerce_id = produto.woocommerce_id
            );
            """;

        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(criarTabelaTemporariaSql, transaction: transaction);

            await using (var importer = await connection.BeginBinaryImportAsync(
                "COPY tmp_produto_woocommerce (woocommerce_id, sku) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var produto in produtos)
                {
                    await importer.StartRowAsync();
                    await importer.WriteAsync(produto.Id, NpgsqlDbType.Integer);
                    await importer.WriteAsync(produto.Sku!.Trim(), NpgsqlDbType.Varchar);
                }

                await importer.CompleteAsync();
            }

            await connection.ExecuteAsync(upsertSql, transaction: transaction);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private string ReadSetting(string key, string environmentVariable, string defaultValue)
    {
        var environmentValue = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentValue))
            return environmentValue;

        var configValue = ConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(configValue) ? defaultValue : configValue;
    }
}

public sealed record ValoresEditaveisTodasDescricoes(string? DescricaoLicitacao, float? Preco)
{
    public static ValoresEditaveisTodasDescricoes From(PropostaDescricaoDimensaoConsultaDto item)
    {
        return new ValoresEditaveisTodasDescricoes(item.descricao_licitacao, item.preco);
    }
}

public sealed class ResultadoSalvarTodasDescricoes
{
    public bool DescricaoLicitacaoFalhou { get; set; }
    public bool PrecoFalhou { get; set; }
    public List<string> Erros { get; } = [];
    public bool TemFalhas => Erros.Count > 0;
    public string Mensagem => string.Join(Environment.NewLine, Erros);
}

public class WooProduct
{
    public int Id { get; set; }
    public string? Sku { get; set; }
}

public sealed class WooCommerceUnauthorizedException : Exception
{
}
