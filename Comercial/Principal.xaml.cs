using BibliotecasSIG;
using Comercial.DataBase;
using Comercial.Views.Consulta;
using Comercial.Views.Proposta;
using Producao;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace Comercial
{
    /// <summary>
    /// Lógica interna para Principal.xaml
    /// </summary>
    public partial class Principal : Window
    {
        private DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        private readonly string CURRENT_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public Principal()
        {
            InitializeComponent();
            txtUsername.Text = BaseSettings.Username;
            txtDataBase.Text = BaseSettings.Database;

            Loaded += async (s, e) => await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // Verificação de atualização em segundo plano
            await CheckForUpdatesAsync();
        }



        private async Task CheckForUpdatesAsync(bool showUpToDate = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BaseSettings.UpdateInfoUrl))
                    return;

                var updateChecker = new UpdateChecker(BaseSettings.UpdateInfoUrl, CURRENT_VERSION);
                var updateInfo = await updateChecker.CheckForUpdatesAsync();

                var updateInfoJson = JsonSerializer.Serialize<UpdateInfo>(updateInfo);

                if (updateInfo == null)
                {
                    if (showUpToDate)
                        MessageBox.Show($"O sistema já está atualizado.\n\nVersão atual: {CURRENT_VERSION}", "Atualização do sistema", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (updateInfo != null)
                {
                    // Pergunta ao usuário se deseja atualizar
                    var result = MessageBox.Show(
                        $"Nova versão disponível!\n\n" +
                        $"Versão atual: {CURRENT_VERSION}\n" +
                        $"Nova versão: {updateInfo.updateVersion}\n\n" +
                        "Changelog:\n" +
                        string.Join("\n", updateInfo.changelog) +
                        "\n\nDeseja baixar a atualização?",
                        "Atualização Disponível",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {

                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string jsonString = JsonSerializer.Serialize(updateInfo, options);

                        //Process.Start("Update.exe", @$"{updateInfoJson}, Operacional.exe");

                        string jsonData = JsonSerializer.Serialize(updateInfo); // Garante que o JSON está bem formatado
                        string appName = "Comercial.exe";


                        string arguments = $"\"{jsonData.Replace("\"", "\\\"")}\" \"{appName}\"";
                        Process.Start("Update.exe", arguments);
                        //this.Shutdown();
                        Application.Current.Shutdown();

                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log do erro ou tratamento de exceção
                MessageBox.Show(
                    $"Erro ao verificar atualizações: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao verificar atualizações: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void AbrirFormularioDinamico(Type tipoFormulario, string titulo, object parametro = null)
        {
            var paneGroup = radDocking.FindChildByType<RadPaneGroup>();

            var paneExistente = paneGroup?.Items.OfType<RadPane>()
                .FirstOrDefault(p => p.Header.ToString() == titulo);

            if (paneExistente != null)
            {
                paneExistente.IsActive = true;
            }
            else
            {
                // Cria instância do formulário
                var formulario = Activator.CreateInstance(tipoFormulario) as UserControl;

                // Se tiver parâmetro, passa via DataContext ou propriedade
                if (parametro != null && formulario != null)
                {
                    formulario.DataContext = parametro;
                }

                var novoPane = new RadPane
                {
                    Header = titulo,
                    CanUserClose = true,
                    Content = formulario,
                    Tag = tipoFormulario.Name // Útil para identificar depois
                };

                paneGroup?.Items.Add(novoPane);
                novoPane.IsActive = true;

                /*
                 * AbrirFormularioDinamico(typeof(MeuFormulario), "Meu Formulário");
                 */

                /*
                 * var dados = new { Id = 123, Nome = "Wesley" };
                 * AbrirFormularioDinamico(typeof(OutroFormulario), "Outro Formulário", dados);
                 */
            }
        }

        private void OnAbrirCadastroFamiliaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            AbrirFormularioDinamico(typeof(CadastoFamilia), "CADASTRO DE FAMÍLIA");
        }

        private void OnAbrirCadastroDescricaoClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            AbrirFormularioDinamico(typeof(CadastroDescricao), "CADASTRO DESCRIÇÃO COMERCIAL");
        }

        private void OnAbrirCadastroHomologacaoComercialClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            AbrirFormularioDinamico(typeof(CadastroHomologacaoProdutoComercial), "HOMOLOGAÇÃO DE PRODUTO COMERCIAL");
        }

        private void OnAbrirQuadroQuantitativoClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            AbrirFormularioDinamico(typeof(PropostaQuadroQuantitativo), "QUADRO QUANTITATIVO");
        }

        private void OnAbrirQuadroRevisaoClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            AbrirFormularioDinamico(typeof(PropostaQuadroPreco), "QUADRO REVISÃO");
        }


        private void OnAbrirQuadroFechaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            AbrirFormularioDinamico(typeof(PropostaQuadroFecha), "QUADRO FECHA");
        }

        private void OnAbrirTodasDescricoesClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            AbrirFormularioDinamico(typeof(TodasDescricoes), "TODAS AS DESCRIÇÕES");
        }

        private void OnAlterarUsuarioClick(object sender, RoutedEventArgs e)
        {
            Login window = new();
            window.ShowDialog();
            try
            {
                var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
                //BaseSettings.Username = appSettings[0];
                txtUsername.Text = BaseSettings.Username;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void OnAlterarBancoDadosClick(object sender, RoutedEventArgs e)
        {
            RadWindow.Prompt(new DialogParameters()
            {
                Header = "Ano Sistema",
                Content = "Alterar o Ano do Sistema",
                Closed = (object sender, WindowClosedEventArgs e) =>
                {
                    if (e.PromptResult != null)
                    {
                        BaseSettings.Database = e.PromptResult;
                        txtDataBase.Text = BaseSettings.Database;
                        BaseSettings.RefreshConnectionString();
                        radDocking.Items.Clear();
                    }
                }
            });
        }

        private async void OnAtualizarSistemaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await CheckForUpdatesAsync(true);
        }

        private void OnSobreSistemaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            MessageBox.Show($"Sistema Integrado de Gerenciamento - Comercial\n\nVersão atual: {CURRENT_VERSION}", "Sobre o sistema", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
