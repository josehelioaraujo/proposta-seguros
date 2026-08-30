using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PropostaService.IntegrationTests.Fixtures;

public class InMemoryApiFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:UsarBancoDados"] = "false",
                ["Features:UsarRabbitMQ"]   = "false"
            });
        });
    }
}
