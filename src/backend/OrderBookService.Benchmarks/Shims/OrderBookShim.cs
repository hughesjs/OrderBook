using BenchmarkDotNet.Attributes;
using MongoDB.Driver;
using OrderBookProtos.ServiceBases;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Repositories.Mongo.OrderBooks;

namespace OrderBookService.Benchmarks.Shims;

internal class OrderBookShim: IOrderBookRepository
{


	private readonly Task<OrderBookEntity> _retTask;

	public OrderBookShim(decimal amountWanted, int ordersNeededToComplete)
	{
		List<OrderEntity> orders = new();
		bool              isBuy  = false;

		for (int i = 0; i < 2 * ordersNeededToComplete; i++)
		{
			orders.Add(new()
					   {
						   EffectiveTime = DateTime.UtcNow,
						   Amount        = amountWanted / ordersNeededToComplete,
						   Id = Guid.NewGuid()
									.ToString(),
						   OrderAction = isBuy
											 ? OrderAction.Buy
											 : OrderAction.Sell,
						   Price = Random.Shared.NextInt64()
					   });
			isBuy = !isBuy;
		}


		OrderBookEntity obe = new()
							   {
								   UnderlyingAsset = null!,
								   Orders          = orders
							   };

		_retTask = Task.FromResult(obe);
	}

	public Task<OrderBookEntity> GetSingleAsync(AssetDefinition key) => _retTask;

	public Task<ReplaceOneResult> UpsertSingleAsync(OrderBookEntity orderBook) => throw new NotImplementedException();

	public Task AddOrderToOrderBook(AssetDefinition asset, OrderEntity order) => throw new NotImplementedException();

	public Task ModifyOrderInOrderBook(AssetDefinition asset, OrderEntity order) => throw new NotImplementedException();

	public Task RemoveOrderFromBook(AssetDefinition asset, string orderId) => throw new NotImplementedException();
}
