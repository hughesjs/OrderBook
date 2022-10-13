using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderBookService.Application.Config;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.AssetClasses;

namespace OrderBookService.Domain.Repositories;

internal sealed class OrderBookRepository<TAsset>: MongoRepositoryBase<OrderBookEntity<TAsset>, TAsset> where TAsset: AssetClassBase
{
	protected string CollectionName => $"{typeof(TAsset).Name}OrderBooks";

	protected override IMongoCollection<OrderBookEntity<TAsset>> Collection { get; }

	protected OrderBookRepository(IOptions<MongoDbSettings> mongoSettings) : base(mongoSettings)
	{
		Collection = Database.GetCollection<OrderBookEntity<TAsset>>(CollectionName);
	}

	public override async Task<OrderBookEntity<TAsset>?> GetSingleAsync(TAsset asset)
	{
		IAsyncCursor<OrderBookEntity<TAsset>>? res = await Collection.FindAsync(f => f.AssetClass == asset);
		return await res.SingleOrDefaultAsync();
	}

	public override async Task UpsertSingleAsync(OrderBookEntity<TAsset> orderBook)
	{
		ReplaceOneResult? res = await Collection.ReplaceOneAsync(f => f.AssetClass == orderBook.AssetClass, orderBook);
		if (!res.IsAcknowledged) throw new IOException("Failed to save document to MongoDB"); //TODO consider this more
	}
}
