using ClosedXML.Excel;
using Comercial.Data.Model;
using Comercial.Data.Model.Dto;
using Comercial.DataBase;
using Comercial.Repositores;
using System.Data;
using System.Windows;


namespace Comercial.Services;

public class QuadroPrecoExportService
{
    private static readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;
    private QuadroRepository? _repo;

    // ── helpers de estilo ────────────────────────────────────────────────

    private static XLColor ColIndex(int idx) =>
        idx switch
        {
            3 => XLColor.Red,
            10 => XLColor.DarkGreen,
            33 => XLColor.CornflowerBlue,
            _ => XLColor.NoColor
        };

    /// <summary>Borda média em todos os 4 lados, sem diagonais.</summary>
    private static void SetMediumBorder(IXLRange range)
    {
        range.Style.Border.TopBorder = XLBorderStyleValues.Medium;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        range.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
        range.Style.Border.RightBorder = XLBorderStyleValues.Medium;
        range.Style.Border.DiagonalBorder = XLBorderStyleValues.None;
    }

    /// <summary>Borda média externa + thin interna vertical.</summary>
    private static void SetHeaderBorder(IXLRange range)
    {
        range.Style.Border.TopBorder = XLBorderStyleValues.Medium;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        range.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
        range.Style.Border.RightBorder = XLBorderStyleValues.Medium;
        range.Style.Border.DiagonalBorder = XLBorderStyleValues.None;
        // bordas internas: itera célula a célula dentro do range
        foreach (var cell in range.Cells())
        {
            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        }
        // reaplica borda externa mais grossa
        range.FirstColumn().Style.Border.LeftBorder = XLBorderStyleValues.Medium;
        range.LastColumn().Style.Border.RightBorder = XLBorderStyleValues.Medium;
    }

    private static void SetDataBorder(IXLRange range)
    {
        range.Style.Border.TopBorder = XLBorderStyleValues.Medium;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        range.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
        range.Style.Border.RightBorder = XLBorderStyleValues.Medium;
        range.Style.Border.DiagonalBorder = XLBorderStyleValues.None;
        // bordas internas verticais tracejadas e horizontais finas
        foreach (var cell in range.Cells())
        {
            cell.Style.Border.RightBorder = XLBorderStyleValues.Dashed;
            cell.Style.Border.LeftBorder = XLBorderStyleValues.Dashed;
            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }
        // reaplica bordas externas mais grossas
        range.FirstColumn().Style.Border.LeftBorder = XLBorderStyleValues.Medium;
        range.LastColumn().Style.Border.RightBorder = XLBorderStyleValues.Medium;
        range.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Medium;
        range.LastRow().Style.Border.BottomBorder = XLBorderStyleValues.Medium;
    }

    private static void SetDataFont(IXLCell cell)
    {
        cell.Style.Font.FontName = "Arial";
        cell.Style.Font.FontSize = 8;
        cell.Style.Font.Bold = false;
    }

    // ── método principal ─────────────────────────────────────────────────

    public async Task GerarQuadro(string caminho, string _Sigla, long _CodBrief)
    {
        // ── lê valores do formulário ─────────────────────────────────────
        string sigla = _Sigla.Trim();
        int codBrief = Convert.ToInt32(_CodBrief);
        int anoAnterior = Convert.ToInt32(BaseSettings.Database) - 1; //DateTime.Today.Year - 1;

        _repo = new QuadroRepository();

        // ── cria workbook ────────────────────────────────────────────────
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("CUSTO");

        int row = 4;   // começa na linha 4 (equivalente ao linhaAtual inicial + 1)

        // ════════════════════════════════════════════════════════════════
        // SEÇÃO: FECHA ANO ANTERIOR
        // ════════════════════════════════════════════════════════════════
        var titleRange = ws.Range(row, 1, row, 6);
        titleRange.Merge();
        titleRange.Style.Fill.BackgroundColor = ColIndex(10);
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        SetMediumBorder(titleRange);
        ws.Cell(row, 1).Value = "FECHA ANO ANTERIOR";
        row++;

        // ── cabeçalho ano anterior ───────────────────────────────────────
        string[] headersAno = {
                "Sigla","Sigla Fecha","Ano","Transporte","Valor Transporte",
                "Valor Liquido","Valor Bruto","Desconto Especial",
                "Desconto Comprometimento","Desconto Rede","Desconto Cond Pagto",
                "Forma Pagto","Led Opcional","Munck","Plataforma","Alpinista"
            };
        for (int c = 0; c < headersAno.Length; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = headersAno[c];
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.WrapText = true;
        }
        SetHeaderBorder(ws.Range(row, 1, row, headersAno.Length));
        row++;

        // ── dados ano anterior ───────────────────────────────────────────
        //DataTable dtAno = PreencherDataTable($"SELECT * FROM comercial.tblfecha WHERE sigla = '{sigla}' AND ano = {anoAnterior}");

        var fecha = await _repo.GetFechaAsync(sigla, anoAnterior);

        foreach (ComercialFechaModel dr in fecha)
        {
            WriteCellText(ws, row, 1, dr.sigla, XLAlignmentHorizontalValues.Center);
            WriteCellText(ws, row, 2, dr.siglafecha, XLAlignmentHorizontalValues.Left);
            WriteCellText(ws, row, 3, dr.ano, XLAlignmentHorizontalValues.Left);
            WriteCellText(ws, row, 4, dr.transporte, XLAlignmentHorizontalValues.Center);
            WriteCellMoney(ws, row, 5, dr.valortransporte, XLAlignmentHorizontalValues.Left);
            WriteCellMoney(ws, row, 6, dr.valorliquido, XLAlignmentHorizontalValues.Left);
            WriteCellMoney(ws, row, 7, dr.valor_bruto, XLAlignmentHorizontalValues.Left);
            WriteCellMoney(ws, row, 8, dr.descontoespecial, XLAlignmentHorizontalValues.Center);
            WriteCellMoney(ws, row, 9, dr.descontocomprometimento, XLAlignmentHorizontalValues.Center);
            WriteCellMoney(ws, row, 10, dr.descontorede, XLAlignmentHorizontalValues.Center);
            WriteCellPct(ws, row, 11, dr.descontocondpagto, XLAlignmentHorizontalValues.Center);
            WriteCellText(ws, row, 12, dr.formapagto, XLAlignmentHorizontalValues.Center);
            WriteCellText(ws, row, 13, dr.led_opcional, XLAlignmentHorizontalValues.Left);
            WriteCellText(ws, row, 14, dr.munck, XLAlignmentHorizontalValues.Left);
            WriteCellText(ws, row, 15, dr.plataforma, XLAlignmentHorizontalValues.Left);
            WriteCellText(ws, row, 16, dr.alpinista, XLAlignmentHorizontalValues.Left);
            row++;
        }

        // ════════════════════════════════════════════════════════════════
        // SEÇÃO: QUADRO DE PREÇOS POR TEMA / BLOCO / TIPO
        // ════════════════════════════════════════════════════════════════

        // Carrega todos os itens do quadro de preço de uma vez
        //DataTable dtItems = PreencherDataTable($"SELECT * FROM qryquadropreco WHERE codbrief = {codBrief} ORDER BY tema, ordem, item");

        var dtItems = await _repo.GetQuadroAsync(codBrief);
        var temas = dtItems
            .GroupBy(r => r.idtema) // Agrupa pelos IDs únicos
            .Select(g => g.First()) // Pega o primeiro objeto de cada grupo para acessar as outras propriedades
            .OrderBy(t => t.idtema_ordem) // Ordena pela propriedade de ordem
            .Select(t => t.idtema) // Seleciona apenas o ID final
            .ToList();

        // Itens selecionados na listbox
        foreach (int idTema in temas.Select(v => (int)v))
        {
            //int idTema = Convert.ToInt32(LstDimensao.GetItemColumn(slIdx, 1)); // ajuste conforme seu controle

            //DataTable dtTemas = PreencherDataTable($"SELECT * FROM comercial.proposta_temas_briefing WHERE codbriefing = {codBrief} AND idtema = {idTema} ORDER BY ordem_escolha");

            var dtTemas = await _repo.GetBriefingTemaAsync(codBrief, idTema);

            foreach (PropostaBriefingTemaDto drTema in dtTemas)
            {
                string nomeTema = drTema.temas;
                long idTemaVal = drTema.idtema;

                // ── cabeçalho tema ───────────────────────────────────────
                row++;
                var temaRange = ws.Range(row, 1, row, 6);
                temaRange.Merge();
                temaRange.Style.Fill.BackgroundColor = ColIndex(33);
                temaRange.Style.Font.Bold = true;
                temaRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                SetMediumBorder(temaRange);
                ws.Cell(row, 1).Value = $"TEMA: {nomeTema}";
                row++;

                // ── blocos ───────────────────────────────────────────────
                //DataTable dtBlocos = PreencherDataTable($"SELECT bloco, tipo FROM qryprecobloco WHERE codbrief = {codBrief} AND idtema = {idTemaVal} AND tipo = 'Proposta' ORDER BY tema, item, tipo");

                var dtBlocos = await _repo.GetPrecoBlocosAsync(codBrief, idTemaVal);

                foreach (PropostaPrecoBlocoDto drBloco in dtBlocos)
                {
                    string nomeBloco = drBloco.bloco;

                    // título do bloco
                    var blocoRange = ws.Range(row, 1, row, 6);
                    blocoRange.Merge();
                    blocoRange.Style.Font.Bold = true;
                    blocoRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    SetMediumBorder(blocoRange);
                    ws.Cell(row, 1).Value = nomeBloco;
                    row++;

                    // cabeçalho colunas proposta
                    row = EscreverCabecalhoProposta(ws, row);

                    // filtra itens
                    /*var viewBloco = new DataView(dtItems)
                    {
                        RowFilter = $"idtema = {idTemaVal} AND bloco = '{nomeBloco.Replace("'", "''")}' AND tipo = 'Proposta'"
                    };*/
                    var viewBloco = dtItems.Where(x => x.idtema == idTemaVal && x.bloco == nomeBloco && x.tipo == "Proposta" ).ToList();

                    int linhaInicial = row;
                    int qtdLinhas = viewBloco.Count;

                    foreach (QuadroPrecoDetalheDto drv in viewBloco)
                    {
                        EscreverLinhaItem(ws, row, drv);
                        row++;
                    }

                    int linhaFinal = row - 1;
                    if (linhaFinal >= linhaInicial)
                        SetDataBorder(ws.Range(linhaInicial, 1, linhaFinal, 17));

                    // total coluna M
                    ws.Cell(row, 13).FormulaA1 = $"=SUM(M{linhaInicial}:M{linhaFinal})";
                    ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";
                    SetMediumBorder(ws.Range(row, 13, row, 13));
                    row++;
                }

                row++; // linha em branco entre blocos

                // ── tipos (Complemento, etc.) ────────────────────────────
                //DataTable dtTipos = PreencherDataTable($"SELECT tipo FROM qryTiposPreco WHERE codbrief = {codBrief} AND idtema = {idTemaVal} ORDER BY tema, ordem, tipo");
                var dtTipos = await _repo.GetPrecoTiposAsync(codBrief, idTemaVal);

                foreach (var drTipo in dtTipos)
                {
                    string tipo = drTipo.tipo;

                    // cabeçalho tipo
                    var tipoRange = ws.Range(row, 1, row, 6);
                    tipoRange.Merge();
                    tipoRange.Style.Fill.BackgroundColor = tipo == "Complemento" ? ColIndex(3) : ColIndex(33);
                    tipoRange.Style.Font.Bold = true;
                    tipoRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    SetMediumBorder(tipoRange);
                    ws.Cell(row, 1).Value = tipo;
                    row++;

                    row = EscreverCabecalhoProposta(ws, row);

                    /*var viewTipo = new DataView(dtItems)
                    {
                        RowFilter = $"tipo = '{tipo.Replace("'", "''")}' AND idtema = {idTemaVal}"
                    };*/
                    var viewTipo = dtItems.Where(x => x.idtema == idTemaVal && x.tipo == tipo).ToList();
                    int linhaInicial = row;

                    foreach (var drv in viewTipo)
                    {
                        EscreverLinhaItem(ws, row, drv);
                        row++;
                    }

                    int linhaFinal = row - 1;
                    if (linhaFinal >= linhaInicial)
                        SetDataBorder(ws.Range(linhaInicial, 1, linhaFinal, 17));

                    ws.Cell(row, 13).FormulaA1 = $"=SUM(M{linhaInicial}:M{linhaFinal})";
                    ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";
                    SetMediumBorder(ws.Range(row, 13, row, 13));
                    row++;
                }
            }
        }

        // ── larguras de coluna ────────────────────────────────────────────
        ws.Column(1).AdjustToContents();
        ws.Column(2).Width = 22.57;
        ws.Column(3).Width = 24.71;
        ws.Column(4).AdjustToContents();
        ws.Column(5).Width = 21.15;
        ws.Column(6).Width = 25.57;
        ws.Column(7).AdjustToContents();
        ws.Column(8).AdjustToContents();
        ws.Column(9).AdjustToContents();
        ws.Column(10).AdjustToContents();
        ws.Column(11).AdjustToContents();
        ws.Column(12).AdjustToContents();
        ws.Column(13).AdjustToContents();
        ws.Column(14).AdjustToContents();
        ws.Column(15).Width = 20;
        ws.Column(16).AdjustToContents();
        ws.Column(17).AdjustToContents();

        ws.Rows(4, row).AdjustToContents();

        // ── salvar ────────────────────────────────────────────────────────
        

        wb.SaveAs(caminho);
        //MessageBox.Show($"Arquivo salvo:\n{caminho}", "Exportação concluída",MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── escreve cabeçalho padrão de proposta (colunas A..Q) ──────────────
    private static int EscreverCabecalhoProposta(IXLWorksheet ws, int row)
    {
        string[] cols = {
                "Item","Local","Descrição","Qtd","Dimensão","Observação","Índice",
                "Preço Sugerido Unitário","Unitário","Sub Total","Preço Sugerido Total",
                "Desconto","Total","anotações","m3 unitario","m3 total","Formula"
            };
        for (int c = 0; c < cols.Length; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = cols[c];
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.WrapText = true;
        }
        SetHeaderBorder(ws.Range(row, 1, row, cols.Length));
        return row + 1;
    }

    // ── escreve uma linha de item do quadro de preços ─────────────────────
    private static void EscreverLinhaItem(IXLWorksheet ws, int row, QuadroPrecoDetalheDto dr)
    {
        // A – Item
        //WriteCellText(ws, row, 1, dr.item, XLAlignmentHorizontalValues.Center);
        ws.Cell(row, 1).Value = dr.item;
        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        // B – Local
        WriteCellText(ws, row, 2, dr.localitem, XLAlignmentHorizontalValues.Left);
        // C – Descrição
        WriteCellText(ws, row, 3, dr.descricao, XLAlignmentHorizontalValues.Left);
        // D – Qtd
        //WriteCellText(ws, row, 4, dr.qtd, XLAlignmentHorizontalValues.Center);
        ws.Cell(row, 4).Value = dr.qtd;
        ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        // E – Dimensão
        WriteCellText(ws, row, 5, dr.dimensao, XLAlignmentHorizontalValues.Left);
        // F – Obs
        WriteCellText(ws, row, 6, dr.obs, XLAlignmentHorizontalValues.Left);
        // G – Índice / faixapreco
        //WriteCellText(ws, row, 7, dr.faixapreco, XLAlignmentHorizontalValues.Left);
        WriteCellText(ws, row, 7, "", XLAlignmentHorizontalValues.Left);
        // H – Preço Sugerido Unitário (bloqueado)
        WriteCellMoney(ws, row, 8, dr.vlrunidsugerido, XLAlignmentHorizontalValues.Center, locked: true);
        // I – Unitário
        WriteCellMoney(ws, row, 9, dr.valor_unitario, XLAlignmentHorizontalValues.Center);
        // J – Sub Total = Qtd * Unitário  (D * I)
        ws.Cell(row, 10).FormulaA1 = $"=D{row}*I{row}";
        ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell(row, 10).Style.Protection.Locked = true;
        // K – Preço Sugerido Total = Qtd * Sugerido * (1 – Desconto)
        ws.Cell(row, 11).FormulaA1 = $"=(D{row}*H{row})-(D{row}*H{row})*L{row}";
        ws.Cell(row, 11).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell(row, 11).Style.Protection.Locked = true;
        // L – Desconto %
        WriteCellPct(ws, row, 12, dr.desconto, XLAlignmentHorizontalValues.Center);
        // M – Total = SubTotal – SubTotal * Desconto
        ws.Cell(row, 13).FormulaA1 = $"=J{row}-(J{row}*L{row})";
        ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell(row, 13).Style.Protection.Locked = true;
        // N – Anotações / obsinterna
        string? anotCol = dr.anotacoes == null || dr.anotacoes == "" ? dr.anotacoes : dr.obsinterna;
        WriteCellText(ws, row, 14, anotCol, XLAlignmentHorizontalValues.Left);
        // O – m3 unitario
        //WriteCellText(ws, row, 15, dr.m3_unitario, XLAlignmentHorizontalValues.Left);
        ws.Cell(row, 15).Value = dr.m3_unitario;
        ws.Cell(row, 15).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        // P – m3 total
        //WriteCellText(ws, row, 16, dr.m3_unitario * dr.qtd, XLAlignmentHorizontalValues.Left);
        ws.Cell(row, 16).FormulaA1 = $"=D{row}*O{row}";
        ws.Cell(row, 16).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell(row, 16).Style.Protection.Locked = true;
        // Q – Formula
        //string formula = dr.formula ? "" : dr.formula.ToString().Trim();
        string formula = "";
        var cellQ = ws.Cell(row, 17);
        if (!string.IsNullOrEmpty(formula))
            cellQ.FormulaA1 = formula.StartsWith("=") ? formula : "=" + formula;
        else
            cellQ.Value = "";
        cellQ.Style.NumberFormat.Format = "@";
        cellQ.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // fonte 8 Arial em toda a linha
        ws.Row(row).Style.Font.FontName = "Arial";
        ws.Row(row).Style.Font.FontSize = 8;
    }

    // ── helpers de célula ─────────────────────────────────────────────────

    private static void WriteCellText(IXLWorksheet ws, int row, int col, object value, XLAlignmentHorizontalValues align, bool locked = false)
    {
        var cell = ws.Cell(row, col);
        cell.Style.NumberFormat.Format = "@";
        cell.Value = value?.ToString() ?? "";
        cell.Style.Alignment.Horizontal = align;
        cell.Style.Protection.Locked = locked;
        SetDataFont(cell);
    }

    private static void WriteCellMoney(IXLWorksheet ws, int row, int col, object value, XLAlignmentHorizontalValues align, bool locked = false)
    {
        var cell = ws.Cell(row, col);
        cell.Style.NumberFormat.Format = "#,##0.00";
        cell.Value = value is DBNull || value == null ? 0d : Convert.ToDouble(value);
        cell.Style.Alignment.Horizontal = align;
        cell.Style.Protection.Locked = locked;
        SetDataFont(cell);
    }

    private static void WriteCellPct(IXLWorksheet ws, int row, int col, object value, XLAlignmentHorizontalValues align)
    {
        var cell = ws.Cell(row, col);
        cell.Style.NumberFormat.Format = "0.00%";
        cell.Value = value is DBNull || value == null ? 0d : Convert.ToDouble(value);
        cell.Style.Alignment.Horizontal = align;
        SetDataFont(cell);
    }

}
