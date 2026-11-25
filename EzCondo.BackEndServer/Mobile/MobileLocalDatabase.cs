using System.Diagnostics;

namespace EzCondo.BackEndServer.Mobile;

public static class MobileLocalDatabase
{
    public static async Task StartAsync(string localPath)
    {
        // Only the explicit Mobile Local profile starts the isolated developer database.
        var control = Path.Combine(localPath, "postgresql/pgsql/bin/pg_ctl.exe");
        var data = Path.Combine(localPath, "postgresql/data");
        if (!File.Exists(control) || !Directory.Exists(data))
            throw new InvalidOperationException("Run mobile/Initialize-Postgres.ps1 first.");
        async Task<int> Run(params string[] arguments)
        {
            // The server daemon inherits pipes; waiting for their EOF would block API startup.
            var info = new ProcessStartInfo(control) { CreateNoWindow = true, UseShellExecute = false };
            foreach (var argument in arguments) info.ArgumentList.Add(argument);
            using var process = Process.Start(info)!;
            await process.WaitForExitAsync();
            return process.ExitCode;
        }
        if (await Run("-D", data, "status") == 0) return;
        if (await Run("-D", data, "-l", Path.Combine(localPath, "postgresql/server.log"), "-w", "start") != 0)
            throw new InvalidOperationException("Local PostgreSQL could not start. Read mobile/.local/postgresql/server.log.");
    }
}
