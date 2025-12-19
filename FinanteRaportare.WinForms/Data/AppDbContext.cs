using FinanteRaportare.WinForms.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanteRaportare.WinForms.Data;

public sealed class AppDbContext : DbContext
{
    private readonly string _dbPath;

    public AppDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    public DbSet<Client> Clienti => Set<Client>();
    public DbSet<Furnizor> Furnizori => Set<Furnizor>();
    public DbSet<FacturaClient> FacturiClienti => Set<FacturaClient>();
    public DbSet<FacturaFurnizor> FacturiFurnizori => Set<FacturaFurnizor>();
    public DbSet<SoldZilnic> SolduriZilnice => Set<SoldZilnic>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SoldZilnic>()
            .HasIndex(x => x.Data)
            .IsUnique();

        modelBuilder.Entity<Client>()
            .HasIndex(x => x.Denumire)
            .IsUnique();

        modelBuilder.Entity<Furnizor>()
            .HasIndex(x => x.Denumire)
            .IsUnique();
    }
}
