using OrderBookService.Protos.ServiceBases;

namespace OrderBookService.Services.ApplicationServices;

internal class OrderBookService: IOrderbookService
{

	public async Task<OrderBookModificationResponse> AddOrder(AddOrModifyOrderRequest request) => new() {Status = new() {IsSuccess = false, Message = "Not Yet Implemented"}};

	public async Task<OrderBookModificationResponse> RemoveOrder(RemoveOrderRequest request) =>  new() {Status = new() {IsSuccess = false, Message = "Not Yet Implemented"}};

	public async Task<OrderBookModificationResponse> ModifyOrder(AddOrModifyOrderRequest request) => new() {Status = new() {IsSuccess = false, Message = "Not Yet Implemented"}};

	public async Task<PriceResponse> GetPrice(GetPriceRequest getPriceRequest) =>  new() {Status = new() {IsSuccess = false, Message = "Not Yet Implemented"}, Price = new() { Units = 0, Nanos = 0}};
}
