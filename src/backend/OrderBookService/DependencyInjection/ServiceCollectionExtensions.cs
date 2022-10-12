using OrderBookService.Services.ApplicationServices;

namespace OrderBookService.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrderBookServices(this IServiceCollection services)
	{
		services.AddTransient<IOrderbookService, Services.ApplicationServices.OrderBookService>();

		return services;
	}
}
