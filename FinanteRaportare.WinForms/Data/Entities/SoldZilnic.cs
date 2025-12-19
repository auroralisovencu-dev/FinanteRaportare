namespace FinanteRaportare.WinForms.Data.Entities;

public sealed class SoldZilnic
{
    public int Id { get; set; }
    public DateTime Data { get; set; } = DateTime.Today;

    public decimal CashSeif { get; set; }
    public decimal BT { get; set; }
    public decimal BCR { get; set; }
    public decimal Trezorerie { get; set; }

    public string? Observatii { get; set; }

    public decimal Total => CashSeif + BT + BCR + Trezorerie;
}
