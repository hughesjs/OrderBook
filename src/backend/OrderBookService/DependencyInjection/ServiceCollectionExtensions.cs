using OrderBookService.Domain.Repositories.Mongo.OrderBooks;
using OrderBookService.Domain.Services;
using StackExchange.Redis;

namespace OrderBookService.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOrderBookServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<IOrderBookService, Domain.Services.OrderBookService>();
		services.AddTransient<IOrderBookRepository, OrderBookRepository>();

		string                redisConnectionString = config.GetSection("RedisSettings:ConnectionString").Get<string>();
		ConnectionMultiplexer multiplexer           = ConnectionMultiplexer.Connect(redisConnectionString);
		services.AddSingleton<IConnectionMultiplexer>(multiplexer);
		
		services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

		return services;
	}
}
