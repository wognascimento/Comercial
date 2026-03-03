using ClosedXML.Excel;
using Comercial.DataBase;
using Comercial.Repositores;
using System.Diagnostics;


namespace Comercial.Services;

public class ExcelQuadroPrecoService
{
    private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;
    private QuadroRepository? _repo;


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

    /*
    public static void Borda(
        IXLRange range,
        XLColor corFundo = null,
        XLColor corFonte = null,
        bool mesclado = false,
        int fontSize = 9,
        bool negrito = false,
        XLAlignmentHorizontalValues alinhamento = XLAlignmentHorizontalValues.Left,
        bool bloqueado = false,
        bool quebraLinha = false,
        string formatoNumero = null)
    {
        if (range == null)
            return;

        AplicarEstilo(range, corFundo, corFonte, mesclado, fontSize,
            negrito, alinhamento, bloqueado, quebraLinha, formatoNumero);
    }

    public static void Borda(
        IXLCell cell,
        XLColor corFundo = null,
        XLColor corFonte = null,
        int fontSize = 9,
        bool negrito = false,
        XLAlignmentHorizontalValues alinhamento = XLAlignmentHorizontalValues.Left,
        bool bloqueado = false,
        bool quebraLinha = false,
        string formatoNumero = null)
    {
        if (cell == null)
            return;

        AplicarEstilo(cell.AsRange(), corFundo, corFonte, false, fontSize,
            negrito, alinhamento, bloqueado, quebraLinha, formatoNumero);
    }

    private static void AplicarEstilo(
        IXLRange range,
        XLColor corFundo,
        XLColor corFonte,
        bool mesclado,
        int fontSize,
        bool negrito,
        XLAlignmentHorizontalValues alinhamento,
        bool bloqueado,
        bool quebraLinha,
        string formatoNumero)
    {
        if (mesclado)
            range.Merge();

        range.Style.Alignment.Horizontal = alinhamento;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Alignment.WrapText = quebraLinha;
        range.Style.Protection.Locked = bloqueado;

        range.Style.Font.FontName = "Verdana";
        range.Style.Font.FontSize = fontSize;
        range.Style.Font.Bold = negrito;

        if (corFonte != null)
            range.Style.Font.FontColor = corFonte;

        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        if (corFundo != null)
            range.Style.Fill.BackgroundColor = corFundo;

        if (!string.IsNullOrWhiteSpace(formatoNumero))
            range.Style.NumberFormat.Format = formatoNumero;
    }
    */
    
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
