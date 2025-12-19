using System.Globalization;
using FinanteRaportare.WinForms.Data.Entities;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace FinanteRaportare.WinForms.Services;

public sealed class PdfReportBuilder
{
    private const string FontName = "Courier New";
    private static readonly CultureInfo Ro = CultureInfo.GetCultureInfo("ro-RO");

    private enum LineKind
    {
        Text,
        RuleFullWidth
    }

    private sealed record ReportLine(LineKind Kind, string Text);

    public void BuildFurnizoriNeachitati(string filePath, DateTime dataRaport, List<FacturaFurnizor> facturi)
    {
        var rows = facturi
            .Where(f => !f.Achitata)
            .OrderBy(f => f.Scadenta)
            .ThenBy(f => f.Furnizor != null ? f.Furnizor.Denumire : string.Empty)
            .ThenBy(f => f.Document)
            .ToList();

        var lines = BuildFurnizoriLines(rows);

        var doc = new PdfDocument();
        doc.Info.Title = $"FURNIZORI NEACHITATI LA DATA: {dataRaport:dd.MM.yyyy}";

        // A4 portret
        const double marginLeft = 36;
        const double marginTop = 36;
        const double marginRight = 36;
        const double marginBottom = 36;

        var teal = XColor.FromArgb(0, 128, 128);
        var penTeal = new XPen(teal, 0.8);

        // Linie separare subtotaluri / total general
        var penRule = new XPen(XColors.DarkSlateGray, 0.8);

        // Fonturi (incape tot)
        var fontTitle = new XFont(FontName, 10.5, XFontStyle.Bold);
        var fontHeader = new XFont(FontName, 8.5, XFontStyle.Bold);
        var fontBody = new XFont(FontName, 8.0, XFontStyle.Regular);
        var fontTotal = new XFont(FontName, 8.5, XFontStyle.Bold);
        var fontFooter = new XFont(FontName, 8.0, XFontStyle.Regular);

        const double lineHeight = 10.0;

        PdfPage? page = null;
        XGraphics? gfx = null;
        double y = 0;
        int pageNo = 0;

        void DrawFooter()
        {
            if (page == null || gfx == null) return;

            gfx.DrawString(
                $"Pag.: {pageNo}",
                fontFooter,
                XBrushes.Gray,
                new XRect(marginLeft, page.Height - marginBottom + 10, page.Width - marginLeft - marginRight, 12),
                XStringFormats.TopRight
            );
        }

        void DrawHeader()
        {
            if (page == null || gfx == null) return;

            y = marginTop;

            var title = $"FURNIZORI NEACHITATI LA DATA: {dataRaport:dd.MM.yyyy}";
            gfx.DrawString(
                title,
                fontTitle,
                XBrushes.Black,
                new XRect(marginLeft, y, page.Width - marginLeft - marginRight, 18),
                XStringFormats.TopCenter
            );
            y += 20;

            gfx.DrawLine(penTeal, marginLeft, y, page.Width - marginRight, y);
            y += 8;

            gfx.DrawString(BuildFurnizoriColumnHeader(), fontHeader, XBrushes.Black, new XPoint(marginLeft, y));
            y += lineHeight;

            gfx.DrawLine(penTeal, marginLeft, y + 1, page.Width - marginRight, y + 1);
            y += 6;

            // CERINTA: prima factura mai jos cu un rand
            y += lineHeight;
        }

        void NewPage()
        {
            page = doc.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            page.Orientation = PdfSharpCore.PageOrientation.Portrait;
            gfx = XGraphics.FromPdfPage(page);
            pageNo++;

            DrawHeader();
            DrawFooter();
        }

        void EnsureSpace(int neededLines)
        {
            if (page == null || gfx == null)
            {
                NewPage();
                return;
            }

            var usableBottom = page.Height - marginBottom;
            if (y + neededLines * lineHeight > usableBottom)
                NewPage();
        }

        void DrawReportLine(ReportLine rl)
        {
            if (page == null || gfx == null) return;

            switch (rl.Kind)
            {
                case LineKind.Text:
                    {
                        var isTotal = rl.Text.StartsWith("TOTAL", StringComparison.OrdinalIgnoreCase);
                        var font = isTotal ? fontTotal : fontBody;
                        gfx.DrawString(rl.Text, font, XBrushes.Black, new XPoint(marginLeft, y));
                        y += lineHeight;
                        break;
                    }
                case LineKind.RuleFullWidth:
                    {
                        var yLine = y + (lineHeight * 0.25);
                        gfx.DrawLine(penRule, marginLeft, yLine, page.Width - marginRight, yLine);
                        y += lineHeight;
                        break;
                    }
            }
        }

        NewPage();

        foreach (var rl in lines)
        {
            EnsureSpace(1);
            DrawReportLine(rl);
        }

        doc.Save(filePath);
    }

    private static string BuildFurnizoriColumnHeader()
        => FormatRow("FACTURI NEPLATITE", "FURNIZOR", "CEC/OP", "Total de plata", "SCADENTA");

    private static List<ReportLine> BuildFurnizoriLines(List<FacturaFurnizor> rows)
    {
        var lines = new List<ReportLine>(rows.Count + 128);

        DateTime? currentDay = null;
        decimal ziSum = 0m;

        DateTime? currentWeekStart = null;
        DateTime? currentWeekEnd = null;
        decimal weekSum = 0m;

        decimal total = 0m;

        foreach (var f in rows)
        {
            var d = f.Scadenta.Date;

            // init saptamana curenta
            if (currentWeekStart == null || currentWeekEnd == null)
            {
                (currentWeekStart, currentWeekEnd) = GetWeekRange(d);
            }

            // daca am trecut intr-o alta saptamana (nu asteptam Duminica sa existe factura!)
            if (currentWeekEnd != null && d > currentWeekEnd.Value)
            {
                // inainte inchidem ultima zi (daca era in acea saptamana)
                if (currentDay != null)
                {
                    lines.Add(new ReportLine(LineKind.Text, BuildTotalZi(currentDay.Value, ziSum)));
                    ziSum = 0m;
                    currentDay = null;
                }

                // inchidem saptamana precedenta
                lines.Add(new ReportLine(LineKind.Text, BuildTotalPerioada(currentWeekStart!.Value, currentWeekEnd!.Value, weekSum)));
                lines.Add(new ReportLine(LineKind.RuleFullWidth, string.Empty));

                weekSum = 0m;

                // setam noua saptamana
                (currentWeekStart, currentWeekEnd) = GetWeekRange(d);
            }

            // schimbare zi => TOTAL ZI
            if (currentDay != null && d != currentDay.Value)
            {
                lines.Add(new ReportLine(LineKind.Text, BuildTotalZi(currentDay.Value, ziSum)));
                ziSum = 0m;
            }

            currentDay = d;

            var furn = f.Furnizor != null ? f.Furnizor.Denumire : "";
            var cecop = string.IsNullOrWhiteSpace(f.CecOp) ? "" : f.CecOp!.Trim();
            var suma = f.Suma;

            lines.Add(new ReportLine(LineKind.Text, FormatRow(
                f.Document,
                furn,
                cecop,
                FormatAmount(suma),
                d.ToString("dd.MM.yyyy", Ro)
            )));

            ziSum += suma;
            weekSum += suma;
            total += suma;
        }

        // inchidem ultima zi
        if (currentDay != null)
            lines.Add(new ReportLine(LineKind.Text, BuildTotalZi(currentDay.Value, ziSum)));

        // inchidem ultima saptamana (TOTAL PERIOADA mereu la final)
        if (currentWeekStart != null && currentWeekEnd != null)
        {
            lines.Add(new ReportLine(LineKind.Text, BuildTotalPerioada(currentWeekStart.Value, currentWeekEnd.Value, weekSum)));
            lines.Add(new ReportLine(LineKind.RuleFullWidth, string.Empty));
        }

        // linie inainte de total general (ceruta)
        lines.Add(new ReportLine(LineKind.RuleFullWidth, string.Empty));
        lines.Add(new ReportLine(LineKind.Text, $"TOTAL GENERAL: {FormatAmount(total)}"));

        return lines;
    }

    private static (DateTime weekStart, DateTime weekEnd) GetWeekRange(DateTime d)
    {
        // Luni–Duminica
        int diff = ((int)d.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var weekStart = d.AddDays(-diff);
        var weekEnd = weekStart.AddDays(6);
        return (weekStart, weekEnd);
    }

    private static string BuildTotalZi(DateTime zi, decimal suma)
        => $"TOTAL ZI ({zi:dd.MM.yyyy}): {FormatAmount(suma)}";

    private static string BuildTotalPerioada(DateTime start, DateTime end, decimal suma)
        => $"TOTAL PERIOADA ({start:dd.MM.yyyy}-{end:dd.MM.yyyy}): {FormatAmount(suma)}";

    private static string FormatRow(string doc, string furn, string cecop, string amount, string date)
    {
        // doc 24, furn 28, cecop 10, amount 14, date 12
        return Trunc(doc, 24).PadRight(24) + " "
             + Trunc(furn, 28).PadRight(28) + " "
             + Trunc(cecop, 10).PadRight(10) + " "
             + Trunc(amount, 14).PadLeft(14) + " "
             + Trunc(date, 12).PadLeft(12);
    }

    private static string Trunc(string s, int max)
        => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s.Substring(0, max));

    private static string FormatAmount(decimal value)
        => value.ToString("0.00", Ro);
}
