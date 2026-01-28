using BibliotecasSIG;
using Comercial.DataBase;
using Comercial.Views.Proposta;
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

        private const string UPDATE_URL = "http://192.168.0.49/downloads/comercial/version.json";
        private readonly string CURRENT_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public Principal()
        {
            InitializeComponent();
            StyleManager.ApplicationTheme = new Office2016Theme();

            txtUsername.Text = BaseSettings.Username;
            txtDataBase.Text = BaseSettings.Database;

            Loaded += async (s, e) => await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // Verificação de atualização em segundo plano
            await CheckForUpdatesAsync();
        }



        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var updateChecker = new UpdateChecker(UPDATE_URL, CURRENT_VERSION);
                var updateInfo = await updateChecker.CheckForUpdatesAsync();

                var updateInfoJson = JsonSerializer.Serialize<UpdateInfo>(updateInfo);

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
    }
}
