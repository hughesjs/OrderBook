using Grpc.Core;
using OrderBookService.Protos.ServiceBases;
using OrderBookService.Services.ApplicationServices;

namespace OrderBookService.Services.ProtosServices;

public class OrderBookProtosService: Protos.ServiceBases.OrderBookService.OrderBookServiceBase
{
	private readonly ILogger<OrderBookProtosService> _logger;
	private readonly IOrderbookService               _orderbookService;
	
	public OrderBookProtosService(ILogger<OrderBookProtosService> logger, IOrderbookService orderbookService)
	{
		_logger           = logger;
		_orderbookService = orderbookService;
	}


	public override async Task<OrderBookModificationResponse> AddOrder(AddOrModifyOrderRequest request, ServerCallContext context) => await _orderbookService.AddOrder(request);

	public override async Task<OrderBookModificationResponse> ModifyOrder(AddOrModifyOrderRequest request, ServerCallContext context) => await _orderbookService.ModifyOrder(request);

	public override async Task<OrderBookModificationResponse> RemoveOrder(RemoveOrderRequest request, ServerCallContext context) => await _orderbookService.RemoveOrder(request);

	public override async Task<PriceResponse> GetPrice(GetPriceRequest request, ServerCallContext context) => await _orderbookService.GetPrice(request);
}
