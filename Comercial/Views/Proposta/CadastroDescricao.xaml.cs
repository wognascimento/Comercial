using Comercial.Data;
using Comercial.Data.Model;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Npgsql;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Comercial.Views.Proposta;

/// <summary>
/// Interação lógica para CadastroDescricao.xam
/// </summary>
public partial class CadastroDescricao : UserControl
{
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;
    private Dictionary<object, ComercialPropostaDescricaoComercialModel> _backupDados = [];

    public CadastroDescricao()
    {
        InitializeComponent();
        DataContext = new CadastroDescricaoViewModel();
        Loaded += CadastroDescricao_Loaded;
    }

    private async void CadastroDescricao_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroDescricaoViewModel vm)
            await vm.CarregarFamiliaAsync();
    }

    private async void rcBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var familia = e.AddedItems[0] as ComercialPropostaFamiliaModel;

        if (DataContext is CadastroDescricaoViewModel vm)
        {
            await vm.CarregarDescricaoAsync(familia);
            vm.IsActive = !(bool)(familia?.resp_descricao.Contains(BaseSettings.Username));
        }
    }

    private async void OnRowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
    {
        if (e.EditAction == GridViewEditAction.Cancel)
            return;

        var item = e.Row.Item as ComercialPropostaDescricaoComercialModel;
        if (item == null) return;

        bool isNovaLinha = (item.coddesccoml == 0);

        try
        {
            rgView.IsBusy = true;

            if (DataContext is CadastroDescricaoViewModel vm)
            {
                // Salva e recebe o código retornado
                long codigoRetornado = await vm.SalvarAsync(item);

                // Atualiza o código no item
                item.coddesccoml = codigoRetornado;

                // Força atualização visual
                rgView.Items.Refresh();

                // 📌 Seleciona e destaca a linha recém criada
                if (isNovaLinha)
                {
                    rgView.SelectedItem = item;
                    rgView.CurrentItem = item;
                    rgView.ScrollIntoView(item);
                }

                // Remove backup se houver
                _backupDados.Remove(item);
            }
        }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao salvar", MessageBoxButton.OK, MessageBoxImage.Warning);

            if (isNovaLinha)
            {
                var source = rgView.ItemsSource as ObservableCollection<ComercialPropostaDescricaoComercialModel>;
                source?.Remove(item);
                rgView.Items.Refresh();
            }
            else if (_backupDados.ContainsKey(item))
            {
                var backup = _backupDados[item];
                item.coddesccoml = backup.coddesccoml;
                _backupDados.Remove(item);
                rgView.Items.Refresh();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

            if (isNovaLinha)
            {
                var source = rgView.ItemsSource as ObservableCollection<ComercialPropostaDescricaoComercialModel>;
                source?.Remove(item);
            }
            else if (_backupDados.ContainsKey(item))
            {
                var backup = _backupDados[item];
                item.coddesccoml = backup.coddesccoml;
                _backupDados.Remove(item);
                rgView.Items.Refresh();
            }
        }
        finally
        {
            rgView.IsBusy = false;
        }
    }

    private void rgView_AddingNewDataItem(object sender, GridViewAddingNewEventArgs e)
    {
        var encontrado = (rcBox.SelectedItem as ComercialPropostaFamiliaModel)?.resp_descricao.Contains(BaseSettings.Username);
        e.Cancel = (bool)!encontrado;
        e.NewObject = new ComercialPropostaDescricaoComercialModel
        {
            id_familia = (rcBox.SelectedItem as ComercialPropostaFamiliaModel)?.id ?? 0,
            familia = (rcBox.SelectedItem as ComercialPropostaFamiliaModel)?.familia ?? string.Empty
        };
    }

    private void RadContextMenu_Opening(object sender, RadRoutedEventArgs e)
    {
        var menu = (RadContextMenu)sender;
        // Verifica em qual linha o menu foi aberto
        var row = menu.GetClickedElement<GridViewRow>();
        if (row != null)
            rgView.SelectedItem = row.Item;
        else
            // Cancela se não clicar em uma linha
            e.Handled = true;
    }

    private void OnCadastroDimensaoClick(object sender, RadRoutedEventArgs e)
    {
        if (rgView.SelectedItem is not ComercialPropostaDescricaoComercialModel itemSelecionado) return;

        if (DataContext is not CadastroDescricaoViewModel vm) return;

        

        var meuUserControl = new CadastroDescricaoDimensao(vm.IsActive, itemSelecionado.coddesccoml);
        // Cria o RadWindow
        RadWindow radWindow = new()
        {
            Content = meuUserControl,
            Header = $"Dimenssões da Descrição: {itemSelecionado.descricaocomercial}",
            Width = 1000,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,  // Define a janela pai

            // 🔒 IMPEDE QUE SAIA DA JANELA PAI
            RestrictedAreaMargin = new Thickness(0),
            IsRestricted = false  // Não permite mover para fora do Owner
        };

        // Abre como modal
        radWindow.ShowDialog();
    }
}

public partial class CadastroDescricaoViewModel : ObservableObject
{
    private readonly GenericRepository _repo = new();
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaFamiliaModel> comercialPropostaFamilias = [];

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaDescricaoComercialModel> descricoesComercial = [];

    [ObservableProperty]
    private ComercialPropostaDescricaoComercialModel? descricaoComerciaSelecionada;

    [ObservableProperty]
    private bool isActive;

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

    public async Task<long> SalvarAsync(ComercialPropostaDescricaoComercialModel proposta)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var filtros = new Dictionary<string, object>
        {
            { 
                "coddesccoml", proposta.coddesccoml 
            }
        };
        var encontrado = await _repo.GetWhereAsync<ComercialPropostaDescricaoComercialModel>(conn, filtros, "descricaocomercial", false);

        if (!encontrado.Any())
            proposta.coddesccoml = await _repo.InsertAsync(conn, proposta);
        else
            await _repo.UpdateAsync(conn, proposta);

        return proposta.coddesccoml;
    }
}