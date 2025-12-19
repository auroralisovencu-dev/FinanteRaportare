using FinanteRaportare.WinForms.Data;

namespace FinanteRaportare.WinForms.Services;

public sealed class DbFactory
{
    private readonly string _dbPath;

    public DbFactory(string dbPath)
    {
        _dbPath = dbPath;
    }

    public AppDbContext Create() => new(_dbPath);
}
