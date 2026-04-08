using Comercial.Repositores;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Comercial.Services;

public class DocumentoWordService
{
    private const string VerdePadrao = "008000"; //#008000 -- 00B050
    private static readonly Dictionary<string, string> MapaTipos = new()
    {
        { "Proposta", "Quadro Quantitativo" },
        { "Opcional", "Opcional" },
        { "Complemento", "Complemento" },
        { "Venda", "Venda" }
    };

    public async Task CriarDocumentoFormatado(
        string templatePath,
        string destinoPath,
        IEnumerable<TemaModel> temas,
        Func<long, string, Task<IEnumerable<ItemTabelaModel>>> buscarItens)
    {
        File.Copy(templatePath, destinoPath, true);

        using var doc = WordprocessingDocument.Open(destinoPath, true);
        var body = doc.MainDocumentPart?.Document?.Body!;

        // 🔥 1️⃣ Encontrar marcador corretamente (mesmo se Word dividir em runs)
        var paragrafoMarcador = LocalizarParagrafoMarcador(body);

        if (paragrafoMarcador == null)
            throw new Exception("Tag [[INICIO_CONTEUDO]] não encontrada no template.");

        // 🔥 pegar elemento anterior antes de remover
        OpenXmlElement? pontoInsercao = paragrafoMarcador.PreviousSibling();

        if (pontoInsercao == null)
        {
            // inserir no início do body
            pontoInsercao = new Paragraph();
            body.InsertBefore(pontoInsercao, body.FirstChild);
        }

        paragrafoMarcador.Remove();


        int indice = 0;

        var listaTemas = temas.OrderBy(t => t.OrdemEscolha).ToList();
        bool primeiroTitulo = true;

        for (int i = 0; i < listaTemas.Count; i++)
        {
            var tema = listaTemas[i];

            indice++;
            string letra = ((char)(64 + indice)).ToString();

            string[] tipos =
            {
                "Proposta",
                "Opcional",
                "Complemento",
                "Venda",
            };       

            foreach (var tipo in tipos)
            {
                bool novaPagina = !primeiroTitulo;

                pontoInsercao = await CriarTabelaTipo(
                    body,
                    pontoInsercao,
                    buscarItens,
                    tema.IdTema,
                    tipo,
                    letra,
                    tema.TemaEscolhido,
                    novaPagina
                );

                primeiroTitulo = false;

                
            }
        }

        // 🔥 Buscar complemento geral em TODOS os temas
        var todosComplementos = new List<ItemTabelaModel>();

        foreach (var tema in listaTemas)
        {
            var itens = await buscarItens(
                tema.IdTema,
                "Complemento para todos os temas"
            );

            todosComplementos.AddRange(itens);
        }

        // 🔥 Remover duplicados (caso usuário tenha cadastrado em vários temas)
        todosComplementos = todosComplementos
            .GroupBy(i => new
            {
                i.Item,
                i.Descricao,
                i.LocalItem,
                i.Qtd,
                i.Dimensao,
                i.Obs,
                Bloco = i.Bloco ?? "GERAL"
            })
            .Select(g => g.First())
            .ToList();

        if (todosComplementos.Any())
        {
            var tituloGeral = CriarSubTitulo(
                "Complemento para todos os temas",
                true // nova página
            );

            body.InsertAfter(tituloGeral, pontoInsercao);
            pontoInsercao = tituloGeral;

            var grupos = todosComplementos
                .GroupBy(i => i.Bloco ?? "GERAL")
                .OrderBy(g => g.Key);

            foreach (var grupo in grupos)
            {
                // 🔹 Espaçamento antes da nova tabela
                var espacamento = new Paragraph(
                    new ParagraphProperties(
                        new SpacingBetweenLines { Before = "200", After = "100" }
                    )
                );

                body.InsertAfter(espacamento, pontoInsercao);
                pontoInsercao = espacamento;

                // 🔹 Nova tabela
                var table = new Table();
                table.AppendChild(CriarPropriedadesTabela());

                table.AppendChild(CriarLinhaBloco(grupo.Key));
                table.AppendChild(CriarLinhaCabecalho("Complemento"));

                foreach (var item in grupo.OrderBy(i => i.Item))
                    table.AppendChild(CriarLinhaDados(item));

                body.InsertAfter(table, pontoInsercao);
                pontoInsercao = table;
            }
        }

        var quebra = new Paragraph(
            new ParagraphProperties(new PageBreakBefore())
        );

        body.InsertAfter(quebra, pontoInsercao);
        pontoInsercao = quebra;

        var legenda = CriarTabelaLegenda();
        body.InsertAfter(legenda, pontoInsercao);
        pontoInsercao = legenda;

        var settingsPart = doc.MainDocumentPart.GetPartsOfType<DocumentSettingsPart>().FirstOrDefault();

        if (settingsPart == null)
        {
            settingsPart = doc.MainDocumentPart.AddNewPart<DocumentSettingsPart>();
            settingsPart.Settings = new Settings();
        }

        settingsPart.Settings.AppendChild(
            new UpdateFieldsOnOpen { Val = true }
        );

        settingsPart.Settings.Save();

        doc.MainDocumentPart.Document.Save();

        
    }

    private Table CriarTabelaLegenda()
    {
        var table = new Table();
        table.AppendChild(CriarPropriedadesTabela());

        // Linha título
        table.AppendChild(
            new TableRow(
                new TableCell(
                    new TableCellProperties(new GridSpan { Val = 3 }),
                    new Paragraph(
                        new Run(
                            new RunProperties(new Bold()),
                            new Text("Legenda")
                        )
                    )
                )
            )
        );

        // Linhas
        table.AppendChild(CriarLinhaLegenda("A=Área", "H=Altura", "P=Profundidade"));
        table.AppendChild(CriarLinhaLegenda("C=Complemento", "L=Largura", "Qtd=Quantidade"));
        table.AppendChild(CriarLinhaLegenda("Cm=Centímetro", "M=Metro", "V=Volume cúbico"));
        table.AppendChild(CriarLinhaLegenda("D=Diâmetro", "M²=Metro quadrado", ""));

        return table;
    }

    private TableRow CriarLinhaLegenda(string c1, string c2, string c3)
    {
        return new TableRow(
            CriarCelula(c1),
            CriarCelula(c2),
            CriarCelula(c3)
        );
    }

    // 🔎 Localiza marcador mesmo se estiver dividido em vários runs
    private Paragraph? LocalizarParagrafoMarcador(Body body)
    {
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            var textoCompleto = string.Concat(
                paragraph.Descendants<Text>().Select(t => t.Text)
            );

            if (textoCompleto.Contains("[[INICIO_CONTEUDO]]"))
                return paragraph;
        }

        return null;
    }

    private async Task<OpenXmlElement> CriarTabelaTipo(
        Body body,
        OpenXmlElement pontoInsercao,
        Func<long, string, Task<IEnumerable<ItemTabelaModel>>> buscarItens,
        long idTema,
        string tipoBD,
        string letraProjeto,
        string nomeTema,
        bool novaPagina)
    {
        var itens = (await buscarItens(idTema, tipoBD)).ToList();
        if (!itens.Any())
            return pontoInsercao;

        string tituloWord = MapaTipos.ContainsKey(tipoBD)
            ? MapaTipos[tipoBD]
            : tipoBD;

        // 🔹 Título do tipo
        var tituloSecao = CriarSubTitulo(
            $"Projeto {letraProjeto} - {ToTitleCasePtBr(nomeTema)} - {tituloWord}",
            novaPagina
        );

        body.InsertAfter(tituloSecao, pontoInsercao);
        pontoInsercao = tituloSecao;

        // 🔥 AGRUPAR POR BLOCO
        var grupos = itens
            .GroupBy(i => i.Bloco ?? "GERAL");

        foreach (var grupo in grupos)
        {
            // 🔹 Espaço antes de cada bloco
            var espacamento = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { Before = "150", After = "150" }
                )
            );

            body.InsertAfter(espacamento, pontoInsercao);
            pontoInsercao = espacamento;

            // 🔥 NOVA TABELA PARA CADA BLOCO
            var table = new Table();

            // 🔹 Propriedades próprias (instância nova)
            table.AppendChild(CriarPropriedadesTabela());

            // 🔹 Linha do bloco (título interno)
            table.AppendChild(CriarLinhaBloco(grupo.Key));

            // 🔹 Cabeçalho
            table.AppendChild(CriarLinhaCabecalho(tituloWord));

            // 🔹 Dados
            foreach (var item in grupo)
            {
                table.AppendChild(CriarLinhaDados(item));
            }

            body.InsertAfter(table, pontoInsercao);
            pontoInsercao = table;
        }

        return pontoInsercao;
    }

    private TableRow CriarLinhaBloco(string bloco)
    {
        return new TableRow(
            new TableCell(
                new TableCellProperties(
                    new GridSpan { Val = 6 }, // 🔥 Mescla 6 colunas
                    new Shading
                    {
                        Val = ShadingPatternValues.Clear
                    }
                ),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification { Val = JustificationValues.Center }
                    ),
                    new Run(
                        new RunProperties(
                            new Bold(),
                            new RunFonts { Ascii = "Verdana" },
                            new FontSize { Val = "20" } // 11pt
                        ),
                        new Text((bloco ?? "GERAL").ToUpper())
                    )
                )
            )
        );
    }
    /*
    private Paragraph CriarSubTitulo(string texto, bool novaPagina)
    {
        var props = new ParagraphProperties(
            new ParagraphStyleId { Val = "Ttulo1" },
            new SpacingBetweenLines { After = "150" },
            new Justification { Val = JustificationValues.Right }
        );

        if (novaPagina)
            props.Append(new PageBreakBefore());

        return new Paragraph(
            props,
            new Run(
                new RunProperties(
                    new RunFonts { Ascii = "Verdana" },
                    new FontSize { Val = "20" }
                ),
                new Text(texto)
            )
        );
    }
    */

    private Paragraph CriarSubTitulo(string texto, bool novaPagina)
    {
        var props = new ParagraphProperties(
            new ParagraphStyleId { Val = "Ttulo1" },
            new SpacingBetweenLines { After = "150" },
            new Justification { Val = JustificationValues.Right },
            // Adicionando a borda inferior aqui
            new ParagraphBorders(
                new BottomBorder()
                {
                    Val = BorderValues.Single,
                    Size = 4,      // 0.5 pt
                    Space = 1,     // Distância do texto
                    Color = "000000"
                }
            )
        );

        if (novaPagina)
            props.Append(new PageBreakBefore());

        return new Paragraph(
            props,
            new Run(
                new RunProperties(
                    new RunFonts { Ascii = "Verdana" },
                    new FontSize { Val = "22" }
                ),
                new Text(texto)
            )
        );
    }

    private TableProperties CriarPropriedadesTabela()
    {
        return new TableProperties(
            new TableWidth
            {
                Width = "5000",
                Type = TableWidthUnitValues.Pct
            },
            new TableBorders(

                // 🔹 BORDAS EXTERNAS — 1,5 pt sólida verde
                new TopBorder
                {
                    Val = BorderValues.Single,
                    Size = 12, // 1.5pt
                    Color = VerdePadrao
                },
                new BottomBorder
                {
                    Val = BorderValues.Single,
                    Size = 12,
                    Color = VerdePadrao
                },
                new LeftBorder
                {
                    Val = BorderValues.Single,
                    Size = 12,
                    Color = VerdePadrao
                },
                new RightBorder
                {
                    Val = BorderValues.Single,
                    Size = 12,
                    Color = VerdePadrao
                },

                // 🔹 BORDAS INTERNAS — 0,5 pt tracejada preta
                new InsideHorizontalBorder
                {
                    Val = BorderValues.Dotted,
                    Size = 4, // 0.5pt
                    Color = "000000"
                },
                new InsideVerticalBorder
                {
                    Val = BorderValues.Dotted,
                    Size = 4,
                    Color = "000000"
                }
            )
        );
    }

    private TableRow CriarLinhaCabecalho(string tipo)
    {
        return new TableRow(
            CriarCelula(texto: "Item", bold: true, fundoVerde: false, Hcentralizar: true),
            CriarCelula(texto: "Local", bold: true, fundoVerde: false, Hcentralizar: true),
            CriarCelula(texto: "Descrição", bold: true, fundoVerde: false, Hcentralizar: true),
            CriarCelula(texto: "Qtd", bold: true, fundoVerde: false, Hcentralizar: true),
            CriarCelula(texto: "Dimensão", bold: true, fundoVerde: false, Hcentralizar: true),
            CriarCelula(texto: "Observação", bold: true, fundoVerde: false, Hcentralizar: true)
        );
    }

    private TableRow CriarLinhaDados(ItemTabelaModel item)
    {
        return new TableRow(
            CriarCelula(texto: item.Item, Vcentralizar: true, Hcentralizar: true),
            CriarCelula(texto: item.LocalItem, Vcentralizar: true, Hcentralizar: false),
            CriarCelula(texto: item.Descricao, Vcentralizar: true, Hcentralizar: false),
            CriarCelula(texto: item.Qtd, Vcentralizar: true, Hcentralizar: true),
            CriarCelula(texto: item.Dimensao, Vcentralizar: true, Hcentralizar: false),
            CriarCelula(texto: item.Obs, Vcentralizar: true, Hcentralizar: false)
        );
    }

    private TableCell CriarCelula(string? texto, bool bold = false, bool fundoVerde = false, bool Vcentralizar = false, bool Hcentralizar = false)
    {
        var runProps = new RunProperties(
            new RunFonts { Ascii = "Verdana" },
            new FontSize { Val = "20" }
        );

        if (bold)
            runProps.Append(new Bold());

        var cellProps = new TableCellProperties();

        if (fundoVerde)
        {
            cellProps.Append(new Shading
            {
                Fill = VerdePadrao,
                Val = ShadingPatternValues.Clear
            });
        }

        // Controle da Centralização VERTICAL (Célula)
        if (Vcentralizar)
        {
            cellProps.Append(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
        }

        // Controle da Centralização HORIZONTAL (Parágrafo)
        var pProps = new ParagraphProperties();
        if (Hcentralizar)
        {
            pProps.Append(new Justification { Val = JustificationValues.Center });
        }

        return new TableCell(
            cellProps,
            new Paragraph(
                pProps,
                new Run(runProps, new Text(texto ?? ""))
            )
        );
    }

    public static string ToTitleCasePtBr(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return texto;

        var textInfo = new CultureInfo("pt-BR", false).TextInfo;
        texto = textInfo.ToTitleCase(texto.ToLower());

        // palavras que devem ficar minúsculas
        string[] minusculas =
        {
        "De", "Da", "Das", "Do", "Dos",
        "E", "Em", "Na", "Nas", "No", "Nos",
        "Com", "Para", "Por"
    };

        foreach (var palavra in minusculas)
        {
            texto = Regex.Replace(
                texto,
                $@"\b{palavra}\b",
                palavra.ToLower()
            );
        }

        return texto;
    }
}