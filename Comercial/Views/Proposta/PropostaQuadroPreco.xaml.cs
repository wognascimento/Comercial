using Comercial.Data;
using Comercial.Data.Model;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using Comercial.Repositores;
using Comercial.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
using Npgsql;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

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
                    }
                    else
                    {
                        btnAlterar.IsEnabled = true;
                        btnIncluir.IsEnabled = true;
                        btnLimpar.IsEnabled = true;
                        btnExcluir.IsEnabled = true;
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

                await vm.AtualizarPropostaAsync(
                    new PropostaQuadroPrecoModel
                    {
                        codquadro_preco = vm.ItemProposta.codquadro_preco,
                        codbrief = vm.SelectedBriefing.codbriefing,
                        sigla = vm.SelectedBriefing.sigla,
                        tema = vm.SelectedBriefingTema.temas,
                        tipo = cbTipo.SelectedItem as string,
                        item = txtItem.Text,
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
                        cadastradopor = BaseSettings.Username,
                        datacadastro = DateTime.Now
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

                var codQuadroPreco = await vm.InserirItemPropostaAsync(
                    new PropostaQuadroPrecoModel
                    {
                        codbrief = vm.SelectedBriefing.codbriefing,
                        sigla = vm.SelectedBriefing.sigla,
                        tema = vm.SelectedBriefingTema.temas,
                        tipo = cbTipo.SelectedItem as string,
                        item = txtItem.Text,
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
                        datacadastro = DateTime.Now
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

                await vm.ExcluirItemPropostaAsync(vm.ItemProposta.codquadro_quantitativo);
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

    private void RadGridView_AddingNewDataItem(object sender, Telerik.Windows.Controls.GridView.GridViewAddingNewEventArgs e)
    {

    }

    private void RadGridViewIlustracaoRowValidating(object sender, Telerik.Windows.Controls.GridViewRowValidatingEventArgs e)
    {

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

                string template = $@"{BaseSettings.CaminhoSistema}Modelos\MODELO_PROJ_COM.docx";
                string destino = $@"{BaseSettings.CaminhoSistema}Impressos\{DateTime.Now:yyyy_MM_dd}_{vm.SelectedBriefing.sigla}_PROJ_COM_{BaseSettings.Username}.docx"; //2026_01_28_PIR_PROJ_COM_nina_bordenalli.doc

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

                string caminho = $@"{BaseSettings.CaminhoSistema}Impressos\{DateTime.Now:yyyy_MM_dd}_{vm.SelectedBriefing.sigla}_QUADRO_DE_PREÇO_{BaseSettings.Username}.xlsx";

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
        ObservableCollection<PropostaIlustracaoModel> MapearIlustracoes(long codQuadroQuantitativo)
        {
            var cargasParaSigla = iluminacaoList
                .Where(c => c.codquadro_quantitativo == codQuadroQuantitativo)
                .OrderBy(c => c.codilustracao)
                .ToList();
            return new ObservableCollection<PropostaIlustracaoModel>(cargasParaSigla);
        }
        var resultado = new ObservableCollection<QuadroPrecoDto>(
            [.. quadroList.Select(q => new QuadroPrecoDto {
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
                    Ilustracoes = MapearIlustracoes(q.codquadro_quantitativo)
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
                    idtema = @idtema
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
                 qtd, obs, obsinterna, ledml, desconto, bloco, idtema, cadastradopor, datacadastro)
                VALUES
                (@codbrief, @sigla, @tema, @tipo, @item, @local, @localdetalhe, @coddimensao,
                 @qtd, @obs, @obsinterna, @ledml, @desconto, @bloco, @idtema, @cadastradopor, @datacadastro)
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

}
