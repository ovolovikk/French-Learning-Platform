using FrenchLearningPlatform.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace French_Learning_Platform.Services;

public static class AuthSeedService
{
    public static async Task EnsureAuthSchemaAsync(FrenchLearningPlatformDbContext context)
    {
        // Keep schema in sync for new registration fields in environments without migrations.
        await context.Database.ExecuteSqlRawAsync(
            "ALTER TABLE users ADD COLUMN IF NOT EXISTS first_name character varying;");
        await context.Database.ExecuteSqlRawAsync(
            "ALTER TABLE users ADD COLUMN IF NOT EXISTS last_name character varying;");
    }
}
