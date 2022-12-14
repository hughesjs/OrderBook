using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Models.OrderBooks;
using OrderBookService.Domain.Models.Orders;
using OrderBookService.Domain.Repositories.Mongo.OrderBooks;


namespace OrderBookService.Domain.Services;

internal class OrderBookService : IOrderBookService
{
	private readonly IMapper              _mapper;
	private readonly IOrderBookRepository _orderBookRepository;


	public OrderBookService(IMapper mapper, IOrderBookRepository orderBookRepository)
	{
		_mapper              = mapper;
		_orderBookRepository = orderBookRepository;
	}

	// NB: Failure states are to be handled by the exception interceptor.
	//     The repo handles anticipated failure states before rethrowing if necessary.

	public async Task<AddOrderResponse> AddOrder(AddOrderRequest request)
	{
		AssetDefinition assetDefinition = _mapper.Map<AssetDefinition>(request.AssetDefinition);

		DateTime    effectiveFrom = DateTime.UtcNow;
		Guid        orderId       = Guid.NewGuid();
		OrderEntity orderEntity   = _mapper.Map<OrderEntity>(request) with
									{
										EffectiveTime = effectiveFrom,
										Id = orderId.ToString()
									};

		await _orderBookRepository.AddOrderToOrderBook(assetDefinition, orderEntity);

		return new()
			   {
				   OrderId       = new(){Value = orderId.ToString()},
				   EffectiveFrom = effectiveFrom.ToTimestamp(),
				   Status = new()
							{
								Code    = (int)StatusCode.OK,
								Message = StaticStrings.SuccessMessage
							}
				   
			   };
	}

	public async Task<ModifyOrderResponse> ModifyOrder(ModifyOrderRequest request)
	{
		AssetDefinition assetDefinition = _mapper.Map<AssetDefinition>(request.AssetDefinition);

		DateTime    effectiveFrom = DateTime.UtcNow;
		OrderEntity orderEntity   = _mapper.Map<OrderEntity>(request) with {EffectiveTime = effectiveFrom};

		await _orderBookRepository.ModifyOrderInOrderBook(assetDefinition, orderEntity);

		return new()
			   {
				   EffectiveFrom = effectiveFrom.ToTimestamp(),
				   Status = new()
							{
								Code    = (int)StatusCode.OK,
								Message = StaticStrings.SuccessMessage
							}
			   };
	}

	public async Task<ModifyOrderResponse> RemoveOrder(RemoveOrderRequest request)
	{
		AssetDefinition assetDefinition = _mapper.Map<AssetDefinition>(request.AssetDefinition);
		DateTime        effectiveFrom   = DateTime.UtcNow;

		await _orderBookRepository.RemoveOrderFromBook(assetDefinition, request.OrderId.Value);

		return new()
			   {
				   EffectiveFrom = effectiveFrom.ToTimestamp(),
				   Status = new()
							{
								Code    = (int)StatusCode.OK,
								Message = StaticStrings.SuccessMessage
							}
			   };
	}

	public async Task<PriceResponse> GetPrice(GetPriceRequest request)
	{
		AssetDefinition assetDefinition = _mapper.Map<AssetDefinition>(request.AssetDefinition);
		DateTime        validAt         = DateTime.UtcNow;

		OrderBookEntity orderBookEntity = await _orderBookRepository.GetSingleAsync(assetDefinition);
		OrderBook       orderBook       = _mapper.Map<OrderBook>(orderBookEntity);


		IOrderedEnumerable<Order> relevantOrders = request.OrderAction == OrderAction.Buy
													   ? orderBook.Where(o => o.OrderAction != request.OrderAction).OrderBy(o => o.Price)
													   : orderBook.Where(o => o.OrderAction != request.OrderAction).OrderByDescending(o => o.Price);


		decimal? vwap = CalculateVwapForSortedOrders(relevantOrders, request.Amount);


		return vwap is not null
				   ? new()
					 {
						 Price   = vwap,
						 ValidAt = validAt.ToTimestamp(),
						 Status = new()
								  {
									  Code    = (int)StatusCode.OK,
									  Message = StaticStrings.SuccessMessage
								  }
					 }
				   : new()
					 {
						 Price   = null,
						 ValidAt = validAt.ToTimestamp(),
						 Status = new()
								  {
									  Code    = (int)StatusCode.Internal,
									  Message = StaticStrings.UnsatisfiableOrderMessage
								  }
					 };
	}

	private decimal? CalculateVwapForSortedOrders(IEnumerable<Order> sortedOrders, decimal amountWanted)
	{
		decimal costAccumulator       = 0;
		decimal assetsLeftDecumulator = amountWanted;
		foreach (Order order in sortedOrders)
		{
			if (assetsLeftDecumulator - order.Amount < 0)
			{
				costAccumulator       += order.Price*assetsLeftDecumulator;
				assetsLeftDecumulator =  0;
				break;
			}

			costAccumulator       += order.Amount*order.Price;
			assetsLeftDecumulator -= order.Amount;
		}

		decimal price = costAccumulator/amountWanted;

		return assetsLeftDecumulator == 0 ? price : null;
	}
}
