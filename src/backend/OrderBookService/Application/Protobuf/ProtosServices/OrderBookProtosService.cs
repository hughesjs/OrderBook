using Grpc.Core;
using OrderBookProtos.ServiceBases;
using OrderBookService.Domain.Services;

namespace OrderBookService.Application.Protobuf.ProtosServices;

internal class OrderBookProtosService: OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceBase
{
	private readonly ILogger<OrderBookProtosService> _logger;
	private readonly IOrderBookService               _orderBookService;
	
	public OrderBookProtosService(ILogger<OrderBookProtosService> logger, IOrderBookService orderBookService)
	{
		_logger           = logger;
		_orderBookService = orderBookService;
	}


	public override async Task<AddOrderResponse> AddOrder(AddOrderRequest request, ServerCallContext context) => await _orderBookService.AddOrder(request);

	public override async Task<ModifyOrderResponse> ModifyOrder(ModifyOrderRequest request, ServerCallContext context) => await _orderBookService.ModifyOrder(request);

	public override async Task<ModifyOrderResponse> RemoveOrder(RemoveOrderRequest request, ServerCallContext context) => await _orderBookService.RemoveOrder(request);

	public override async Task<PriceResponse> GetPrice(GetPriceRequest request, ServerCallContext context) => await _orderBookService.GetPrice(request);
}
