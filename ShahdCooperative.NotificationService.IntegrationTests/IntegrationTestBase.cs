namespace ShahdCooperative.NotificationService.IntegrationTests;

/// <summary>
/// Base class for integration tests that provides automatic cleanup after each test.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        // Clean before each test to ensure isolation
        await Factory.CleanupDatabaseAsync();

        // Larger delay to ensure cleanup is fully committed
        await Task.Delay(200);
    }

    public virtual async Task DisposeAsync()
    {
        // Clean after each test as well for safety
        await Factory.CleanupDatabaseAsync();

        // Delay to ensure cleanup completes before next test
        await Task.Delay(100);
    }
}
