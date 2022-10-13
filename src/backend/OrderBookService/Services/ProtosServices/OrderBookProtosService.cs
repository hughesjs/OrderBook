using Grpc.Core;
using OrderBookService.Domain.Services.OrderBook;
using OrderBookService.Protos.ServiceBases;

namespace OrderBookService.Services.ProtosServices;

public class OrderBookProtosService: Protos.ServiceBases.OrderBookService.OrderBookServiceBase
{
	private readonly ILogger<OrderBookProtosService> _logger;
	private readonly IOrderBookService               _orderBookService;
	
	public OrderBookProtosService(ILogger<OrderBookProtosService> logger, IOrderBookService orderBookService)
	{
		_logger           = logger;
		_orderBookService = orderBookService;
	}


	public override async Task<OrderBookModificationResponse> AddOrder(AddOrModifyOrderRequest request, ServerCallContext context) => await _orderBookService.AddOrder(request);

	public override async Task<OrderBookModificationResponse> ModifyOrder(AddOrModifyOrderRequest request, ServerCallContext context) => await _orderBookService.ModifyOrder(request);

	public override async Task<OrderBookModificationResponse> RemoveOrder(RemoveOrderRequest request, ServerCallContext context) => await _orderBookService.RemoveOrder(request);

	public override async Task<PriceResponse> GetPrice(GetPriceRequest request, ServerCallContext context) => await _orderBookService.GetPrice(request);
}
