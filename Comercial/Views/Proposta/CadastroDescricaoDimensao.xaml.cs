using Comercial.Data;
using Comercial.Data.Model;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Npgsql;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Comercial.Views.Proposta;

/// <summary>
/// Interação lógica para CadastroDescricaoDimensao.xam
/// </summary>
public partial class CadastroDescricaoDimensao : UserControl
{
    private bool IsActive;
    private long CodDescComl;

    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;
    private Dictionary<object, ComercialPropostaDimensaoDescricaoComercialModel> _backupDados = [];

    public CadastroDescricaoDimensao(bool isActive, long coddesccoml)
    {
        InitializeComponent();
        IsActive = isActive;
        CodDescComl = coddesccoml;

        DataContext = new CadastroDescricaoDimensaoViewModel();

        Loaded += CadastroDescricaoDimensao_Loaded;

    }

    private async void CadastroDescricaoDimensao_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroDescricaoDimensaoViewModel vm)
        {
            vm.IsActive = IsActive;
            await vm.CarregarDimensoesAsync(CodDescComl);
        }
    }

    private void rgView_AddingNewDataItem(object sender, Telerik.Windows.Controls.GridView.GridViewAddingNewEventArgs e)
    {
        e.NewObject = new ComercialPropostaDimensaoDescricaoComercialModel
        {
            coddesccoml = CodDescComl
        };
    }

    private async void OnRowEditEnded(object sender, Telerik.Windows.Controls.GridViewRowEditEndedEventArgs e)
    {
        if (e.EditAction == GridViewEditAction.Cancel)
            return;

        var item = e.Row.Item as ComercialPropostaDimensaoDescricaoComercialModel;
        if (item == null) return;

        bool isNovaLinha = (item.coddimensao == 0);

        try
        {
            rgView.IsBusy = true;

            if (DataContext is CadastroDescricaoDimensaoViewModel vm)
            {
                // Salva e recebe o código retornado
                long codigoRetornado = await vm.SalvarAsync(item);

                // Atualiza o código no item
                item.coddimensao = codigoRetornado;

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
                var source = rgView.ItemsSource as ObservableCollection<ComercialPropostaDimensaoDescricaoComercialModel>;
                source?.Remove(item);
                rgView.Items.Refresh();
            }
            else if (_backupDados.ContainsKey(item))
            {
                var backup = _backupDados[item];
                item.coddimensao = backup.coddimensao;
                _backupDados.Remove(item);
                rgView.Items.Refresh();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

            if (isNovaLinha)
            {
                var source = rgView.ItemsSource as ObservableCollection<ComercialPropostaDimensaoDescricaoComercialModel>;
                source?.Remove(item);
            }
            else if (_backupDados.ContainsKey(item))
            {
                var backup = _backupDados[item];
                item.coddimensao = backup.coddimensao;
                _backupDados.Remove(item);
                rgView.Items.Refresh();
            }
        }
        finally
        {
            rgView.IsBusy = false;
        }
    }
}

public partial class CadastroDescricaoDimensaoViewModel : ObservableObject
{
    private readonly GenericRepository _repo = new();
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaDimensaoDescricaoComercialModel> dimensoesComercial = [];

    [ObservableProperty]
    private ComercialPropostaDimensaoDescricaoComercialModel? dimensaoComerciaSelecionada;

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

    public async Task<long> SalvarAsync(ComercialPropostaDimensaoDescricaoComercialModel proposta)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var filtros = new Dictionary<string, object>
        {
            {
                "coddimensao", proposta.coddimensao
            }
        };
        var encontrado = await _repo.GetWhereAsync<ComercialPropostaDimensaoDescricaoComercialModel>(conn, filtros, "dimensao", false);

        if (!encontrado.Any())
        {
            proposta.cadastradopor = BaseSettings.Username;
            proposta.datacadastro = DateTime.Now;
            proposta.coddimensao = await _repo.InsertAsync(conn, proposta);
        }
        else
        {
            await _repo.UpdateAsync(conn, proposta);
        }

        return proposta.coddimensao;
    }

}