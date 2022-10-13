using OrderBookService.Domain.Repositories.Mongo.OrderBooks;
using OrderBookService.Domain.Services;

namespace OrderBookService.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrderBookServices(this IServiceCollection services)
	{
		services.AddTransient<IOrderBookService, Domain.Services.OrderBookService>();
		services.AddTransient<IOrderBookRepository, OrderBookRepository>();
		
		services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

		return services;
	}
}
