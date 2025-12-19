namespace FinanteRaportare.WinForms.Data.Entities;

public sealed class FacturaClient
{
    public int Id { get; set; }

    // Ex: F:34398/31.10.2025
    public string Document { get; set; } = "";
    public DateTime DataDocument { get; set; } = DateTime.Today;
    public DateTime Scadenta { get; set; } = DateTime.Today;

    // coloana CEC/OP (text liber)
    public string? CecOp { get; set; }

    // suma de incasat (pozitiv/negativ)
    public decimal Suma { get; set; }

    public bool Incasata { get; set; } = false;

    public int ClientId { get; set; }
    public Client? Client { get; set; }
}
