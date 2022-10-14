using Grpc.Core;
using Grpc.Core.Interceptors;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;

namespace OrderBookService.Application.Interceptors;

public class ExceptionInterceptor: Interceptor
{
	private readonly ILogger<ExceptionInterceptor> _logger;
 
	public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
	{
		_logger = logger;
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
			_logger.LogError(exception, exception.Message);

			ResponseStatus responseStatus = new ResponseStatus()
											{
												IsSuccess = false,
												// If this API was exposed publicly, we might want to make this a generic message
												Message   = exception.Message 
											};

			return MapResponse<TRequest, TResponse>(responseStatus);
		}
	}
	
				
	private TResponse MapResponse<TRequest, TResponse>(ResponseStatus responseStatus)
	{
		var concreteResponse = Activator.CreateInstance<TResponse>();
        
		concreteResponse?.GetType().GetProperty(nameof(PriceResponse.Status))?.SetValue(concreteResponse, responseStatus);
		
		return concreteResponse;
	}
}
