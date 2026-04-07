using Comercial.Data;
using Comercial.Data.Model;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using Comercial.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
using Npgsql;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Comercial.Views.Proposta;

/// <summary>
/// Interação lógica para PropostaQuadroFecha.xam
/// </summary>
public partial class PropostaQuadroFecha : UserControl
{
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    public PropostaQuadroFecha()
    {
        InitializeComponent();
        DataContext = new PropostaQuadroFechaViewModel();
        Loaded += PropostaQuadroFecha_Loaded;
    }

    private async void PropostaQuadroFecha_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is PropostaQuadroFechaViewModel vm)
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
            Loaded -= PropostaQuadroFecha_Loaded;
        }
    }

    private async void boxBrienfing_SelectionChanged(object sender, SelectionChangeEventArgs e)
    {
        if (DataContext is PropostaQuadroFechaViewModel vm)
        {
            try
            {
                vm.PropostaFechaTemas = [];
                vm.SelectedFechaTema = null;

                vm.ItensProposta = [];

                //btnAlterar.IsEnabled = true;
                //btnIncluir.IsEnabled = true;
                //btnLimpar.IsEnabled = true;
                btnExcluir.IsEnabled = true;
                //itensProposta.IsReadOnly = false;

                this.dtConclusao.SelectionChanged -= dtConclusao_SelectionChanged;
                this.dtConclusao.SelectedValue = null;
                this.dtConclusao.SelectionChanged += dtConclusao_SelectionChanged;

                if (e.AddedItems.Count > 0 && e.AddedItems[0] is PropostaFechaSiglaDto selectedBriefing)
                {
                    await vm.CarregarBrifinTemasAsync(selectedBriefing.codbriefing);
                    await vm.CarregarAprovadosAsync(selectedBriefing.sigla);
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
        if (DataContext is PropostaQuadroFechaViewModel vm)
        {
            try
            {
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is PropostaFechaTemaDto selectedTema)
                {
                    await vm.CarregarDetalhesLocalDetalhesLocaisAsync(vm.SelectedFechaSigla.codbriefing, selectedTema.idtema);
                    await vm.CarregarItensPropostaAsync(vm.SelectedFechaSigla.codbriefing, selectedTema.idtema);

                    this.dtConclusao.SelectionChanged -= dtConclusao_SelectionChanged;
                    this.dtConclusao.SelectedDate = selectedTema.data_tema_fecha;

                    this.dtConclusao.SelectionChanged += dtConclusao_SelectionChanged;

                    LimparCampos();


                    if (selectedTema?.data_tema_fecha != null)
                    {
                        //btnAlterar.IsEnabled = false;
                        //btnIncluir.IsEnabled = false;
                        //btnLimpar.IsEnabled = false;
                        this.btnExcluir.IsEnabled = false;
                        this.dtConclusao.IsEnabled = false;
                        //itensProposta.IsReadOnly = true;
                    }
                    else
                    {
                        //btnAlterar.IsEnabled = true;
                        //btnIncluir.IsEnabled = true;
                        //btnLimpar.IsEnabled = true;
                        this.btnExcluir.IsEnabled = true;
                        this.dtConclusao.IsEnabled = true;
                        //itensProposta.IsReadOnly = false;
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

    private void RadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {

            if (DataContext is PropostaQuadroFechaViewModel vm)
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
                        vm.ComercialPropostaFamilias.FirstOrDefault(f => f.familia == resultado.familia)
                    );

                    // Define descrição (use SelectedItem se possível)
                    this.cbDescricao.SelectedItem = vm.DescricoesComercial.FirstOrDefault(d => d.descricaocomercial == resultado.descricaocomercial);

                    // Carrega dimensões
                    await vm.CarregarDimensoesAsync(resultado.coddesccoml);

                    // Define dimensão (use SelectedItem)
                    this.cbDimenssao.SelectedItem = vm.DimensoesComercial.FirstOrDefault(d => d.dimensao == resultado.dimensao);

                    // Resto dos campos

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

    private async void dtConclusao_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is PropostaQuadroFechaViewModel vm)
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
                        this.dtConclusao.SelectedValue = vm.SelectedFechaTema.data_tema_fecha;
                        //this.dtConclusao.SelectedDate = vm.SelectedBriefingTema.data_conclusao;
                        return;
                    }
                    //DateTime? conclusao, long briefing, long idtema, string resp, string strsigla
                    await vm.ConcluirProjetoAsync(null, vm.SelectedFechaTema.cod_brief, vm.SelectedFechaTema.idtema, BaseSettings.Username);
                    //btnAlterar.IsEnabled = true;
                    //btnIncluir.IsEnabled = true;
                    //btnLimpar.IsEnabled = true;
                    //btnExcluir.IsEnabled = true;
                }
                else
                {
                    var confirmResult = MessageBox.Show("Ao definir a data de inicio, o quadro quantitativo será bloqueado para alterações. Deseja continuar?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirmResult != MessageBoxResult.Yes)
                    {
                        // Reverter a seleção para a data anterior
                        this.dtConclusao.SelectedValue = vm.SelectedFechaTema.data_tema_fecha;
                        return;
                    }
                    DateTime selectedDate = (DateTime)inicioData;
                    await vm.ConcluirProjetoAsync(selectedDate, vm.SelectedFechaTema.cod_brief, vm.SelectedFechaTema.idtema, BaseSettings.Username);
                    //btnAlterar.IsEnabled = false;
                    //btnIncluir.IsEnabled = false;
                    //btnLimpar.IsEnabled = false;
                    //btnExcluir.IsEnabled = false;
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
            //_ctsCarregarDados?.Cancel();
            //_ctsCarregarDados = new CancellationTokenSource();
            //var token = _ctsCarregarDados.Token;

            if (DataContext is PropostaQuadroFechaViewModel vm)
            {
                var selectedItem = itensProposta.SelectedItem as PropostaFechaViewDto;

                if (selectedItem == null)
                    return;

                cbFamilia.SelectionChanged -= rasBoxSelectionChanged;
                cbDescricao.SelectionChanged -= rasBoxSelectionChanged;
                cbDimenssao.SelectionChanged -= rasBoxSelectionChanged;

                // Campos síncronos
                this.txtItem.Text = selectedItem.item;
                this.txtQuantidade.Text = selectedItem.qtd.ToString();
                this.cbLocal.SelectedItem = selectedItem.local;
                this.txtLocalDetalhes.SelectedItem = selectedItem.detalhe_local;
                this.cbTipo.SelectedItem = selectedItem.tipo;
                this.cbBloco.SelectedItem = selectedItem.bloco;
                this.cbFamilia.SelectedItem = vm.ComercialPropostaFamilias.FirstOrDefault(f => f.familia == selectedItem.familia);

                // Carrega descrições
                await vm.CarregarDescricaoAsync(
                    vm.ComercialPropostaFamilias.FirstOrDefault(f => f.familia == selectedItem.familia)
                );

                // Define descrição (use SelectedItem se possível)
                this.cbDescricao.SelectedItem = vm.DescricoesComercial.FirstOrDefault(d => d.descricaocomercial == selectedItem.descricao); //descricaocomercial

                // Carrega dimensões
                await vm.CarregarDimensoesAsync(selectedItem.coddesccoml);

                // Define dimensão (use SelectedItem)
                this.cbDimenssao.SelectedItem = vm.DimensoesComercial.FirstOrDefault(d => d.dimensao == selectedItem.dimensao);

                // Resto dos campos
                this.cbLED.SelectedItem = selectedItem.ledml;
                this.txtObservacao.Text = selectedItem.obs;
                this.txtObservacaoInterna.Text = selectedItem.obs_interna;
                this.txtObservacaoAlteracao.Text = selectedItem.obs_memorial;

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
    private PropostaFechaViewDto _item;


    private void itensProposta_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
    {
        if (e.Cell?.DataContext is not PropostaFechaViewDto item)
            return;

        _item = item;
        _coluna = e.Cell.Column.UniqueName;

        _oldValue = _coluna switch
        {
            "item" => item.item,
            "qtd" => item.qtd,
            "id_aprovado" => item.id_aprovado,
            _ => null
        };
    }

    private async void itensProposta_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
    {
        if (DataContext is PropostaQuadroFechaViewModel vm)
        {
            if (e.EditAction != GridViewEditAction.Commit)
                return;

            if (e.Cell?.DataContext is not PropostaFechaViewDto item)
                return;

            var coluna = e.Cell.Column.UniqueName;

            if (coluna != "item" && coluna != "qtd" && coluna != "id_aprovado")
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

                if (coluna == "id_aprovado")
                    item.id_aprovado = Convert.ToInt64(_oldValue); 

                MessageBox.Show($"Erro ao salvar:\n{ex.Message}");
            }
        }

    }

    private async void rasBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var box = (sender as RadComboBox);
        if (DataContext is PropostaQuadroFechaViewModel vm)
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
            if (DataContext is PropostaQuadroFechaViewModel vm)
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
                var tema = vm.SelectedFechaTema?.tema;

                // 3. Aplicação da nova regra de prefixos
                var item = (tema.Contains("DECORAÇÃO EXTERNA")) ? $"E{limpo}" :
                           (tipo == "Complemento para todos os temas") ? $"CTT{limpo}" :
                           (tipo == "Complemento") ? $"C{limpo}" :
                           limpo;

                await vm.AtualizarPropostaAsync(
                    new PropostaQuadroFechaModel
                    {
                        cod_linha_qdfecha = vm.ItemProposta.cod_linha_qdfecha,
                        tipo = cbTipo.SelectedItem as string,
                        coddimensao = vm.DimenssaoComercial.coddimensao,
                        local = cbLocal.SelectedItem as string,
                        detalhe_local = txtLocalDetalhes.SearchText,
                        item = item,
                        qtd = double.Parse(txtQuantidade.Text),
                        obs = txtObservacao.Text,
                        alterado_por = dtConclusao.SelectedDate == null ? null : BaseSettings.Username,
                        data_altera = dtConclusao.SelectedDate == null ? null : DateTime.Now,
                        ledml = cbLED.SelectedItem as string,
                        obs_interna = txtObservacaoInterna.Text,
                        bloco = cbBloco.SelectedItem as string,
                        obs_memorial = txtObservacaoAlteracao.Text,
                        idtema = vm.SelectedFechaTema.idtema,
                        //item_novo = IIf(cmb_item_novo = "" Or IsNull(cmb_item_novo), "", cmb_item_novo)
                    });

                await vm.CarregarItensPropostaAsync(vm.SelectedFechaSigla.codbriefing, vm.SelectedFechaTema.idtema);
                await vm.CarregarDetalhesLocalDetalhesLocaisAsync(vm.SelectedFechaSigla.codbriefing, vm.SelectedFechaTema.idtema);
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
            if (DataContext is PropostaQuadroFechaViewModel vm)
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
                var tema = vm.SelectedFechaTema?.tema;

                // 3. Aplicação da nova regra de prefixos
                var item = (tema.Contains("DECORAÇÃO EXTERNA")) ? $"E{limpo}" :
                           (tipo == "Complemento para todos os temas") ? $"CTT{limpo}" :
                           (tipo == "Complemento") ? $"C{limpo}" :
                           limpo;

                var codQuadroPreco = await vm.InserirItemPropostaAsync(
                    new PropostaQuadroFechaModel
                    {
                        sigla = vm.SelectedFechaSigla.sigla,
                        tema = vm.SelectedFechaTema.tema,
                        tipo = cbTipo.SelectedItem as string,
                        coddimensao = vm.DimenssaoComercial.coddimensao,
                        local = cbLocal.SelectedItem as string,
                        item = item,
                        qtd = double.Parse(txtQuantidade.Text),
                        obs = txtObservacao.Text,
                        cadastrado_por = BaseSettings.Username,
                        data_cadastro = DateTime.Now,
                        detalhe_local = txtLocalDetalhes.SearchText,
                        ledml = cbLED.SelectedItem as string,
                        cod_brief = vm.SelectedFechaSigla.codbriefing,
                        obs_interna = txtObservacaoInterna.Text,
                        bloco = cbBloco.SelectedItem as string,
                        obs_memorial = txtObservacaoAlteracao.Text,
                        ok = vm.DescricaoComercial.descricaocomercial == "Operador" || vm.DescricaoComercial.descricaocomercial == "Terceirizado" ? "-1" : "0",
                        idtema = vm.SelectedFechaTema.idtema

                    });

                await vm.CarregarItensPropostaAsync(vm.SelectedFechaSigla.codbriefing, vm.SelectedFechaTema.idtema);
                await vm.CarregarDetalhesLocalDetalhesLocaisAsync(vm.SelectedFechaSigla.codbriefing, vm.SelectedFechaTema.idtema);

                var itemParaSelecionar = itensProposta.Items.Cast<PropostaFechaViewDto>().FirstOrDefault(item => item.cod_linha_qdfecha == codQuadroPreco);

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
            if (DataContext is PropostaQuadroFechaViewModel vm)
            {
                var confirmResult = MessageBox.Show("Confirma a exclusão deste item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult != MessageBoxResult.Yes)
                    return;

                await vm.ExcluirItemPropostaAsync(vm.ItemProposta.cod_linha_qdfecha);
                await vm.CarregarItensPropostaAsync(vm.SelectedFechaSigla.codbriefing, vm.SelectedFechaTema.idtema);
                await vm.CarregarDetalhesLocalDetalhesLocaisAsync(vm.SelectedFechaSigla.codbriefing, vm.SelectedFechaTema.idtema);
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


    private async void OnDestravaDataClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is PropostaQuadroFechaViewModel vm)
            {
                var confirmResult = MessageBox.Show("Confirma o destravamento da data de conclusão deste tema? Isso permitirá alterações no quadro fecha.", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult != MessageBoxResult.Yes)
                    return;
                var permitido = await vm.DestravaDataAsync(BaseSettings.Username);
                if (permitido)
                {
                    MessageBox.Show("Data destravada com sucesso. O quadro fecha agora pode ser editado.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.dtConclusao.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Não foi possível destravar a data. Verifique se você tem permissão para realizar esta ação.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

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

    private Task<bool> ValidarCamposAsync()
    {
        if (DataContext is PropostaQuadroFechaViewModel vm)
        {
            if (vm.SelectedFechaSigla == null)
            {
                MessageBox.Show("Selecione um briefing.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }
            else if (vm.SelectedFechaTema == null)
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
        if (DataContext is PropostaQuadroFechaViewModel vm)
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
            txtObservacaoAlteracao.Text = null;

            vm.DescricoesComercial = [];
            vm.DimensoesComercial = [];
            vm.DimenssaoComercial = null;

            txtItem.Focus();

            cbFamilia.SelectionChanged += rasBoxSelectionChanged;
            cbDescricao.SelectionChanged += rasBoxSelectionChanged;
            cbDimenssao.SelectionChanged += rasBoxSelectionChanged;
        }
    }

}

public partial class PropostaQuadroFechaViewModel : ObservableObject
{
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;


    [ObservableProperty]
    private ObservableCollection<PropostaFechaSiglaDto> propostaFechaSiglas = [];

    [ObservableProperty]
    private PropostaFechaSiglaDto selectedFechaSigla;

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
    private ObservableCollection<PropostaFechaTemaDto> propostaFechaTemas = [];

    [ObservableProperty]
    private ObservableCollection<ProducaoAprovadoModel> producaoAprovados = [];

    [ObservableProperty]
    private ProducaoAprovadoModel producaoAprovado;

    [ObservableProperty]
    private PropostaFechaTemaDto selectedFechaTema;

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaDetalhesLocais = [];

    [ObservableProperty]
    private ObservableCollection<PropostaFechaViewDto> itensProposta = [];

    [ObservableProperty]
    private PropostaFechaViewDto itemProposta;

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaTipos = ["Proposta", "Opcional", "Complemento", "Complemento para todos os temas", "Venda"];

    [ObservableProperty]
    private ObservableCollection<string> comercialPropostaLeds = ["LED AZ", "LED AZ/ML", "LED BC", "LED BC/COL", "LED BC/ML", "LED BC QUENTE", "LED BC QUENTE/ML", "LED COL", "LED COL/ML"];


    public async Task CarregarBrifinsAsync()
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var itens = await conn.QueryAsync<PropostaFechaSiglaDto>(
        @"SELECT 
	        sigla, 
	        nome, 
	        codbriefing, 
	        diretorcliente, 
	        responsavelprojeto, 
	        praca, 
	        tipo_evento 
        FROM comercial.proposta_fecha_siglas;");
        PropostaFechaSiglas = new ObservableCollection<PropostaFechaSiglaDto>(itens);
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

    public async Task CarregarBrifinTemasAsync(long codbriefing)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { codbriefing };
        var itens = await conn.QueryAsync<PropostaFechaTemaDto>(
        @"SELECT cod_brief, ordem_escolha, tema, faixapreco, resp_conclusao_preco, data_tema_fecha, indice, resp_tema, texto, idtema
	      FROM comercial.proposta_fecha_tema
          WHERE cod_brief = @codbriefing
          ORDER BY ordem_escolha;", parametros);
        PropostaFechaTemas = new ObservableCollection<PropostaFechaTemaDto>(itens);
    }

    public async Task CarregarAprovadosAsync(string Sigla)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { Sigla };
        var itens = await conn.QueryAsync<ProducaoAprovadoModel>(
        @"SELECT sigla_serv, id_aprovado
          FROM producao.t_aprovados
          WHERE sigla = @Sigla
          ORDER BY sigla_serv;", parametros);
        ProducaoAprovados = new ObservableCollection<ProducaoAprovadoModel>(itens);
    }

    public async Task CarregarDetalhesLocalDetalhesLocaisAsync(long codbrief, long idtema)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { codbrief, idtema };
        var itens = await conn.QueryAsync<string>(@"SELECT detalhe_local FROM comercial.tbl_fecha_qd_quantitativo WHERE cod_brief = @codbrief AND idtema = @idtema GROUP BY detalhe_local ORDER BY detalhe_local;", parametros);
        ComercialPropostaDetalhesLocais = new ObservableCollection<string>(itens);
    }

    public async Task CarregarItensPropostaAsync(long codbrief, long idtema)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { codbrief, idtema };
        var sqlQuadro = @"
            SELECT
                *
            FROM comercial.proposta_view_fecha
            WHERE cod_brief = @codbrief AND idtema = @idtema;";

        // busca ambas as listas em paralelo
        var itens = await conn.QueryAsync<PropostaFechaViewDto>(sqlQuadro, parametros);
        ItensProposta = new ObservableCollection<PropostaFechaViewDto>(itens);
    }

    public async Task<long> ConcluirProjetoAsync(DateTime? conclusao, long briefing, long idtema, string resp)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);

        // Se 'conclusao' for nulo, deleta. Se tiver valor, insere (e ignora ou atualiza se já existir).
        var sql = @"
        IF @conclusao IS NULL THEN
            DELETE FROM comercial.tbl_status_fecha 
            WHERE codbrief = @briefing AND idtema = @idtema;
        ELSE
            INSERT INTO comercial.tbl_status_fecha (codbrief, idtema, resp_tema_fecha, data_tema_fecha)
            VALUES (@briefing, @idtema, @resp, @conclusao)
            ON CONFLICT (codbrief, idtema) DO UPDATE 
            SET resp_tema_fecha = EXCLUDED.resp_tema_fecha, 
                data_tema_fecha = EXCLUDED.data_tema_fecha;
        END IF;";

        // Nota: Para usar blocos IF simples assim, o PostgreSQL exige que estejam dentro de uma FUNCTION ou bloco anônimo.
        // Uma alternativa direta e compatível com Dapper sem criar funções é:

        var sqlDireto = @"
        DELETE FROM comercial.tbl_status_fecha WHERE codbrief = @briefing AND idtema = @idtema;
        
        INSERT INTO comercial.tbl_status_fecha (codbrief, idtema, resp_tema_fecha, data_tema_fecha)
        SELECT @briefing, @idtema, @resp, @conclusao
        WHERE @conclusao IS NOT NULL
        ON CONFLICT (codbrief, idtema) DO UPDATE 
        SET resp_tema_fecha = EXCLUDED.resp_tema_fecha, data_tema_fecha = EXCLUDED.data_tema_fecha;";

        return await conn.ExecuteAsync(sqlDireto, new
        {
            briefing,
            idtema,
            resp,
            conclusao
        });
    }

    public async Task CarregarDescricaoAsync(ComercialPropostaFamiliaModel familia, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var filtros = new Dictionary<string, object>
        {
            { "id_familia", familia.id }
        };

        var sql = @"SELECT *
                    FROM comercial.proposta_descricaocomercial
                    WHERE id_familia = @id_familia
                    ORDER BY descricaocomercial;";
        var itens = await conn.QueryAsync<ComercialPropostaDescricaoComercialModel>(sql, filtros);
        DescricoesComercial = new ObservableCollection<ComercialPropostaDescricaoComercialModel>(itens);
    }

    public async Task CarregarDimensoesAsync(long coddesccoml, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var filtros = new Dictionary<string, object>
        {
            { "coddesccoml", coddesccoml }
        };
        var sql = @"SELECT *
                    FROM comercial.proposta_dimensaodescricaocomercial
                    WHERE coddesccoml = @coddesccoml
                    ORDER BY dimensao;";
        var itens = await conn.QueryAsync<ComercialPropostaDimensaoDescricaoComercialModel>(sql, filtros);
        DimensoesComercial = new ObservableCollection<ComercialPropostaDimensaoDescricaoComercialModel>(itens);
    }

    public async Task SalvarAsync(PropostaFechaViewDto item, string coluna, object novoValor)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);

        string sql = coluna switch
        {
            "item" => "UPDATE comercial.tbl_fecha_qd_quantitativo SET item = @valor WHERE cod_linha_qdfecha = @cod_linha_qdfecha",
            "qtd" => "UPDATE comercial.tbl_fecha_qd_quantitativo SET qtd = @valor WHERE cod_linha_qdfecha = @cod_linha_qdfecha",
            "id_aprovado" => "UPDATE comercial.tbl_fecha_qd_quantitativo SET id_aprovado = @valor WHERE cod_linha_qdfecha = @cod_linha_qdfecha",
            _ => throw new Exception("Coluna inválida")
        };

        await conn.ExecuteAsync(sql, new
        {
            valor = novoValor,
            item.cod_linha_qdfecha
        });
    }

    public async Task<long> AtualizarPropostaAsync(PropostaQuadroFechaModel model)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = @"
                UPDATE comercial.tbl_fecha_qd_quantitativo
                SET 
                    tipo = @tipo
                    coddimensao = @coddimensao
                    local = @local
                    item = @item
                    qtd = @qtd
                    obs = @obs
                    alterado_por = @alterado_por
                    data_altera = @data_altera
                    detalhe_local = @detalhe_local
                    ledml = @ledml
                    obs_interna = @obs_interna
                    bloco = @bloco
                    obs_memorial = @obs_memorial
                    idtema = @idtema
                    item_novo = @item_novo
                WHERE cod_linha_qdfecha = @cod_linha_qdfecha;
                ";
        return await conn.ExecuteAsync(sql, model);
    }

    public async Task<long> InserirItemPropostaAsync(PropostaQuadroFechaModel model)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = @"
                INSERT INTO comercial.tbl_fecha_qd_quantitativo
                (sigla, Tema, tipo, coddimensao, local, Item, qtd, Obs, cadastrado_por, data_cadastro, detalhe_local, ledml, cod_brief, obs_interna, bloco, obs_memorial, ok, idtema, item_novo)
                VALUES
                (@sigla, @tema, @tipo, @coddimensao, @local, @item, @qtd, @obs, @cadastrado_por, @data_cadastro, @detalhe_local, @ledml, @cod_brief, @obs_interna, @bloco, @obs_memorial, @ok, @idtema, @item_novo)
                RETURNING cod_linha_qdfecha;
            ";

        return await conn.ExecuteScalarAsync<long>(sql, model);
    }

    public async Task<long> ExcluirItemPropostaAsync(long cod_linha_qdfecha)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = @"DELETE FROM comercial.tbl_fecha_qd_quantitativo WHERE cod_linha_qdfecha = @cod_linha_qdfecha;";
        return await conn.ExecuteAsync(sql, new { cod_linha_qdfecha });
    }

    public async Task<bool> DestravaDataAsync(string Usuario)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var sql = "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM comercial.tbl_destrava_quadro_fecha WHERE usuario = @Usuario) THEN 1 ELSE 0 END AS BIT)";
        return await conn.ExecuteScalarAsync<bool>(sql, new { Usuario });
    }

}
