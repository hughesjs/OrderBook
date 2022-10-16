using OrderBookService.Application.Config;

namespace OrderBookService.Application.DependencyInjection;

public static class WebApplicationBuilderExtensions
{
	public static WebApplicationBuilder ConfigureOrderBookServices(this WebApplicationBuilder builder)
	{
		builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));
		return builder;
	}
}
