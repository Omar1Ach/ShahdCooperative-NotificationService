namespace ShahdCooperative.NotificationService.IntegrationTests;

/// <summary>
/// Defines a collection for integration tests to ensure they run sequentially
/// and share the same CustomWebApplicationFactory instance.
/// </summary>
[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // This class is never instantiated, it's just used to define the collection
}
