namespace Zap.Tests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncDisposable
{
    protected readonly ZapApplication _app;
    protected readonly AppDbContext _db;

    protected IntegrationTestBase(bool useInMemoryDatabase = true)
        : this(null, useInMemoryDatabase)
    {
    }

    protected IntegrationTestBase(Dictionary<string, string?>? configurationOverrides, bool useInMemoryDatabase = true)
    {
        _app = new ZapApplication(configurationOverrides, useInMemoryDatabase);
        _db = _app.CreateAppDbContext();
    }

    public virtual async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
        await _db.DisposeAsync();
    }
}