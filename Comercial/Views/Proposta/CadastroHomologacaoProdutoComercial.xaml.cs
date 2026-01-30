using Comercial.Data;
using Comercial.Data.Model;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
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
/// Interação lógica para CadastroHomologacaoProdutoComercial.xam
/// </summary>
public partial class CadastroHomologacaoProdutoComercial : UserControl
{
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    public CadastroHomologacaoProdutoComercial()
    {
        InitializeComponent();
        DataContext = new CadastroHomologacaoProdutoComercialViewModel();
        Loaded += CadastroHomologacaoProdutoComercial_Loaded;

        this.rasBoxDescricao.ClearButtonCommand = new DelegateCommand(execute: obj => OnClearExecuted(this.rasBoxDescricao));
        this.rasBoxDimenssao.ClearButtonCommand = new DelegateCommand(execute: obj => OnClearExecuted(this.rasBoxDimenssao));

        //this.rasBoxPlanilha.ClearButtonCommand = new DelegateCommand(execute: obj => OnClearExecuted(this.rasBoxPlanilha));
        //this.rasBoxDescricaoProduto.ClearButtonCommand = new DelegateCommand(execute: obj => OnClearExecuted(this.rasBoxDescricaoProduto));
        //this.rasBoxDescricaoAdicional.ClearButtonCommand = new DelegateCommand(execute: obj => OnClearExecuted(this.rasBoxDescricaoAdicional));
        //this.rasBoxComplementoAdicional.ClearButtonCommand = new DelegateCommand(execute: obj => OnClearExecuted(this.rasBoxComplementoAdicional));
    }

    private void OnClearExecuted(RadAutoSuggestBox suggestBox)
    {
        /*
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
        else if (suggestBox.Name == "rasBoxPlanilha")
        {
            var vm = suggestBox.DataContext as CadastroHomologacaoProdutoComercialViewModel;
            this.rasBoxDescricaoProduto.Text = string.Empty;
            this.rasBoxDescricaoAdicional.Text = string.Empty;
            this.rasBoxComplementoAdicional.Text = string.Empty;
            this.txtUnidade.Text = string.Empty;
            vm.Descricoes = [];
            vm.DescricaoAdicionais = [];
            vm.ComplementoAdicionais = [];
            vm.Planilha = null;
            vm.Descricao = null;
            vm.DescricaoAdicional = null;
            vm.ComplementoAdicional = null;
        }
        else if (suggestBox.Name == "rasBoxDescricaoProduto")
        {
            var vm = suggestBox.DataContext as CadastroHomologacaoProdutoComercialViewModel;
            this.rasBoxDescricaoAdicional.Text = string.Empty;
            this.rasBoxComplementoAdicional.Text = string.Empty;
            this.txtUnidade.Text = string.Empty;
            vm.DescricaoAdicionais = [];
            vm.ComplementoAdicionais = [];
            vm.Descricao = null;
            vm.DescricaoAdicional = null;
            vm.ComplementoAdicional = null;
        }
        else if (suggestBox.Name == "rasBoxDescricaoAdicional")
        {
            var vm = suggestBox.DataContext as CadastroHomologacaoProdutoComercialViewModel;
            this.rasBoxComplementoAdicional.Text = string.Empty;
            this.txtUnidade.Text = string.Empty;
            vm.ComplementoAdicionais = [];
            vm.DescricaoAdicional = null;
            vm.ComplementoAdicional = null;
        }
        else if (suggestBox.Name == "rasBoxComplementoAdicional")
        {
            this.txtUnidade.Text = string.Empty;
        }
        */
    }

    private async void CadastroHomologacaoProdutoComercial_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
        {
            await vm.CarregarFamiliaAsync();
            await vm.CarregarPlanilhasAsync();
        }
    }

    private async void rcBoxFamilia_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var familia = e.AddedItems[0] as ComercialPropostaFamiliaModel;

        if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
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

    private void rasBoxDescricao_TextChanged(object sender, Telerik.Windows.Controls.AutoSuggestBox.TextChangedEventArgs e)
    {
        if (e.Reason == TextChangeReason.UserInput)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                this.rasBoxDescricao.ItemsSource = vm.GetByText(vm.DescricoesComercial, x => x.descricaocomercial, this.rasBoxDescricao.Text);
                vm.DimenssaoComercial = null;
            }
        }
    }

    private async void rasBoxDescricao_SuggestionChosen(object sender, Telerik.Windows.Controls.AutoSuggestBox.SuggestionChosenEventArgs e)
    {
        if (e.Suggestion is ComercialPropostaDescricaoComercialModel descricao)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                await vm.CarregarDimensoesAsync(descricao.coddesccoml);
                rasBoxDimenssao.IsDropDownOpen = true;
                rasBoxDimenssao.Focus();
                vm.DescricaoComercial = descricao;
                vm.ItensHomologados = [];
            }
        }
    }

    private void rasBoxDimenssao_TextChanged(object sender, Telerik.Windows.Controls.AutoSuggestBox.TextChangedEventArgs e)
    {
        if (e.Reason == TextChangeReason.UserInput)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
                this.rasBoxDimenssao.ItemsSource = vm.GetByText(vm.DimensoesComercial, x => x.dimensao, this.rasBoxDimenssao.Text); //vm.GetDescricaoByText(this.rasBoxDescricao.Text);
        }
    }

    private async void rasBoxDimenssao_SuggestionChosen(object sender, Telerik.Windows.Controls.AutoSuggestBox.SuggestionChosenEventArgs e)
    {
        if (e.Suggestion is ComercialPropostaDimensaoDescricaoComercialModel dimensao)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                rgViewHomologacao.IsBusy = true;
                await vm.CarregarItensHomologacao(dimensao.coddimensao);
                vm.DimenssaoComercial = dimensao;
                txtcustoapurado.Text = dimensao?.custounitarioapurado?.ToString();
                txtcustoestimado.Text = dimensao?.custounitarioestimado?.ToString();
                txtindicedimensoa.Text = dimensao?.indicedimensao?.ToString();
                txtindiceled.Text = dimensao?.indiceled?.ToString();

                concluido.IsChecked = dimensao.insumo_concluido.Contains('1') ? true : false;
                concluido_por.Text = dimensao?.insumo_concluido_por?.ToString();
                concluido_data.DateTimeText = dimensao?.insumo_concluido_data?.ToString("dd/MM/yyyy");
                if(dimensao.insumo_concluido.Contains('1'))
                {
                    concluido.IsEnabled = false;
                    btnIncluir.IsEnabled = false;
                    btnAlterar.IsEnabled = false;
                    btnCopiar.IsEnabled = false;
                }
                else
                {
                    concluido.IsEnabled = true;
                    btnIncluir.IsEnabled = true;
                    btnAlterar.IsEnabled = true;
                    btnCopiar.IsEnabled = true;
                }


                custo_historico.Text = dimensao?.custo_historico?.ToString();
                preco_nf.Text = dimensao?.preco_nf?.ToString();

                rgViewHomologacao.IsBusy = false;
                txtOrdem.Focus();
            }
        }
    }

    private void rasBoxPlanilha_TextChanged(object sender, Telerik.Windows.Controls.AutoSuggestBox.TextChangedEventArgs e)
    {
        /*
        if (e.Reason == TextChangeReason.UserInput)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                this.rasBoxPlanilha.ItemsSource = vm.GetByText(vm.Planilhas, x => x.planilha, this.rasBoxPlanilha.Text);
                rasBoxDescricaoProduto.Text = null;
                rasBoxDescricaoAdicional.Text = null;
                rasBoxComplementoAdicional.Text = null;
                txtUnidade.Text = null;

                vm.Descricoes = [];
                vm.DescricaoAdicionais = [];
                vm.ComplementoAdicionais = [];

                vm.Planilha = null;
                vm.Descricao = null;
                vm.DescricaoAdicional = null;
                vm.ComplementoAdicional = null;
            }
        }
        */

        
    }

    private async void rasBoxPlanilha_SuggestionChosen(object sender, SuggestionChosenEventArgs e)
    {
        /*
        if (e.Suggestion is ProducaoRelPlanModel planilha)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                this.rasBoxDescricaoProduto.Text = string.Empty;
                this.rasBoxDescricaoAdicional.Text = string.Empty;
                this.rasBoxComplementoAdicional.Text = string.Empty;
                this.txtUnidade.Text = string.Empty;
                vm.Descricoes = [];
                vm.DescricaoAdicionais = [];
                vm.ComplementoAdicionais = [];
                vm.Planilha = null;
                vm.Descricao = null;
                vm.DescricaoAdicional = null;
                vm.ComplementoAdicional = null;

                await vm.CarregarDescricoesAsync(planilha.planilha);
                rasBoxDescricaoProduto.Focus();
                rasBoxDescricaoProduto.IsDropDownOpen = true;
                vm.Planilha = planilha;
            }
        }
        */
    }

    private void rasBoxDescricaoProduto_TextChanged(object sender, Telerik.Windows.Controls.AutoSuggestBox.TextChangedEventArgs e)
    {
        /*
        if (e.Reason == TextChangeReason.UserInput)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                this.rasBoxDescricaoProduto.ItemsSource = vm.GetByText(vm.Descricoes, x => x.descricao, this.rasBoxDescricaoProduto.Text);
                rasBoxDescricaoAdicional.Text = null;
                rasBoxComplementoAdicional.Text = null;
                txtUnidade.Text = null;

                vm.DescricaoAdicionais = [];
                vm.ComplementoAdicionais = [];

                vm.Descricao = null;
                vm.DescricaoAdicional = null;
                vm.ComplementoAdicional = null;
            }
        }
        */

        /*
        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm)
            return;

        // 🔹 ClearButton ou Text limpo via código
        if (string.IsNullOrWhiteSpace(rasBoxDescricaoProduto.Text))
        {
            this.rasBoxDescricaoProduto.ItemsSource = vm.GetByText(vm.Descricoes, x => x.descricao, this.rasBoxDescricaoProduto.Text);
            rasBoxDescricaoAdicional.Text = null;
            rasBoxComplementoAdicional.Text = null;
            txtUnidade.Text = null;

            vm.DescricaoAdicionais = [];
            vm.ComplementoAdicionais = [];

            vm.Descricao = null;
            vm.DescricaoAdicional = null;
            vm.ComplementoAdicional = null;
            return;
        }

        // 🔹 Digitação manual
        if (e.Reason == TextChangeReason.UserInput)
        {
            this.rasBoxDescricaoProduto.ItemsSource = vm.GetByText(vm.Descricoes, x => x.descricao, this.rasBoxDescricaoProduto.Text);
            rasBoxDescricaoAdicional.Text = null;
            rasBoxComplementoAdicional.Text = null;
            txtUnidade.Text = null;

            vm.DescricaoAdicionais = [];
            vm.ComplementoAdicionais = [];

            vm.Descricao = null;
            vm.DescricaoAdicional = null;
            vm.ComplementoAdicional = null;
        }
        */
    }

    private async void rasBoxDescricaoProduto_SuggestionChosen(object sender, SuggestionChosenEventArgs e)
    {
        /*
        if (e.Suggestion is ProducaoProdutoModel descricao)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                this.rasBoxDescricaoAdicional.Text = string.Empty;
                this.rasBoxComplementoAdicional.Text = string.Empty;
                this.txtUnidade.Text = string.Empty;
                vm.DescricaoAdicionais = [];
                vm.ComplementoAdicionais = [];
                vm.Descricao = null;
                vm.DescricaoAdicional = null;
                vm.ComplementoAdicional = null;

                await vm.CarregarDescricaoAdicionaisAsync(descricao.codigo);
                rasBoxDescricaoAdicional.Focus();
                rasBoxDescricaoAdicional.IsDropDownOpen = true;
                vm.Descricao = descricao;
            }
        }
        */
    }

    private void rasBoxDescricaoAdicionalo_TextChanged(object sender, Telerik.Windows.Controls.AutoSuggestBox.TextChangedEventArgs e)
    {
        /*
        if (e.Reason == TextChangeReason.UserInput)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                this.rasBoxDescricaoAdicional.ItemsSource = vm.GetByText(vm.DescricaoAdicionais, x => x.descricao_adicional, this.rasBoxDescricaoAdicional.Text);
                rasBoxComplementoAdicional.Text = null;
                txtUnidade.Text = null;

                vm.ComplementoAdicionais = [];

                vm.DescricaoAdicional = null;
                vm.ComplementoAdicional = null;
            }
        }
        */
        /*
        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm)
            return;

        // 🔹 ClearButton ou Text limpo via código
        if (string.IsNullOrWhiteSpace(rasBoxDescricaoAdicional.Text))
        {
            this.rasBoxDescricaoAdicional.ItemsSource = vm.GetByText(vm.DescricaoAdicionais, x => x.descricao_adicional, this.rasBoxDescricaoAdicional.Text);
            rasBoxComplementoAdicional.Text = null;
            txtUnidade.Text = null;

            vm.ComplementoAdicionais = [];

            vm.DescricaoAdicional = null;
            vm.ComplementoAdicional = null;
            return;
        }

        // 🔹 Digitação manual
        if (e.Reason == TextChangeReason.UserInput)
        {
            this.rasBoxDescricaoAdicional.ItemsSource = vm.GetByText(vm.DescricaoAdicionais, x => x.descricao_adicional, this.rasBoxDescricaoAdicional.Text);
            rasBoxComplementoAdicional.Text = null;
            txtUnidade.Text = null;

            vm.ComplementoAdicionais = [];

            vm.DescricaoAdicional = null;
            vm.ComplementoAdicional = null;
        }
        */
    }

    private async void rasBoxDescricaoAdicionalo_SuggestionChosen(object sender, SuggestionChosenEventArgs e)
    {
        /*
        if (e.Suggestion is ProducaoDescAdicionalModel descAdicional)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                this.rasBoxComplementoAdicional.Text = string.Empty;
                this.txtUnidade.Text = string.Empty;
                vm.ComplementoAdicionais = [];
                vm.DescricaoAdicional = null;
                vm.ComplementoAdicional = null;

                await vm.CarregarComplementoAdicionaisAsync(descAdicional.coduniadicional);
                rasBoxComplementoAdicional.Focus();
                rasBoxComplementoAdicional.IsDropDownOpen = true;
                vm.DescricaoAdicional = descAdicional;
            }
        }
        */
    }

    private void rasBoxComplementoAdicional_TextChanged(object sender, Telerik.Windows.Controls.AutoSuggestBox.TextChangedEventArgs e)
    {
        /*
        if (e.Reason == TextChangeReason.UserInput)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                this.rasBoxComplementoAdicional.ItemsSource = vm.GetByText(vm.ComplementoAdicionais, x => x.complementoadicional, this.rasBoxComplementoAdicional.Text);
                txtUnidade.Text = null;
                vm.ComplementoAdicional = null;
            }
        }
        */
        /*
        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm)
            return;

        // 🔹 ClearButton ou Text limpo via código
        if (string.IsNullOrWhiteSpace(rasBoxComplementoAdicional.Text))
        {
            this.rasBoxComplementoAdicional.ItemsSource = vm.GetByText(vm.ComplementoAdicionais, x => x.complementoadicional, this.rasBoxComplementoAdicional.Text);
            txtUnidade.Text = null;
            vm.ComplementoAdicional = null;
            return;
        }

        // 🔹 Digitação manual
        if (e.Reason == TextChangeReason.UserInput)
        {
            this.rasBoxComplementoAdicional.ItemsSource = vm.GetByText(vm.ComplementoAdicionais, x => x.complementoadicional, this.rasBoxComplementoAdicional.Text);
            txtUnidade.Text = null;
            vm.ComplementoAdicional = null;
        }
        */
    }

    private void rasBoxComplementoAdicional_SuggestionChosen(object sender, SuggestionChosenEventArgs e)
    {
        /*
        if (e.Suggestion is ProducaoComplementoAdicionalModel compleAdicional)
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                txtUnidade.Text = compleAdicional.unidade;
                txtQuantidade.Focus();
                vm.ComplementoAdicional = compleAdicional;
            }
        }
        */
    }

    private void rasBox_GotFocus(object sender, RoutedEventArgs e)
    {
        (sender as RadAutoSuggestBox).IsDropDownOpen = true;
    }

    private void btnLimpar_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as CadastroHomologacaoProdutoComercialViewModel;
        this.txtOrdem.Text = string.Empty;
        //this.rasBoxPlanilha.Text = string.Empty;

        /*
        this.rasBoxDescricaoProduto.Text = string.Empty;
        this.rasBoxDescricaoAdicional.Text = string.Empty;
        this.rasBoxComplementoAdicional.Text = string.Empty;
        */
        this.rcBoxPlanilha.SelectedItem = null;
        this.rcBoxDescricaoProduto.SelectedItem = null;
        this.rcBoxDescricaoAdicional.SelectedItem = null;
        this.rcBoxComplementoAdicional.SelectedItem = null;
        this.txtUnidade.Text = string.Empty;
        this.txtQuantidade.Text = string.Empty;
        vm.Descricoes = [];
        vm.DescricaoAdicionais = [];
        vm.ComplementoAdicionais = [];
        vm.Planilha = null;
        vm.Descricao = null;
        vm.DescricaoAdicional = null;
        vm.ComplementoAdicional = null;
        this.txtOrdem.Focus();
    }

    private async void btnIncluir_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                var cod = await vm.InserirAsync(new ComercialPropostaInsumoDescComlModel
                {
                    coddimensao = vm.DimenssaoComercial.coddimensao,
                    codcompladicional = vm.ComplementoAdicional.codcompladicional,
                    id = txtOrdem.Text,
                    consulta_estoque = "0",
                    qtd = Convert.ToDouble(txtQuantidade.Text),
                });

                await vm.CarregarItensHomologacao(vm.DimenssaoComercial.coddimensao);
                this.btnLimpar_Click(sender, e);
                MessageBox.Show("Item incluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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

    private async void btnAlterar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var confirmResult = MessageBox.Show("Tem certeza que deseja alterar este item?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmResult != MessageBoxResult.Yes)
                return;

            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                var cod = await vm.AlterarAsync(new ComercialPropostaInsumoDescComlModel
                {
                    codinsumo = vm.ItemHomologado.codinsumo,
                    coddimensao = vm.DimenssaoComercial.coddimensao,
                    codcompladicional = vm.ComplementoAdicional.codcompladicional,
                    id = txtOrdem.Text,
                    consulta_estoque = "0",
                    qtd = Convert.ToDouble(txtQuantidade.Text),
                });

                await vm.CarregarItensHomologacao(vm.DimenssaoComercial.coddimensao);
                this.btnLimpar_Click(sender, e);
                MessageBox.Show("Item alterado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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

    private async void rgViewHomologacao_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
        {
            if (rgViewHomologacao.SelectedItem is PropostaInsumoDescComlDto item)
            {
                //vm.ItemHomologado = item;
                //await vm.CarregarPlanilhasAsync();
                await vm.CarregarDescricoesByCodComplAdicionalAsync(item.codcompladicional);

                vm.Planilha = vm.Planilhas.FirstOrDefault(f => f.planilha == vm.ProducaoDescricao.planilha);
                //rasBoxPlanilha.Text = vm.ProducaoDescricao.planilha;
                rcBoxPlanilha.SelectedItem = vm.Planilha;

                await vm.CarregarDescricoesAsync(vm.ProducaoDescricao.planilha);
                vm.Descricao = vm.Descricoes.FirstOrDefault(f => f.codigo == vm.ProducaoDescricao.codigo);
                //rasBoxDescricaoProduto.Text = vm.ProducaoDescricao.descricao;
                rcBoxDescricaoProduto.SelectedItem = vm.Descricao;

                await vm.CarregarDescricaoAdicionaisAsync(vm.ProducaoDescricao.codigo);
                vm.DescricaoAdicional = vm.DescricaoAdicionais.FirstOrDefault(f => f.coduniadicional == vm.ProducaoDescricao.coduniadicional);
                //rasBoxDescricaoAdicional.Text = vm.ProducaoDescricao.descricao_adicional;
                rcBoxDescricaoAdicional.SelectedItem = vm.DescricaoAdicional;

                await vm.CarregarComplementoAdicionaisAsync(vm.ProducaoDescricao.coduniadicional);
                vm.ComplementoAdicional = vm.ComplementoAdicionais.FirstOrDefault(f => f.codcompladicional == item.codcompladicional);
                //rasBoxComplementoAdicional.Text = vm.ProducaoDescricao.complementoadicional;
                rcBoxComplementoAdicional.SelectedItem = vm.ComplementoAdicional;

                txtOrdem.Text = item.id;
                txtUnidade.Text = item.unidade;
                txtQuantidade.Text = item.qtd.ToString();


                //vm.Planilha = planilha;
                //vm.Descricao = descricao;
                //vm.DescricaoAdicional = descAdicional;
                //vm.ComplementoAdicional = compleAdicional;

            }
        }
    }

    private void btnCopiar_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm) return;

        if (vm.DimenssaoComercial == null)
        {
            MessageBox.Show("Selecione uma Dimensão Comercial antes de copiar.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var meuUserControl = new CopiaItensHomologados(vm.DescricaoComercial, vm.DimenssaoComercial);
        RadWindow radWindow = new()
        {
            Content = meuUserControl,
            Header = $"Copiar Para: {vm.DescricaoComercial.descricaocomercial} {vm.DimenssaoComercial.dimensao}",
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
        // Evento disparado após fechar
        radWindow.Closed += async (sender, e) =>
        {
            // Exemplo: atualizar dados
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
            await vm.CarregarItensHomologacao(vm.DimenssaoComercial.coddimensao);
            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
        };
        // Abre como modal
        radWindow.ShowDialog();
    }

    private async void btnAtualizarCusto_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is CadastroHomologacaoProdutoComercialViewModel vm)
            {
                await vm.AtualizarCustoAsync(vm.DimenssaoComercial.coddimensao);
                MessageBox.Show("Custos atualizados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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

    private void concluido_Click(object sender, RoutedEventArgs e)
    {

        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm) return;
        var concluidoCheck = sender as CheckBox; //{System.Windows.Controls.CheckBox Content:FINALIZADO IsChecked:True}
        if (concluidoCheck?.IsChecked == true)
        {

        }

        //concluido_por
        //concluido_data

        /*
         * 
         * concluido.IsChecked = dimensao.insumo_concluido.Contains("1") ? true : false;
         * concluido_por.Text = dimensao?.insumo_concluido_por?.ToString();
         * concluido_data.DateTimeText = dimensao?.insumo_concluido_data?.ToString("dd/MM/yyyy");
         * 
         */

    }

    private async void rcBoxPlanilha_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm)
            return;

        if (e.AddedItems.Count == 0)
            return;

        if (e.AddedItems[0] is not ProducaoRelPlanModel planilha)
            return;

        this.rcBoxDescricaoProduto.SelectedItem = null;
        this.rcBoxDescricaoAdicional.SelectedItem = null;
        this.rcBoxComplementoAdicional.SelectedItem = null;
        this.txtUnidade.Text = string.Empty;
        vm.Descricoes = [];
        vm.DescricaoAdicionais = [];
        vm.ComplementoAdicionais = [];
        vm.Planilha = null;
        vm.Descricao = null;
        vm.DescricaoAdicional = null;
        vm.ComplementoAdicional = null;

        await vm.CarregarDescricoesAsync(planilha.planilha);
        //rasBoxDescricaoProduto.Focus();
        //rasBoxDescricaoProduto.IsDropDownOpen = true;
        vm.Planilha = planilha;

    }

    private async void rcBoxDescricaoProduto_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm)
            return;

        if(e.AddedItems.Count == 0)
            return;

        if (e.AddedItems[0] is not ProducaoProdutoModel descricao)
            return;

        this.rcBoxDescricaoAdicional.SelectedItem = null;
        this.rcBoxComplementoAdicional.SelectedItem = null;
        this.txtUnidade.Text = string.Empty;
        vm.DescricaoAdicionais = [];
        vm.ComplementoAdicionais = [];
        vm.Descricao = null;
        vm.DescricaoAdicional = null;
        vm.ComplementoAdicional = null;

        await vm.CarregarDescricaoAdicionaisAsync(descricao.codigo);
        //rasBoxDescricaoAdicional.Focus();
        //rasBoxDescricaoAdicional.IsDropDownOpen = true;
        vm.Descricao = descricao;
          
    }

    private async void rcBoxDescricaoAdicional_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm)
            return;

        if (e.AddedItems.Count == 0)
            return;

        if (e.AddedItems[0] is not ProducaoDescAdicionalModel descAdicional)
            return;

        this.rcBoxComplementoAdicional.SelectedItem = null;
        this.txtUnidade.Text = string.Empty;
        vm.ComplementoAdicionais = [];
        vm.DescricaoAdicional = null;
        vm.ComplementoAdicional = null;

        await vm.CarregarComplementoAdicionaisAsync(descAdicional.coduniadicional);
        //rasBoxComplementoAdicional.Focus();
        //rasBoxComplementoAdicional.IsDropDownOpen = true;
        vm.DescricaoAdicional = descAdicional;

    }

    private void rcBoxComplementoAdicional_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not CadastroHomologacaoProdutoComercialViewModel vm)
            return;

        if (e.AddedItems.Count == 0)
            return;

        if (e.AddedItems[0] is not ProducaoComplementoAdicionalModel compleAdicional)
            return;

        txtUnidade.Text = compleAdicional.unidade;
        txtQuantidade.Focus();
        vm.ComplementoAdicional = compleAdicional;

    }
}

public partial class CadastroHomologacaoProdutoComercialViewModel : ObservableObject
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
    private ComercialPropostaFamiliaModel comercialPropostaFamilia;

    [ObservableProperty]
    private ComercialPropostaDescricaoComercialModel descricaoComercial;

    [ObservableProperty]
    private ComercialPropostaDimensaoDescricaoComercialModel dimenssaoComercial;

    [ObservableProperty]
    private PropostaInsumoDescComlDto itemHomologado;

    [ObservableProperty]
    private ObservableCollection<ProducaoRelPlanModel> planilhas = [];

    [ObservableProperty]
    private ObservableCollection<ProducaoProdutoModel> descricoes = [];

    [ObservableProperty]
    private ObservableCollection<ProducaoDescAdicionalModel> descricaoAdicionais = [];

    [ObservableProperty]
    private ObservableCollection<ProducaoComplementoAdicionalModel> complementoAdicionais = [];

    [ObservableProperty]
    private ProducaoRelPlanModel planilha = new();

    [ObservableProperty]
    private ProducaoProdutoModel descricao = new();

    [ObservableProperty]
    private ProducaoDescAdicionalModel descricaoAdicional = new();

    [ObservableProperty]
    private ProducaoComplementoAdicionalModel complementoAdicional = new();

    [ObservableProperty]
    private ProducaoDescricaoModel producaoDescricao = new();

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
        if (string.IsNullOrWhiteSpace(searchText))
            return [.. source];

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

    public async Task CarregarPlanilhasAsync()
    {
        try
        {
            var notLike = new[] { "ESTOQUE", "ALMOX" };
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var lista = await _repo.GetAllAsync<ProducaoRelPlanModel>(conn, "planilha");
            // filtra client-side: mantém itens cuja planilha NÃO contenha nenhuma das palavras em notLike
            var filtered = lista.Where(x =>
            {
                var p = x.planilha;
                if (string.IsNullOrWhiteSpace(p)) return true; // ou false, conforme sua regra
                return !notLike.Any(n => p.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
            }).ToList();
            Planilhas = new ObservableCollection<ProducaoRelPlanModel>(filtered);
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

    public async Task CarregarDescricoesAsync(string planilha)
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var filtros = new Dictionary<string, object>
            {
                { "planilha", planilha },
                { "inativo", "0"}
            };
            //var lista = await _repo.GetWhereAsync<ProducaoProdutoModel>(conn, filtros, "descricao", false);
            //[Table("producao.produtos")]

            string sql = @$"
                SELECT 
                    *
                FROM producao.produtos
                WHERE planilha = '{planilha}' AND inativo = '0'
                ORDER BY descricao;
            ";

            var lista = await conn.QueryAsync<ProducaoProdutoModel>(
                sql
            );

            Descricoes = new ObservableCollection<ProducaoProdutoModel>(lista);
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

    public async Task CarregarDescricaoAdicionaisAsync(long codigoproduto)
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var filtros = new Dictionary<string, object>
            {
                { "codigoproduto", codigoproduto },
                { "inativo", "0"}
            };
            var lista = await _repo.GetWhereAsync<ProducaoDescAdicionalModel>(conn, filtros, "descricao_adicional", false);
            DescricaoAdicionais = new ObservableCollection<ProducaoDescAdicionalModel>(lista);
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

    public async Task CarregarComplementoAdicionaisAsync(long coduniadicional)
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var filtros = new Dictionary<string, object>
            {
                { "coduniadicional", coduniadicional },
                { "inativo", "0"}
            };
            var lista = await _repo.GetWhereAsync<ProducaoComplementoAdicionalModel>(conn, filtros, "complementoadicional", false);
            ComplementoAdicionais = new ObservableCollection<ProducaoComplementoAdicionalModel>(lista);
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

    public async Task CarregarDescricoesByCodComplAdicionalAsync(long codcompladicional)
    {
        try
        {
            using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
            var filtros = new Dictionary<string, object>
            {
                { "codcompladicional", codcompladicional },
            };
            var lista = await _repo.GetWhereAsync<ProducaoDescricaoModel>(conn, filtros, "descricao", false);
            ProducaoDescricao = lista.FirstOrDefault();
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

    public async Task<long> InserirAsync(ComercialPropostaInsumoDescComlModel proposta)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        proposta.codinsumo = await _repo.InsertAsync(conn, proposta);
        return proposta.codinsumo;
    }

    public async Task<long> AlterarAsync(ComercialPropostaInsumoDescComlModel proposta)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var filtros = new Dictionary<string, object>
        {
            {
                "codinsumo", proposta.codinsumo
            }
        };
        var encontrado = await _repo.GetWhereAsync<ComercialPropostaInsumoDescComlModel>(conn, filtros, "codinsumo", false);

        if (encontrado.Any())
            await _repo.UpdateAsync(conn, proposta);

        return proposta.codinsumo;
    }

    public async Task AtualizarCustoAsync(long coddimensao)
    {
        using var conn = new NpgsqlConnection(BaseSettings.ConnectionString);
        var parametros = new { coddimensao };
        var item = await conn.QueryFirstOrDefaultAsync<AtualizaCustoDto>(
            @"SELECT coddimensao, somapreco_nf, somafinal, somax, somapeso_ FROM comercial.qry_atualizacusto
	          WHERE coddimensao = @coddimensao;", parametros);

        if (item != null)
            await conn.ExecuteAsync(
                @"UPDATE comercial.proposta_dimensaodescricaocomercial
	                SET custounitarioapurado = Round(@somafinal, 2), custo_historico = Round(@somax, 2), preco_nf = Round(@somapreco_nf, 2), peso = Round(@somapeso_, 2)
	                WHERE coddimensao = @coddimensao;",
              new
              {
                  item.coddimensao,
                  item.somapreco_nf,
                  item.somafinal,
                  item.somax,
                  item.somapeso_
              });
    }
}