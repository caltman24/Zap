namespace Zap.Tests.Infrastructure;

public abstract class TicketIntegrationTestBase : IntegrationTestBase
{
    protected readonly TicketScenarioBuilder _tickets;

    protected TicketIntegrationTestBase(bool useInMemoryDatabase = true)
        : base(useInMemoryDatabase)
    {
        _tickets = new TicketScenarioBuilder(_app, _db);
    }
}
