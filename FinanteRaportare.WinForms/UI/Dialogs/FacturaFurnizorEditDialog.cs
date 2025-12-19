using FinanteRaportare.WinForms.Data.Entities;

namespace FinanteRaportare.WinForms.UI.Dialogs;

public sealed class FacturaFurnizorEditDialog : Form
{
    public FacturaFurnizor Model { get; }

    private readonly ComboBox _cbFurnizor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
    private readonly TextBox _tbDocument = new() { Width = 320 };
    private readonly DateTimePicker _dpDataDoc = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy" };
    private readonly DateTimePicker _dpScadenta = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy" };
    private readonly TextBox _tbCecOp = new() { Width = 160 };
    private readonly NumericUpDown _numSuma = new() { Width = 160, DecimalPlaces = 2, ThousandsSeparator = true, Minimum = -1000000000, Maximum = 1000000000 };
    private readonly CheckBox _chkAchitata = new() { Text = "Achitata" };

    private readonly List<Furnizor> _furnizori;

    public FacturaFurnizorEditDialog(FacturaFurnizor model, List<Furnizor> furnizori)
    {
        Model = model;
        _furnizori = furnizori;

        Text = "Factura furnizor";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 620;
        Height = 330;

        BuildUi();
        LoadFromModel();
    }

    private void BuildUi()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(12),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label { Text = "Furnizor", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 0);
        layout.Controls.Add(_cbFurnizor, 1, 0);

        layout.Controls.Add(new Label { Text = "Document", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 1);
        layout.Controls.Add(_tbDocument, 1, 1);

        layout.Controls.Add(new Label { Text = "Data document", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 2);
        layout.Controls.Add(_dpDataDoc, 1, 2);

        layout.Controls.Add(new Label { Text = "Scadenta", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 3);
        layout.Controls.Add(_dpScadenta, 1, 3);

        var row4 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false };
        row4.Controls.Add(new Label { Text = "CEC/OP", AutoSize = true, Padding = new Padding(0, 6, 8, 0) });
        row4.Controls.Add(_tbCecOp);
        row4.Controls.Add(new Label { Text = "Suma", AutoSize = true, Padding = new Padding(16, 6, 8, 0) });
        row4.Controls.Add(_numSuma);
        row4.Controls.Add(_chkAchitata);

        layout.Controls.Add(row4, 1, 4);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 44,
            Padding = new Padding(12, 6, 12, 6)
        };

        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
        btnOk.Click += (_, _) =>
        {
            if (!SaveToModel())
            {
                DialogResult = DialogResult.None;
            }
        };

        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(btnOk);
        buttons.Controls.Add(btnCancel);

        Controls.Add(layout);
        Controls.Add(buttons);
    }

    private void LoadFromModel()
    {
        _cbFurnizor.DataSource = _furnizori;
        _cbFurnizor.DisplayMember = nameof(Furnizor.Denumire);
        _cbFurnizor.ValueMember = nameof(Furnizor.Id);

        var idx = _furnizori.FindIndex(f => f.Id == Model.FurnizorId);
        _cbFurnizor.SelectedIndex = idx >= 0 ? idx : 0;

        _tbDocument.Text = Model.Document;
        _dpDataDoc.Value = Model.DataDocument == default ? DateTime.Today : Model.DataDocument;
        _dpScadenta.Value = Model.Scadenta == default ? DateTime.Today : Model.Scadenta;
        _tbCecOp.Text = Model.CecOp ?? "";
        _numSuma.Value = Model.Suma;
        _chkAchitata.Checked = Model.Achitata;
    }

    private bool SaveToModel()
    {
        if (_cbFurnizor.SelectedItem is not Furnizor f)
        {
            MessageBox.Show(this, "Selecteaza un furnizor.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var doc = _tbDocument.Text.Trim();
        if (string.IsNullOrWhiteSpace(doc))
        {
            MessageBox.Show(this, "Document este obligatoriu (ex: F:3634/08.08.2019).", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        Model.FurnizorId = f.Id;
        Model.Document = doc;
        Model.DataDocument = _dpDataDoc.Value.Date;
        Model.Scadenta = _dpScadenta.Value.Date;
        Model.CecOp = string.IsNullOrWhiteSpace(_tbCecOp.Text) ? null : _tbCecOp.Text.Trim();
        Model.Suma = _numSuma.Value;
        Model.Achitata = _chkAchitata.Checked;

        return true;
    }
}
