using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderBookService.Application.Config;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Exceptions;

namespace OrderBookService.Domain.Repositories.Mongo.OrderBooks;

internal sealed class OrderBookRepository: MongoRepositoryBase<OrderBookEntity, AssetDefinition>, IOrderBookRepository
{
	private readonly        ILogger<OrderBookRepository> _logger;
	private static readonly ReplaceOptions               UpsertOptions = new() {IsUpsert = true};

	public OrderBookRepository(IOptions<MongoDbSettings> mongoSettings, ILogger<OrderBookRepository> logger) : base(mongoSettings)
	{
		_logger = logger;
	}

	public override async Task<OrderBookEntity?> GetSingleAsync(AssetDefinition key)
	{
		IMongoCollection<OrderBookEntity> collection = GetCollection(key);
		IAsyncCursor<OrderBookEntity> res = await collection.FindAsync(f => f.UnderlyingAsset == key);
		return await res.SingleOrDefaultAsync();
	}

	public override async Task<ReplaceOneResult> UpsertSingleAsync(OrderBookEntity orderBook)
	{
		IMongoCollection<OrderBookEntity> collection = GetCollection(orderBook.UnderlyingAsset);
		ReplaceOneResult res  = await collection.ReplaceOneAsync(f => f.UnderlyingAsset == orderBook.UnderlyingAsset, orderBook, UpsertOptions);
		return res;
	}

	/// <summary>
	/// Add an order to an order book.
	/// Will attempt to insert it into an existing document first.
	/// If that fails, it will attempt to create a new document with the order.
	/// </summary>
	/// <param name="asset">Asset class the order pertains to</param>
	/// <param name="order">The order parameters</param>
	/// <returns></returns>
	public async Task AddOrderToOrderBook(AssetDefinition asset, OrderEntity order)
	{
		_logger.LogDebug("Attempting to add {OrderId} to {@Asset}", order.Id, asset);
		UpdateDefinition<OrderBookEntity> update     = Builders<OrderBookEntity>.Update.AddToSet(o => o.Orders, order);
		IMongoCollection<OrderBookEntity> collection = GetCollection(asset);
		UpdateResult?                     res        = await collection.UpdateOneAsync(f => f.UnderlyingAsset == asset, update);
		bool                              success    = (res is not null && res.ModifiedCount > 0);

		if (success)
		{
			_logger.LogDebug("Successfully added {OrderId} to {@Asset}", order.Id, asset);
			return;
		}
		
		await HandleAddOrderHotPathBadResult(asset, order, collection);
	}

	public async Task ModifyOrderInOrderBook(AssetDefinition asset, OrderEntity order)   => throw new NotImplementedException();
	public async Task RemoveOrderFromBook(AssetDefinition    asset, string      orderId) => throw new NotImplementedException();

	
	
	private IMongoCollection<OrderBookEntity> GetCollection(AssetDefinition asset) => Database.GetCollection<OrderBookEntity>(asset.Class.ToString());

	private async Task HandleAddOrderHotPathBadResult(AssetDefinition asset, OrderEntity order, IMongoCollection<OrderBookEntity> collection)
	{
		_logger.LogInformation("Failed to add order to {Document} list directly, checking if document exists...", asset.Class);
		
		IAsyncCursor<OrderBookEntity>? docRes = await collection.FindAsync(o => o.UnderlyingAsset == asset);

		if (await docRes.AnyAsync())
		{
			throw new FailedToAddOrderException("Order didn't add despite the document already existing, this could be because the Order Id is a duplicate", asset, order);
		}
		
		_logger.LogInformation("Document for {AssetClass} not found, creating a new one", asset.Class);
		OrderBookEntity obe = new()
							  {
								  UnderlyingAsset = asset,
								  Orders          = new() {order}
							  };
		ReplaceOneResult upsertRes = await UpsertSingleAsync(obe);

		if (upsertRes.IsAcknowledged) return;

		throw new FailedToAddOrderException("Order failed to add for unknown reason", asset, order);
	}
}



