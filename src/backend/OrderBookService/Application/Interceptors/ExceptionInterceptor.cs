using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;
using StackExchange.Redis;
using Status = OrderBookProtos.CustomTypes.Status;

namespace OrderBookService.Application.Interceptors;

public class ExceptionInterceptor: InterceptorBase
{
	private readonly IConnectionMultiplexer        _redisMultiplexer;
	private readonly IDatabaseAsync                _database;
	private readonly ILogger<ExceptionInterceptor> _logger;
 
	public ExceptionInterceptor(IConnectionMultiplexer redisMultiplexer , ILogger<ExceptionInterceptor> logger)
	{
		_redisMultiplexer = redisMultiplexer;
		_database         = _redisMultiplexer.GetDatabase();
		_logger           = logger;
	}
     
	public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
		TRequest                               request,
		ServerCallContext                      context,
		UnaryServerMethod<TRequest, TResponse> continuation)
	{
		try
		{
			return await continuation(request, context);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "{Message}", exception.Message);

			await WipeIdempotencyKey(request);
			
			Status responseStatus = new()
											{
												Code = (int)StatusCode.Internal,
												// If this API was exposed publicly, we might want to make this a generic message
												Message   = exception.Message 
											};

			return MapResponse<TRequest, TResponse>(responseStatus);
		}
	}

	private async Task WipeIdempotencyKey<TRequest>(TRequest request) where TRequest : class
	{
		if (typeof(TRequest).GetProperty(nameof(AddOrderRequest.IdempotencyKey))?.GetValue(request) is not string idempotencyKey) return;
		
		_logger.LogInformation("Clearing idempotency key due to exception for {@Request}", request);

		await _database.KeyDeleteAsync($"{StaticStrings.IdempotencyPrefix}{idempotencyKey}");
	}
}
