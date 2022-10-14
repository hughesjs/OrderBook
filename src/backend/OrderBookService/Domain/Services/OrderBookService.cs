using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using MongoDB.Driver;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Repositories.Mongo.OrderBooks;


namespace OrderBookService.Domain.Services;

internal class OrderBookService: IOrderBookService
{
	private readonly IMapper              _mapper;
	private readonly IOrderBookRepository _orderBookRepository;

	
	
	public OrderBookService(IMapper mapper, IOrderBookRepository orderBookRepository)
	{
		_mapper = mapper;
		_orderBookRepository = orderBookRepository;
	}

	// NB: Failure states are to be handled by the exception interceptor.
	//     The repo handles anticipated failure states before rethrowing if necessary.
	
	public async Task<OrderBookModificationResponse> AddOrder(AddOrModifyOrderRequest request)
	{
		AssetDefinition assetDefinition = _mapper.Map<AssetDefinition>(request.AssetDefinition);
		
		DateTime        effectiveFrom   = DateTime.UtcNow;
		OrderEntity     orderEntity     = _mapper.Map<OrderEntity>(request) with {EffectiveTime = effectiveFrom};

		await _orderBookRepository.AddOrderToOrderBook(assetDefinition, orderEntity);
		
		return new()
			   {
				   EffectiveFrom = effectiveFrom.ToTimestamp(),
				   Status = new()
							{
								IsSuccess = true,
								Message = "Successfully added order"
							}
			   };
	}

	public async Task<OrderBookModificationResponse> ModifyOrder(AddOrModifyOrderRequest request)
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
								IsSuccess = true,
								Message   = "Successfully modified order"
							}
			   };
	}

	public async Task<OrderBookModificationResponse> RemoveOrder(RemoveOrderRequest request)
	{
		AssetDefinition assetDefinition = _mapper.Map<AssetDefinition>(request.AssetDefinition);
		
		DateTime    effectiveFrom = DateTime.UtcNow;

		await _orderBookRepository.RemoveOrderFromBook(assetDefinition, request.OrderId.Value);
		
		return new()
			   {
				   EffectiveFrom = effectiveFrom.ToTimestamp(),
				   Status = new()
							{
								IsSuccess = true,
								Message   = "Successfully removed order"
							}
			   };
	}

	public async Task<PriceResponse> GetPrice(GetPriceRequest getPriceRequest) =>  new() {Status = new() {IsSuccess = false, Message = "Not Yet Implemented"}, Price = new() { Units = 0, Nanos = 0}};
}
