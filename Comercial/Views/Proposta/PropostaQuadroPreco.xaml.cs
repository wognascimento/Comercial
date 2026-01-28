using Comercial.Data;
using Comercial.Data.Model;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
using Npgsql;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Comercial.Views.Proposta;

/// <summary>
/// Interação lógica para PropostaQuadroPreco.xam
/// </summary>
public partial class PropostaQuadroPreco : UserControl
{
    public PropostaQuadroPreco()
    {
        InitializeComponent();
        DataContext = new PropostaQuadroPrecoViewModel();
        Loaded += PropostaQuadroPreco_Loaded;
    }

    private async void PropostaQuadroPreco_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                await vm.CarregarBrifinsAsync();
                await vm.CarregarFamiliaAsync();
                await vm.CarregarBlocosAsync();
                await vm.CarregarLocaisAsync();
            }
            catch (RepositoryException ex)
            {
                MessageBox.Show(ex.Message, "Erro ao salvar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void boxBrienfing_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                vm.PropostaBriefingTemas = [];
                vm.SelectedBriefingTema = null;

                vm.ResumosProposta = [];
                vm.ItensProposta = [];

                btnAlterar.IsEnabled = true;
                btnIncluir.IsEnabled = true;
                btnLimpar.IsEnabled = true;
                btnExcluir.IsEnabled = true;
                btnCopiar.IsEnabled = true;

                if (e.AddedItems.Count > 0 && e.AddedItems[0] is PropostaBriefingQuadroDto selectedBriefing)
                {
                    await vm.CarregarBrifinTemasAsync(selectedBriefing.codbriefing);
                    await vm.CarregarResumoCustoPropostaAsync(selectedBriefing.codbriefing);
                    vm.ItensProposta = [];
                    vm.ItemProposta = null;

                }
            }
            catch (RepositoryException ex)
            {
                MessageBox.Show(ex.Message, "Erro ao salvar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void boxBrienfingTema_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void rasBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void OnAlterarClick(object sender, RoutedEventArgs e)
    {

    }

    private void OnIncluirClick(object sender, RoutedEventArgs e)
    {

    }

    private void OnLimparClick(object sender, RoutedEventArgs e)
    {

    }

    private void OnExcluirClick(object sender, RoutedEventArgs e)
    {

    }

    private void OnCopiarClick(object sender, RoutedEventArgs e)
    {

    }

    private void dtInicial_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void dtConclusao_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void itensProposta_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {

    }

    private void RadGridView_AddingNewDataItem(object sender, Telerik.Windows.Controls.GridView.GridViewAddingNewEventArgs e)
    {

    }

    private void RadGridViewIlustracaoRowValidating(object sender, Telerik.Windows.Controls.GridViewRowValidatingEventArgs e)
    {

    }
}

public partial class PropostaQuadroPrecoViewModel : ObservableObject
{
    private readonly GenericRepository _repo = new();
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    [ObservableProperty]
    private ObservableCollection<PropostaBriefingQuadroDto> propostaBriefings = [];

    [ObservableProperty]
    private PropostaBriefingQuadroDto selectedBriefing;

    [ObservableProperty]
    private ObservableCollection<PropostaBriefingTemaDto> propostaBriefingTemas = [];

    [ObservableProperty]
    private PropostaBriefingTemaDto selectedBriefingTema;

    [ObservableProperty]
    private ObservableCollection<QuadroQuantitativoDto> itensProposta = [];

    [ObservableProperty]
    private QuadroQuantitativoDto itemProposta;

    [ObservableProperty]
    private ObservableCollection<QuadroQuantitativoResumoDto> resumosProposta = [];


    [ObservableProperty]
    private ObservableCollection<ComercialPropostaFamiliaModel> comercialPropostaFamilias = [];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaBlocos = [];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaLocais = [];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaTipos = ["Proposta", "Opcional", "Complemento", "Complemento para todos os temas", "Venda"];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaLeds = ["LED AZ", "LED AZ/ML", "LED BC", "LED BC/COL", "LED BC/ML", "LED BC QUENTE", "LED BC QUENTE/ML", "LED COL", "LED COL/ML"];


    public async Task CarregarBrifinsAsync()
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var itens = await conn.QueryAsync<PropostaBriefingQuadroDto>(
        @"SELECT sigla, nome, codbriefing, verbaminint, 
	                 verbamaxint, verbaintdefinidapor, verbaminext, 
	                 verbamaxext, verbaextdefinidapor, verbaunicadefinidapor, verba_nao_definida, 
	                 moeda, verbaunica, cancelado, diretorcliente, 
	                 responsavelprojeto, tema, valorfechainterno, indiceproposta, 
	                 novo, tot_cenografia, vlr_inicial, praca, tipo_evento
              FROM comercial.proposta_briefing_quadro
              ORDER BY sigla, codbriefing;");
        PropostaBriefings = new ObservableCollection<PropostaBriefingQuadroDto>(itens);
    }

    public async Task CarregarBrifinTemasAsync(long codbriefing)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { codbriefing };
        var itens = await conn.QueryAsync<PropostaBriefingTemaDto>(
        @"SELECT sigla, temas, faixapreco, indiceproposta, 
                     resp_tema, codbriefing, data_conclusao, tot_cenografia, 
	                 data_inicio_preco, data_conclusao_preco, ordem_escolha, idtema, ativo
              FROM comercial.proposta_temas_briefing
              WHERE codbriefing = @codbriefing AND data_conclusao IS NOT NULL
              ORDER BY ordem_escolha;", parametros);
        PropostaBriefingTemas = new ObservableCollection<PropostaBriefingTemaDto>(itens);
    }

    public async Task CarregarResumoCustoPropostaAsync(long codbrief)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { codbrief };
        var itens = await conn.QueryAsync<QuadroQuantitativoResumoDto>(
        @"
                SELECT 
	                tema,
                    tipo,
                    SUM(preco_excel_total) as total
                FROM comercial.view_quadro_preco
                WHERE codbrief = @codbrief
                GROUP BY tema, tipo
            ", parametros);
        ResumosProposta = new ObservableCollection<QuadroQuantitativoResumoDto>(itens);
    }

    public async Task CarregarFamiliaAsync()
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var lista = await conn.QueryAsync<ComercialPropostaFamiliaModel>(@"SELECT * FROM comercial.proposta_familia WHERE familia <> 'DELETAR'and familia <> 'URGENTE'  ORDER BY familia;");
        ComercialPropostaFamilias = new ObservableCollection<ComercialPropostaFamiliaModel>(lista);
    }

    public async Task CarregarBlocosAsync()
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var itens = await conn.QueryAsync<string>(@"SELECT bloco FROM comercial.proposta_blocos WHERE bloco <> 'CARROSSEL' ORDER BY bloco;");
        ComercialPropostaBlocos = new ObservableCollection<string>(itens);
    }

    public async Task CarregarLocaisAsync()
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var itens = await conn.QueryAsync<string>(@"SELECT local FROM comercial.proposta_local ORDER BY local;");
        ComercialPropostaLocais = new ObservableCollection<string>(itens);
    }


}
