using Dapper;
using Pgvector.Dapper;

namespace AICore.SemanticKernel.Database;

public static class DatabaseExtensions
{
    public static void AddDatabaseServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<DbContext>();
    }

    public static void ConfigureDatabaseServices(this WebApplication app)
    {
        SqlMapper.AddTypeHandler(new VectorTypeHandler());

        using IServiceScope scope = app.Services.CreateScope();
        DbContext context = scope.ServiceProvider.GetRequiredService<DbContext>();
        Task.Run(() => context.Init()).Wait();
    }
}