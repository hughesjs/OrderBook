using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderBookService.Application.Config;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;

namespace OrderBookService.Domain.Repositories.Mongo.OrderBooks;

internal sealed class OrderBookRepository: MongoRepositoryBase<OrderBookEntity, AssetDefinition>, IOrderBookRepository
{
	private static readonly ReplaceOptions UpsertOptions = new() {IsUpsert = true};

	public OrderBookRepository(IOptions<MongoDbSettings> mongoSettings) : base(mongoSettings) { }

	public override async Task<OrderBookEntity?> GetSingleAsync(AssetDefinition key)
	{
		IMongoCollection<OrderBookEntity> Collection = GetCollection(key);
		IAsyncCursor<OrderBookEntity> res = await Collection.FindAsync(f => f.UnderlyingAsset == key);
		return await res.SingleOrDefaultAsync();
	}

	public override async Task<ReplaceOneResult> UpsertSingleAsync(OrderBookEntity orderBook)
	{
		IMongoCollection<OrderBookEntity> Collection = GetCollection(orderBook.UnderlyingAsset);
		ReplaceOneResult res  = await Collection.ReplaceOneAsync(f => f.UnderlyingAsset == orderBook.UnderlyingAsset, orderBook, UpsertOptions);
		return res;
	}

	private IMongoCollection<OrderBookEntity> GetCollection(AssetDefinition asset) => Database.GetCollection<OrderBookEntity>($"{asset.Class}${asset.Symbol}");
}



