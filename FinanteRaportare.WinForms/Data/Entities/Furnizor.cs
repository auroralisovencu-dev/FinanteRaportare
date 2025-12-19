namespace FinanteRaportare.WinForms.Data.Entities;

public sealed class Furnizor
{
    public int Id { get; set; }
    public string Denumire { get; set; } = "";
    public string? Cui { get; set; }
    public string? Adresa { get; set; }
    public string? Observatii { get; set; }

    public override string ToString() => Denumire;
}
