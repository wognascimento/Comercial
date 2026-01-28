using Comercial.DataBase;
using Microsoft.EntityFrameworkCore;
using Producao;
using Syncfusion.Presentation;
using Syncfusion.Presentation.Drawing;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Tools.Controls;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Comercial;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    DataBaseSettings BaseSettings = DataBaseSettings.Instance;

    public MainWindow()
    {
        InitializeComponent();
        StyleManager.ApplicationTheme = new Windows11Theme();

        VisualStyles visualStyle = VisualStyles.Default;
        Enum.TryParse("Metro", out visualStyle);
        if (visualStyle != VisualStyles.Default)
        {
            SfSkinManager.ApplyStylesOnApplication = true;
            SfSkinManager.SetVisualStyle(this, visualStyle);
            SfSkinManager.ApplyStylesOnApplication = false;
        }

        Syncfusion.SfSkinManager.SizeMode sizeMode = Syncfusion.SfSkinManager.SizeMode.Default;
        Enum.TryParse("Default", out sizeMode);
        if (sizeMode != Syncfusion.SfSkinManager.SizeMode.Default)
        {
            SfSkinManager.ApplyStylesOnApplication = true;
            SfSkinManager.SetSizeMode(this, sizeMode);
            SfSkinManager.ApplyStylesOnApplication = false;
        }

        var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
        if (appSettings[0].Length > 0)
            BaseSettings.Username = appSettings[0];

        txtUsername.Text = BaseSettings.Username;
        txtDataBase.Text = BaseSettings.Database;
    }

    public void adicionarFilho(object filho, string title, string name)
    {
        var doc = ExistDocumentInDocumentContainer(name);
        if (doc == null)
        {
            doc = (FrameworkElement?)filho;
            DocumentContainer.SetHeader(doc, title);
            doc.Name = name.ToLower();
            _mdi.Items.Add(doc);
        }
        else
        {
            //_mdi.RestoreDocument(doc as UIElement);
            _mdi.ActiveDocument = doc;
        }
    }

    private FrameworkElement ExistDocumentInDocumentContainer(string name_)
    {
        foreach (FrameworkElement element in _mdi.Items)
        {
            if (name_.ToLower() == element.Name)
            {
                return element;
            }
        }
        return null;
    }


    private void _mdi_CloseAllTabs(object sender, CloseTabEventArgs e)
    {
        _mdi.Items.Clear();
    }

    private void _mdi_CloseButtonClick(object sender, CloseButtonEventArgs e)
    {
        var tab = (DocumentContainer)sender;
        _mdi.Items.Remove(tab.ActiveDocument);
    }


    private void OnAlterarUsuario(object sender, MouseButtonEventArgs e)
    {
        Login window = new();
        window.ShowDialog();

        try
        {
            var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
            BaseSettings.Username = appSettings[0];
            txtUsername.Text = BaseSettings.Username;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
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
                    _mdi.Items.Clear();
                }
            }
        });
    }

    private async void OnApresentcaoPPTClick(object sender, RoutedEventArgs e)
    {
        try
        {

            using Context context = new();

            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

            var temas = await context.PropostaViewQuadroPrecos.AsNoTracking().Where(x => x.sigla == "BOR").GroupBy(x => x.tema).Select(x => x.Key).ToListAsync();

            // Abre a apresentação
            IPresentation presentation = Presentation.Open("Modelos/BASE.pptx");

            // Obtém todos os masters da apresentação
            //ISlideMaster slideMaster = presentation.Masters[0]; // Use o índice apropriado
            IMasterSlide slideMaster = presentation.Masters[0]; // Use o índice apropriado

            // ESTA PARTE PRECISA SER DENTRO DO LOOP TEMAS
            foreach (var tema in temas)
            {
                // Obtém o layout desejado do master
                ILayoutSlide layoutSlide = slideMaster.LayoutSlides[0]; // Use o índice apropriado para o layout desejado

                // Adiciona um novo slide usando o layout específico
                presentation.Slides.Add(slideMaster.LayoutSlides[0]);
                ISlide slide = presentation.Slides.Add(slideMaster.LayoutSlides[1]);

                // Método 1: Encontrar por nome ou índice específico (se você conhece a posição)
                IShape textBoxShape = (IShape)slide.Shapes[0];
                // Limpa o texto existente
                textBoxShape.TextBody.Paragraphs.Clear();
                // Adiciona o novo texto
                IParagraph paragraph = textBoxShape.TextBody.AddParagraph();
                paragraph.AddTextPart(tema);

                var locais = await context.PropostaViewQuadroPrecos.AsNoTracking().Where(x => x.sigla == "BOR" && x.tema == tema).GroupBy(x => x.localitem).Select(x => x.Key).ToListAsync();
                // ESTA PARTE PRECISA SER DENTRO DO LOOP ITENS
                foreach (var local in locais)
                {
                    // Obtém o layout desejado do master
                    ILayoutSlide lSItem = slideMaster.LayoutSlides[0]; // Use o índice apropriado para o layout desejado

                    // Adiciona um novo slide usando o layout específico
                    //presentation.Slides.Add(slideMaster.LayoutSlides[2]);
                    ISlide sItem = presentation.Slides.Add(slideMaster.LayoutSlides[2]);

                    // Método 1: Encontrar por nome ou índice específico (se você conhece a posição)
                    IShape tBSItem = (IShape)sItem.Shapes[0];
                    // Limpa o texto existente
                    tBSItem.TextBody.Paragraphs.Clear();
                    // Adiciona o novo texto
                    IParagraph pItem = tBSItem.TextBody.AddParagraph();
                    pItem.AddTextPart(local);

                    FileStream pictureStream = new("imagem_1.png", FileMode.Open);

                    IPicture? imagem = sItem.Pictures.AddPicture(pictureStream, 85.322992125984257, 87.023779527559057, 1269.356220472441, 714.04834645669291);
                    pictureStream.Close();
                    imagem = null;
                }

            }
            // Adiciona um novo slide usando o layout específico
            presentation.Slides.Add(slideMaster.LayoutSlides[5]);
            presentation.Slides.Add(slideMaster.LayoutSlides[6]);


            // Salva e fecha
            presentation.Save("ApresentacaoComLayoutPersonalizado.pptx");
            presentation.Close();

            Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

            MessageBox.Show("APRENTAÇÃO PPT GERADA COM SUCESSO!!!");

            Process.Start("explorer", @$"ApresentacaoComLayoutPersonalizado.pptx");

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
}