using System;
using System.IO;

namespace FinanteRaportare.WinForms.Services;

public static class AppPaths
{
    public static string GetAppDataDir()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FinanteRaportare");

        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string GetDbPath() => Path.Combine(GetAppDataDir(), "finante.db");
}
