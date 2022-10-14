using MongoDB.Driver;
using OrderBookProtos.CustomTypes;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;

namespace OrderBookService.Domain.Repositories.Mongo.OrderBooks;

internal interface IOrderBookRepository
{
	public Task<OrderBookEntity>  GetSingleAsync(AssetDefinition      key);
	public Task<ReplaceOneResult> UpsertSingleAsync(OrderBookEntity   orderBook);
	public Task                   AddOrderToOrderBook(AssetDefinition asset, OrderEntity order);
	
	public Task ModifyOrderInOrderBook(AssetDefinition asset, OrderEntity order);
	Task        RemoveOrderFromBook(AssetDefinition    asset, string      orderId);
}
