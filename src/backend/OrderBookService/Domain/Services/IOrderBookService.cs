using OrderBookProtos.ServiceBases;

namespace OrderBookService.Domain.Services;

public interface IOrderBookService
{
	public Task<AddOrderResponse>    AddOrder(AddOrderRequest request);
	public Task<ModifyOrderResponse> RemoveOrder(RemoveOrderRequest   request);
	public Task<ModifyOrderResponse> ModifyOrder(ModifyOrderRequest request);
	public Task<PriceResponse> GetPrice(GetPriceRequest getPriceRequest);
}
