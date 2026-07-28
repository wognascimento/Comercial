using Comercial.Data;
using Comercial.Data.Model;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Npgsql;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows;
using Telerik.Windows.Controls;
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
    private ComercialPropostaDimensaoDescricaoComercialModel? _linhaCopiada;

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
            coddesccoml = CodDescComl,
            ativo = "1",
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

    private void RadContextMenu_Opening(object sender, RadRoutedEventArgs e)
    {
        var menu = (RadContextMenu)sender;
        var row = menu.GetClickedElement<GridViewRow>();
        if (row != null)
            rgView.SelectedItem = row.Item;
        else if (_linhaCopiada == null)
            e.Handled = true;
    }

    private void rgView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CancelarOperacaoAtual();
            e.Handled = true;
            return;
        }

        if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            return;

        if (e.Key == Key.C)
        {
            CopiarLinhaSelecionada();
            e.Handled = true;
        }
        else if (e.Key == Key.V)
        {
            ColarLinhaCopiada();
            e.Handled = true;
        }
    }

    private void OnCopiarLinhaClick(object sender, RadRoutedEventArgs e) => CopiarLinhaSelecionada();

    private void OnColarLinhaClick(object sender, RadRoutedEventArgs e) => ColarLinhaCopiada();

    private void CopiarLinhaSelecionada()
    {
        if (rgView.SelectedItem is not ComercialPropostaDimensaoDescricaoComercialModel item)
            return;

        _linhaCopiada = new ComercialPropostaDimensaoDescricaoComercialModel
        {
            coddesccoml = CodDescComl,
            dimensao = item.dimensao,
            dimensaofantasia = item.dimensaofantasia,
            nomefantasia = item.nomefantasia,
            observacao = item.observacao,
            obsobrigatoria = item.obsobrigatoria,
            travaled = item.travaled,
            indicedimensao = item.indicedimensao,
            indiceled = item.indiceled,
            indicedesconto = item.indicedesconto,
            indicedescontopreco = item.indicedescontopreco,
            cargaeletrica = item.cargaeletrica,
            areareal = item.areareal,
            cubagem = item.cubagem,
            peso = item.peso,
            custounitarioapurado = item.custounitarioapurado,
            custounitarioestimado = item.custounitarioestimado,
            preco = item.preco,
            valor_desconto_area_projecao = item.valor_desconto_area_projecao,
            ativo = item.ativo,
            insumo_concluido = item.insumo_concluido,
            insumo_concluido_por = item.insumo_concluido_por,
            insumo_concluido_data = item.insumo_concluido_data,
            concatenar = item.concatenar,
            relatorio_estabilidade = item.relatorio_estabilidade,
            verificador = item.verificador,
            cargaeletrica_led = item.cargaeletrica_led,
            aux = item.aux,
            projecao_area = item.projecao_area,
            custo_historico = item.custo_historico,
            preco_nf = item.preco_nf,
            cubagem_estimada = item.cubagem_estimada,
            horas_estimadas = item.horas_estimadas,
            a_metragem = item.a_metragem,
            v_metragem = item.v_metragem,
            h_metragem = item.h_metragem,
            h_tot_metragem = item.h_tot_metragem,
            c_metragem = item.c_metragem,
            l_metragem = item.l_metragem,
            d_metragem = item.d_metragem,
            pessoas_montagem = item.pessoas_montagem,
            noites_montagem = item.noites_montagem,
            intervalo = item.intervalo,
            fator = item.fator,
            custo_unitario_apurado_anterior = item.custo_unitario_apurado_anterior,
            custo_estimado_anterior = item.custo_estimado_anterior,
            sub_classificacao = item.sub_classificacao,
            categoria = item.categoria,
            cat_online = item.cat_online,
            cat_offline = item.cat_offline,
            foto = item.foto,
            layout = item.layout,
            cat_preco = item.cat_preco,
            descricao_licitacao = item.descricao_licitacao
        };
    }

    private void ColarLinhaCopiada()
    {
        if (_linhaCopiada == null || DataContext is not CadastroDescricaoDimensaoViewModel vm)
            return;

        var novoItem = new ComercialPropostaDimensaoDescricaoComercialModel
        {
            coddimensao = 0,
            coddesccoml = CodDescComl,
            dimensao = _linhaCopiada.dimensao,
            dimensaofantasia = _linhaCopiada.dimensaofantasia,
            nomefantasia = _linhaCopiada.nomefantasia,
            observacao = _linhaCopiada.observacao,
            obsobrigatoria = _linhaCopiada.obsobrigatoria,
            travaled = _linhaCopiada.travaled,
            indicedimensao = _linhaCopiada.indicedimensao,
            indiceled = _linhaCopiada.indiceled,
            indicedesconto = _linhaCopiada.indicedesconto,
            indicedescontopreco = _linhaCopiada.indicedescontopreco,
            cargaeletrica = _linhaCopiada.cargaeletrica,
            areareal = _linhaCopiada.areareal,
            cubagem = _linhaCopiada.cubagem,
            peso = _linhaCopiada.peso,
            custounitarioapurado = _linhaCopiada.custounitarioapurado,
            custounitarioestimado = _linhaCopiada.custounitarioestimado,
            preco = _linhaCopiada.preco,
            valor_desconto_area_projecao = _linhaCopiada.valor_desconto_area_projecao,
            ativo = string.IsNullOrWhiteSpace(_linhaCopiada.ativo) ? "1" : _linhaCopiada.ativo,
            insumo_concluido = _linhaCopiada.insumo_concluido,
            insumo_concluido_por = _linhaCopiada.insumo_concluido_por,
            insumo_concluido_data = _linhaCopiada.insumo_concluido_data,
            concatenar = _linhaCopiada.concatenar,
            relatorio_estabilidade = _linhaCopiada.relatorio_estabilidade,
            verificador = _linhaCopiada.verificador,
            cargaeletrica_led = _linhaCopiada.cargaeletrica_led,
            aux = _linhaCopiada.aux,
            projecao_area = _linhaCopiada.projecao_area,
            custo_historico = _linhaCopiada.custo_historico,
            preco_nf = _linhaCopiada.preco_nf,
            cubagem_estimada = _linhaCopiada.cubagem_estimada,
            horas_estimadas = _linhaCopiada.horas_estimadas,
            a_metragem = _linhaCopiada.a_metragem,
            v_metragem = _linhaCopiada.v_metragem,
            h_metragem = _linhaCopiada.h_metragem,
            h_tot_metragem = _linhaCopiada.h_tot_metragem,
            c_metragem = _linhaCopiada.c_metragem,
            l_metragem = _linhaCopiada.l_metragem,
            d_metragem = _linhaCopiada.d_metragem,
            pessoas_montagem = _linhaCopiada.pessoas_montagem,
            noites_montagem = _linhaCopiada.noites_montagem,
            intervalo = _linhaCopiada.intervalo,
            fator = _linhaCopiada.fator,
            custo_unitario_apurado_anterior = _linhaCopiada.custo_unitario_apurado_anterior,
            custo_estimado_anterior = _linhaCopiada.custo_estimado_anterior,
            sub_classificacao = _linhaCopiada.sub_classificacao,
            categoria = _linhaCopiada.categoria,
            cat_online = _linhaCopiada.cat_online,
            cat_offline = _linhaCopiada.cat_offline,
            foto = _linhaCopiada.foto,
            layout = _linhaCopiada.layout,
            cat_preco = _linhaCopiada.cat_preco,
            descricao_licitacao = _linhaCopiada.descricao_licitacao
        };

        vm.DimensoesComercial.Add(novoItem);
        rgView.SelectedItem = novoItem;
        rgView.CurrentItem = novoItem;
        rgView.ScrollIntoView(novoItem);
    }

    private void CancelarOperacaoAtual()
    {
        if (rgView.SelectedItem is ComercialPropostaDimensaoDescricaoComercialModel { coddimensao: 0 } itemNovo)
        {
            if (DataContext is CadastroDescricaoDimensaoViewModel vm)
                vm.DimensoesComercial.Remove(itemNovo);

            rgView.Items.Refresh();
            return;
        }

        rgView.CancelEdit();
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
