using AutoMapper;
using MongoDB.Driver;
using OrderBookProtos.ServiceBases;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Models.Orders;
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

	public async Task<OrderBookModificationResponse> AddOrder(AddOrModifyOrderRequest request)
	{
		AssetDefinition assetDefinition = _mapper.Map<AssetDefinition>(request.AssetDefinition);
		OrderEntity     orderEntity     = _mapper.Map<OrderEntity>(request);

		UpdateResult res = await _orderBookRepository.AddOrderToOrderBook(assetDefinition, orderEntity);

		return new() {Status = new() {IsSuccess = res.IsAcknowledged, Message = res.IsAcknowledged ? "Success" : "Failed to alter order-book"}};
	}

	public async Task<OrderBookModificationResponse> RemoveOrder(RemoveOrderRequest request) =>  new() {Status = new() {IsSuccess = false, Message = "Not Yet Implemented"}};

	public async Task<OrderBookModificationResponse> ModifyOrder(AddOrModifyOrderRequest request) => new() {Status = new() {IsSuccess = false, Message = "Not Yet Implemented"}};

	public async Task<PriceResponse> GetPrice(GetPriceRequest getPriceRequest) =>  new() {Status = new() {IsSuccess = false, Message = "Not Yet Implemented"}, Price = new() { Units = 0, Nanos = 0}};
}
