using FinanteRaportare.WinForms.Data.Entities;

namespace FinanteRaportare.WinForms.UI.Dialogs;

public sealed class ClientEditDialog : Form
{
    public Client Model { get; }

    private readonly TextBox _txtDenumire = new() { Width = 360 };
    private readonly TextBox _txtCui = new() { Width = 360 };
    private readonly TextBox _txtAdresa = new() { Width = 360 };
    private readonly TextBox _txtObs = new() { Width = 360, Multiline = true, Height = 80 };

    public ClientEditDialog(Client model)
    {
        Model = model;

        Text = "Client";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 520;
        Height = 360;

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(12),
            AutoSize = true
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        panel.Controls.Add(new Label { Text = "Denumire", AutoSize = true }, 0, 0);
        panel.Controls.Add(_txtDenumire, 1, 0);

        panel.Controls.Add(new Label { Text = "CUI", AutoSize = true }, 0, 1);
        panel.Controls.Add(_txtCui, 1, 1);

        panel.Controls.Add(new Label { Text = "Adresa", AutoSize = true }, 0, 2);
        panel.Controls.Add(_txtAdresa, 1, 2);

        panel.Controls.Add(new Label { Text = "Observatii", AutoSize = true }, 0, 3);
        panel.Controls.Add(_txtObs, 1, 3);

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
        _txtDenumire.Text = Model.Denumire;
        _txtCui.Text = Model.Cui ?? "";
        _txtAdresa.Text = Model.Adresa ?? "";
        _txtObs.Text = Model.Observatii ?? "";
    }

    private bool Apply()
    {
        var den = _txtDenumire.Text.Trim();
        if (string.IsNullOrWhiteSpace(den))
        {
            MessageBox.Show(this, "Denumirea este obligatorie.", "Validare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        Model.Denumire = den;
        Model.Cui = NullIfEmpty(_txtCui.Text);
        Model.Adresa = NullIfEmpty(_txtAdresa.Text);
        Model.Observatii = NullIfEmpty(_txtObs.Text);
        return true;
    }

    private static string? NullIfEmpty(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
