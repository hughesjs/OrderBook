using OrderBookService.Protos.ServiceBases;

namespace OrderBookService.Services.ApplicationServices;

public interface IOrderbookService
{
	public Task<OrderBookModificationResponse> AddOrder(AddOrModifyOrderRequest request);
	public Task<OrderBookModificationResponse> RemoveOrder(RemoveOrderRequest request);
	public Task<OrderBookModificationResponse> ModifyOrder(AddOrModifyOrderRequest request);
	public Task<PriceResponse> GetPrice(GetPriceRequest getPriceRequest);
}
