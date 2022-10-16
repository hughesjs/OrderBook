using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OrderBookService.Application.Config;
using OrderBookService.Domain.Entities;

namespace OrderBookService.Domain.Repositories.Mongo;

internal abstract class MongoRepositoryBase<TDocument, TKey>
{
	protected readonly IOptions<MongoDbSettings> MongoSettings;
	
	protected readonly IMongoDatabase Database;

	protected MongoRepositoryBase(IOptions<MongoDbSettings> mongoSettings)
	{
		MongoSettings = mongoSettings;
		Database = new MongoClient(MongoSettings.Value.ConnectionString).GetDatabase(MongoSettings.Value.DatabaseName);
	}

	public abstract Task<OrderBookEntity> GetSingleAsync(TKey         key);
	public abstract Task                  UpsertSingleAsync(TDocument orderBook);
	
	protected async Task<bool> CollectionExistsAsync(string collectionName)
	{
		BsonDocument                filter      = new("name", collectionName);
		IAsyncCursor<BsonDocument>? collections = await Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
		return await collections.AnyAsync();
	}
	
	/* You'd have all your other CRUD operations here... But we don't need them for this
	public abstract Task                        DeleteSingleAsync();
	public abstract Task<IQueryable<TDocument>> GetAllAsync();
	*/
}
