using Comercial.DataBase;
using Dapper;
using Microsoft.Win32;
using Npgsql;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Comercial.Views.Consulta;

public partial class TodasDescricoesImagensWindow : Window
{
    private readonly TodasDescricoesImagensViewModel viewModel;

    public TodasDescricoesImagensWindow(int coddimensao, string? descricao)
    {
        InitializeComponent();
        viewModel = new TodasDescricoesImagensViewModel(coddimensao, descricao);
        DataContext = viewModel;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            SetBusy(true);
            await viewModel.CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível carregar as imagens.\n\n{ex.Message}",
                "Imagens",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnAdicionarImagemClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Selecionar imagem",
            Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp|Todos os arquivos|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog(this) != true)
            return;

        try
        {
            SetBusy(true);

            foreach (var fileName in dialog.FileNames)
                await viewModel.AdicionarImagemAsync(fileName);

            await viewModel.CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível adicionar a imagem.\n\n{ex.Message}",
                "Imagens",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnDefinirCapaClick(object sender, RoutedEventArgs e)
    {
        if (viewModel.ImagemSelecionada is null)
        {
            MessageBox.Show(
                "Selecione uma imagem para definir como capa.",
                "Imagens",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        await DefinirCapaAsync(viewModel.ImagemSelecionada);
    }

    private async void OnExcluirImagemClick(object sender, RoutedEventArgs e)
    {
        if (viewModel.ImagemSelecionada is null)
        {
            MessageBox.Show(
                "Selecione uma imagem para excluir.",
                "Imagens",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var confirmar = MessageBox.Show(
            "Deseja excluir a imagem selecionada?",
            "Imagens",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        try
        {
            SetBusy(true);
            await viewModel.ExcluirImagemAsync(viewModel.ImagemSelecionada);
            await viewModel.CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível excluir a imagem.\n\n{ex.Message}",
                "Imagens",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnCapaClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: DescricaoDimensaoImagemModel imagem })
            await DefinirCapaAsync(imagem);
    }

    private void OnImagemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2 ||
            sender is not FrameworkElement { DataContext: DescricaoDimensaoImagemModel imagem })
        {
            return;
        }

        viewModel.ImagemSelecionada = imagem;
        AbrirImagemOriginal(imagem);
    }

    private static void AbrirImagemOriginal(DescricaoDimensaoImagemModel imagem)
    {
        if (string.IsNullOrWhiteSpace(imagem.caminho_arquivo) || !File.Exists(imagem.caminho_arquivo))
        {
            MessageBox.Show(
                "O arquivo original da imagem não foi encontrado.",
                "Imagens",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = imagem.caminho_arquivo,
            UseShellExecute = true
        });
    }

    private async Task DefinirCapaAsync(DescricaoDimensaoImagemModel imagem)
    {
        try
        {
            SetBusy(true);
            imagem.capa = true;
            viewModel.ImagemSelecionada = imagem;
            await viewModel.AtualizarCapaAsync(imagem);
            await viewModel.CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível atualizar a imagem de capa.\n\n{ex.Message}",
                "Imagens",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            await viewModel.CarregarAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        Mouse.OverrideCursor = busy ? Cursors.Wait : null;
        adicionarImagemButton.IsEnabled = !busy;
        definirCapaButton.IsEnabled = !busy;
        excluirImagemButton.IsEnabled = !busy;
        imagensList.IsEnabled = !busy;
    }
}

public class TodasDescricoesImagensViewModel : INotifyPropertyChanged
{
    private readonly DataBaseSettings baseSettings = DataBaseSettings.Instance;
    private readonly int coddimensao;

    public event PropertyChangedEventHandler? PropertyChanged;

    public TodasDescricoesImagensViewModel(int coddimensao, string? descricao)
    {
        this.coddimensao = coddimensao;
        Titulo = $"Imagens da dimensão {coddimensao}";
        Subtitulo = descricao ?? string.Empty;
    }

    public string Titulo { get; }
    public string Subtitulo { get; }

    private ObservableCollection<DescricaoDimensaoImagemModel> imagens = [];
    public ObservableCollection<DescricaoDimensaoImagemModel> Imagens
    {
        get => imagens;
        set
        {
            imagens = value;
            RaisePropertyChanged(nameof(Imagens));
        }
    }

    private DescricaoDimensaoImagemModel? imagemSelecionada;
    public DescricaoDimensaoImagemModel? ImagemSelecionada
    {
        get => imagemSelecionada;
        set
        {
            imagemSelecionada = value;
            RaisePropertyChanged(nameof(ImagemSelecionada));
        }
    }

    public async Task CarregarAsync()
    {
        const string sql = """
            SELECT
                id_imagem,
                coddimensao,
                nome_arquivo,
                content_type,
                caminho_arquivo,
                caminho_thumbnail,
                capa,
                cadastrado_por,
                cadastrado_em,
                alterado_por,
                alterado_em
            FROM comercial.proposta_descricaodimensao_imagem
            WHERE coddimensao = @coddimensao
            ORDER BY capa DESC, id_imagem DESC;
            """;

        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        var dados = await connection.QueryAsync<DescricaoDimensaoImagemModel>(sql, new { coddimensao });
        Imagens = new ObservableCollection<DescricaoDimensaoImagemModel>(dados);
    }

    public async Task AdicionarImagemAsync(string fileName)
    {
        var fileInfo = new FileInfo(fileName);
        if (!fileInfo.Exists)
            throw new FileNotFoundException("Arquivo de imagem não encontrado.", fileName);

        if (fileInfo.Length == 0)
            throw new InvalidOperationException("O arquivo selecionado está vazio.");

        var destino = CriarArquivosImagem(fileInfo);

        var imagem = new DescricaoDimensaoImagemModel
        {
            coddimensao = coddimensao,
            nome_arquivo = fileInfo.Name,
            content_type = ObterContentType(fileInfo.Extension),
            caminho_arquivo = destino.caminhoArquivo,
            caminho_thumbnail = destino.caminhoThumbnail,
            capa = Imagens.Count == 0,
            cadastrado_por = Environment.UserName,
            cadastrado_em = DateTime.Now
        };

        const string inserirSql = """
            INSERT INTO comercial.proposta_descricaodimensao_imagem
                (coddimensao, nome_arquivo, content_type, caminho_arquivo, caminho_thumbnail, capa, cadastrado_por, cadastrado_em)
            VALUES
                (@coddimensao, @nome_arquivo, @content_type, @caminho_arquivo, @caminho_thumbnail, @capa, @cadastrado_por, @cadastrado_em);
            """;

        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            if (imagem.capa)
            {
                await connection.ExecuteAsync(
                    "UPDATE comercial.proposta_descricaodimensao_imagem SET capa = false WHERE coddimensao = @coddimensao;",
                    new { coddimensao },
                    transaction);
            }

            await connection.ExecuteAsync(inserirSql, imagem, transaction);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            ExcluirArquivoSeExistir(destino.caminhoArquivo);
            ExcluirArquivoSeExistir(destino.caminhoThumbnail);
            throw;
        }
    }

    public async Task AtualizarCapaAsync(DescricaoDimensaoImagemModel imagem)
    {
        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            if (imagem.capa)
            {
                await connection.ExecuteAsync(
                    "UPDATE comercial.proposta_descricaodimensao_imagem SET capa = false WHERE coddimensao = @coddimensao;",
                    new { coddimensao },
                    transaction);
            }

            await connection.ExecuteAsync(
                """
                UPDATE comercial.proposta_descricaodimensao_imagem
                SET capa = @capa,
                    alterado_por = @alterado_por,
                    alterado_em = @alterado_em
                WHERE id_imagem = @id_imagem;
                """,
                new
                {
                    imagem.id_imagem,
                    imagem.capa,
                    alterado_por = Environment.UserName,
                    alterado_em = DateTime.Now
                },
                transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ExcluirImagemAsync(DescricaoDimensaoImagemModel imagem)
    {
        const string sql = """
            DELETE FROM comercial.proposta_descricaodimensao_imagem
            WHERE id_imagem = @id_imagem;
            """;

        await using var connection = new NpgsqlConnection(baseSettings.ConnectionString);
        await connection.ExecuteAsync(sql, new { imagem.id_imagem });

        ExcluirArquivoSeExistir(imagem.caminho_arquivo);
        ExcluirArquivoSeExistir(imagem.caminho_thumbnail);
    }

    private (string caminhoArquivo, string caminhoThumbnail) CriarArquivosImagem(FileInfo origem)
    {
        var diretorio = Path.Combine(ObterDiretorioBaseImagens(), coddimensao.ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(diretorio);

        var extensao = NormalizarExtensao(origem.Extension);
        var nomeBase = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}";
        var caminhoArquivo = Path.Combine(diretorio, $"{nomeBase}{extensao}");
        var caminhoThumbnail = Path.Combine(diretorio, $"{nomeBase}_thumb.jpg");

        File.Copy(origem.FullName, caminhoArquivo, overwrite: false);
        CriarThumbnail(caminhoArquivo, caminhoThumbnail);

        return (caminhoArquivo, caminhoThumbnail);
    }

    private string ObterDiretorioBaseImagens()
    {
        var configurado = ConfigurationManager.AppSettings["DescricaoImagemPath"];
        if (!string.IsNullOrWhiteSpace(configurado))
            return configurado;

        return Path.Combine(baseSettings.CaminhoSistema, "Imagens", "Descricoes");
    }

    private static void CriarThumbnail(string origem, string destino)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.DecodePixelWidth = 320;
        bitmap.UriSource = new Uri(origem, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();

        var encoder = new JpegBitmapEncoder { QualityLevel = 82 };
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var stream = File.Create(destino);
        encoder.Save(stream);
    }

    private static string NormalizarExtensao(string extension)
    {
        var ext = extension.ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png" or ".bmp" ? ext : ".jpg";
    }

    private static void ExcluirArquivoSeExistir(string? caminho)
    {
        if (string.IsNullOrWhiteSpace(caminho))
            return;

        try
        {
            if (File.Exists(caminho))
                File.Delete(caminho);
        }
        catch
        {
            // O registro no banco e a acao principal ja foram concluidos; falha ao limpar arquivo nao deve bloquear a tela.
        }
    }

    private static string ObterContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }

    private void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class DescricaoDimensaoImagemModel
{
    public int id_imagem { get; set; }
    public int coddimensao { get; set; }
    public string? nome_arquivo { get; set; }
    public string? content_type { get; set; }
    public string? caminho_arquivo { get; set; }
    public string? caminho_thumbnail { get; set; }
    public bool capa { get; set; }
    public string? cadastrado_por { get; set; }
    public DateTime? cadastrado_em { get; set; }
    public string? alterado_por { get; set; }
    public DateTime? alterado_em { get; set; }
}

public class ImagePathToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string caminho || string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho))
            return null;

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(caminho, UriKind.Absolute);
        image.DecodePixelWidth = 160;
        image.EndInit();
        image.Freeze();
        return image;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
