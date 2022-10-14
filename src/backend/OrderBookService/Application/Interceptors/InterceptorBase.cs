using Grpc.Core.Interceptors;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;

namespace OrderBookService.Application.Interceptors;

public abstract class InterceptorBase: Interceptor
{
	protected TResponse MapResponse<TRequest, TResponse>(ResponseStatus responseStatus)
	{
		var concreteResponse = Activator.CreateInstance<TResponse>();
        
		concreteResponse?.GetType().GetProperty(nameof(PriceResponse.Status))?.SetValue(concreteResponse, responseStatus);
		
		return concreteResponse;
	}
}
