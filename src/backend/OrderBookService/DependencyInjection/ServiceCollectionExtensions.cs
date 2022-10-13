using OrderBookService.Domain.Services.OrderBook;

namespace OrderBookService.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrderBookServices(this IServiceCollection services)
	{
		services.AddTransient<IOrderBookService, Domain.Services.OrderBook.OrderBookService>();
		return services;
	}
}
