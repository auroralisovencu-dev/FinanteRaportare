using FinanteRaportare.WinForms.Data.Entities;

namespace FinanteRaportare.WinForms.UI.Dialogs;

public sealed class SoldZilnicEditDialog : Form
{
    public SoldZilnic Model { get; }

    private readonly DateTimePicker _dtData = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy" };
    private readonly NumericUpDown _numSeif = MakeMoney();
    private readonly NumericUpDown _numBt = MakeMoney();
    private readonly NumericUpDown _numBcr = MakeMoney();
    private readonly NumericUpDown _numTrez = MakeMoney();
    private readonly TextBox _txtObs = new() { Width = 360, Multiline = true, Height = 60 };

    private readonly Label _lblTotal = new() { AutoSize = true, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold) };

    public SoldZilnicEditDialog(SoldZilnic model)
    {
        Model = model;

        Text = "Disponibilitati zilnice";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 560;
        Height = 420;

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(12),
            AutoSize = true
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int r = 0;
        panel.Controls.Add(new Label { Text = "Data", AutoSize = true }, 0, r);
        panel.Controls.Add(_dtData, 1, r++);

        panel.Controls.Add(new Label { Text = "Cash Seif", AutoSize = true }, 0, r);
        panel.Controls.Add(_numSeif, 1, r++);

        panel.Controls.Add(new Label { Text = "BT", AutoSize = true }, 0, r);
        panel.Controls.Add(_numBt, 1, r++);

        panel.Controls.Add(new Label { Text = "BCR", AutoSize = true }, 0, r);
        panel.Controls.Add(_numBcr, 1, r++);

        panel.Controls.Add(new Label { Text = "Trezorerie", AutoSize = true }, 0, r);
        panel.Controls.Add(_numTrez, 1, r++);

        panel.Controls.Add(new Label { Text = "Total", AutoSize = true }, 0, r);
        panel.Controls.Add(_lblTotal, 1, r++);

        panel.Controls.Add(new Label { Text = "Observatii", AutoSize = true }, 0, r);
        panel.Controls.Add(_txtObs, 1, r++);

        _numSeif.ValueChanged += (_, _) => UpdateTotal();
        _numBt.ValueChanged += (_, _) => UpdateTotal();
        _numBcr.ValueChanged += (_, _) => UpdateTotal();
        _numTrez.ValueChanged += (_, _) => UpdateTotal();

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12),
            WrapContents = false
        };

        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        btnOk.Click += (_, _) => { if (!Apply()) DialogResult = DialogResult.None; };

        buttons.Controls.Add(btnOk);
        buttons.Controls.Add(btnCancel);

        Controls.Add(panel);
        Controls.Add(buttons);

        LoadModel();
    }

    private void LoadModel()
    {
        _dtData.Value = Model.Data;
        _numSeif.Value = Model.CashSeif;
        _numBt.Value = Model.BT;
        _numBcr.Value = Model.BCR;
        _numTrez.Value = Model.Trezorerie;
        _txtObs.Text = Model.Observatii ?? "";
        UpdateTotal();
    }

    private void UpdateTotal()
    {
        var total = _numSeif.Value + _numBt.Value + _numBcr.Value + _numTrez.Value;
        _lblTotal.Text = $"{total:n2}";
    }

    private bool Apply()
    {
        Model.Data = _dtData.Value.Date;
        Model.CashSeif = _numSeif.Value;
        Model.BT = _numBt.Value;
        Model.BCR = _numBcr.Value;
        Model.Trezorerie = _numTrez.Value;
        Model.Observatii = string.IsNullOrWhiteSpace(_txtObs.Text) ? null : _txtObs.Text.Trim();
        return true;
    }

    private static NumericUpDown MakeMoney()
        => new()
        {
            Width = 180,
            DecimalPlaces = 2,
            Maximum = 1000000000,
            Minimum = -1000000000,
            ThousandsSeparator = true,
            Increment = 10
        };
}
