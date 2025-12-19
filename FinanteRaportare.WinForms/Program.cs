using FinanteRaportare.WinForms.Services;
using FinanteRaportare.WinForms.UI;

namespace FinanteRaportare.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var dbPath = AppPaths.GetDbPath();
        var factory = new DbFactory(dbPath);
        DbInitializer.EnsureCreated(factory);

        Application.Run(new MainForm(factory));
    }
}
