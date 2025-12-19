using FinanteRaportare.WinForms.Data;

namespace FinanteRaportare.WinForms.Services;

public static class DbInitializer
{
    public static void EnsureCreated(DbFactory factory)
    {
        using var db = factory.Create();
        db.Database.EnsureCreated();
    }
}
