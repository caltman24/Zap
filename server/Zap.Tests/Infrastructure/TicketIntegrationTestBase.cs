namespace Zap.Tests.Infrastructure;

public abstract class TicketIntegrationTestBase : IntegrationTestBase
{
    protected readonly TicketScenarioBuilder _tickets;

    protected TicketIntegrationTestBase()
    {
        _tickets = new TicketScenarioBuilder(_app, _db);
    }
}