using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;


namespace OrderBookService.ApiTests.TestInfrastructure;

public class OrderBookTestFixture : WebApplicationFactory<Program>
{
	protected override IHost CreateHost(IHostBuilder builder)
	{
		return base.CreateHost(builder);
	}

	public HttpMessageHandler Handler => Server.CreateHandler();
}
