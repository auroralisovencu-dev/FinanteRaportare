using FinanteRaportare.WinForms.Data.Entities;
using FinanteRaportare.WinForms.Services;
using FinanteRaportare.WinForms.UI.Dialogs;
using Microsoft.EntityFrameworkCore;

namespace FinanteRaportare.WinForms.UI;

public sealed class MainForm : Form
{
    private readonly DbFactory _db;

    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };

    // tab Clients
    private readonly DataGridView _gridClienti = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly BindingSource _bsClienti = new();

    // tab Furnizori
    private readonly DataGridView _gridFurnizori = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly BindingSource _bsFurnizori = new();

    // tab Solduri
    private readonly DataGridView _gridSolduri = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly BindingSource _bsSolduri = new();

    // tab Facturi Furnizori
    private readonly DataGridView _gridFacturiFurnizori = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly BindingSource _bsFacturiFurnizori = new();

    // tab Rapoarte
    private readonly DateTimePicker _dpRaportDate = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy" };

    public MainForm(DbFactory db)
    {
        _db = db;

        Text = "FinanteRaportare (Win11) - baza + formulare";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1100;
        Height = 700;

        Controls.Add(_tabs);

        BuildTabs();
        LoadAll();
    }

    private void BuildTabs()
    {
        _tabs.TabPages.Add(BuildTabClienti());
        _tabs.TabPages.Add(BuildTabFurnizori());
        _tabs.TabPages.Add(BuildTabSolduri());
        _tabs.TabPages.Add(BuildTabPlaceholder("Facturi clienti"));
        _tabs.TabPages.Add(BuildTabFacturiFurnizori());
        _tabs.TabPages.Add(BuildTabRapoarte());
    }

    private TabPage BuildTabPlaceholder(string title)
    {
        var tp = new TabPage(title);
        var lbl = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = @"Pasul 1: DB + ecrane de baza.

Aici adaugam in pasii urmatori.",
            Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)
        };
        tp.Controls.Add(lbl);
        return tp;
    }

    private TabPage BuildTabClienti()
    {
        var tp = new TabPage("Clienti");

        var header = MakeHeader("CLIENTI");
        var buttons = MakeButtonBar(
            onAdd: (_, _) => AddClient(),
            onEdit: (_, _) => EditClient(),
            onDelete: (_, _) => DeleteClient(),
            onRefresh: (_, _) => LoadClienti()
        );

        _gridClienti.DataSource = _bsClienti;

        tp.Controls.Add(_gridClienti);
        tp.Controls.Add(buttons);
        tp.Controls.Add(header);
        return tp;
    }

    private TabPage BuildTabFurnizori()
    {
        var tp = new TabPage("Furnizori");

        var header = MakeHeader("FURNIZORI");
        var buttons = MakeButtonBar(
            onAdd: (_, _) => AddFurnizor(),
            onEdit: (_, _) => EditFurnizor(),
            onDelete: (_, _) => DeleteFurnizor(),
            onRefresh: (_, _) => LoadFurnizori()
        );

        _gridFurnizori.DataSource = _bsFurnizori;

        tp.Controls.Add(_gridFurnizori);
        tp.Controls.Add(buttons);
        tp.Controls.Add(header);
        return tp;
    }

    private TabPage BuildTabSolduri()
    {
        var tp = new TabPage("Disponibilitati");

        var header = MakeHeader("DISPONIBILITATI ZILNICE (Seif + BT + BCR + Trezorerie)");
        var buttons = MakeButtonBar(
            onAdd: (_, _) => AddSold(),
            onEdit: (_, _) => EditSold(),
            onDelete: (_, _) => DeleteSold(),
            onRefresh: (_, _) => LoadSolduri(),
            addText: "Adauga zi",
            editText: "Editeaza zi",
            deleteText: "Sterge zi"
        );

        _gridSolduri.DataSource = _bsSolduri;

        tp.Controls.Add(_gridSolduri);
        tp.Controls.Add(buttons);
        tp.Controls.Add(header);
        return tp;
    }

    private TabPage BuildTabFacturiFurnizori()
    {
        var tp = new TabPage("Facturi furnizori");

        var header = MakeHeader("FACTURI FURNIZORI (de plata)");
        var buttons = MakeButtonBar(
            onAdd: (_, _) => AddFacturaFurnizor(),
            onEdit: (_, _) => EditFacturaFurnizor(),
            onDelete: (_, _) => DeleteFacturaFurnizor(),
            onRefresh: (_, _) => LoadFacturiFurnizori(),
            addText: "Adauga factura",
            editText: "Editeaza factura",
            deleteText: "Sterge factura"
        );

        _gridFacturiFurnizori.DataSource = _bsFacturiFurnizori;

        tp.Controls.Add(_gridFacturiFurnizori);
        tp.Controls.Add(buttons);
        tp.Controls.Add(header);
        return tp;
    }

    private Panel MakeHeader(string title)
    {
        var p = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.FromArgb(0, 128, 128) // verde-turcoaz discret
        };

        var lbl = new Label
        {
            Dock = DockStyle.Fill,
            Text = title,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)
        };

        p.Controls.Add(lbl);
        return p;
    }

    private static FlowLayoutPanel MakeButtonBar(
        EventHandler onAdd,
        EventHandler onEdit,
        EventHandler onDelete,
        EventHandler onRefresh,
        string addText = "Adauga",
        string editText = "Editeaza",
        string deleteText = "Sterge")
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 8, 8, 8)
        };

        Button MakeBtn(string t, EventHandler h) => new Button()
        {
            Text = t,
            AutoSize = true,
            Margin = new Padding(4, 0, 4, 0)
        }.Also(b => b.Click += h);

        bar.Controls.Add(MakeBtn(addText, onAdd));
        bar.Controls.Add(MakeBtn(editText, onEdit));
        bar.Controls.Add(MakeBtn(deleteText, onDelete));
        bar.Controls.Add(MakeBtn("Refresh", onRefresh));

        return bar;
    }

    private void LoadAll()
    {
        LoadClienti();
        LoadFurnizori();
        LoadSolduri();
        LoadFacturiFurnizori();
    }

    private void LoadClienti()
    {
        using var db = _db.Create();
        var data = db.Clienti.AsNoTracking()
            .OrderBy(x => x.Denumire)
            .ToList();

        _bsClienti.DataSource = data;
    }

    private void LoadFurnizori()
    {
        using var db = _db.Create();
        var data = db.Furnizori.AsNoTracking()
            .OrderBy(x => x.Denumire)
            .ToList();

        _bsFurnizori.DataSource = data;
    }

    private void LoadSolduri()
    {
        using var db = _db.Create();
        var data = db.SolduriZilnice.AsNoTracking()
            .OrderByDescending(x => x.Data)
            .ToList();

        _bsSolduri.DataSource = data;
    }

    private void LoadFacturiFurnizori()
    {
        using var db = _db.Create();
        var data = db.FacturiFurnizori
            .AsNoTracking()
            .Include(x => x.Furnizor)
            .OrderBy(x => x.Scadenta)
            .ThenBy(x => x.Furnizor!.Denumire)
            .ThenBy(x => x.Document)
            .Select(x => new FacturaFurnizorRow
            {
                Id = x.Id,
                Document = x.Document,
                DataDocument = x.DataDocument,
                Scadenta = x.Scadenta,
                Furnizor = x.Furnizor != null ? x.Furnizor.Denumire : "",
                FurnizorId = x.FurnizorId,
                CecOp = x.CecOp,
                Suma = x.Suma,
                Achitata = x.Achitata
            })
            .ToList();

        _bsFacturiFurnizori.DataSource = data;
    }

    private Client? GetSelectedClient()
        => _gridClienti.CurrentRow?.DataBoundItem as Client;

    private Furnizor? GetSelectedFurnizor()
        => _gridFurnizori.CurrentRow?.DataBoundItem as Furnizor;

    private SoldZilnic? GetSelectedSold()
        => _gridSolduri.CurrentRow?.DataBoundItem as SoldZilnic;

    private FacturaFurnizorRow? GetSelectedFacturaFurnizor()
        => _gridFacturiFurnizori.CurrentRow?.DataBoundItem as FacturaFurnizorRow;

    private void AddClient()
    {
        using var dlg = new ClientEditDialog(new Client());
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        using var db = _db.Create();
        db.Clienti.Add(dlg.Model);
        db.SaveChanges();
        LoadClienti();
    }

    private void EditClient()
    {
        var selected = GetSelectedClient();
        if (selected == null) return;

        using var db = _db.Create();
        var entity = db.Clienti.First(x => x.Id == selected.Id);

        using var dlg = new ClientEditDialog(entity);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        db.SaveChanges();
        LoadClienti();
    }

    private void DeleteClient()
    {
        var selected = GetSelectedClient();
        if (selected == null) return;

        if (MessageBox.Show(this, $"Stergi clientul '{selected.Denumire}'?", "Confirmare",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        using var db = _db.Create();
        var entity = db.Clienti.First(x => x.Id == selected.Id);
        db.Clienti.Remove(entity);
        db.SaveChanges();
        LoadClienti();
    }

    private void AddFurnizor()
    {
        using var dlg = new FurnizorEditDialog(new Furnizor());
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        using var db = _db.Create();
        db.Furnizori.Add(dlg.Model);
        db.SaveChanges();
        LoadFurnizori();
    }

    private void EditFurnizor()
    {
        var selected = GetSelectedFurnizor();
        if (selected == null) return;

        using var db = _db.Create();
        var entity = db.Furnizori.First(x => x.Id == selected.Id);

        using var dlg = new FurnizorEditDialog(entity);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        db.SaveChanges();
        LoadFurnizori();
    }

    private void DeleteFurnizor()
    {
        var selected = GetSelectedFurnizor();
        if (selected == null) return;

        if (MessageBox.Show(this, $"Stergi furnizorul '{selected.Denumire}'?", "Confirmare",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        using var db = _db.Create();
        var entity = db.Furnizori.First(x => x.Id == selected.Id);
        db.Furnizori.Remove(entity);
        db.SaveChanges();
        LoadFurnizori();
    }

    private void AddSold()
    {
        using var dlg = new SoldZilnicEditDialog(new SoldZilnic { Data = DateTime.Today });
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        using var db = _db.Create();
        db.SolduriZilnice.Add(dlg.Model);
        db.SaveChanges();
        LoadSolduri();
    }

    private void EditSold()
    {
        var selected = GetSelectedSold();
        if (selected == null) return;

        using var db = _db.Create();
        var entity = db.SolduriZilnice.First(x => x.Id == selected.Id);

        using var dlg = new SoldZilnicEditDialog(entity);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        db.SaveChanges();
        LoadSolduri();
    }

    private void DeleteSold()
    {
        var selected = GetSelectedSold();
        if (selected == null) return;

        if (MessageBox.Show(this, $"Stergi disponibilitatile din data {selected.Data:dd.MM.yyyy}?", "Confirmare",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        using var db = _db.Create();
        var entity = db.SolduriZilnice.First(x => x.Id == selected.Id);
        db.SolduriZilnice.Remove(entity);
        db.SaveChanges();
        LoadSolduri();
    }

    // -------------------- Facturi furnizori --------------------

   

    private void AddFacturaFurnizor()
    {
        using var db = _db.Create();
        var furnizori = db.Furnizori.AsNoTracking().OrderBy(x => x.Denumire).ToList();
        if (furnizori.Count == 0)
        {
            MessageBox.Show(this, "Adauga mai intai cel putin un furnizor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var model = new FacturaFurnizor { DataDocument = DateTime.Today, Scadenta = DateTime.Today, FurnizorId = furnizori[0].Id };
        using var dlg = new FacturaFurnizorEditDialog(model, furnizori);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        db.FacturiFurnizori.Add(dlg.Model);
        db.SaveChanges();
        LoadFacturiFurnizori();
    }

    private void EditFacturaFurnizor()
    {
        var selected = GetSelectedFacturaFurnizor();
        if (selected == null) return;

        using var db = _db.Create();
        var furnizori = db.Furnizori.AsNoTracking().OrderBy(x => x.Denumire).ToList();
        var entity = db.FacturiFurnizori.First(x => x.Id == selected.Id);

        using var dlg = new FacturaFurnizorEditDialog(entity, furnizori);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        db.SaveChanges();
        LoadFacturiFurnizori();
    }

    private void DeleteFacturaFurnizor()
    {
        var selected = GetSelectedFacturaFurnizor();
        if (selected == null) return;

        if (MessageBox.Show(this, $"Stergi factura '{selected.Document}'?", "Confirmare",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        using var db = _db.Create();
        var entity = db.FacturiFurnizori.First(x => x.Id == selected.Id);
        db.FacturiFurnizori.Remove(entity);
        db.SaveChanges();
        LoadFacturiFurnizori();
    }

    // -------------------- Rapoarte --------------------

    private TabPage BuildTabRapoarte()
    {
        var tp = new TabPage("Rapoarte (PDF)");
        var header = MakeHeader("RAPOARTE PDF (stil listare)");

        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };

        var row1 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        row1.Controls.Add(new Label { Text = "Data raport:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 8, 8, 0) });
        _dpRaportDate.Value = DateTime.Today;
        row1.Controls.Add(_dpRaportDate);

        var btn = new Button { Text = "Genereaza PDF: Furnizori neachitati", AutoSize = true, Margin = new Padding(16, 0, 0, 0) };
        btn.Click += (_, _) => GenerateFurnizoriNeachitatiPdf();
        row1.Controls.Add(btn);

        var info = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = false,
            Height = 90,
            Padding = new Padding(0, 10, 0, 0),
            Text = "Raportul genereaza subtotal pe zi (scadenta), subtotal saptamanal la fiecare DUMINICA, apoi TOTAL GENERAL.\r\nStil monospace (Courier New) + linii discret verde-turcoaz.",
        };

        panel.Controls.Add(info);
        panel.Controls.Add(row1);

        tp.Controls.Add(panel);
        tp.Controls.Add(header);
        return tp;
    }

    private void GenerateFurnizoriNeachitatiPdf()
    {
        var reportDate = _dpRaportDate.Value.Date;
        using var db = _db.Create();

        var data = db.FacturiFurnizori
            .AsNoTracking()
            .Include(x => x.Furnizor)
            .Where(x => !x.Achitata)
            .OrderBy(x => x.Scadenta)
            .ThenBy(x => x.Furnizor!.Denumire)
            .ThenBy(x => x.Document)
            .ToList();

        if (data.Count == 0)
        {
            MessageBox.Show(this, "Nu exista facturi furnizori neachitate.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = $"Furnizori_neachitati_{reportDate:yyyy-MM-dd}.pdf",
            AddExtension = true
        };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        var builder = new PdfReportBuilder();
        builder.BuildFurnizoriNeachitati(sfd.FileName, reportDate, data);

        MessageBox.Show(this, "PDF generat cu succes.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

public sealed class FacturaFurnizorRow
{
    public int Id { get; set; }
    public string Document { get; set; } = "";
    public DateTime DataDocument { get; set; }
    public DateTime Scadenta { get; set; }
    public string Furnizor { get; set; } = "";
    public int FurnizorId { get; set; }
    public string? CecOp { get; set; }
    public decimal Suma { get; set; }
    public bool Achitata { get; set; }
}

internal static class WinFormsHelpers
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
