using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;
using StackExchange.Redis;

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
		if (typeof(TRequest).GetProperty(nameof(AddOrModifyOrderRequest.IdempotencyKey)) is null)
		{
			return await continuation(request, context);
		}

		if (typeof(TRequest).GetProperty(nameof(AddOrModifyOrderRequest.IdempotencyKey))?.GetValue(request) is not GuidValue idempotencyKey)
		{
			ResponseStatus status = new()
									{
										IsSuccess = false,
										Message   = StaticStrings.NoIdempotencyKeyProvidedMessage
									};
			return MapResponse<TRequest, TResponse>(status);
		}

		string redisKey = $"{StaticStrings.IdempotencyPrefix}{idempotencyKey}";

		if (await _redis.KeyExistsAsync(redisKey))
		{
			ResponseStatus status = new()
									{
										IsSuccess = false,
										Message   = StaticStrings.IdempotentOperationAlreadyCompleteMessage
									};
			return MapResponse<TRequest, TResponse>(status);
		}

		await _redis.SetAddAsync(redisKey, "");
		await _redis.KeyExpireAsync(redisKey, DateTime.Now + TimeSpan.FromHours(1));
		
		return await continuation(request, context);
	}
}
