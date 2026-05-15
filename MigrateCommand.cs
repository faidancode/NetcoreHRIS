using NetcoreHRIS.Data;
using Microsoft.EntityFrameworkCore;

namespace NetcoreHRIS;

public static class MigrateCommand
{
    public static async Task RunAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
    }
}
