namespace FinanteRaportare.WinForms.Data.Entities;

public sealed class FacturaFurnizor
{
    public int Id { get; set; }

    // Ex: F:3634/08.08.2019
    public string Document { get; set; } = "";
    public DateTime DataDocument { get; set; } = DateTime.Today;
    public DateTime Scadenta { get; set; } = DateTime.Today;

    // coloana CEC/OP (text liber)
    public string? CecOp { get; set; }

    public decimal Suma { get; set; }

    public bool Achitata { get; set; } = false;

    public int FurnizorId { get; set; }
    public Furnizor? Furnizor { get; set; }
}
