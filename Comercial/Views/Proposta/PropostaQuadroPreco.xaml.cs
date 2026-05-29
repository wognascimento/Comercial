using Comercial.Data;
using Comercial.Data.Model;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using Comercial.Repositores;
using Comercial.Services;
using Comercial.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Syncfusion.Presentation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Comercial.Views.Proposta;

/// <summary>
/// Interação lógica para PropostaQuadroPreco.xam
/// </summary>
public partial class PropostaQuadroPreco : UserControl
{
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;
    private CancellationTokenSource _ctsCarregarDados;

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
            Loaded -= PropostaQuadroPreco_Loaded;
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
                itensProposta.IsReadOnly = false;

                this.dtInicio.SelectionChanged -= dtInicial_SelectionChanged;
                this.dtConclusao.SelectionChanged -= dtConclusao_SelectionChanged;

                this.dtInicio.SelectedValue = null;
                this.dtConclusao.SelectedValue = null;

                this.dtInicio.SelectionChanged += dtInicial_SelectionChanged;
                this.dtConclusao.SelectionChanged += dtConclusao_SelectionChanged;


                if (e.AddedItems.Count > 0 && e.AddedItems[0] is PropostaBriefingQuadroDto selectedBriefing)
                {
                    await vm.CarregarBrifinTemasAsync(selectedBriefing.codbriefing);
                    await vm.CarregarResumoCustoPropostaAsync(selectedBriefing.codbriefing);
                    vm.ItensProposta = [];
                    vm.ItemProposta = null;

                }

                LimparCampos();
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

    private async void boxBrienfingTema_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is PropostaBriefingTemaDto selectedTema)
                {
                    await vm.CarregarDetalhesLocalDetalhesLocaisAsync(vm.SelectedBriefing.codbriefing, selectedTema.idtema);
                    await vm.CarregarItensPropostaAsync(vm.SelectedBriefing.codbriefing, selectedTema.idtema);

                    this.dtInicio.SelectionChanged -= dtInicial_SelectionChanged;
                    this.dtConclusao.SelectionChanged -= dtConclusao_SelectionChanged;

                    this.dtInicio.SelectedDate = selectedTema.data_inicio_preco;
                    this.dtConclusao.SelectedDate = selectedTema.data_conclusao_preco;
                    
                    this.dtInicio.SelectionChanged += dtInicial_SelectionChanged;
                    this.dtConclusao.SelectionChanged += dtConclusao_SelectionChanged;

                    LimparCampos();


                    if ((selectedTema?.data_conclusao_preco != null) || selectedTema.ativo)
                    {
                        btnAlterar.IsEnabled = false;
                        btnIncluir.IsEnabled = false;
                        btnLimpar.IsEnabled = false;
                        btnExcluir.IsEnabled = false;
                        itensProposta.IsReadOnly = true;
                    }
                    else
                    {
                        btnAlterar.IsEnabled = true;
                        btnIncluir.IsEnabled = true;
                        btnLimpar.IsEnabled = true;
                        btnExcluir.IsEnabled = true;
                        itensProposta.IsReadOnly = false;
                    }
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

    private async void rasBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var box = (sender as RadComboBox);
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                if (box.DisplayMemberPath == "familia")
                {
                    if (e.AddedItems?.Cast<ComercialPropostaFamiliaModel>().FirstOrDefault() is ComercialPropostaFamiliaModel familia) //e.AddedItems?.Cast<ComercialPropostaFamiliaModel>().FirstOrDefault()
                    {
                        vm.DimensoesComercial = [];
                        vm.DescricoesComercial = [];
                        await vm.CarregarDescricaoAsync(familia);
                    }
                }
                else if (box.DisplayMemberPath == "descricaocomercial")
                {
                    if (e.AddedItems?.Cast<ComercialPropostaDescricaoComercialModel>().FirstOrDefault() is ComercialPropostaDescricaoComercialModel descricaoComercial) //e.AddedItems?.Cast<ComercialPropostaDescricaoComercialModel>().FirstOrDefault()
                    {
                        await vm.CarregarDimensoesAsync(descricaoComercial.coddesccoml);
                    }
                }
                else if (box.DisplayMemberPath == "dimensao")
                {
                    if (e.AddedItems?.Cast<ComercialPropostaDimensaoDescricaoComercialModel>().FirstOrDefault() is ComercialPropostaDimensaoDescricaoComercialModel dimensaoDescricaoComercial) //e.AddedItems?.Cast<ComercialPropostaDimensaoDescricaoComercialModel>().FirstOrDefault()
                    {
                        vm.DimenssaoComercial = dimensaoDescricaoComercial;
                    }
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

    private async void OnAlterarClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is PropostaQuadroPrecoViewModel vm)
            {
                bool camposValidos = await ValidarCamposAsync();
                if (!camposValidos)
                    return;

                var confirmResult = MessageBox.Show("Confirma a alteração deste item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult != MessageBoxResult.Yes)
                    return;

                // 1. Limpeza: remove QUALQUER prefixo conhecido do início (CTT, C ou E)
                string textoOriginal = txtItem.Text ?? "";
                string limpo = textoOriginal.StartsWith("CTT") ? textoOriginal.Substring(3) :
                               textoOriginal.StartsWith("C") ? textoOriginal.Substring(1) :
                               textoOriginal.StartsWith("E") ? textoOriginal.Substring(1) :
                               textoOriginal;

                // 2. Definição das variáveis de contexto
                var tipo = cbTipo.SelectedItem?.ToString();
                var tema = vm.SelectedBriefingTema?.temas;

                // 3. Aplicação da nova regra de prefixos
                var item = (tema.Contains("DECORAÇÃO EXTERNA")) ? $"E{limpo}" :
                           (tipo == "Complemento para todos os temas") ? $"CTT{limpo}" :
                           (tipo == "Complemento") ? $"C{limpo}" :
                           limpo;

                var PrecoBase = await vm.PrecoDimensaoBaseAsync(vm.DimenssaoComercial?.coddimensao ?? 0);

                await vm.AtualizarPropostaAsync(
                    new PropostaQuadroPrecoModel
                    {
                        codquadro_preco = vm.ItemProposta.codquadro_preco,
                        codbrief = vm.SelectedBriefing.codbriefing,
                        sigla = vm.SelectedBriefing.sigla,
                        tema = vm.SelectedBriefingTema.temas,
                        tipo = cbTipo.SelectedItem as string,
                        item = item, //txtItem.Text,
                        local = cbLocal.SelectedItem as string,
                        localdetalhe = txtLocalDetalhes.SearchText,
                        coddimensao = vm.DimenssaoComercial?.coddimensao,
                        qtd = double.Parse(txtQuantidade.Text),
                        obs = txtObservacao.Text,
                        obsinterna = txtObservacaoInterna.Text,
                        ledml = cbLED.SelectedItem as string,
                        //desconto = 0,
                        bloco = cbBloco.SelectedItem as string,
                        idtema = vm.SelectedBriefingTema.idtema,
                        alteradopor = BaseSettings.Username,
                        dataaltera = DateTime.Now,
                        valor_unitario = PrecoBase,
                        preco_excel = PrecoBase
                    });

                await vm.CarregarItensPropostaAsync(vm.SelectedBriefing.codbriefing, vm.SelectedBriefingTema.idtema);
                await vm.CarregarResumoCustoPropostaAsync(vm.SelectedBriefing.codbriefing);
                await vm.CarregarDetalhesLocalDetalhesLocaisAsync(vm.SelectedBriefing.codbriefing, vm.SelectedBriefingTema.idtema);
                LimparCampos();
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

    private async void OnIncluirClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is PropostaQuadroPrecoViewModel vm)
            {
                bool camposValidos = await ValidarCamposAsync();
                if (!camposValidos)
                    return;

                // 1. Limpeza: remove QUALQUER prefixo conhecido do início (CTT, C ou E)
                string textoOriginal = txtItem.Text ?? "";
                string limpo = textoOriginal.StartsWith("CTT") ? textoOriginal.Substring(3) :
                               textoOriginal.StartsWith("C") ? textoOriginal.Substring(1) :
                               textoOriginal.StartsWith("E") ? textoOriginal.Substring(1) :
                               textoOriginal;

                // 2. Definição das variáveis de contexto
                var tipo = cbTipo.SelectedItem?.ToString();
                var tema = vm.SelectedBriefingTema?.temas;

                // 3. Aplicação da nova regra de prefixos
                var item = (tema.Contains("DECORAÇÃO EXTERNA")) ? $"E{limpo}" :
                           (tipo == "Complemento para todos os temas") ? $"CTT{limpo}" :
                           (tipo == "Complemento") ? $"C{limpo}" :
                           limpo;

                var PrecoBase = await vm.PrecoDimensaoBaseAsync(vm.DimenssaoComercial?.coddimensao ?? 0);

                var codQuadroPreco = await vm.InserirItemPropostaAsync(
                    new PropostaQuadroPrecoModel
                    {
                        codbrief = vm.SelectedBriefing.codbriefing,
                        sigla = vm.SelectedBriefing.sigla,
                        tema = vm.SelectedBriefingTema.temas,
                        tipo = cbTipo.SelectedItem as string,
                        item = item, //txtItem.Text,
                        local = cbLocal.SelectedItem as string,
                        localdetalhe = txtLocalDetalhes.SearchText,
                        coddimensao = vm.DimenssaoComercial?.coddimensao,
                        qtd = double.Parse(txtQuantidade.Text),
                        obs = txtObservacao.Text,
                        obsinterna = txtObservacaoInterna.Text,
                        ledml = cbLED.SelectedItem as string,
                        desconto = 0,
                        bloco = cbBloco.SelectedItem as string,
                        idtema = vm.SelectedBriefingTema.idtema,
                        cadastradopor = BaseSettings.Username,
                        datacadastro = DateTime.Now,
                        valor_unitario = PrecoBase,
                        preco_excel = PrecoBase
                    });

                await vm.CarregarItensPropostaAsync(vm.SelectedBriefing.codbriefing, vm.SelectedBriefingTema.idtema);
                await vm.CarregarResumoCustoPropostaAsync(vm.SelectedBriefing.codbriefing);
                await vm.CarregarDetalhesLocalDetalhesLocaisAsync(vm.SelectedBriefing.codbriefing, vm.SelectedBriefingTema.idtema);

                // Assumindo que o ItemsSource do grid é uma coleção de objetos (ex.: List<MinhaEntidade>)
                var itemParaSelecionar = itensProposta.Items.Cast<QuadroPrecoDto>().FirstOrDefault(item => item.codquadro_preco == codQuadroPreco);

                if (itemParaSelecionar != null)
                {
                    // Limpa seleções anteriores (opcional)
                    itensProposta.SelectedItems.Clear();

                    // Seleciona o item encontrado
                    itensProposta.SelectedItem = itemParaSelecionar;

                    // Opcional: Rola o grid pra deixar o item visível
                    itensProposta.ScrollIntoView(itemParaSelecionar);
                }

                LimparCampos();
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

    private void OnLimparClick(object sender, RoutedEventArgs e)
    {
        LimparCampos();
    }

    private async void OnExcluirClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is PropostaQuadroPrecoViewModel vm)
            {
                var confirmResult = MessageBox.Show("Confirma a exclusão deste item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult != MessageBoxResult.Yes)
                    return;

                await vm.ExcluirItemPropostaAsync(vm.ItemProposta.codquadro_preco);
                await vm.CarregarItensPropostaAsync(vm.SelectedBriefing.codbriefing, vm.SelectedBriefingTema.idtema);
                await vm.CarregarResumoCustoPropostaAsync(vm.SelectedBriefing.codbriefing);
                await vm.CarregarDetalhesLocalDetalhesLocaisAsync(vm.SelectedBriefing.codbriefing, vm.SelectedBriefingTema.idtema);
                LimparCampos();
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

    private async void dtInicial_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                var inicioData = e.AddedItems?.Cast<object>().FirstOrDefault();
                if (inicioData == null)
                {
                    var confirmResult = MessageBox.Show("Remover a data de inicio permitirá alterações no quadro quantitativo. Deseja continuar?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirmResult != MessageBoxResult.Yes)
                    {
                        // Reverter a seleção para a data anterior
                        this.dtConclusao.SelectedValue = vm.SelectedBriefingTema.data_conclusao;
                        //this.dtConclusao.SelectedDate = vm.SelectedBriefingTema.data_conclusao;
                        return;
                    }
                    await vm.InicioProjetoAsync(null, vm.SelectedBriefingTema.codbriefing, vm.SelectedBriefingTema.idtema);
                    btnAlterar.IsEnabled = true;
                    btnIncluir.IsEnabled = true;
                    btnLimpar.IsEnabled = true;
                    btnExcluir.IsEnabled = true;
                }
                else
                {
                    var confirmResult = MessageBox.Show("Ao definir a data de inicio, o quadro quantitativo será bloqueado para alterações. Deseja continuar?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirmResult != MessageBoxResult.Yes)
                    {
                        // Reverter a seleção para a data anterior
                        this.dtConclusao.SelectedValue = vm.SelectedBriefingTema.data_conclusao;
                        return;
                    }
                    DateTime selectedDate = (DateTime)inicioData;
                    await vm.InicioProjetoAsync(selectedDate, vm.SelectedBriefingTema.codbriefing, vm.SelectedBriefingTema.idtema);
                    btnAlterar.IsEnabled = false;
                    btnIncluir.IsEnabled = false;
                    btnLimpar.IsEnabled = false;
                    btnExcluir.IsEnabled = false;
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

    private async void dtConclusao_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                var conclusaoData = e.AddedItems?.Cast<object>().FirstOrDefault();
                if (conclusaoData == null)
                {
                    var confirmResult = MessageBox.Show("Remover a data de conclusão permitirá alterações no quadro revisão. Deseja continuar?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirmResult != MessageBoxResult.Yes)
                    {
                        // Reverter a seleção para a data anterior
                        this.dtInicio.SelectedValue = vm.SelectedBriefingTema.data_inicio_preco;
                        return;
                    }
                    await vm.ConcluirProjetoAsync(null, vm.SelectedBriefingTema.codbriefing, vm.SelectedBriefingTema.idtema);
                    btnAlterar.IsEnabled = true;
                    btnIncluir.IsEnabled = true;
                    btnLimpar.IsEnabled = true;
                    btnExcluir.IsEnabled = true;
                    itensProposta.IsReadOnly = false;
                }
                else
                {
                    var confirmResult = MessageBox.Show("Ao definir data de conclusão, o quadro revisão será bloqueado para alterações. Deseja continuar?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirmResult != MessageBoxResult.Yes)
                    {
                        // Reverter a seleção para a data anterior
                        this.dtConclusao.SelectedValue = vm.SelectedBriefingTema.data_conclusao_preco;
                        return;
                    }
                    DateTime selectedDate = (DateTime)conclusaoData;
                    await vm.ConcluirProjetoAsync(selectedDate, vm.SelectedBriefingTema.codbriefing, vm.SelectedBriefingTema.idtema);
                    btnAlterar.IsEnabled = false;
                    btnIncluir.IsEnabled = false;
                    btnLimpar.IsEnabled = false;
                    btnExcluir.IsEnabled = false;
                    itensProposta.IsReadOnly = true;
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

    private async void itensProposta_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            _ctsCarregarDados?.Cancel();
            _ctsCarregarDados = new CancellationTokenSource();
            var token = _ctsCarregarDados.Token;

            if (DataContext is PropostaQuadroPrecoViewModel vm)
            {
                var selectedItem = itensProposta.SelectedItem as QuadroPrecoDto;

                if (selectedItem == null)
                    return;

                cbFamilia.SelectionChanged -= rasBoxSelectionChanged;
                cbDescricao.SelectionChanged -= rasBoxSelectionChanged;
                cbDimenssao.SelectionChanged -= rasBoxSelectionChanged;

                // Campos síncronos
                this.txtItem.Text = selectedItem.item;
                this.txtQuantidade.Text = selectedItem.qtd.ToString();
                this.cbLocal.SelectedItem = selectedItem.local;
                this.txtLocalDetalhes.SelectedItem = selectedItem.localdetalhe;
                this.cbTipo.SelectedItem = selectedItem.tipo;
                this.cbBloco.SelectedItem = selectedItem.bloco;
                this.cbFamilia.SelectedItem = vm.ComercialPropostaFamilias.FirstOrDefault(f => f.familia == selectedItem.familia);

                // Carrega descrições
                await vm.CarregarDescricaoAsync(
                    vm.ComercialPropostaFamilias.FirstOrDefault(f => f.familia == selectedItem.familia),
                    token
                );
                if (token.IsCancellationRequested) return;

                // Define descrição (use SelectedItem se possível)
                this.cbDescricao.SelectedItem = vm.DescricoesComercial.FirstOrDefault(d => d.descricaocomercial == selectedItem.descricaocomercial);

                // Carrega dimensões
                await vm.CarregarDimensoesAsync(selectedItem.coddesccoml, token);
                if (token.IsCancellationRequested) return;

                // Define dimensão (use SelectedItem)
                this.cbDimenssao.SelectedItem = vm.DimensoesComercial.FirstOrDefault(d => d.dimensao == selectedItem.dimensao);

                // Resto dos campos
                this.cbLED.SelectedItem = selectedItem.ledml;
                this.txtObservacao.Text = selectedItem.obs;
                this.txtObservacaoInterna.Text = selectedItem.obsinterna;
                this.txtObservacaoObrigatoria.Text = selectedItem.obsobrigatoria;

                cbFamilia.SelectionChanged += rasBoxSelectionChanged;
                cbDescricao.SelectionChanged += rasBoxSelectionChanged;
                cbDimenssao.SelectionChanged += rasBoxSelectionChanged;

            }
        }
        catch (OperationCanceledException) { /* Ignora */ }
        catch (RepositoryException ex)
        {
            MessageBox.Show(ex.Message, "Erro ao salvar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private object _oldValue;
    private string _coluna;
    private QuadroPrecoDto _item;

    private async void itensProposta_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            if (e.EditAction != GridViewEditAction.Commit)
                return;

            if (e.Cell?.DataContext is not QuadroPrecoDto item)
                return;

            var coluna = e.Cell.Column.UniqueName;

            if (coluna != "item" && coluna != "qtd" && coluna != "valor_unitario")
                return;

            var novoValor = e.NewData;

            // evita salvar sem mudança real
            if (Equals(novoValor, _oldValue))
                return;

            try
            {
                await vm.SalvarAsync(item, coluna, novoValor);
            }
            catch (Exception ex)
            {
                // 🔴 rollback
                if (coluna == "item")
                    item.item = (string)_oldValue;

                if (coluna == "qtd")
                    item.qtd = Convert.ToDouble(_oldValue);

                if (coluna == "valor_unitario")
                    item.valor_unitario = Convert.ToDouble(_oldValue);

                MessageBox.Show($"Erro ao salvar:\n{ex.Message}");
            }
        }

    }

    private void itensProposta_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
    {
        if (e.Cell?.DataContext is not QuadroPrecoDto item)
            return;

        _item = item;
        _coluna = e.Cell.Column.UniqueName;

        _oldValue = _coluna switch
        {
            "item" => item.item,
            "qtd" => item.qtd,
            "valor_unitario" => item.valor_unitario,
            _ => null
        };
    }

    private void RadGridView_AddingNewDataItem(object sender, Telerik.Windows.Controls.GridView.GridViewAddingNewEventArgs e)
    {
        var gridFilho = sender as RadGridView;
        if (gridFilho?.DataContext is QuadroPrecoDto itemPai)
        {
            e.NewObject = new PropostaIlustracaoModel
            {
                codpreco = itemPai.codquadro_preco,
                codquadro_quantitativo = itemPai.codquadro_quantitativo,
                item = itemPai.item,
                sigla = itemPai.sigla,
                tema = itemPai.tema,
                idtema = itemPai.idtema,
                codbriefing = itemPai.codbrief,
                inserido_por = BaseSettings.Username,
                data_pedido = DateTime.Now,
                SomenteLeitura = false,
                Origem = "PRECO"
            };
        }
    }

    private void RadGridViewIlustracaoBeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
    {
        if (e.Row?.Item is PropostaIlustracaoModel { SomenteLeitura: true })
            e.Cancel = true;
    }

    private async void RadGridViewIlustracaoRowValidating(object sender, Telerik.Windows.Controls.GridViewRowValidatingEventArgs e)
    {
        try
        {
            if (DataContext is not PropostaQuadroPrecoViewModel vm || !e.Row.IsInEditMode)
                return;

            if (e.Row.Item is PropostaIlustracaoModel i)
            {
                if (i.SomenteLeitura)
                {
                    e.IsValid = false;
                    e.ValidationResults.Add(new GridViewCellValidationResult
                    {
                        ErrorMessage = "Ilustracao criada no quadro quantitativo e somente leitura no quadro de preco.",
                        PropertyName = string.Empty
                    });
                    return;
                }

                await vm.UpserIlustracao(i);

                if (sender is RadGridView gridFilho && gridFilho.DataContext is QuadroPrecoDto itemPai)
                {
                    itemPai.ilustracao = "SIM";
                    itensProposta.Rebind();
                }
            }
        }
        catch (DbUpdateException ex)
        {
            e.IsValid = false;
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            e.IsValid = false;
            MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private Task<bool> ValidarCamposAsync()
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            if (vm.SelectedBriefing == null)
            {
                MessageBox.Show("Selecione um briefing.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (vm.SelectedBriefingTema == null)
            {
                MessageBox.Show("Selecione um tema do briefing.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (string.IsNullOrWhiteSpace(txtItem.Text))
            {
                MessageBox.Show("Informe o item.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (string.IsNullOrWhiteSpace(txtQuantidade.Text))
            {
                MessageBox.Show("Informe uma quantidade válida.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (cbLocal.SelectedItem == null)
            {
                MessageBox.Show("Selecione um local.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (cbTipo.SelectedItem == null)
            {
                MessageBox.Show("Selecione um tipo.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (cbBloco.SelectedItem == null)
            {
                MessageBox.Show("Selecione um bloco.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (cbFamilia.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma família.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (cbDescricao.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma descrição comercial.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (cbDimenssao.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma dimensão.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (!string.IsNullOrEmpty(vm.DimenssaoComercial.obsobrigatoria) && string.IsNullOrEmpty(txtObservacao.Text))
            {
                MessageBox.Show("Esta descrição requer uma observação.\nFavor informá-la.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    private void LimparCampos()
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            cbFamilia.SelectionChanged -= rasBoxSelectionChanged;
            cbDescricao.SelectionChanged -= rasBoxSelectionChanged;
            cbDimenssao.SelectionChanged -= rasBoxSelectionChanged;

            txtItem.Text = null;
            txtQuantidade.Text = null;
            cbLocal.SelectedItem = null;
            txtLocalDetalhes.SelectedItem = null;
            txtLocalDetalhes.SearchText = null;
            cbTipo.SelectedItem = null;
            cbBloco.SelectedItem = null;
            cbFamilia.SelectedItem = null;
            cbDescricao.SelectedItem = null;
            cbDimenssao.SelectedItem = null;
            cbLED.SelectedItem = null;
            txtObservacao.Text = null;
            txtObservacaoInterna.Text = null;
            txtObservacaoObrigatoria.Text = null;

            vm.DescricoesComercial = [];
            vm.DimensoesComercial = [];
            vm.DimenssaoComercial = null;

            txtItem.Focus();

            cbFamilia.SelectionChanged += rasBoxSelectionChanged;
            cbDescricao.SelectionChanged += rasBoxSelectionChanged;
            cbDimenssao.SelectionChanged += rasBoxSelectionChanged;
        }
    }

    private DocumentoWordService _docService;
    private ExcelQuadroPrecoService _excelService;
    private QuadroPrecoExportService _quadroPrecoExportService;
    private QuadroRepository _repo;

    private async void RadMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                _repo = new QuadroRepository();
                _docService = new DocumentoWordService();

                var temas = await _repo.GetTemasAsync(vm.SelectedBriefing.codbriefing);

                string template = BaseSettings.ResolveModeloPath("MODELO_PROJ_COM.docx");
                string destino = BaseSettings.ResolveImpressosPath($"{DateTime.Now:yyyy_MM_dd}_{vm.SelectedBriefing.sigla}_PROJ_COM_{BaseSettings.Username}.docx"); //2026_01_28_PIR_PROJ_COM_nina_bordenalli.doc

                await _docService.CriarDocumentoFormatado(
                    template,
                    destino,
                    temas,
                    async (idTema, tipo) =>
                        await _repo.GetItensTabelaAsync(vm.SelectedBriefing.codbriefing, idTema, tipo)
                );

                Process.Start(new ProcessStartInfo
                {
                    FileName = destino,
                    UseShellExecute = true
                });
            }
            catch (OperationCanceledException) { /* Ignora */ }
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

    private async void RadMenuItem_Click_1(object sender, Telerik.Windows.RadRoutedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                _excelService = new ExcelQuadroPrecoService();

                string caminho = BaseSettings.ResolveImpressosPath($"{DateTime.Now:yyyy_MM_dd}_{vm.SelectedBriefing.sigla}_QUADRO_DE_PREÇO_{BaseSettings.Username}.xlsx");

               await _excelService.GerarExcelCusto(caminho, vm.SelectedBriefing.codbriefing);
               Process.Start(new ProcessStartInfo
               {
                   FileName = caminho,
                   UseShellExecute = true
               });
            }
            catch (OperationCanceledException) { /* Ignora */ }
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

    private async void RadMenuItem_Click_2(object sender, Telerik.Windows.RadRoutedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                _excelService = new ExcelQuadroPrecoService();

                string caminho = BaseSettings.ResolveImpressosPath($"{DateTime.Now:yyyy_MM_dd}_{vm.SelectedBriefing.sigla}_QUADRO_DE_CUSTO_{BaseSettings.Username}.xlsx");

                await _excelService.GerarExcelCustoDetalhado(caminho, vm.SelectedBriefing.codbriefing);
                Process.Start(new ProcessStartInfo
                {
                    FileName = caminho,
                    UseShellExecute = true
                });
            }
            catch (OperationCanceledException) { /* Ignora */ }
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

    private void RadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ctsCarregarDados?.Cancel();
            _ctsCarregarDados = new CancellationTokenSource();
            var token = _ctsCarregarDados.Token;

            if (DataContext is PropostaQuadroPrecoViewModel vm)
            {
                PropostaDescricaoDimensaoDto? resultado = null;
                var meuUserControl = new BuscaDescricaoComercial();
                RadWindow radWindow = new()
                {
                    Content = meuUserControl,
                    Header = $"Buscar Descrição",
                    Width = 1000,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Application.Current.MainWindow,
                    RestrictedAreaMargin = new Thickness(0),
                    IsRestricted = false,
                    ResizeMode = ResizeMode.NoResize,
                    CanClose = true,
                    HideMinimizeButton = true,
                    HideMaximizeButton = true
                };

                // captura seleção
                meuUserControl.ItemSelecionado += async item =>
                {
                    resultado = item;

                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                    cbFamilia.SelectionChanged -= rasBoxSelectionChanged;
                    cbDescricao.SelectionChanged -= rasBoxSelectionChanged;
                    cbDimenssao.SelectionChanged -= rasBoxSelectionChanged;

                    this.cbFamilia.SelectedItem = vm.ComercialPropostaFamilias.FirstOrDefault(f => f.familia == resultado.familia);

                    // Carrega descrições
                    await vm.CarregarDescricaoAsync(
                        vm.ComercialPropostaFamilias.FirstOrDefault(f => f.familia == resultado.familia),
                        token
                    );
                    if (token.IsCancellationRequested) return;

                    // Define descrição (use SelectedItem se possível)
                    this.cbDescricao.SelectedItem = vm.DescricoesComercial.FirstOrDefault(d => d.descricaocomercial == resultado.descricaocomercial);

                    // Carrega dimensões
                    await vm.CarregarDimensoesAsync(resultado.coddesccoml, token);
                    if (token.IsCancellationRequested) return;

                    // Define dimensão (use SelectedItem)
                    this.cbDimenssao.SelectedItem = vm.DimensoesComercial.FirstOrDefault(d => d.dimensao == resultado.dimensao);

                    // Resto dos campos
                    this.txtObservacaoObrigatoria.Text = resultado.obsobrigatoria;

                    cbFamilia.SelectionChanged += rasBoxSelectionChanged;
                    cbDescricao.SelectionChanged += rasBoxSelectionChanged;
                    cbDimenssao.SelectionChanged += rasBoxSelectionChanged;

                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                };

                // Abre como modal
                radWindow.ShowDialog();
            }
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

    private async void RadMenuItem_Click_3(object sender, Telerik.Windows.RadRoutedEventArgs e)
    {
        try
        {
            if (DataContext is PropostaQuadroPrecoViewModel vm)
            {
                using var context = new NpgsqlConnection(BaseSettings.ConnectionString);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                IPresentation presentation = Syncfusion.Presentation.Presentation.Open(BaseSettings.ResolveModeloPath("MODELO-PADRAO.pptx"));
                IMasterSlide slideMaster = presentation.Masters.First(x=>x.Name.Equals("pre-proposta")); // Use o índice apropriado

                foreach (var tema in vm.PropostaBriefingTemas)
                {
                    ILayoutSlide layoutSlide = slideMaster.LayoutSlides.First(x => x.Name.Equals("inicio"));

                    ISlide slide = presentation.Slides.Add(layoutSlide);

                    IShape textBoxShapeTema = (IShape)slide.Shapes[0];
                    textBoxShapeTema.TextBody.Paragraphs.Clear();
                    IParagraph paragraph = textBoxShapeTema.TextBody.AddParagraph();
                    paragraph.AddTextPart(tema.temas);
                    //paragraph.Text = tema.temas;

                    IShape textBoxShapeShop = (IShape)slide.Shapes[1];
                    textBoxShapeShop.TextBody.Paragraphs.Clear();
                    IParagraph paragraphShop = textBoxShapeShop.TextBody.AddParagraph();
                    paragraphShop.AddTextPart(vm.SelectedBriefing.nome);
                    //paragraphShop.Text = vm.SelectedBriefing.nome;

                    await vm.CarregarItensPropostaAsync(vm.SelectedBriefing.codbriefing, tema.idtema);
                    
                    foreach (var item in vm.ItensProposta)
                    {
                        ILayoutSlide lSItem = slideMaster.LayoutSlides.First(x => x.Name.Equals("item"));
                        ISlide sItem = presentation.Slides.Add(lSItem);
                        IShape tBSItem = (IShape)sItem.Shapes[0];
                        tBSItem.TextBody.Paragraphs.Clear();
                        IParagraph pItem = tBSItem.TextBody.AddParagraph();
                        pItem.AddTextPart($@"{item.localitem} - {item.descricaocomercial}");
                        //pItem.Text = $@"{item.localitem} - {item.descricaocomercial}";
                    }
               
                    vm.ItensProposta = [];
                }
                presentation.Slides.Add(slideMaster.LayoutSlides.First(x => x.Name.Equals("informacao")));
                presentation.Slides.Add(slideMaster.LayoutSlides.First(x => x.Name.Equals("encerramento")));

                string destino = BaseSettings.ResolveImpressosPath($"ESQUELETO-PRE-PROPOSTA-{vm.SelectedBriefing.sigla}.pptx");
                presentation.Save(destino);
                presentation.Close();

                //CorrigirIdiomaPpt($@"{BaseSettings.CaminhoSistema}Impressos\ESQUELETO-PRE-PROPOSTA-{vm.SelectedBriefing.sigla}.pptx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show("APRENTAÇÃO PPT GERADA COM SUCESSO!!!");
                Process.Start("explorer", destino);
            }
        }
        catch (DbUpdateException ex)
        {
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)  // Para qualquer outro erro
        {
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void RadMenuItem_Click_4(object sender, Telerik.Windows.RadRoutedEventArgs e)
    {
        
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                MessageBoxResult confirmResult = MessageBox.Show("Confirma enviar tema para o fecha?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult != MessageBoxResult.Yes)
                    return;

                await vm.EnviarTemaFechaAsync(vm.SelectedBriefing.codbriefing, vm.SelectedBriefingTema.idtema);
                
            }

            catch (PostgresException ex)
            {
                MessageBox.Show(ex.Message, "Erro ao salvar dados", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }

    private async void RadMenuItem_Click_5(object sender, Telerik.Windows.RadRoutedEventArgs e)
    {
        if (DataContext is PropostaQuadroPrecoViewModel vm)
        {
            try
            {
                _quadroPrecoExportService = new QuadroPrecoExportService();
                

                string pasta = BaseSettings.ResolveImpressosDirectory();
                string usuario = Environment.UserName;
                string arquivo = System.IO.Path.Combine(pasta, $"{DateTime.Today:yyyy_MM_dd}_{vm.SelectedBriefing.sigla}_QUADRO_QUANT_REVISÃO_{usuario}.xlsx");

                await _quadroPrecoExportService.GerarQuadro(arquivo, vm.SelectedBriefing.sigla, vm.SelectedBriefing.codbriefing);
                Process.Start(new ProcessStartInfo
                {
                    FileName = arquivo,
                    UseShellExecute = true
                });
            }
            catch (OperationCanceledException) { /* Ignora */ }
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

    public static void CorrigirIdiomaPpt(string caminhoArquivo)
    {
        using var presentation = PresentationDocument.Open(caminhoArquivo, true);
        var presentationPart = presentation.PresentationPart;

        foreach (var slidePart in presentationPart.SlideParts)
        {
            var texts = slidePart.Slide.Descendants<Run>();

            foreach (var run in texts)
            {
                run.RunProperties ??= new RunProperties();

                run.RunProperties.Language = "pt-BR";
                run.RunProperties.Dirty = false;
            }
        }

        presentation.Save();
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
    private ObservableCollection<QuadroPrecoDto> itensProposta = [];

    [ObservableProperty]
    private QuadroPrecoDto itemProposta;

    [ObservableProperty]
    private ObservableCollection<QuadroQuantitativoResumoDto> resumosProposta = [];

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaFamiliaModel> comercialPropostaFamilias = [];

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaDescricaoComercialModel> descricoesComercial = [];

    [ObservableProperty]
    private ComercialPropostaDescricaoComercialModel descricaoComercial;

    [ObservableProperty]
    private ObservableCollection<ComercialPropostaDimensaoDescricaoComercialModel> dimensoesComercial = [];

    [ObservableProperty]
    private ComercialPropostaDimensaoDescricaoComercialModel dimenssaoComercial;

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaBlocos = [];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaLocais = [];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaTipos = ["Proposta", "Opcional", "Complemento", "Complemento para todos os temas", "Venda"];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaLeds = ["LED AZ", "LED AZ/ML", "LED BC", "LED BC/COL", "LED BC/ML", "LED BC QUENTE", "LED BC QUENTE/ML", "LED COL", "LED COL/ML"];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaDetalhesLocais = [];

    [ObservableProperty]
    private ObservableCollection<string> tipos = [
        "ANIMAÇÃO",
            "ESQUEMA VOLUMÉTRICO",
            "KIT FOTOS",
            "LAYOUT CORTE TÉCNICO",
            "LAYOUT EXISTENTE",
            "LAYOUT GENÉRICO",
            "LAYOUT PERSONALIZADO",
            "PLANTA ILUSTRADA SIMPLES",
            "PLANTA ILUSTRADA COMPLETA",
            "LAYOUT TEMA NOVO",
            "MAQUETE VIRTUAL",
            "MAQUETE",
            "PLANTA PRAÇA",
            "PLANTA TETO",
            "VÍDEO"
    ];

    public async Task CarregarBrifinsAsync()
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var itens = await conn.QueryAsync<PropostaBriefingQuadroDto>(
        @"SELECT sigla, nome, codbriefing, verbaminint, 
	           verbamaxint, verbaintdefinidapor, verbaminext, 
	           verbamaxext, verbaextdefinidapor, verbaunicadefinidapor, verba_nao_definida, 
	           moeda, verbaunica, cancelado, diretorcliente, 
	           responsavelprojeto, tot_cenografia, vlr_inicial, praca, tipo_evento
          FROM comercial.proposta_briefing_quadro
          GROUP BY sigla, nome, codbriefing, verbaminint, 
	           verbamaxint, verbaintdefinidapor, verbaminext, 
	           verbamaxext, verbaextdefinidapor, verbaunicadefinidapor, verba_nao_definida, 
	           moeda, verbaunica, cancelado, diretorcliente, 
	           responsavelprojeto, tot_cenografia, vlr_inicial, praca, tipo_evento
          ORDER BY sigla, codbriefing;");
        PropostaBriefings = new ObservableCollection<PropostaBriefingQuadroDto>(itens);
    }

    public async Task CarregarBrifinTemasAsync(long codbriefing)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { codbriefing };
        var itens = await conn.QueryAsync<PropostaBriefingTemaDto>(
        @"SELECT sigla, temas, faixapreco, indiceproposta, 
                     resp_tema, codbriefing, data_conclusao::timestamp AS data_conclusao, tot_cenografia, 
	                 data_inicio_preco::timestamp AS data_inicio_preco, data_conclusao_preco::timestamp AS data_conclusao_preco, ordem_escolha, idtema, ativo
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
                    SUM(total) as total
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

    public async Task CarregarDetalhesLocalDetalhesLocaisAsync(long codbrief, long idtema)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { codbrief, idtema };
        var itens = await conn.QueryAsync<string>(@"SELECT localdetalhe	FROM comercial.proposta_quadro_preco	WHERE codbrief = @codbrief AND idtema = @idtema GROUP BY localdetalhe ORDER BY localdetalhe;", parametros);
        ComercialPropostaDetalhesLocais = new ObservableCollection<string>(itens);
    }

    public async Task CarregarItensPropostaAsync(long codbrief, long idtema)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { codbrief, idtema };
        var sqlQuadro = @"
            SELECT
                *
            FROM comercial.view_quadro_preco
            WHERE codbrief = @codbrief AND idtema = @idtema;";
        var sqlIlustracoes = @"
            SELECT
                *
            FROM comercial.proposta_ilustracoes
            WHERE codbriefing = @codbrief AND idtema = @idtema;";
        // busca ambas as listas em paralelo
        var quadroTask = await conn.QueryAsync<QuadroPrecoDto>(sqlQuadro, parametros);
        var iluminacaoTask = await conn.QueryAsync<PropostaIlustracaoModel>(sqlIlustracoes, parametros);
        var quadroList = (quadroTask).ToList();
        var iluminacaoList = (iluminacaoTask).ToList();
        ObservableCollection<PropostaIlustracaoModel> MapearIlustracoes(QuadroPrecoDto quadro)
        {
            var ilustracoesProprias = iluminacaoList
                .Where(c => c.codpreco == quadro.codquadro_preco)
                .Select(c =>
                {
                    c.SomenteLeitura = false;
                    c.Origem = "PRECO";
                    return c;
                });

            var ilustracoesQuantitativo = iluminacaoList
                .Where(c =>
                    c.codquadro_quantitativo == quadro.codquadro_quantitativo &&
                    c.codpreco != quadro.codquadro_preco)
                .Select(c =>
                {
                    c.SomenteLeitura = true;
                    c.Origem = "QUANTITATIVO";
                    return c;
                });

            var cargasParaSigla = ilustracoesProprias
                .Concat(ilustracoesQuantitativo)
                .GroupBy(c => c.codilustracao)
                .Select(g => g.First())
                .OrderBy(c => c.codilustracao)
                .ToList();

            return new ObservableCollection<PropostaIlustracaoModel>(cargasParaSigla);
        }
        var resultado = new ObservableCollection<QuadroPrecoDto>(
            [.. quadroList.Select(q => new QuadroPrecoDto {
                    codquadro_preco = q.codquadro_preco,
                     codquadro_quantitativo = q.codquadro_quantitativo,
                     ordem = q.ordem,
                     sigla = q.sigla,
                     tipo =  q.tipo,
                     familia = q.familia,
                     item = q.item,
                     localitem = q.localitem,
                     local = q.local,
                     localdetalhe = q.localdetalhe,
                     descricao = q.descricao,
                     descricaocomercial = q.descricaocomercial,
                     nomefantasia = q.nomefantasia,
                     qtd = q.qtd,
                     qtdanterior = q.qtdanterior,
                     dimensao = q.dimensao,
                     obs = q.obs,
                     obsinterna = q.obsinterna,
                     custounitarioapurado = q.custounitarioapurado,
                     custounitarioestimado = q.custounitarioestimado,
                     custo_total = q.custo_total,
                     custo_item = q.custo_item,
                     vlr_indice = q.vlr_indice,
                     ledml = q.ledml,
                     vlr_led = q.vlr_led,
                     desconto = q.desconto,
                     custo_tot_item = q.custo_tot_item,
                     total_desc = q.total_desc,
                     codbrief = q.codbrief,
                     tema = q.tema,
                     produtocliente_cod = q.produtocliente_cod,
                     produtocliente_qtd = q.produtocliente_qtd,
                     coddesccoml = q.coddesccoml,
                     coddimensao = q.coddimensao,
                     dimensaofantasia = q.dimensaofantasia,
                     bloco = q.bloco,
                     obsobrigatoria = q.obsobrigatoria,
                     ilustracao = q.ilustracao,
                     fecha_atualiza_desc = q.fecha_atualiza_desc,
                     fecha_atualiza_dimensao = q.fecha_atualiza_dimensao,
                     fecha_atualiza_local = q.fecha_atualiza_local,
                     idtema = q.idtema,
                     cubagem = q.cubagem,
                     m3_total = q.m3_total,
                     projecao_area = q.projecao_area,
                     valor_desconto_area_projecao = q.valor_desconto_area_projecao,
                     custo_historico = q.custo_historico,
                     preco_nf = q.preco_nf,
                     custo_historico_total = q.custo_historico_total,
                     preco_nf_total = q.preco_nf_total,
                     preco_excel = q.preco_excel,
                     preco_excel_total = q.preco_excel_total,
                     valor_unitario = q.valor_unitario,
                    Ilustracoes = MapearIlustracoes(q)
                })]
        );
        ItensProposta = resultado;
    }

    public async Task CarregarDescricaoAsync(ComercialPropostaFamiliaModel familia, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var filtros = new Dictionary<string, object>
                {
                    { "id_familia", familia.id }
                };
        var descricoes = await _repo.GetWhereAsync<ComercialPropostaDescricaoComercialModel>(conn, filtros, "descricaocomercial", false);
        DescricoesComercial = new ObservableCollection<ComercialPropostaDescricaoComercialModel>(descricoes);
        // Atualiza a coleção (não recria!)
        /*DescricoesComercial.Clear();
        foreach (var desc in descricoes)
        {
            DescricoesComercial.Add(desc);
        }*/
    }

    public async Task CarregarDimensoesAsync(long coddesccoml, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var filtros = new Dictionary<string, object>
                {
                    { "coddesccoml", coddesccoml }
                };
        var dimensoes = await _repo.GetWhereAsync<ComercialPropostaDimensaoDescricaoComercialModel>(conn, filtros, "dimensao", false);
        DimensoesComercial = new ObservableCollection<ComercialPropostaDimensaoDescricaoComercialModel>(dimensoes);
        /*DimensoesComercial.Clear();
        foreach (var dim in dimensoes)
        {
            DimensoesComercial.Add(dim);
        }*/
    }

    public async Task<long> AtualizarPropostaAsync(PropostaQuadroPrecoModel model)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = @"
                UPDATE comercial.proposta_quadro_preco
                SET 
                    codbrief = @codbrief,
                    sigla = @sigla,
                    tema = @tema,
                    tipo = @tipo,
                    item = @item,
                    local = @local,
                    localdetalhe = @localdetalhe,
                    coddimensao = @coddimensao,
                    qtd = @qtd,
                    obs = @obs,
                    obsinterna = @obsinterna,
                    ledml = @ledml,
                    desconto = @desconto,
                    bloco = @bloco,
                    idtema = @idtema,
                    alteradopor = @alteradopor,
                    dataaltera = @dataaltera,
                    valor_unitario = @valor_unitario,
                    preco_excel = @preco_excel
                WHERE codquadro_preco = @codquadro_preco;
                ";
        return await conn.ExecuteAsync(sql, model);
    }

    public async Task<long> InserirItemPropostaAsync(PropostaQuadroPrecoModel model)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = @"
                INSERT INTO comercial.proposta_quadro_preco
                (codbrief, sigla, tema, tipo, item, local, localdetalhe, coddimensao,
                 qtd, obs, obsinterna, ledml, desconto, bloco, idtema, cadastradopor, datacadastro, valor_unitario, preco_excel)
                VALUES
                (@codbrief, @sigla, @tema, @tipo, @item, @local, @localdetalhe, @coddimensao,
                 @qtd, @obs, @obsinterna, @ledml, @desconto, @bloco, @idtema, @cadastradopor, @datacadastro, @valor_unitario, @preco_excel)
                RETURNING codquadro_preco;
            ";

        return await conn.ExecuteScalarAsync<long>(sql, model);
    }

    public async Task<long> ExcluirItemPropostaAsync(long codquadro_preco)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = @"DELETE FROM comercial.proposta_quadro_preco WHERE codquadro_preco = @codquadro_preco;";
        return await conn.ExecuteAsync(sql, new { codquadro_preco });
    }

    public async Task UpserIlustracao(PropostaIlustracaoModel model)
    {
        if (!model.codpreco.HasValue)
            throw new InvalidOperationException("Codigo do quadro de preco nao informado para a ilustracao.");

        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);

        var sqlSelect = @"SELECT * FROM comercial.proposta_ilustracoes WHERE codilustracao = @codilustracao";
        var existente = await conn.QueryFirstOrDefaultAsync<PropostaIlustracaoModel?>(sqlSelect, new { model.codilustracao });

        if (existente == null)
        {
            var sqlInsert = @"
                INSERT INTO comercial.proposta_ilustracoes
                    (sigla, tema, data_pedido, tipo, qtd, resp, data_conclusao, inserido_por, obs, codquadro_quantitativo, controle_pedidos, link, proposta, item, codbriefing, tipo_quadro, codpreco, cancelado, cancelado_por, cancelado_data, cancelado_obs, data_inicio, alterado_por, alterado_em, resp_layout, data_inicio_layout, data_fim_layout, obs_layout, resp_planta_layout, data_inicio_planta_layout, data_fim_planta_layout, obs_planta_layout, idtema)
                VALUES
                    (@sigla, @tema, @data_pedido, @tipo, @qtd, @resp, @data_conclusao, @inserido_por, @obs, @codquadro_quantitativo, @controle_pedidos, @link, @proposta, @item, @codbriefing, @tipo_quadro, @codpreco, @cancelado, @cancelado_por, @cancelado_data, @cancelado_obs, @data_inicio, @alterado_por, @alterado_em, @resp_layout, @data_inicio_layout, @data_fim_layout, @obs_layout, @resp_planta_layout, @data_inicio_planta_layout, @data_fim_planta_layout, @obs_planta_layout, @idtema)
                RETURNING codilustracao;";

            model.codilustracao = await conn.ExecuteScalarAsync<long>(sqlInsert, model);

            var sqlUpdateQuadro = @"UPDATE comercial.proposta_quadro_preco
                                    SET ilustracao = 'SIM'
                                    WHERE codquadro_preco = @codpreco;";
            await conn.ExecuteAsync(sqlUpdateQuadro, model);
        }
        else
        {
            var tipo = typeof(PropostaIlustracaoModel);
            var setList = new List<string>();
            var parametros = new DynamicParameters();

            foreach (var prop in tipo.GetProperties())
            {
                if (prop.Name.Equals("codilustracao", StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals(nameof(PropostaIlustracaoModel.SomenteLeitura), StringComparison.OrdinalIgnoreCase) ||
                    prop.Name.Equals(nameof(PropostaIlustracaoModel.Origem), StringComparison.OrdinalIgnoreCase))
                    continue;

                var valorNovo = prop.GetValue(model);
                var valorAntigo = prop.GetValue(existente);

                if (valorNovo == null)
                    continue;

                if (!Equals(valorNovo, valorAntigo))
                {
                    setList.Add($"{prop.Name} = @{prop.Name}");
                    parametros.Add(prop.Name, valorNovo);
                }
            }

            if (setList.Count == 0)
                return;

            parametros.Add("codilustracao", model.codilustracao);

            var sqlUpdate = $@"
                UPDATE comercial.proposta_ilustracoes
                SET {string.Join(", ", setList)}
                WHERE codilustracao = @codilustracao;";
            await conn.ExecuteAsync(sqlUpdate, parametros);
        }
    }

    public async Task<long> InicioProjetoAsync(DateTime? inicio, long briefing, long idtema)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = @"
                UPDATE comercial.propostas
                SET 
                    data_inicio_preco = @inicio
                WHERE codproposta = @briefing AND idtema = @idtema;
                ";
        return await conn.ExecuteAsync(sql, new { inicio, briefing, idtema });
    }

    public async Task<long> ConcluirProjetoAsync(DateTime? conclusao, long briefing, long idtema)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = @"
                UPDATE comercial.propostas
                SET 
                    data_conclusao_preco = @conclusao,
                    resp_conclusao_preco = @conclusaopor
                WHERE codproposta = @briefing AND idtema = @idtema;
                ";
        return await conn.ExecuteAsync(sql, new { conclusao, conclusaopor = BaseSettings.Username, briefing, idtema });
    }

    public async Task SalvarAsync(QuadroPrecoDto item, string coluna, object novoValor)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);

        string sql = coluna switch
        {
            "item" => "UPDATE comercial.proposta_quadro_preco SET item = @valor WHERE codquadro_preco = @codquadro_preco",
            "qtd" => "UPDATE comercial.proposta_quadro_preco SET qtd = @valor WHERE codquadro_preco = @codquadro_preco",
            "valor_unitario" => "UPDATE comercial.proposta_quadro_preco SET valor_unitario = @valor WHERE codbrief = @codbrief AND coddimensao = @coddimensao",
            _ => throw new Exception("Coluna inválida")
        };

        await conn.ExecuteAsync(sql, new
        {
            valor = novoValor,
            item.codquadro_preco,
            item.codbrief,
            item.coddimensao,
        });
    }

    public async Task EnviarTemaFechaAsync(long codbrief, long idtema)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        await conn.OpenAsync();

        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            var sql = @"INSERT INTO comercial.tbl_fecha_qd_quantitativo
                        (
                            codquadro_preco,
                            cod_brief,
                            sigla,
                            tema,
                            tipo,
                            item,
                            local,
                            detalhe_local,
                            coddimensao,
                            qtd,
                            obs,
                            obs_interna,
                            ledml,
                            bloco,
                            produtocliente_cod,
                            produtocliente_qtd,
                            cadastrado_por,
                            data_cadastro,
                            idtema
                        )
                        SELECT
                            p.codquadro_preco,
                            p.codbrief,
                            p.sigla,
                            p.tema,
                            p.tipo,
                            p.item,
                            p.local,
                            p.localdetalhe,
                            p.coddimensao,
                            p.qtd,
                            p.obs,
                            p.obsinterna,
                            p.ledml,
                            p.bloco,
                            p.produtocliente_cod,
                            p.produtocliente_qtd,
                            @usuario,
                            NOW(),
                            p.idtema
                        FROM comercial.proposta_quadro_preco p
                        WHERE p.codbrief = @codbrief
                          AND p.idtema = @idtema
                          AND NOT EXISTS (
                                SELECT 1
                                FROM comercial.tbl_fecha_qd_quantitativo q
                                WHERE q.codquadro_preco = p.codquadro_preco
                          );";
            var linhas = await conn.ExecuteAsync(sql, new
            {
                codbrief,
                idtema,
                usuario = BaseSettings.Username
            }, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<double> PrecoDimensaoBaseAsync(long coddimensao)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        return await conn.QueryFirstOrDefaultAsync<double>(@"SELECT preco_base FROM comercial.proposta_base_preco_zefe WHERE coddimensao = @coddimensao;", new { coddimensao });
        //PropostaBriefings = new ObservableCollection<PropostaBriefingQuadroDto>(itens);
    }

}
