using Comercial.Data;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
using Npgsql;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Comercial.Utils;

/// <summary>
/// Interação lógica para BuscaDescricaoComercial.xam
/// </summary>
public partial class BuscaDescricaoComercial : UserControl
{
    public event Action<PropostaDescricaoDimensaoDto>? ItemSelecionado;

    public BuscaDescricaoComercial()
    {
        InitializeComponent();
        DataContext = new BuscaDescricaoComercialViewModel();
        Loaded += BuscaDescricaoComercial_Loaded;
    }

    private async void BuscaDescricaoComercial_Loaded(object sender, RoutedEventArgs e)
    {

        if (DataContext is BuscaDescricaoComercialViewModel vm)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                await vm.CarregarDescricoesAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (RepositoryException ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message, "Erro ao carregar descricoes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void rgViewItens_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (rgViewItens.SelectedItem is PropostaDescricaoDimensaoDto item)
        {
            ItemSelecionado?.Invoke(item);

            // fecha a janela que contém o UserControl
            var janela = Window.GetWindow(this);
            janela?.Close();
        }
    }

    private bool _isInternalSearch = false;

    private void rgViewItens_Searching(object sender, Telerik.Windows.Controls.GridView.GridViewSearchingEventArgs e)
    {
        if (_isInternalSearch)
        {
            _isInternalSearch = false;
            return;
        }

        string originalText = e.SearchText;

        // Se o texto não estiver vazio e não começar com aspas
        if (!string.IsNullOrWhiteSpace(originalText) && !originalText.StartsWith("\""))
        {
            e.Cancel = true; // Cancela a busca atual sem aspas
            _isInternalSearch = true;

            string exactSearch = $"\"{originalText.Trim()}\"";

            // A forma correta de disparar o comando estático no WPF:
            // 1º Parâmetro: O texto da busca (CommandParameter)
            // 2º Parâmetro: O controle que receberá o comando (Target)
            RadGridViewCommands.SearchByText.Execute(exactSearch);
        }
    }
}

public partial class BuscaDescricaoComercialViewModel : ObservableObject
{
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    [ObservableProperty]
    private PropostaDescricaoDimensaoDto propostaDescricao;

    [ObservableProperty]
    private ObservableCollection<PropostaDescricaoDimensaoDto> propostaDescricoes = [];

    public async Task CarregarDescricoesAsync()
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var itens = await conn.QueryAsync<PropostaDescricaoDimensaoDto>(
        @$"
            SELECT 
	            familia, 
	            descricaocomercial, 
	            dimensao, 
	            descricao_completa, 
	            coddesccoml, 
	            coddimensao, 
	            indicedimensao, 
	            custounitarioestimado, 
	            obsobrigatoria, 
	            travaled 
            FROM 
	            comercial.proposta_descricaodimensao;
        ");
        PropostaDescricoes = new ObservableCollection<PropostaDescricaoDimensaoDto>(itens);
    }
}
