namespace BooksPortal.IntegrationTests;

[CollectionDefinition("Integration API", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebApplicationFactory>
{
}
