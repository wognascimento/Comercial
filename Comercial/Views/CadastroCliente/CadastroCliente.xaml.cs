using Comercial.Data;
using Comercial.Data.Model;
using Comercial.DataBase;
using CommunityToolkit.Mvvm.ComponentModel;
using Dapper;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Comercial.Views.CadastroCliente;

public partial class CadastroCliente : UserControl
{
    public CadastroCliente()
    {
        InitializeComponent();
        DataContext = new CadastroClienteViewModel();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroClienteViewModel vm)
            await vm.CarregarAsync();
    }

    private async void OnPrimeiroClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroClienteViewModel vm)
            await vm.PrimeiroAsync();
    }

    private async void OnAnteriorClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroClienteViewModel vm)
            await vm.AnteriorAsync();
    }

    private async void OnProximoClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroClienteViewModel vm)
            await vm.ProximoAsync();
    }

    private async void OnUltimoClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroClienteViewModel vm)
            await vm.UltimoAsync();
    }

    private void OnNovoClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroClienteViewModel vm)
            vm.Novo();
    }

    private async void OnSalvarClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CadastroClienteViewModel vm)
            return;

        try
        {
            IsEnabled = false;
            await vm.SalvarClienteAsync();
            MessageBox.Show("Cliente salvo com sucesso.", "Cadastro de cliente", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar cliente: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsEnabled = true;
        }
    }

    private async void OnBuscarClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is CadastroClienteViewModel vm)
            await vm.BuscarAsync(txtPesquisa.Text);
    }

    private async void OnPesquisaKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is CadastroClienteViewModel vm)
        {
            await vm.BuscarAsync(txtPesquisa.Text);
            e.Handled = true;
        }
    }

    private void OnContatoAddingNewDataItem(object sender, GridViewAddingNewEventArgs e)
    {
        if (DataContext is not CadastroClienteViewModel vm || vm.Cliente.id_cliente <= 0 || string.IsNullOrWhiteSpace(vm.Cliente.sigla))
        {
            MessageBox.Show("Salve o cliente com sigla antes de incluir contatos.", "Cadastro de cliente", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Cancel = true;
            return;
        }

        e.NewObject = new ClienteContatoModel
        {
            sigla = vm.Cliente.sigla,
            inativo = "0",
            brindedenatal = "0",
            respdec = "0",
            vip = "0",
            ok = "0",
            ok1 = "0",
            cartaodenatal = "0",
            conviteshowroom = "0",
            convitefesta = "0",
            preconviteshowroom = "0",
            pesquisabriefing = "0",
            operacional = "0",
            vipouro = "0",
            revista = "0",
            convitesrfase2 = "0",
            resp_financeiro = "0",
            resp_operacoes = "0"
        };
    }

    private async void OnContatoRowEditEnded(object sender, GridViewRowEditEndedEventArgs e)
    {
        if (e.EditAction == GridViewEditAction.Cancel || DataContext is not CadastroClienteViewModel vm)
            return;

        if (e.Row.Item is not ClienteContatoModel contato)
            return;

        if (!vm.ContatoTemAlteracao(contato))
        {
            vm.RemoverContatoNovoVazio(contato);
            gridContatos.Items.Refresh();
            return;
        }

        try
        {
            gridContatos.IsBusy = true;
            await vm.SalvarContatoAsync(contato);
            gridContatos.Items.Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar contato: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            await vm.CarregarContatosAsync();
        }
        finally
        {
            gridContatos.IsBusy = false;
        }
    }

    private void OnContatoRowValidating(object sender, GridViewRowValidatingEventArgs e)
    {
        if (e.Row.Item is not ClienteContatoModel contato)
            return;

        if (DataContext is CadastroClienteViewModel vm && !vm.ContatoTemAlteracao(contato))
            return;

        if (!string.IsNullOrWhiteSpace(contato.contato) &&
            !string.IsNullOrWhiteSpace(contato.funcao) &&
            !string.IsNullOrWhiteSpace(contato.cargo) &&
            !string.IsNullOrWhiteSpace(contato.email))
        {
            return;
        }

        e.IsValid = false;
        e.ValidationResults.Add(new GridViewCellValidationResult
        {
            ErrorMessage = "Informe contato, função, cargo e email.",
            PropertyName = string.Empty
        });
    }
}

public partial class CadastroClienteViewModel : ObservableObject
{
    private readonly DataBaseSettings _baseSettings = DataBaseSettings.Instance;
    private string? _siglaCarregada;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RegistroTexto))]
    private ObservableCollection<ClienteResumoModel> clientes = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RegistroTexto))]
    private int indiceAtual = -1;

    [ObservableProperty]
    private ClienteModel cliente = new();

    [ObservableProperty]
    private ObservableCollection<ClienteContatoModel> contatos = [];

    [ObservableProperty]
    private ClienteContatoModel? contatoSelecionado;

    [ObservableProperty]
    private string? pesquisa;

    [ObservableProperty]
    private ObservableCollection<string> tiposClientes = [];

    [ObservableProperty]
    private ObservableCollection<string> redesClientes = [];

    [ObservableProperty]
    private ObservableCollection<string> regioesAtendimento = [];

    public string RegistroTexto => Clientes.Count == 0 || IndiceAtual < 0
        ? "Registro: 0 de 0"
        : $"Registro: {IndiceAtual + 1} de {Clientes.Count}";

    public async Task CarregarAsync()
    {
        using var db = DbConnectionFactory.Create();
        await CarregarListasAsync(db);

        var data = await db.QueryAsync<ClienteResumoModel>(
            @"SELECT id_cliente, nome, sigla, cnpj
              FROM comercial.clientes
              ORDER BY nome NULLS LAST, sigla NULLS LAST;");

        Clientes = new ObservableCollection<ClienteResumoModel>(data);

        if (Clientes.Count > 0)
        {
            IndiceAtual = 0;
            await CarregarClienteAtualAsync();
        }
        else
        {
            Novo();
        }
    }

    private async Task CarregarListasAsync(System.Data.IDbConnection db)
    {
        var tipos = await db.QueryAsync<string>(
            @"SELECT tipo
              FROM comercial.tiposdeclientes
              ORDER BY tipo;");

        var redes = await db.QueryAsync<string>(
            @"SELECT grupo
              FROM comercial.redes
              ORDER BY grupo;");

        var regioes = await db.QueryAsync<string>(
            @"SELECT reg_atend
              FROM comercial.t_reg_atend
              GROUP BY reg_atend
              ORDER BY reg_atend;");

        TiposClientes = new ObservableCollection<string>(tipos.Where(x => !string.IsNullOrWhiteSpace(x)));
        RedesClientes = new ObservableCollection<string>(redes.Where(x => !string.IsNullOrWhiteSpace(x)));
        RegioesAtendimento = new ObservableCollection<string>(regioes.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    public async Task BuscarAsync(string? textoPesquisa = null)
    {
        Pesquisa = textoPesquisa ?? Pesquisa;

        if (string.IsNullOrWhiteSpace(Pesquisa))
        {
            await CarregarAsync();
            return;
        }

        using var db = DbConnectionFactory.Create();
        var termo = Pesquisa.Trim();
        var termoContem = $"%{termo}%";
        var data = (await db.QueryAsync<ClienteModel>(
            @"SELECT *
              FROM comercial.clientes
              WHERE COALESCE(nome, '') ILIKE @termoContem
                 OR COALESCE(sigla, '') ILIKE @termo
                 OR COALESCE(sigla, '') ILIKE @termoContem
                 OR COALESCE(sigla_externa, '') ILIKE @termo
                 OR COALESCE(sigla_externa, '') ILIKE @termoContem
                 OR COALESCE(nome_fantasia, '') ILIKE @termoContem
                 OR COALESCE(rsocial, '') ILIKE @termoContem
                 OR COALESCE(grupo, '') ILIKE @termoContem
                 OR COALESCE(cnpj, '') ILIKE @termoContem
              ORDER BY nome NULLS LAST, sigla NULLS LAST;",
            new { termo, termoContem })).AsList();

        Clientes = new ObservableCollection<ClienteResumoModel>(
            data.Select(cliente => new ClienteResumoModel
            {
                id_cliente = cliente.id_cliente,
                nome = cliente.nome,
                sigla = cliente.sigla,
                cnpj = cliente.cnpj
            }));
        IndiceAtual = Clientes.Count > 0 ? 0 : -1;

        if (IndiceAtual >= 0)
        {
            Cliente = data[IndiceAtual];
            _siglaCarregada = Cliente.sigla;
            await CarregarContatosAsync();
            OnPropertyChanged(nameof(RegistroTexto));
        }
        else
        {
            LimparResultadoPesquisa();
        }
    }

    public async Task PrimeiroAsync()
    {
        if (Clientes.Count == 0)
            return;

        IndiceAtual = 0;
        await CarregarClienteAtualAsync();
    }

    public async Task AnteriorAsync()
    {
        if (Clientes.Count == 0 || IndiceAtual <= 0)
            return;

        IndiceAtual--;
        await CarregarClienteAtualAsync();
    }

    public async Task ProximoAsync()
    {
        if (Clientes.Count == 0 || IndiceAtual >= Clientes.Count - 1)
            return;

        IndiceAtual++;
        await CarregarClienteAtualAsync();
    }

    public async Task UltimoAsync()
    {
        if (Clientes.Count == 0)
            return;

        IndiceAtual = Clientes.Count - 1;
        await CarregarClienteAtualAsync();
    }

    public void Novo()
    {
        Cliente = new ClienteModel
        {
            internacional = "0",
            abrasce = "0",
            inativo = "0",
            atualizacao = DateTimeOffset.UtcNow
        };
        Contatos = [];
        ContatoSelecionado = null;
        IndiceAtual = Clientes.Count;
        _siglaCarregada = null;
    }

    private void LimparResultadoPesquisa()
    {
        Cliente = new ClienteModel();
        Contatos = [];
        ContatoSelecionado = null;
        _siglaCarregada = null;
        OnPropertyChanged(nameof(RegistroTexto));
    }

    public async Task SalvarClienteAsync()
    {
        if (string.IsNullOrWhiteSpace(Cliente.nome))
            throw new InvalidOperationException("Informe o nome do cliente.");

        if (string.IsNullOrWhiteSpace(Cliente.sigla))
            throw new InvalidOperationException("Informe a sigla do cliente.");

        Cliente.sigla = Cliente.sigla.Trim();
        Cliente.aniversario = Cliente.aniversario?.ToUniversalTime();
        Cliente.atualizacao = DateTimeOffset.UtcNow;
        Cliente.respatualizacao = _baseSettings.Username;

        using var db = DbConnectionFactory.Create();
        if (Cliente.id_cliente <= 0)
        {
            Cliente.id_cliente = await db.ExecuteScalarAsync<int>(
                $@"INSERT INTO comercial.clientes ({ClienteColumns})
                   VALUES ({ClienteParameters})
                   RETURNING id_cliente;",
                Cliente);

            Clientes.Add(new ClienteResumoModel
            {
                id_cliente = Cliente.id_cliente,
                nome = Cliente.nome,
                sigla = Cliente.sigla,
                cnpj = Cliente.cnpj
            });
            IndiceAtual = Clientes.Count - 1;
            _siglaCarregada = Cliente.sigla;
            OnPropertyChanged(nameof(RegistroTexto));
        }
        else
        {
            var siglaAnterior = _siglaCarregada;
            await db.ExecuteAsync(
                $@"UPDATE comercial.clientes
                   SET {ClienteSetColumns}
                   WHERE id_cliente = @id_cliente;",
                Cliente);

            if (!string.IsNullOrWhiteSpace(siglaAnterior) &&
                !string.Equals(siglaAnterior, Cliente.sigla, StringComparison.OrdinalIgnoreCase))
            {
                await db.ExecuteAsync(
                    @"UPDATE comercial.contatos
                      SET sigla = @novaSigla
                      WHERE sigla = @siglaAnterior;",
                    new { novaSigla = Cliente.sigla, siglaAnterior });
            }

            var resumo = Clientes.FirstOrDefault(x => x.id_cliente == Cliente.id_cliente);
            if (resumo != null)
            {
                resumo.nome = Cliente.nome;
                resumo.sigla = Cliente.sigla;
                resumo.cnpj = Cliente.cnpj;
            }

            _siglaCarregada = Cliente.sigla;
        }
    }

    public async Task CarregarContatosAsync()
    {
        if (Cliente.id_cliente <= 0 || string.IsNullOrWhiteSpace(Cliente.sigla))
        {
            Contatos = [];
            return;
        }

        using var db = DbConnectionFactory.Create();
        var data = (await db.QueryAsync<ClienteContatoModel>(
            @"SELECT ctid::text AS row_id,
                     sigla, contato, funcao, cargo, celular, email, inativo, brindedenatal,
                     respdec, vip, ok, ok1, cartaodenatal, conviteshowroom, convitefesta,
                     preconviteshowroom, pesquisabriefing, operacional, vipouro, revista,
                     aniversario, sexo, incluidopor, datainclusao, alteradopor, dataalteracao,
                     auxiliar, convitesrfase2, resp_financeiro, resp_operacoes
              FROM comercial.contatos
              WHERE sigla = @sigla
              ORDER BY contato NULLS LAST;",
            new { Cliente.sigla })).AsList();

        Contatos = new ObservableCollection<ClienteContatoModel>(data);
        foreach (var contato in Contatos)
            MarcarContatoOriginal(contato);
    }

    public async Task SalvarContatoAsync(ClienteContatoModel contato)
    {
        if (Cliente.id_cliente <= 0 || string.IsNullOrWhiteSpace(Cliente.sigla))
            throw new InvalidOperationException("Salve o cliente com sigla antes de incluir contatos.");

        ValidarContato(contato);

        contato.sigla = Cliente.sigla;
        NormalizarDatasContato(contato);
        using var db = DbConnectionFactory.Create();

        if (string.IsNullOrWhiteSpace(contato.row_id))
        {
            contato.incluidopor = _baseSettings.Username;
            contato.datainclusao = DateTimeOffset.UtcNow;

            await db.ExecuteAsync(
                $@"INSERT INTO comercial.contatos ({ContatoColumns})
                   VALUES ({ContatoParameters});",
                contato);
        }
        else
        {
            contato.alteradopor = _baseSettings.Username;
            contato.dataalteracao = DateTimeOffset.UtcNow;

            await db.ExecuteAsync(
                $@"UPDATE comercial.contatos
                   SET {ContatoSetColumns}
                   WHERE ctid = CAST(@row_id AS tid);",
                contato);
        }

        await CarregarContatosAsync();
    }

    public bool ContatoTemAlteracao(ClienteContatoModel contato)
    {
        if (string.IsNullOrWhiteSpace(contato.row_id))
            return ContatoTemAlgumaInformacao(contato);

        return !string.Equals(contato.original_signature, CriarAssinaturaContato(contato), StringComparison.Ordinal);
    }

    public void RemoverContatoNovoVazio(ClienteContatoModel contato)
    {
        if (!string.IsNullOrWhiteSpace(contato.row_id) || ContatoTemAlgumaInformacao(contato))
            return;

        Contatos.Remove(contato);
        if (ReferenceEquals(ContatoSelecionado, contato))
            ContatoSelecionado = null;
    }

    private static bool ContatoTemAlgumaInformacao(ClienteContatoModel contato)
    {
        return !string.IsNullOrWhiteSpace(contato.contato) ||
               !string.IsNullOrWhiteSpace(contato.funcao) ||
               !string.IsNullOrWhiteSpace(contato.cargo) ||
               !string.IsNullOrWhiteSpace(contato.email) ||
               !string.IsNullOrWhiteSpace(contato.celular);
    }

    private static void ValidarContato(ClienteContatoModel contato)
    {
        var campos = new List<string>();

        if (string.IsNullOrWhiteSpace(contato.contato))
            campos.Add("contato");

        if (string.IsNullOrWhiteSpace(contato.funcao))
            campos.Add("função");

        if (string.IsNullOrWhiteSpace(contato.cargo))
            campos.Add("cargo");

        if (string.IsNullOrWhiteSpace(contato.email))
            campos.Add("email");

        if (campos.Count > 0)
            throw new InvalidOperationException($"Informe os campos obrigatórios: {string.Join(", ", campos)}.");
    }

    private static void NormalizarDatasContato(ClienteContatoModel contato)
    {
        contato.aniversario = contato.aniversario?.ToUniversalTime();
        contato.datainclusao = contato.datainclusao?.ToUniversalTime();
        contato.dataalteracao = contato.dataalteracao?.ToUniversalTime();
    }

    private static void MarcarContatoOriginal(ClienteContatoModel contato)
    {
        contato.original_signature = CriarAssinaturaContato(contato);
    }

    private static string CriarAssinaturaContato(ClienteContatoModel contato)
    {
        return string.Join("|",
            contato.sigla,
            contato.contato,
            contato.funcao,
            contato.cargo,
            contato.celular,
            contato.email,
            contato.inativo,
            contato.brindedenatal,
            contato.respdec,
            contato.vip,
            contato.ok,
            contato.ok1,
            contato.cartaodenatal,
            contato.conviteshowroom,
            contato.convitefesta,
            contato.preconviteshowroom,
            contato.pesquisabriefing,
            contato.operacional,
            contato.vipouro,
            contato.revista,
            contato.aniversario?.ToUniversalTime().ToString("O"),
            contato.sexo,
            contato.auxiliar,
            contato.convitesrfase2,
            contato.resp_financeiro,
            contato.resp_operacoes);
    }

    private async Task CarregarClienteAtualAsync()
    {
        if (IndiceAtual < 0 || IndiceAtual >= Clientes.Count)
            return;

        using var db = DbConnectionFactory.Create();
        Cliente = await db.QueryFirstOrDefaultAsync<ClienteModel>(
            @"SELECT *
              FROM comercial.clientes
              WHERE id_cliente = @id_cliente;",
            new { Clientes[IndiceAtual].id_cliente }) ?? new ClienteModel();

        _siglaCarregada = Cliente.sigla;
        await CarregarContatosAsync();
        OnPropertyChanged(nameof(RegistroTexto));
    }

    private const string ClienteColumns = @"
        grupo, sigla, nome, tipo, internacional, rsocial, nome_fantasia, endereco,
        cidade, bairro, est, cep, cep_internacional, praca, regiao, pais, ddi, ddd,
        fone1, fone2, fax, cnpj, inscestad, aniversario, abl, lojas, abrasce,
        website, email, inativo, obsinatvo, atualizacao, respatualizacao, pisos,
        reg_atend, obs, fluxo, referencia_cliente, publico, area_construida,
        publico_mensal, qtdpiso, qtdvao, qtdcorredores, area_praca_principal,
        area_praca_aliment, lojas_ancora, ccm, opiniao, publico_classe,
        publico_sexo, publico_fluxo, distancia, totvs, aux, cnpj_retencao,
        sigla_externa";

    private const string ClienteParameters = @"
        @grupo, @sigla, @nome, @tipo, @internacional, @rsocial, @nome_fantasia, @endereco,
        @cidade, @bairro, @est, @cep, @cep_internacional, @praca, @regiao, @pais, @ddi, @ddd,
        @fone1, @fone2, @fax, @cnpj, @inscestad, @aniversario, @abl, @lojas, @abrasce,
        @website, @email, @inativo, @obsinatvo, @atualizacao, @respatualizacao, @pisos,
        @reg_atend, @obs, @fluxo, @referencia_cliente, @publico, @area_construida,
        @publico_mensal, @qtdpiso, @qtdvao, @qtdcorredores, @area_praca_principal,
        @area_praca_aliment, @lojas_ancora, @ccm, @opiniao, @publico_classe,
        @publico_sexo, @publico_fluxo, @distancia, @totvs, @aux, @cnpj_retencao,
        @sigla_externa";

    private const string ClienteSetColumns = @"
        grupo = @grupo, sigla = @sigla, nome = @nome, tipo = @tipo,
        internacional = @internacional, rsocial = @rsocial, nome_fantasia = @nome_fantasia,
        endereco = @endereco, cidade = @cidade, bairro = @bairro, est = @est,
        cep = @cep, cep_internacional = @cep_internacional, praca = @praca,
        regiao = @regiao, pais = @pais, ddi = @ddi, ddd = @ddd, fone1 = @fone1,
        fone2 = @fone2, fax = @fax, cnpj = @cnpj, inscestad = @inscestad,
        aniversario = @aniversario, abl = @abl, lojas = @lojas, abrasce = @abrasce,
        website = @website, email = @email, inativo = @inativo, obsinatvo = @obsinatvo,
        atualizacao = @atualizacao, respatualizacao = @respatualizacao, pisos = @pisos,
        reg_atend = @reg_atend, obs = @obs, fluxo = @fluxo,
        referencia_cliente = @referencia_cliente, publico = @publico,
        area_construida = @area_construida, publico_mensal = @publico_mensal,
        qtdpiso = @qtdpiso, qtdvao = @qtdvao, qtdcorredores = @qtdcorredores,
        area_praca_principal = @area_praca_principal,
        area_praca_aliment = @area_praca_aliment, lojas_ancora = @lojas_ancora,
        ccm = @ccm, opiniao = @opiniao, publico_classe = @publico_classe,
        publico_sexo = @publico_sexo, publico_fluxo = @publico_fluxo,
        distancia = @distancia, totvs = @totvs, aux = @aux,
        cnpj_retencao = @cnpj_retencao, sigla_externa = @sigla_externa";

    private const string ContatoColumns = @"
        sigla, contato, funcao, cargo, celular, email, inativo, brindedenatal,
        respdec, vip, ok, ok1, cartaodenatal, conviteshowroom, convitefesta,
        preconviteshowroom, pesquisabriefing, operacional, vipouro, revista,
        aniversario, sexo, incluidopor, datainclusao, alteradopor, dataalteracao,
        auxiliar, convitesrfase2, resp_financeiro, resp_operacoes";

    private const string ContatoParameters = @"
        @sigla, @contato, @funcao, @cargo, @celular, @email, @inativo, @brindedenatal,
        @respdec, @vip, @ok, @ok1, @cartaodenatal, @conviteshowroom, @convitefesta,
        @preconviteshowroom, @pesquisabriefing, @operacional, @vipouro, @revista,
        @aniversario, @sexo, @incluidopor, @datainclusao, @alteradopor, @dataalteracao,
        @auxiliar, @convitesrfase2, @resp_financeiro, @resp_operacoes";

    private const string ContatoSetColumns = @"
        funcao = @funcao, cargo = @cargo, celular = @celular, email = @email,
        inativo = @inativo, brindedenatal = @brindedenatal, respdec = @respdec,
        vip = @vip, ok = @ok, ok1 = @ok1, cartaodenatal = @cartaodenatal,
        conviteshowroom = @conviteshowroom, convitefesta = @convitefesta,
        preconviteshowroom = @preconviteshowroom, pesquisabriefing = @pesquisabriefing,
        operacional = @operacional, vipouro = @vipouro, revista = @revista,
        aniversario = @aniversario, sexo = @sexo, alteradopor = @alteradopor,
        dataalteracao = @dataalteracao, auxiliar = @auxiliar,
        convitesrfase2 = @convitesrfase2, resp_financeiro = @resp_financeiro,
        resp_operacoes = @resp_operacoes";
}
