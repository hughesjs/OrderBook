using Grpc.Core.Interceptors;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;

namespace OrderBookService.Application.Interceptors;

public abstract class InterceptorBase: Interceptor
{
	protected TResponse MapResponse<TRequest, TResponse>(Status responseStatus)
	{
		TResponse concreteResponse = Activator.CreateInstance<TResponse>();
        
		concreteResponse?.GetType().GetProperty(nameof(PriceResponse.Status))?.SetValue(concreteResponse, responseStatus);
		
		return concreteResponse;
	}
}
