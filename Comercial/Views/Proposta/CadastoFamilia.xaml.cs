using Comercial.Data;
using Comercial.Data.Model;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Npgsql;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Comercial.Views.Proposta;

/// <summary>
/// Interação lógica para CadastoFamilia.xam
/// </summary>
public partial class CadastoFamilia : UserControl
{
    private Dictionary<object, ComercialPropostaFamiliaModel> _backupDados = [];

    public CadastoFamilia()
    {
        InitializeComponent();
        DataContext = new CadastoFamiliaViewModel();
        Loaded += CadastoFamilia_Loaded;
    }

    private async void CadastoFamilia_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastoFamiliaViewModel vm)
            await vm.CarregarAsync();
        //await vm.CarregarCommand.ExecuteAsync(null);
    }

    private async void OnRowValidating(object sender, GridViewRowValidatingEventArgs e)
    {
        var item = e.Row.Item as ComercialPropostaFamiliaModel;
        if (item == null) return;

        try
        {
            rgFamilia.IsBusy = true;

            if (DataContext is CadastoFamiliaViewModel vm)
                await vm.SalvarAsync(item);
        }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao salvar", MessageBoxButton.OK, MessageBoxImage.Warning);
            // CANCELA a edição
            e.IsValid = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            // CANCELA a edição
            e.IsValid = false;
        }
        finally
        {
            rgFamilia.IsBusy = false;
        }
    }

    private async void OnRowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
    {
        if (e.EditAction == GridViewEditAction.Cancel)
            return;

        var item = e.Row.Item as ComercialPropostaFamiliaModel;
        if (item == null) return;

        bool isNovaLinha = (item.id == 0);

        try
        {
            rgFamilia.IsBusy = true;

            if (DataContext is CadastoFamiliaViewModel vm)
                await vm.SalvarAsync(item);
        }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao salvar", MessageBoxButton.OK, MessageBoxImage.Warning);
            if (isNovaLinha)
            {
                // Remove a linha que não foi salva
                var source = rgFamilia.ItemsSource as ObservableCollection<ComercialPropostaFamiliaModel>;
                source?.Remove(item);
                rgFamilia.Items.Refresh();
            }
            else
            {
                // Restaura do backup (código anterior)
                if (_backupDados.ContainsKey(item))
                {
                    var backup = _backupDados[item];
                    item = backup;
                    _backupDados.Remove(item);
                }
                rgFamilia.Items.Refresh();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

            if (isNovaLinha)
            {
                // Remove a linha que não foi salva
                var source = rgFamilia.ItemsSource as ObservableCollection<ComercialPropostaFamiliaModel>;
                source?.Remove(item);
            }
            else
            {
                // Restaura do backup (código anterior)
                if (_backupDados.ContainsKey(item))
                {
                    var backup = _backupDados[item];
                    item = backup;
                    _backupDados.Remove(item);
                }

                rgFamilia.Items.Refresh();
            }
        }
        finally
        {
            rgFamilia.IsBusy = false;
        }
    }
}


public partial class CadastoFamiliaViewModel : ObservableObject
{
    private readonly GenericRepository _repo = new();
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaFamiliaModel> comercialPropostaFamilias = [];

    [ObservableProperty]
    private ComercialPropostaFamiliaModel? propostaSelecionada;

    public async Task CarregarAsync()
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

    public async Task SalvarAsync(ComercialPropostaFamiliaModel proposta)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var filtros = new Dictionary<string, object>
        {
            { 
                "id", proposta.id 
            }
        };
        var encontrado = await _repo.GetWhereAsync<ComercialPropostaFamiliaModel>(conn, filtros, "familia", false);

        if (!encontrado.Any())
            await _repo.InsertAsync(conn, proposta);
        else
            await _repo.UpdateAsync(conn, proposta);

    }

    public async Task ExcluirAsync(int id)
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            await _repo.DeleteAsync<ComercialPropostaFamiliaModel>(conn, id);
            MessageBox.Show("Proposta excluída com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao excluir", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
