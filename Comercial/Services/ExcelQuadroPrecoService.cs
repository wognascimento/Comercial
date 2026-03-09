using ClosedXML.Excel;
using Comercial.DataBase;
using Comercial.Repositores;


namespace Comercial.Services;

public class ExcelQuadroPrecoService
{
    private QuadroRepository? _repo;

    private List<string> colunas =
    [
        "BLOCO",
        "FAMILIA",
        "ITEM",
        "LOCAL",
        "DESCRICAO",
        "QTD",
        "INICIO",
        "PROPOSTAS",
        "FECHA",
        "SALDO",
        "DIMENSAO",
        "OBS",
        "OBS INTERNA",
        "CUSTO UNITARIO APURADO",
        "CUSTO UNITARIO ESTIMADO",
        "CUSTO MATERIAL UNITARIO",
        "CUSTO MATERIAL TOTAL",
        "PRECO EXCEL",
        "PRECO TOTAL EXCEL 12%",
        "DESCONTO AREA PROJECAO",
        "MAJORACAO",
        "VLR INDICE",
        "M3 UNITARIO",
        "CUBAGEM ESTIMADA",
        "M3 TOTAL",
        "N PESS HOMOLOGADA",
        "CUSTO TOTAL MOMADES",
        "N PESS ESTIMADO P M3",
        "CUSTO MO MOMADES",
        "CUSTO TOT MOM",
        "PRO+MMD",
        "PROJ ESTIMA +7,5%",
        $"{DateTime.Now.Year-1} RATEADO",
        $"{DateTime.Now.Year} ESTIMADO",
        "PRO+MMD+OPE+PROJ",
        "MARG 1,9",
        "MARG 2,0",
        "MARG 2,10",
        "MARG 2,20",
        "MARG 2,30",
        "MARG 2,40",
        "MARG 2,50",
        "MARG 2,70",
        "PREÇO EXCEL",
        "PRECO NF",
        "PRECO NF TOTAL",
        "CUSTO HISTORICO TOTAL",
        "PESO",
        "VALOR UNITARIO",
        "CUSTO TOT ITEM",
        "DESCONTO",
        "TOTAL",
        "VLRUNIDSUGERIDO",
        "CUSTO ITEM",
        "LEDML",
        "VLR LED",
        "TOTAL DESC",
        "ANOTACOES",
        "CUSTO TOTAL",
        "CUSTO HISTORICO",
        "CUSTO/M3",
        "PREÇO/M3",
        "PROJECAO AREA",
        "VALOR DESCONTO AREA PROJECAO",
        "PREÇO AREA PROJECAO",
    ];



    public async Task GerarExcelCusto(string caminho, long codBrief)
    {
        _repo = new QuadroRepository();
        using var _workbook = new XLWorkbook();
        var ws = _workbook.Worksheets.Add("CUSTO");

        ws.Column("B").Width = 60;
        ws.Column("C").Width = 23;

        ws.Cell("B1").Value = "";
        Borda(ws.Range("B1"));

        ws.Cell("C1").Value = DateTime.Now.Year - 1;
        Borda(
            ws.Range("C1"),
            corFonte: XLColor.Black,
            mesclado: false,
            fontSize: 9,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center,
            bloqueado: false,
            quebraLinha: false
        );

        // -----------------------------------------------------------
        // LINHA 2
        // -----------------------------------------------------------

        ws.Row(2).Height = 23;
        ws.Cell("B2").Value = "";
        Borda(ws.Range("B2"));

        ws.Cell("C2").Value = "";
        Borda(ws.Range("C2"));

        // -----------------------------------------------------------
        // LINHA 3
        // -----------------------------------------------------------

        ws.Cell("B3").Value = "PROJETO";
        Borda(
            ws.Range("B3"),
            mesclado: false,
            fontSize: 9,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Left,
            bloqueado: false,
            quebraLinha: false
        );

        ws.Cell("C3").Value = "";
        Borda(ws.Range("C3"));

        // -----------------------------------------------------------
        // LINHA 4
        // -----------------------------------------------------------

        ws.Cell("B4").Value = "CUSTO TOTAL PROJETO";
        Borda(ws.Range("B4"));

        ws.Cell("C4").Value = "";
        ws.Cell("C4").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C4"));

        // -----------------------------------------------------------
        // LINHA 6
        // -----------------------------------------------------------

        ws.Cell("B6").Value = "MATERIAIS";
        Borda(
            ws.Range("B6"),
            corFonte: XLColor.Black,
            mesclado: false,
            fontSize: 9,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Left,
            bloqueado: false,
            quebraLinha: false
        );

        ws.Cell("C6").Value = "";
        Borda(ws.Range("C6"));

        // -----------------------------------------------------------
        // LINHA 7
        // -----------------------------------------------------------

        ws.Cell("B7").Value = "CUSTO MATERIAL (VIDA ÚTIL) + MOBRA PROD";
        Borda(ws.Range("B7"));

        ws.Cell("C7").Value = 0;
        ws.Cell("C7").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C7"));

        // -----------------------------------------------------------
        // LINHA 8 – M3
        // -----------------------------------------------------------

        ws.Cell("B8").Value = "M3";
        Borda(
            ws.Range("B8"),
            corFundo: XLColor.FromArgb(192, 192, 192)
        );

        ws.Cell("C8").Value = 0;
        ws.Cell("C8").Style.NumberFormat.Format = "#,##0.00";
        Borda(
            ws.Range("C8"),
            corFundo: XLColor.FromArgb(192, 192, 192)
        );

        // -----------------------------------------------------------
        // LINHA 9 – Fórmula
        // -----------------------------------------------------------

        ws.Cell("B9").Value = "CUSTO MAT VUTIL P/ M3";
        Borda(
            ws.Range("B9"),
            corFundo: XLColor.FromArgb(255, 128, 128)
        );

        ws.Cell("C9").FormulaA1 = "=+C7/C8";
        ws.Cell("C9").Style.NumberFormat.Format = "#,##0.00";
        Borda(
            ws.Range("C9"),
            corFundo: XLColor.FromArgb(255, 128, 128)
        );

        // -----------------------------------------------------------
        // LINHA 11 – SERVIÇOS
        // -----------------------------------------------------------

        ws.Cell("B11").Value = "SERVIÇOS";
        Borda(ws.Range("B11"), negrito: true);

        ws.Cell("C11").Value = "";
        Borda(ws.Range("C11"));

        // -----------------------------------------------------------
        // LINHA 12
        // -----------------------------------------------------------

        ws.Cell("B12").Value = "PRAÇA";
        Borda(ws.Range("B12"));

        ws.Cell("C12").Value = "";
        Borda(ws.Range("C12"));

        // -----------------------------------------------------------
        // LINHA 13
        // -----------------------------------------------------------

        ws.Cell("B13").Value = "TOTAL SERVIÇOS (H NOITES) REALIZADO";
        Borda(ws.Range("B13"), corFundo: XLColor.FromArgb(192, 192, 192));

        ws.Cell("C13").Value = 0;
        ws.Cell("C13").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C13"), corFundo: XLColor.FromArgb(192, 192, 192));

        // -----------------------------------------------------------
        // LINHA 14
        // -----------------------------------------------------------

        ws.Cell("B14").Value = "VALOR MÉDIO PRAÇA";
        Borda(ws.Range("B14"));

        ws.Cell("C14").Value = 0;
        ws.Cell("C14").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C14"));

        // -----------------------------------------------------------
        // LINHA 15
        // -----------------------------------------------------------

        ws.Cell("B15").Value = "CUSTO TOTAL SERVIÇOS";
        Borda(ws.Range("B15"));

        ws.Cell("C15").Value = 0;
        ws.Cell("C15").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C15"));

        // -----------------------------------------------------------
        // LINHA 17
        // -----------------------------------------------------------

        ws.Cell("B17").Value = "VALOR OPERACIONAL (PASS ESTAD TRANSP PESS)";
        Borda(ws.Range("B17"), corFundo: XLColor.FromArgb(192, 192, 192));

        ws.Cell("C17").Value = 0;
        ws.Cell("C17").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C17"), corFundo: XLColor.FromArgb(192, 192, 192));

        // -----------------------------------------------------------
        // LINHA 19 – Fórmula TOTAL
        // -----------------------------------------------------------

        ws.Cell("B19").Value = "TOTAL CUSTOS (INTERNO)";
        Borda(ws.Range("B19"), corFundo: XLColor.FromArgb(255, 128, 128), negrito: true);

        ws.Cell("C19").FormulaA1 = "=+C17+C15+C7+C4";
        ws.Cell("C19").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C19"), corFundo: XLColor.FromArgb(255, 128, 128), negrito: true);

        // -----------------------------------------------------------
        // LINHA 21 – Fórmula
        // -----------------------------------------------------------

        ws.Cell("B21").Value = "MARKUP S/ CUSTO MATERIAIS (INTERNO)";
        Borda(ws.Range("B21"), corFundo: XLColor.FromArgb(204, 255, 255), negrito: true);

        ws.Cell("C21").FormulaA1 = "=C23/C7";
        ws.Cell("C21").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C21"), corFundo: XLColor.FromArgb(204, 255, 255), negrito: true);

        // -----------------------------------------------------------
        // LINHA 22
        // -----------------------------------------------------------

        ws.Cell("B22").Value = "MARKUP S/ CUSTO TOTAL (INTERNO)";
        Borda(ws.Range("B22"), corFundo: XLColor.FromArgb(204, 255, 255), negrito: true);

        ws.Cell("C22").Value = "";
        ws.Cell("C22").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C22"), corFundo: XLColor.FromArgb(204, 255, 255), negrito: true);

        // -----------------------------------------------------------
        // LINHA 23
        // -----------------------------------------------------------

        ws.Cell("B23").Value = "PREÇO LIQUIDO INTERNO (FAT-MKT-TRANS-IMP)";
        Borda(ws.Range("B23"), corFundo: XLColor.FromArgb(153, 204, 255), negrito: true);

        ws.Cell("C23").Value = "";
        ws.Cell("C23").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C23"), corFundo: XLColor.FromArgb(153, 204, 255), negrito: true);

        // -----------------------------------------------------------
        // LINHA 24
        // -----------------------------------------------------------
        ws.Cell("B24").Value = "APOIO (REALIZADO))";
        Borda(ws.Range("B24"));

        ws.Cell("C24").Value = "";
        ws.Cell("C24").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C24"));
        // -----------------------------------------------------------
        // LINHA 25
        // -----------------------------------------------------------
        ws.Cell("B25").Value = "TRANSPORTE (REALIZADO)";
        Borda(ws.Range("B25"));

        ws.Cell("C25").Value = "";
        ws.Cell("C25").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C25"));
        // -----------------------------------------------------------
        // LINHA 26
        // -----------------------------------------------------------
        ws.Cell("B26").Value = "TRANSPORTE P/ CONTA";
        Borda(ws.Range("B26"));

        ws.Cell("C26").Value = "";
        ws.Cell("C26").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C26"));

        // -----------------------------------------------------------
        // LINHA 27
        // -----------------------------------------------------------
        ws.Cell("B27").Value = "ROYALTIES (REALIZADO)";
        Borda(ws.Range("B27"));

        ws.Cell("C27").Value = "";
        ws.Cell("C27").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C27"));

        // -----------------------------------------------------------
        // LINHA 28
        // -----------------------------------------------------------
        ws.Cell("B28").Value = "ISS RETIDO 2X (REALIZADO)";
        Borda(ws.Range("B28"));

        ws.Cell("C28").Value = "";
        ws.Cell("C28").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C28"));

        // -----------------------------------------------------------
        // LINHA 29
        // -----------------------------------------------------------
        ws.Cell("B29").Value = "ISS RETIDO 2X REAL";
        Borda(ws.Range("B29"));

        ws.Cell("C29").Value = "";
        ws.Cell("C29").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C29"));

        // -----------------------------------------------------------
        // LINHA 30
        // -----------------------------------------------------------
        ws.Cell("B30").Value = "MANUTENÇÃO ADIC 2 OU 3 X P SEMANAL";
        Borda(ws.Range("B30"));

        ws.Cell("C30").Value = "";
        ws.Cell("C30").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C30"));

        // -----------------------------------------------------------
        // LINHA 31
        // -----------------------------------------------------------
        ws.Cell("B31").Value = "OUTROS (OPERAÇÃO, ETC)";
        Borda(ws.Range("B31"));

        ws.Cell("C31").Value = "";
        ws.Cell("C31").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C31"));

        // -----------------------------------------------------------
        // LINHA 32
        // -----------------------------------------------------------
        ws.Cell("B32").Value = "IMPOSTOS";
        Borda(ws.Range("B32"));

        ws.Cell("C32").Value = "";
        ws.Cell("C32").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C32"));

        // -----------------------------------------------------------
        // LINHA 33
        // -----------------------------------------------------------
        ws.Cell("B33").Value = "MARKUP S/ CUSTO TOTAL (EXTERNO)";
        Borda(ws.Range("B33"), corFundo: XLColor.FromArgb(204, 255, 255), negrito: true);

        ws.Cell("C33").Value = "";
        ws.Cell("C33").FormulaA1 = "=+C7/C35";
        ws.Cell("C33").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C33"), corFundo: XLColor.FromArgb(204, 255, 255), negrito: true);

        // -----------------------------------------------------------
        // LINHA 34
        // -----------------------------------------------------------
        ws.Cell("B34").Value = "MARKUP S/ CUSTO TOTAL (EXTERNO)";
        Borda(ws.Range("B34"), corFundo: XLColor.FromArgb(204, 255, 255), negrito: true);

        ws.Cell("C34").Value = "";
        ws.Cell("C34").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C34"), corFundo: XLColor.FromArgb(204, 255, 255), negrito: true);

        // -----------------------------------------------------------
        // LINHA 35
        // -----------------------------------------------------------
        ws.Cell("B35").Value = "PREÇO LÍQUIDO TOTAL ";
        Borda(ws.Range("B35"), corFundo: XLColor.FromArgb(153, 204, 255), negrito: true);

        ws.Cell("C35").Value = "";
        ws.Cell("C35").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C35"), corFundo: XLColor.FromArgb(153, 204, 255), negrito: true);

        // -----------------------------------------------------------
        // LINHA 36
        // -----------------------------------------------------------
        ws.Cell("B36").Value = "DESCONTO PAGAMENTO";
        Borda(ws.Range("B36"));

        ws.Cell("C36").Value = "";
        ws.Cell("C36").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C36"));

        // -----------------------------------------------------------
        // LINHA 37
        // -----------------------------------------------------------
        ws.Cell("B37").Value = "DESCONTO REDE";
        Borda(ws.Range("B37"));

        ws.Cell("C37").Value = "";
        ws.Cell("C37").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C37"));

        // -----------------------------------------------------------
        // LINHA 38
        // -----------------------------------------------------------
        ws.Cell("B38").Value = "DESCONTO ESPECIAL";
        Borda(ws.Range("B38"));

        ws.Cell("C38").Value = "";
        ws.Cell("C38").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C38"));

        // -----------------------------------------------------------
        // LINHA 39
        // -----------------------------------------------------------
        ws.Cell("B39").Value = "DESCONTO 2 OU 3 ANOS";
        Borda(ws.Range("B39"));

        ws.Cell("C39").Value = "";
        ws.Cell("C39").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C39"));

        // -----------------------------------------------------------
        // LINHA 40
        // -----------------------------------------------------------
        ws.Cell("B40").Value = "DESCONTO TOTAL APLICADO";
        Borda(ws.Range("B40"), corFundo: XLColor.FromArgb(255, 128, 128));

        ws.Cell("C40").Value = "";
        ws.Cell("C40").FormulaA1 = "=+C39+C38+C37+C36";
        ws.Cell("C40").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C40"), corFundo: XLColor.FromArgb(255, 128, 128));

        // -----------------------------------------------------------
        // LINHA 41
        // -----------------------------------------------------------
        ws.Cell("B41").Value = "PREÇO BRUTO TOTAL";
        Borda(ws.Range("B41"), corFundo: XLColor.FromArgb(153, 204, 255), negrito: true);

        ws.Cell("C41").Value = "";
        ws.Cell("C41").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C41"), corFundo: XLColor.FromArgb(153, 204, 255), negrito: true);

        // -----------------------------------------------------------
        // LINHA 42
        // -----------------------------------------------------------
        ws.Cell("B42").Value = "LUCRO TOTAL BRUTO";
        Borda(ws.Range("B42"));

        ws.Cell("C42").Value = "";
        ws.Cell("C42").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C42"));

        // -----------------------------------------------------------
        // LINHA 43
        // -----------------------------------------------------------
        ws.Cell("B43").Value = "MARGEM 2025";
        Borda(ws.Range("B43"));

        ws.Cell("C43").Value = "";
        ws.Cell("C43").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C43"));

        // -----------------------------------------------------------
        // LINHA 44
        // -----------------------------------------------------------
        ws.Cell("B44").Value = "MARGEM MÉDIA EMPRESA 0,49 / MARGEM MÉDIA EMPRESA IDEAL 0,56";
        Borda(ws.Range("B44"));

        ws.Cell("C44").Value = "";
        ws.Cell("C44").Style.NumberFormat.Format = "#,##0.00";
        Borda(ws.Range("C44"));


        ws.Column("C").Width = 50;
        ws.Column("D").Width = 16;
        ws.Column("E").Width = 16;
        ws.Column("F").Width = 18;
        ws.Column("G").Width = 22;
        ws.Column("H").Width = 16;
        ws.Column("I").Width = 16;
        ws.Column("J").Width = 16;

        var range = ws.Range("B53:J53");
        range.Merge();
        range.Value = $"QUADRO PREÇO {DateTime.Now.Year} - DETALHADO";
        Borda(range, corFundo: XLColor.FromArgb(192, 192, 192), negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center);

        range = ws.Range("B54:B57");
        range.Merge();
        range.Value = $"Tema";
        Borda(range, corFonte: XLColor.FromArgb(0, 128, 0), negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center);

        range = ws.Range("C54:C57");
        range.Merge();
        range.Value = $"Projeto";
        Borda(range, corFonte: XLColor.FromArgb(0, 128, 0), negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center);

        range = ws.Range("D54:D57");
        range.Merge();
        range.Value = $"Preço Base";
        Borda(range, corFonte: XLColor.FromArgb(0, 128, 0), negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center);

        range = ws.Range("E54:H54");
        range.Merge();
        range.Value = $"Descontos";
        Borda(range, corFonte: XLColor.Red, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center);

        range = ws.Range("E55");
        range.Value = $"Condição de Pagto.";
        Borda(range, corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("E56");
        range.Value = $"8 X a partir de Maio";
        Borda(range, corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("F55:F56");
        range.Merge();
        range.Value = $"Aprovação 2 Anos - {string.Join("/", Enumerable.Range(DateTime.Now.Year, 2).Select(a => a.ToString()))}";
        Borda(range, corFonte: XLColor.Blue, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("G55:G56");
        range.Merge();
        range.Value = $"Rede";
        Borda(range, corFonte: XLColor.Green, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("H55:H56");
        range.Merge();
        range.Value = $"Especial";
        Borda(range, corFonte: XLColor.DarkRed, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("I54:I57");
        range.Merge();
        range.Value = $"Preço Líquido";
        Borda(range, corFonte: XLColor.Green, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("J54:J55");
        range.Merge();
        range.Value = $"Valor da Parcela";
        Borda(range, corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("J56");
        range.Value = $"N.º de Parcelas";
        Borda(range, corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("E57");
        range.Value = $"";
        range.Style.NumberFormat.Format = "0.00%";
        Borda(range, corFundo:XLColor.Yellow , corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("F57");
        range.Value = $"";
        range.Style.NumberFormat.Format = "0.00%";
        Borda(range, corFundo:XLColor.Yellow , corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("G57");
        range.Value = $"";
        range.Style.NumberFormat.Format = "0.00%";
        Borda(range, corFundo:XLColor.Yellow , corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("H57");
        range.Value = $"";
        range.Style.NumberFormat.Format = "0.00%";
        Borda(range, corFundo:XLColor.Yellow , corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        range = ws.Range("J57");
        range.Value = $"4 OU 6";
        Borda(range, corFundo:XLColor.Yellow , corFonte: XLColor.Black, negrito: true, fontSize: 10, alinhamento: XLAlignmentHorizontalValues.Center, quebraLinha: true);

        var temas = await _repo.GetTemasPropostaAsync(codBrief);
        string[] tipos =
            [
                "Proposta",
                "Opcional",
                "Complemento",
                "Complemento para todos os temas",
                "Venda",
            ];
        int linhaBase = 58;
        foreach (var tema in temas)
        {
            foreach (var tipo in tipos)
            {

                var itens = await _repo.GetDetalhesPrecoAsync(codBrief, tema.IdTema, tipo);

                if (!itens.Any())
                    continue;

                int linhas = itens.Count;

                // =============================
                // Coluna B (título agrupado)
                // =============================
                var tituloRange = ws.Range($"B{linhaBase}:B{linhaBase + linhas}");
                tituloRange.Merge();
                Borda(
                    tituloRange,
                    fontSize: 10,
                    corFonte: XLColor.Green,
                    negrito: true
                );

                ws.Cell(linhaBase, 2).Value =
                    tipo.Equals("Proposta", StringComparison.OrdinalIgnoreCase)
                        ? itens.First().tema
                        : tipo;

                // =============================
                // Coluna C (Bloco)
                // =============================
                for (int i = 0; i < linhas; i++)
                {
                    ws.Cell(linhaBase + i, 3).Value = itens[i].bloco;
                    Borda(
                        ws.Cell(linhaBase + i, 3).AsRange(),
                        fontSize: 8
                    );
                }

                ws.Cell(linhaBase + linhas, 3).Value = "TOTAL";
                Borda(
                    ws.Cell(linhaBase + linhas, 3).AsRange(),
                    fontSize: 8,
                    negrito: true,
                    alinhamento: XLAlignmentHorizontalValues.Right,
                    corFundo: XLColor.LightGreen
                );

                // =============================
                // Coluna D (Valor base)
                // =============================
                for (int i = 0; i < linhas; i++)
                {
                    ws.Cell(linhaBase + i, 4).Value = itens[i].somadetotal;
                    Borda(
                        ws.Cell(linhaBase + i, 4).AsRange(),
                        fontSize: 8
                    );
                }

                ws.Cell(linhaBase + linhas, 4).FormulaA1 = $"SUM(D{linhaBase}:D{linhaBase + linhas - 1})";
                Borda(
                       ws.Cell(linhaBase + linhas, 4).AsRange(),
                       fontSize: 8,
                       negrito: true,
                       alinhamento: XLAlignmentHorizontalValues.Right,
                       corFundo: XLColor.LightGreen
                   );

                string formatoMoeda = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                ws.Range(linhaBase, 4, linhaBase + linhas, 4).Style.NumberFormat.Format = formatoMoeda;

                // =============================
                // Colunas calculadas
                // =============================

                // E = D * C57
                GerarColunaFormula(ws, linhaBase, linhas, 5, row => $"D{row}*$E$57", formatoMoeda);

                // F = D * C57
                GerarColunaFormula(ws, linhaBase, linhas, 6, row => $"D{row}*$F$57", formatoMoeda);

                // G = D * C57
                GerarColunaFormula(ws, linhaBase, linhas, 7, row => $"D{row}*$G$57", formatoMoeda);

                // H = D * C57
                GerarColunaFormula(ws, linhaBase, linhas, 8, row => $"D{row}*$H$57", formatoMoeda);

                // I = D - E - F - G - H
                GerarColunaFormula(ws, linhaBase, linhas, 9, row => $"D{row}-E{row}-F{row}-G{row}-H{row}", formatoMoeda);

                // J = I / C57 (percentual)
                GerarColunaFormula(ws, linhaBase, linhas, 10, row => $"I{row}/$J$57", formatoMoeda);

                linhaBase += linhas + 1;


            }

            // Linha separadora
            var separador = ws.Range($"B{linhaBase}:J{linhaBase}");
            separador.Merge();
            //separador.Style.Fill.BackgroundColor = XLColor.GreenYellow;
            Borda(range: separador, corFundo: XLColor.GreenYellow);

           linhaBase++;
        }

        string[] comple =
            [
                "TRANSPORTE",
                "MUNK / PLATAFORMA ETC (APÓS VT)",
                "ALPINISTA (APÓS VT)",
                "IGNIFUGAÇÃO COM LAUDO",
                "MANUTENÇÃO SEMANAL",
                "OPERAÇÃO",
                "OUTROS",
            ];

        foreach (var item in comple)
        {
            var _range = ws.Range($"B{linhaBase}:H{linhaBase}");
            _range.Value = item;
            _range.Merge();
            Borda(
                _range,
                fontSize: 10,
                corFonte: XLColor.Blue,
                negrito: true
            );

            ws.Cell(linhaBase, 9).Value = "";
            Borda(
                ws.Cell(linhaBase, 9).AsRange(),
                fontSize: 8,
                negrito: true,
                alinhamento: XLAlignmentHorizontalValues.Right
            );

            ws.Cell(linhaBase, 10).Value = "";
            Borda(
                ws.Cell(linhaBase, 10).AsRange(),
                fontSize: 8,
                negrito: true,
                alinhamento: XLAlignmentHorizontalValues.Right
            );

            linhaBase++;
        }

        var _Trange = ws.Range($"B{linhaBase}:C{linhaBase}");
        _Trange.Value = "TOTAL GERAL";
        _Trange.Merge();
        Borda(
            _Trange,
            fontSize: 10,
            corFundo: XLColor.Green,
            negrito: true
        );

        ws.Cell(linhaBase, 4).Value = "";
        Borda(
            ws.Cell(linhaBase, 4).AsRange(),
            fontSize: 8,
            corFundo: XLColor.Green,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center
        );

        ws.Cell(linhaBase, 5).Value = "";
        Borda(
            ws.Cell(linhaBase, 5).AsRange(),
            fontSize: 8,
            corFundo: XLColor.Green,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center
        );

        ws.Cell(linhaBase, 6).Value = "";
        Borda(
            ws.Cell(linhaBase, 6).AsRange(),
            fontSize: 8,
            corFundo: XLColor.Green,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center
        );

        ws.Cell(linhaBase, 7).Value = "";
        Borda(
            ws.Cell(linhaBase, 7).AsRange(),
            fontSize: 8,
            corFundo: XLColor.Green,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center
        );

        ws.Cell(linhaBase, 8).Value = "";
        Borda(
            ws.Cell(linhaBase, 8).AsRange(),
            fontSize: 8,
            corFundo: XLColor.Green,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center
        );

        ws.Cell(linhaBase, 9).Value = "";
        Borda(
            ws.Cell(linhaBase, 9).AsRange(),
            fontSize: 8,
            corFundo: XLColor.Green,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center
        );

        ws.Cell(linhaBase, 10).Value = "";
        Borda(
            ws.Cell(linhaBase, 10).AsRange(),
            fontSize: 8,
            corFundo: XLColor.Green,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center
        );

        linhaBase++;

        Borda(
            ws.Range(linhaBase, 2, linhaBase, 10),
            mesclado: true,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Center,
            corFundo: XLColor.LightGray
        );

        linhaBase++;

        ws.Range(linhaBase, 2, linhaBase, 10).Value = "Obs.: Nos valores acima informados não está incluso o frete para transporte dos materiais, bem como qualquer equipamento de apoio necessário para instalação dos elementos localizados acima de 6,00m sem acesso (munck, plataforma, etc.).";
        Borda(
            ws.Range(linhaBase, 2, linhaBase, 10),
            mesclado: true,
            negrito: true,
            corFonte: XLColor.Blue
        );

        _workbook.SaveAs(caminho);
    }


    public async Task GerarExcelCustoDetalhado(string caminho, long codBrief)
    {
        _repo = new QuadroRepository();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("CUSTO");

        int linha = 1;
        int inicio = 0;
        int lInicio = 0;
        int lFim = 0;

        var temas = await _repo.GetTemasDetalheAsync(codBrief);

        foreach (var tema in temas)
        {
            var tipos = await _repo.GetTiposAsync(tema.codproposta, tema.idtema_ordem);

            foreach (var tipo in tipos)
            {
                // 🔹 Cabeçalho igual VBA
                var headerRange = ws.Range($"A{linha}:BM{linha}");
                headerRange.Merge();
                headerRange.Value = $"{tema.sigla} - {tema.tema_escolhido} - {tipo.Tipo.ToUpper()}";

                headerRange.Style.Font.FontSize = 20;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Blue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                linha++;

                var dados = await _repo.GetQuadroAsync(
                    tema.codproposta,
                    tema.idtema_ordem,
                    tipo.Tipo);

                var blocos = dados.GroupBy(b => b.bloco).Select(b => b.Key).ToList();
                bool escreveuCabecalho = false;

                int primeiraLinhaTipo = 0;

                // logo após escrever o cabeçalho do tipo
                lInicio = linha; // primeira linha de dados do tipo

                foreach (var bloco in blocos)
                {

                    var sTotal = dados.Where(b => b.bloco == bloco).Count();
                    

                    foreach (var row in dados.Where(b => b.bloco == bloco))
                    {
                        if (!escreveuCabecalho)
                        {
                            int col = 1;
                            foreach (var coluna in colunas)
                            {
                                ws.Cell(linha, col).Value = coluna;
                                ws.Cell(linha, col).Style.Font.Bold = true;
                                col++;
                            }
                            escreveuCabecalho = true;
                            linha++;
                            //lInicio = inicio;
                            //inicio = linha;
                        }

                        if (primeiraLinhaTipo == 0)
                            primeiraLinhaTipo = linha;

                        ws.Cell(linha, "A").Value = row.bloco;
                        ws.Cell(linha, "B").Value = row.familia;
                        ws.Cell(linha, "C").Value = row.item;
                        ws.Cell(linha, "D").Value = row.localitem;
                        ws.Cell(linha, "E").Value = row.descricao;
                        ws.Cell(linha, "F").Value = row.qtd;
                        ws.Cell(linha, "G").Value = row.inicio;
                        ws.Cell(linha, "H").Value = row.propostas;
                        ws.Cell(linha, "I").Value = row.fecha;
                        ws.Cell(linha, "J").FormulaA1 = $"=G{linha}-I{linha}-H{linha}-F{linha}";
                        ws.Cell(linha, "K").Value = row.dimensao;
                        ws.Cell(linha, "L").Value = row.obs;
                        ws.Cell(linha, "M").Value = row.obsinterna;
                        ws.Cell(linha, "N").Value = row.custounitarioapurado;
                        ws.Cell(linha, "O").Value = row.custounitarioestimado;
                        ws.Cell(linha, "P").Value = row.custo_material_unitario;
                        ws.Cell(linha, "Q").FormulaA1 = $"=P{linha}*F{linha}";
                        ws.Cell(linha, "R").Value = row.preco_excel;
                        ws.Cell(linha, "S").FormulaA1 = $"=R{linha}*F{linha}";
                        ws.Cell(linha, "T").Value = row.desconto_area_projecao;
                        ws.Cell(linha, "U").Value = row.majoracao;
                        ws.Cell(linha, "V").Value = row.vlr_indice;
                        ws.Cell(linha, "W").Value = row.m3_unitario;
                        ws.Cell(linha, "X").Value = row.cubagem_estimada;
                        ws.Cell(linha, "Y").FormulaA1 = $"=W{linha}*F{linha}";
                        ws.Cell(linha, "Z").Value = row.n_pess_homologada;
                        ws.Cell(linha, "AA").FormulaA1 = $"=Z{linha}*245";
                        ws.Cell(linha, "AB").FormulaA1 = $"=Y{linha}*1.5";
                        ws.Cell(linha, "AC").FormulaA1 = $"=AB{linha}*245";
                        ws.Cell(linha, "AD").FormulaA1 = $"=IF(AA{linha}=0,AC{linha},AA{linha})";
                        ws.Cell(linha, "AE").FormulaA1 = $"=AD{linha}+Q{linha}";
                        ws.Cell(linha, "AF").FormulaA1 = $"=AE{linha}*0.078";
                        ws.Cell(linha, "AG").FormulaA1 = $"=0*AE{linha}";
                        ws.Cell(linha, "AH").FormulaA1 = $"=0*Q{linha}";
                        ws.Cell(linha, "AI").FormulaA1 = $"=AE{linha}+AF{linha}+AH{linha}";
                        ws.Cell(linha, "AJ").FormulaA1 = $"=AI{linha}*1.9";
                        ws.Cell(linha, "AK").FormulaA1 = $"=AI{linha}*2";
                        ws.Cell(linha, "AL").FormulaA1 = $"=AI{linha}*2.1";
                        ws.Cell(linha, "AM").FormulaA1 = $"=AI{linha}*2.2";
                        ws.Cell(linha, "AN").FormulaA1 = $"=AI{linha}*2.3";
                        ws.Cell(linha, "AO").FormulaA1 = $"=AI{linha}*2.4";
                        ws.Cell(linha, "AP").FormulaA1 = $"=AI{linha}*2.5";
                        ws.Cell(linha, "AQ").FormulaA1 = $"=AI{linha}*2.7";
                        ws.Cell(linha, "AR").Value = row.preco_excel;
                        ws.Cell(linha, "AS").Value = row.preco_nf;
                        ws.Cell(linha, "AT").Value = row.preco_nf_total;
                        ws.Cell(linha, "AU").Value = row.custo_historico_total;
                        ws.Cell(linha, "AV").Value = row.peso;
                        ws.Cell(linha, "AW").Value = row.valor_unitario;
                        ws.Cell(linha, "AX").Value = row.custo_tot_item;
                        ws.Cell(linha, "AY").Value = row.desconto;
                        ws.Cell(linha, "AZ").Value = row.total;
                        ws.Cell(linha, "BA").Value = row.vlrunidsugerido;
                        ws.Cell(linha, "BB").Value = row.custo_item;
                        ws.Cell(linha, "BC").Value = row.ledml;
                        ws.Cell(linha, "BD").Value = row.vlr_led;
                        ws.Cell(linha, "BE").Value = row.total_desc;
                        ws.Cell(linha, "BF").Value = row.anotacoes;
                        ws.Cell(linha, "BG").Value = row.custo_total;
                        ws.Cell(linha, "BH").Value = row.custo_historico;
                        ws.Cell(linha, "BI").FormulaA1 = $"=Q{linha}/Y{linha}";
                        ws.Cell(linha, "BJ").FormulaA1 = $"=S{linha}/Y{linha}";
                        ws.Cell(linha, "BK").Value = row.projecao_area;
                        ws.Cell(linha, "BL").Value = row.valor_desconto_area_projecao;
                        ws.Cell(linha, "BM").FormulaA1 = $"=(BL{linha}*F{linha})*3";

                        linha++;
                    }

                    // subtotal do bloco
                    int linhaFinalBloco = linha - 1;
                    int linhaInicialBloco = linhaFinalBloco - sTotal + 1;

                    ws.Cell(linha, "A").Value = $"{bloco} Total";

                    ws.Cell(linha, "F").FormulaA1 = $"SUBTOTAL(109, F{linhaInicialBloco}:F{linhaFinalBloco})";
                    ws.Range($"A{linha}:BM{linha}").Style.Fill.BackgroundColor = XLColor.Yellow;
                    ws.Range($"A{linha}:BM{linha}").Style.Font.Bold = true;

                    linha++; // espaçamento
                }

                // 🔹 subtotal geral do tipo
                int ultimaLinhaTipo = linha - 1;

                ws.Cell(linha, "A").Value = $"Total Geral";

                ws.Cell(linha, "F").FormulaA1 =$"SUBTOTAL(109, F{primeiraLinhaTipo}:F{ultimaLinhaTipo})";
                ws.Range($"A{linha}:BM{linha}").Style.Fill.BackgroundColor = XLColor.Orange;
                ws.Range($"A{linha}:BM{linha}").Style.Font.Bold = true;

                linha++;

            }

            //linha += 2;
            linha++;

            ws.Cell(linha, "J").Value = $"M3 {DateTime.Now.Year -1}";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange());
            
            linha++;

            ws.Cell(linha, "J").Value = $"M3 {DateTime.Now.Year}";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=Y{linha - 3}*1.05";
            linha++;

            ws.Cell(linha, "J").Value = $"MOBRA (M3X1,58)";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=L{linha - 1}*1.58";
            linha++;

            ws.Cell(linha, "J").Value = $"PRAÇA";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange());
            linha++;

            ws.Cell(linha, "J").Value = $"VALOR MOBRA UNIT PRAÇA";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange());
            ws.Cell(linha, "L").Value = 280;
            linha++;

            ws.Cell(linha, "J").Value = $"MOBRA VALOR (MOBRAXPUNIT PRAÇA)";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=L{linha - 1}*L{linha-3}";
            linha++;

            ws.Cell(linha, "J").Value = $"MOBRA MMD {DateTime.Now.Year}";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=AB{linha - 8}";
            linha++;

            ws.Cell(linha, "J").Value = $"OPERA {DateTime.Now.Year -1}";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange());
            linha++;

            ws.Cell(linha, "J").Value = $"OPERA {DateTime.Now.Year} (ESTIMATIVA {DateTime.Now.Year})";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange());
            linha++;

            ws.Cell(linha, "J").Value = $"CUSTO PROJETOS";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=AF{linha - 11}";
            linha++;

            ws.Cell(linha, "J").Value = $"CUSTO TOTAL (OPE {DateTime.Now.Year} + MOBRA + PROJ + CUSTO MAT)";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=L{linha - 1}+L{linha - 2}+L{linha - 5}+Q{linha - 12}";
            linha++;

            ws.Cell(linha, "J").Value = $"CUSTO P/ M3";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=L{linha - 1}/L{linha - 9}";
            linha++;

            ws.Cell(linha, "J").Value = $"PREÇO IDEAL (2.2)";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=L{linha - 2}*2.2";
            linha++;

            ws.Cell(linha, "J").Value = $"PREÇO EXCEL ZEFI";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=S{linha - 15}";
            linha++;

            ws.Cell(linha, "J").Value = $"PREÇO P M3";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=L{linha - 1}/L{linha - 12}";
            linha++;

            ws.Cell(linha, "J").Value = $"PREÇO EXCEL DESC 10%";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=L{linha - 2}*0.9";
            linha++;

            ws.Cell(linha, "J").Value = $"PREÇO P M3 {DateTime.Now.Year}";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 2150;
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true);
            ws.Cell(linha, "L").FormulaA1 = $"=L{linha-1}/L{linha - 15}";
            linha++;

            ws.Cell(linha, "J").Value = $"PREÇO  ATUAL  POR MÉDIA  M3";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 3000;
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true, corFundo: XLColor.FromArgb(112, 48, 160));
            ws.Cell(linha, "L").FormulaA1 = $"=K{linha}*L{linha - 16}";
            linha++;

            ws.Cell(linha, "J").Value = $"MÉDIA PREÇO M3 NIVEL 1";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 3100;
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true, corFundo: XLColor.FromArgb(255, 0, 0));
            ws.Cell(linha, "L").FormulaA1 = $"=K{linha}*L{linha - 17}";
            linha++;

            ws.Cell(linha, "J").Value = $"MÉDIA PREÇO M3 NIVEL 2";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 3200;
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true, corFundo: XLColor.FromArgb(255, 192, 0));
            ws.Cell(linha, "L").FormulaA1 = $"=K{linha}*L{linha - 18}";
            linha++;

            ws.Cell(linha, "J").Value = $"MÉDIA PREÇO M3 NIVEL 3";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 3300;
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true, corFundo: XLColor.FromArgb(218, 238, 243));
            ws.Cell(linha, "L").FormulaA1 = $"=K{linha}*L{linha - 19}";
            linha++;

            ws.Cell(linha, "J").Value = $"MÉDIA PREÇO M3 NIVEL 4";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 3400;
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true, corFundo: XLColor.FromArgb(255, 255, 0));
            ws.Cell(linha, "L").FormulaA1 = $"=K{linha}*L{linha - 20}";
            linha++;

            ws.Cell(linha, "J").Value = $"MÉDIA PREÇO M3 NIVEL 5";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 3500;
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true, corFundo: XLColor.FromArgb(146, 208, 80));
            ws.Cell(linha, "L").FormulaA1 = $"=K{linha}*L{linha - 21}";
            linha++;

            ws.Cell(linha, "J").Value = $"MÉDIA PREÇO M3 NIVEL 6";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 3600;
            Borda(ws.Cell(linha, "L").AsRange(), bloqueado: true, corFundo: XLColor.FromArgb(0, 176, 80));
            ws.Cell(linha, "L").FormulaA1 = $"=K{linha}*L{linha - 22}";
            linha++;

            ws.Cell(linha, "J").Value = $"MÉDIA PREÇO M3 NIVEL 7";
            Borda(ws.Cell(linha, "J").AsRange());
            Borda(ws.Cell(linha, "K").AsRange());
            ws.Cell(linha, "K").Value = 3700;
            Borda(ws.Cell(linha, "L").AsRange(),bloqueado: true, corFundo: XLColor.FromArgb(0, 176, 240));
            ws.Cell(linha, "L").FormulaA1 = $"=K{linha}*L{linha-23}";
            linha++;

            linha += 2;


        }

        //ws.Range("F:BM").Style.NumberFormat.Format = "#,##0.00;(#,##0.00);-";
        ws.Range($"F1:BM{linha}").Style.NumberFormat.Format = "#,##0.00;-#,##0.00;0.00";
        //ws.Columns().AdjustToContents();
        wb.SaveAs(caminho);
    }

    private static void GerarColunaFormula(
        IXLWorksheet ws,
        int colunaInicial,
        int linhas,
        int colunaIndex,
        Func<int, string> formulaLinha,
        string numberFormat = null)
    {
        for (int i = 0; i < linhas; i++)
        {
            int row = colunaInicial + i;

            var cell = ws.Cell(row, colunaIndex);
            cell.FormulaA1 = formulaLinha(row);
            Borda(
                cell.AsRange(),
                fontSize: 8,
                bloqueado: true
            );

            if (!string.IsNullOrEmpty(numberFormat))
                cell.Style.NumberFormat.Format = numberFormat;
        }

        int totalRow = colunaInicial + linhas;

        ws.Cell(totalRow, colunaIndex).FormulaA1 = $"SUM({ws.Cell(colunaInicial, colunaIndex).Address}:{ws.Cell(totalRow - 1, colunaIndex).Address})";
        ws.Cell(totalRow, colunaIndex).Style.NumberFormat.Format = numberFormat;

        Borda(
            ws.Cell(totalRow, colunaIndex).AsRange(),
            fontSize: 8,
            bloqueado: true,
            negrito: true,
            alinhamento: XLAlignmentHorizontalValues.Right,
            corFundo: XLColor.LightGreen
        );
    }

    private void AplicarBorda(
        IXLRange range,
        bool bold = false,
        XLAlignmentHorizontalValues align = XLAlignmentHorizontalValues.Center,
        XLColor background = null)
    {
        range.Style.Alignment.Horizontal = align;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Font.Bold = bold;

        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        if (background != null)
            range.Style.Fill.BackgroundColor = background;
    }

    public static void Borda(
        IXLRange range,
        XLColor corFundo = null,
        XLColor corFonte = null,
        bool mesclado = false,
        int fontSize = 9,
        bool negrito = false,
        XLAlignmentHorizontalValues alinhamento = XLAlignmentHorizontalValues.Left,
        bool bloqueado = false,
        bool quebraLinha = false)
    {
        if (range == null)
            return;

        // =========================
        // Mesclagem
        // =========================
        if (mesclado)
            range.Merge();

        // =========================
        // Alinhamento
        // =========================
        range.Style.Alignment.Horizontal = alinhamento;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Alignment.WrapText = quebraLinha;

        // =========================
        // Proteção
        // =========================
        range.Style.Protection.Locked = bloqueado;

        // =========================
        // Fonte
        // =========================
        range.Style.Font.FontName = "Verdana";
        range.Style.Font.FontSize = fontSize;
        range.Style.Font.Bold = negrito;
        range.Style.Font.Italic = false;
        range.Style.Font.Strikethrough = false;
        range.Style.Font.Underline = XLFontUnderlineValues.None;

        if (corFonte != null)
            range.Style.Font.FontColor = corFonte;

        // =========================
        // Bordas externas
        // =========================
        range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        // Remove diagonais
        range.Style.Border.DiagonalBorder = XLBorderStyleValues.None;

        // =========================
        // Fundo
        // =========================
        if (corFundo != null)
            range.Style.Fill.BackgroundColor = corFundo;
    }
    
}
