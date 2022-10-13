using OrderBookService.Protos.ServiceBases;

namespace OrderBookService.Domain.Services.OrderBook;

public interface IOrderBookService
{
	public Task<OrderBookModificationResponse> AddOrder(AddOrModifyOrderRequest request);
	public Task<OrderBookModificationResponse> RemoveOrder(RemoveOrderRequest request);
	public Task<OrderBookModificationResponse> ModifyOrder(AddOrModifyOrderRequest request);
	public Task<PriceResponse> GetPrice(GetPriceRequest getPriceRequest);
}
