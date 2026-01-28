using Comercial.Data;
using Comercial.Data.Model;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.AutoSuggestBox;

namespace Comercial.Views.Proposta;

/// <summary>
/// Interação lógica para CopiaItensHomologados.xam
/// </summary>
public partial class CopiaItensHomologados : UserControl
{
    private readonly ComercialPropostaDimensaoDescricaoComercialModel _dimenssaoComercial;
    private readonly ComercialPropostaDescricaoComercialModel _descricaoComercial;

    public CopiaItensHomologados(ComercialPropostaDescricaoComercialModel descricaoComercial, ComercialPropostaDimensaoDescricaoComercialModel dimenssaoComercial)
    {
        InitializeComponent();
        DataContext = new CopiaItensHomologadosViewModel();
        Loaded += CopiaItensHomologados_Loaded;
        _dimenssaoComercial = dimenssaoComercial;
        _descricaoComercial = descricaoComercial;

        this.rasBoxDescricao.ClearButtonCommand = new DelegateCommand(execute: obj => OnClearExecuted(this.rasBoxDescricao));
        this.rasBoxDimenssao.ClearButtonCommand = new DelegateCommand(execute: obj => OnClearExecuted(this.rasBoxDimenssao));

    }

    private void OnClearExecuted(RadAutoSuggestBox suggestBox)
    {
        suggestBox.Text = string.Empty;
        //suggestBox.IsDropDownOpen = false;
        if (suggestBox.Name == "rasBoxDescricao")
        {
            var vm = suggestBox.DataContext as CadastroHomologacaoProdutoComercialViewModel;
            this.rasBoxDimenssao.Text = string.Empty;
            vm.DimensoesComercial = [];
            vm.DimenssaoComercial = null;
            vm.ItensHomologados = [];
        }
        else if (suggestBox.Name == "rasBoxDimenssao")
        {
            var vm = suggestBox.DataContext as CadastroHomologacaoProdutoComercialViewModel;
            vm.ItensHomologados = [];
        }
    }

    private async void CopiaItensHomologados_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CopiaItensHomologadosViewModel vm)
        {
            await vm.CarregarFamiliaAsync();
        }
    }

    private async void rcbFamilia_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var familia = e.AddedItems[0] as ComercialPropostaFamiliaModel;
        if (DataContext is CopiaItensHomologadosViewModel vm)
        {
            await vm.CarregarDescricaoAsync(familia);
            rasBoxDescricao.IsDropDownOpen = true;
            rasBoxDescricao.Focus();

            rasBoxDescricao.Text = null;
            rasBoxDimenssao.Text = null;

            vm.ComercialPropostaFamilia = familia;
            vm.DescricaoComercial = null;
            vm.DimenssaoComercial = null;
            vm.ItensHomologados = [];
        }
    }

    private async void rasBoxDescricao_SuggestionChosen(object sender, Telerik.Windows.Controls.AutoSuggestBox.SuggestionChosenEventArgs e)
    {
        if (e.Suggestion is ComercialPropostaDescricaoComercialModel descricao)
        {
            if (DataContext is CopiaItensHomologadosViewModel vm)
            {
                await vm.CarregarDimensoesAsync(descricao.coddesccoml);
                rasBoxDimenssao.IsDropDownOpen = true;
                rasBoxDimenssao.Focus();
                vm.DescricaoComercial = descricao;
                vm.ItensHomologados = [];
            }
        }
    }

    private void rasBoxDescricao_TextChanged(object sender, Telerik.Windows.Controls.AutoSuggestBox.TextChangedEventArgs e)
    {
        if (e.Reason == TextChangeReason.UserInput)
        {
            if (DataContext is CopiaItensHomologadosViewModel vm)
            {
                this.rasBoxDescricao.ItemsSource = vm.GetByText(vm.DescricoesComercial, x => x.descricaocomercial, this.rasBoxDescricao.Text);
                vm.DimenssaoComercial = null;
            }
        }
    }

    private void rasBoxDimenssao_TextChanged(object sender, Telerik.Windows.Controls.AutoSuggestBox.TextChangedEventArgs e)
    {
        if (e.Reason == TextChangeReason.UserInput)
        {
            if (DataContext is CopiaItensHomologadosViewModel vm)
                this.rasBoxDimenssao.ItemsSource = vm.GetByText(vm.DimensoesComercial, x => x.dimensao, this.rasBoxDimenssao.Text); //vm.GetDescricaoByText(this.rasBoxDescricao.Text);
        }
    }

    private async void rasBoxDimenssao_SuggestionChosen(object sender, SuggestionChosenEventArgs e)
    {
        if (e.Suggestion is ComercialPropostaDimensaoDescricaoComercialModel dimensao)
        {
            if (DataContext is CopiaItensHomologadosViewModel vm)
            {
                rgViewHomologacao.IsBusy = true;
                await vm.CarregarItensHomologacao(dimensao.coddimensao);
                vm.DimenssaoComercial = dimensao;
                rgViewHomologacao.IsBusy = false;
            }
        }
    }

    private void rasBox_GotFocus(object sender, RoutedEventArgs e)
    {
        (sender as RadAutoSuggestBox).IsDropDownOpen = true;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {

        try
        {
            CopiaItensHomologadosViewModel vm = (CopiaItensHomologadosViewModel)DataContext;
            ObservableCollection<PropostaInsumoDescComlDto> itens = new([.. rgViewHomologacao.Items.Cast<PropostaInsumoDescComlDto>()]);

            if (itens.Count == 0)
            {
                MessageBox.Show("Nenhum item para copiar.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirma = MessageBox.Show($"Confirma a cópia de {itens.Count} itens para {_descricaoComercial.descricaocomercial} {_dimenssaoComercial.dimensao}?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirma != MessageBoxResult.Yes)
                return;

            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
            await vm.CopiarItens(itens, _dimenssaoComercial);
            MessageBox.Show("Itens copiados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            MessageBox.Show($"Erro do banco: {pgEx.MessageText}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
        }
    }
}

public partial class CopiaItensHomologadosViewModel : ObservableObject
{
    private readonly GenericRepository _repo = new();
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaFamiliaModel> comercialPropostaFamilias = [];

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaDescricaoComercialModel> descricoesComercial = [];

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaDimensaoDescricaoComercialModel> dimensoesComercial = [];

    [ObservableProperty]
    private ObservableCollection<PropostaInsumoDescComlDto> itensHomologados = [];

    [ObservableProperty]
    private ComercialPropostaFamiliaModel comercialPropostaFamilia = new();

    [ObservableProperty]
    private ComercialPropostaDescricaoComercialModel descricaoComercial = new();

    [ObservableProperty]
    private ComercialPropostaDimensaoDescricaoComercialModel dimenssaoComercial = new();

    [ObservableProperty]
    private PropostaInsumoDescComlDto itemHomologado = new();


    public async Task CarregarFamiliaAsync()
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var lista = await _repo.GetAllAsync<ComercialPropostaFamiliaModel>(conn, "familia");
            ComercialPropostaFamilias = new ObservableCollection<ComercialPropostaFamiliaModel>(lista);
        }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao carregar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task CarregarDescricaoAsync(ComercialPropostaFamiliaModel familia)
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var filtros = new Dictionary<string, object>
            {
                { "id_familia", familia.id }
            };
            var lista = await _repo.GetWhereAsync<ComercialPropostaDescricaoComercialModel>(conn, filtros, "descricaocomercial", false);
            DescricoesComercial = new ObservableCollection<ComercialPropostaDescricaoComercialModel>(lista);
        }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao carregar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public List<ComercialPropostaDescricaoComercialModel> GetDescricaoByText(string searchText)
    {
        var result = new List<ComercialPropostaDescricaoComercialModel>();
        var lowerText = searchText.ToLowerInvariant();
        return [.. DescricoesComercial.Where(x => x.descricaocomercial.Contains(lowerText, StringComparison.InvariantCultureIgnoreCase))];
    }

    public List<T> GetByText<T>(IEnumerable<T> source, Expression<Func<T, string>> propertySelector, string searchText)
    {
        var lowerText = searchText.ToLowerInvariant();
        var compiledSelector = propertySelector.Compile();

        return [.. source
            .Where(x => compiledSelector(x)
                .Contains(lowerText, StringComparison.InvariantCultureIgnoreCase))];
    }

    public async Task CarregarDimensoesAsync(long coddesccoml)
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var filtros = new Dictionary<string, object>
            {
                { "coddesccoml", coddesccoml }
            };
            var lista = await _repo.GetWhereAsync<ComercialPropostaDimensaoDescricaoComercialModel>(conn, filtros, "dimensao", false);
            DimensoesComercial = new ObservableCollection<ComercialPropostaDimensaoDescricaoComercialModel>(lista);
        }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao carregar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task CarregarItensHomologacao(long coddimensao)
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var parametros = new { coddimensao };
            var itens = await conn.QueryAsync<PropostaInsumoDescComlDto>(
            @"SELECT proposta_insumodesccoml.codinsumo,
                proposta_insumodesccoml.coddimensao,
                proposta_insumodesccoml.codcompladicional,
                proposta_insumodesccoml.consulta_estoque,
	            qry3descricoes.planilha,
	            qry3descricoes.descricao_completa,
	            qry3descricoes.unidade,
                proposta_insumodesccoml.qtd,
                qry3descricoes.custo AS vlr_unitario,
                qry3descricoes.custo * proposta_insumodesccoml.qtd AS vlr_total,
                tblcustodescadicional.indiceproposta,
                tblcustodescadicional.led,
                proposta_insumodesccoml.id
              FROM comercial.proposta_insumodesccoml
              JOIN producao.qry3descricoes ON proposta_insumodesccoml.codcompladicional = qry3descricoes.codcompladicional
	          LEFT JOIN comercial.tblcustodescadicional ON proposta_insumodesccoml.codcompladicional = tblcustodescadicional.codcompladicional
	          WHERE coddimensao = @coddimensao 
	          ORDER BY id;", parametros);

            ItensHomologados = new ObservableCollection<PropostaInsumoDescComlDto>(itens);
        }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao carregar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task CopiarItens(ObservableCollection<PropostaInsumoDescComlDto> itens, ComercialPropostaDimensaoDescricaoComercialModel dimenssaoComercial)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        await conn.OpenAsync();
        using var tran = conn.BeginTransaction();
        try
        {
            
            foreach (var item in itens)
            {

                var filtros = new Dictionary<string, object>
                {
                    {"coddimensao", dimenssaoComercial.coddimensao},
                    {"codcompladicional", item.codcompladicional}
                };
                var encontrado = await _repo.GetWhereAsync<ComercialPropostaInsumoDescComlModel>(conn, filtros, "codinsumo", false);

                if (!encontrado.Any())
                    await conn.ExecuteAsync(
                        @"INSERT INTO comercial.proposta_insumodesccoml(
                            coddimensao, codcompladicional, qtd, id, consulta_estoque)
                          VALUES (@coddimensao, @codcompladicional, @qtd, @id, @consulta_estoque);",
                        new
                        {
                            dimenssaoComercial.coddimensao,
                            item.codcompladicional,
                            item.qtd,
                            item.id,
                            item.consulta_estoque
                        },
                        transaction: tran
                );
            }
            await tran.CommitAsync();
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            throw new Exception("Erro ao inserir histórico de checklist", ex);
        }

    }
}
