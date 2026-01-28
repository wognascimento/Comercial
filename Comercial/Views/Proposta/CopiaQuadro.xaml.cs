using Comercial.Data;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

namespace Comercial.Views.Proposta;

/// <summary>
/// Interação lógica para CopiaQuadro.xam
/// </summary>
public partial class CopiaQuadro : UserControl
{
    private readonly PropostaBriefingTemaDto briefing;
    public CopiaQuadro(PropostaBriefingTemaDto _briefing)
    {
        InitializeComponent();
        DataContext = new CopiaQuadroViewModel();
        briefing = _briefing;
        //Loaded += CopiaQuadro_Loaded;
    }

    private async void rcbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CopiaQuadroViewModel vm)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.ComercialPropostaSiglas.Clear();
                vm.ComercialPropostaAnos.Clear();
                vm.ComercialPropostaTemas.Clear();
                vm.ComercialItensProposta.Clear();
                await vm.CarregarSiglasAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (RepositoryException ex)
            {
                MessageBox.Show(ex.Message, "Erro ao salvar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }
    }

    private async void rcbSiglas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CopiaQuadroViewModel vm)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.ComercialPropostaAnos.Clear();
                vm.ComercialPropostaTemas.Clear();
                vm.ComercialItensProposta.Clear();
                await vm.CarregarAnoAsync(rcbSiglas.SelectedItem?.ToString() ?? string.Empty);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (RepositoryException ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message, "Erro ao salvar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void rcbAnos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CopiaQuadroViewModel vm)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.ComercialPropostaTemas.Clear();
                vm.ComercialItensProposta.Clear();
                await vm.CarregarTemasAsync(rcbSiglas.SelectedItem?.ToString() ?? string.Empty, (int)(rcbAnos.SelectedItem ?? 0));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (RepositoryException ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message, "Erro ao salvar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void rcbTemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CopiaQuadroViewModel vm)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                await vm.CarregarItensAsync(
                    rcbSiglas.SelectedItem?.ToString() ?? string.Empty,
                    (rcbTemas.SelectedItem as PropostaHistoricoQuadroTemaDto)?.idtema ?? 0,
                    rcbAnos.SelectedItem is int ano ? ano : 0
                );
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (RepositoryException ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message, "Erro ao salvar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void RadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CopiaQuadroViewModel vm = (CopiaQuadroViewModel)DataContext;
            ObservableCollection<PropostaItemQuadroDto> itens = new([.. rgViewItens.Items.Cast<PropostaItemQuadroDto>()]);

            if (itens.Count == 0)
            {
                MessageBox.Show("Nenhum item para copiar.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirma = MessageBox.Show($"Confirma a cópia de {itens.Count} itens?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirma != MessageBoxResult.Yes)
                return;

            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
            await vm.CopiarItens(itens, briefing);
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

public partial class CopiaQuadroViewModel : ObservableObject
{
    private readonly GenericRepository _repo = new();
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaTipoDto> comercialPropostaTipos =
        [
            new ComercialPropostaTipoDto { tipo = "Proposta", view = "proposta_historico_quadropreco_completo" },
            new ComercialPropostaTipoDto { tipo = "Fecha", view = "proposta_historico_fecha_completo" }
        ];

    [ObservableProperty]
    private ComercialPropostaTipoDto comercialPropostaTipo;

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaSiglas = [];

    [ObservableProperty]
    private ObservableCollection<int> comercialPropostaAnos = [];

    [ObservableProperty]
    private ObservableCollection<PropostaHistoricoQuadroTemaDto> comercialPropostaTemas = [];

    [ObservableProperty]
    private ObservableCollection<PropostaItemQuadroDto> comercialItensProposta = [];


    public async Task CarregarSiglasAsync()
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var itens = await conn.QueryAsync<string>(
        @$"
            SELECT sigla
            FROM comercial.{ComercialPropostaTipo.view}
            GROUP BY sigla
            ORDER BY sigla;
        ");
        ComercialPropostaSiglas = new ObservableCollection<string>(itens);
    }

    public async Task CarregarAnoAsync(string Sigla)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { Sigla };
        var itens = await conn.QueryAsync<int>(
        @$"
            SELECT ano
            FROM comercial.{ComercialPropostaTipo.view}
            WHERE sigla = @Sigla    
            GROUP BY ano
            ORDER BY ano DESC;
        ", parametros);
        ComercialPropostaAnos = new ObservableCollection<int>(itens);
    }

    public async Task CarregarTemasAsync(string Sigla, int Ano)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { Sigla, Ano };
        var itens = await conn.QueryAsync<PropostaHistoricoQuadroTemaDto>(
        @$"
            SELECT idtema, tema
            FROM comercial.{ComercialPropostaTipo.view}
            WHERE sigla = @Sigla AND ano = @Ano   
            GROUP BY idtema, tema
            ORDER BY tema;
        ", parametros);
        ComercialPropostaTemas = new ObservableCollection<PropostaHistoricoQuadroTemaDto>(itens);
    }

    public async Task CarregarItensAsync(string Sigla, long? Idtema, int Ano)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { Sigla, Idtema, Ano };
        var itens = await conn.QueryAsync<PropostaItemQuadroDto>(
        @$"
            SELECT 
                ordem,
	            tipo,
	            bloco,
	            familia,
	            item,
	            localitem,
	            local,
	            localdetalhe,
	            descricaocomercial,
	            dimensao,
	            ledml,
	            qtd,
	            idtema,
	            tema,
	            coddesccoml, 
	            coddimensao
            FROM comercial.{ComercialPropostaTipo.view}
            WHERE sigla = @Sigla AND Idtema = @Idtema AND ano = @Ano
            ORDER BY ordem, item;
        ", parametros);
        ComercialItensProposta = new ObservableCollection<PropostaItemQuadroDto>(itens);
    }

    public async Task CopiarItens(ObservableCollection<PropostaItemQuadroDto> itens, PropostaBriefingTemaDto Briefing)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        await conn.OpenAsync();
        using var tran = conn.BeginTransaction();
        try
        {
            
            foreach (var item in itens)
            {


                var sql = @"
                INSERT INTO comercial.proposta_quadro_quantitativo
                (codbrief, sigla, tema, tipo, item, local, localdetalhe, coddimensao,
                 qtd, obs, obsinterna, ledml, desconto, bloco, idtema, cadastradopor, datacadastro)
                VALUES
                (@codbrief, @sigla, @tema, @tipo, @item, @local, @localdetalhe, @coddimensao,
                 @qtd, @obs, @obsinterna, @ledml, @desconto, @bloco, @idtema, @cadastradopor, @datacadastro)
                RETURNING codquadro_quantitativo;
            ";

                var codcompl = await conn.ExecuteScalarAsync<int>(
                    @"INSERT INTO comercial.proposta_quadro_quantitativo 
                             (
                                  codbrief, sigla, tema, tipo, item, local, localdetalhe, coddimensao,
                                  qtd, qtdanterior, ledml, bloco, idtema, cadastradopor, datacadastro
                             )
                             VALUES 
                             (
                                  @codbrief, @sigla, @tema, @tipo, @item, @local, @localdetalhe, @coddimensao,
                                  @qtd, @qtdanterior, @ledml, @bloco, @idtema, @cadastradopor, @datacadastro
                             )
                      RETURNING codquadro_quantitativo",
                    new
                    {
                        codbrief = Briefing.codbriefing,
                        Briefing.sigla,
                        tema = Briefing.temas,
                        item.tipo,
                        item.item,
                        item.local,
                        item.localdetalhe,
                        item.coddimensao,
                        qtd = 0,
                        qtdanterior = item.qtd,
                        item.ledml,
                        item.bloco,
                        Briefing.idtema,
                        cadastradopor = BaseSettings.Username,
                        datacadastro = DateTime.Now
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