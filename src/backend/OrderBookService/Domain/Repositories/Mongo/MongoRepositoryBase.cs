using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderBookService.Application.Config;

namespace OrderBookService.Domain.Repositories;

internal abstract class MongoRepositoryBase<TDocument, TKey>
{
	protected readonly IOptions<MongoDbSettings> MongoSettings;
	
	protected readonly IMongoDatabase Database;

	protected MongoRepositoryBase(IOptions<MongoDbSettings> mongoSettings)
	{
		MongoSettings = mongoSettings;
		Database = new MongoClient(MongoSettings.Value.ConnectionString).GetDatabase(MongoSettings.Value.DatabaseName);
	}

	public abstract Task<TDocument?> GetSingleAsync(TKey         key);
	public abstract Task             UpsertSingleAsync(TDocument orderBook);
	
	/* You'd have all your other CRUD operations here... But we don't need them for this
	public abstract Task                        DeleteSingleAsync();
	public abstract Task<IQueryable<TDocument>> GetAllAsync();
	*/
}
