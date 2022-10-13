using MongoDB.Driver;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;

namespace OrderBookService.Domain.Repositories.Mongo.OrderBooks;

internal interface IOrderBookRepository
{
	public Task<OrderBookEntity?>  GetSingleAsync(AssetDefinition key);
	public Task<ReplaceOneResult> UpsertSingleAsync(OrderBookEntity orderBook);
}
