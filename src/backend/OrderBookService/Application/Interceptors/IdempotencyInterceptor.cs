using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;
using StackExchange.Redis;
using Status = OrderBookProtos.CustomTypes.Status;

namespace OrderBookService.Application.Interceptors;

public class IdempotencyInterceptor : InterceptorBase
{
	private readonly IConnectionMultiplexer        _redisMultiplexer;
	private readonly IDatabaseAsync                _redis;
	private readonly ILogger<ExceptionInterceptor> _logger;

	public IdempotencyInterceptor(IConnectionMultiplexer redisMultiplexer, ILogger<ExceptionInterceptor> logger)
	{
		_redisMultiplexer = redisMultiplexer;
		_redis            = redisMultiplexer.GetDatabase();
		_logger           = logger;
	}

	public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>
	(
		TRequest                               request,
		ServerCallContext                      context,
		UnaryServerMethod<TRequest, TResponse> continuation
	)
	{
		if (typeof(TRequest).GetProperty(nameof(AddOrderRequest.IdempotencyKey)) is null)
		{
			return await continuation(request, context);
		}

		
		if (typeof(TRequest).GetProperty(nameof(AddOrderRequest.IdempotencyKey))?.GetValue(request) is not GuidValue idempotencyKey)
		{
			Status status = new()
									{
										Code    = (int)StatusCode.InvalidArgument,
										Message = StaticStrings.NoIdempotencyKeyProvidedMessage
									};
			return MapResponse<TRequest, TResponse>(status);
		}

		string redisKey = $"{StaticStrings.IdempotencyPrefix}{idempotencyKey}";

		if (await _redis.KeyExistsAsync(redisKey))
		{
			Status status = new()
									{
										Code    = (int)StatusCode.InvalidArgument,
										Message = StaticStrings.IdempotentOperationAlreadyCompleteMessage
									};
			return MapResponse<TRequest, TResponse>(status);
		}

		await _redis.SetAddAsync(redisKey, "");
		await _redis.KeyExpireAsync(redisKey, DateTime.Now + TimeSpan.FromHours(1));
		
		return await continuation(request, context);
	}
}
