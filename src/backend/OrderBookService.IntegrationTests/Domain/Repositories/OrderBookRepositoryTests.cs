using AutoFixture;
using EphemeralMongo;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using NSubstitute;
using OrderBookProtos.CustomTypes;
using OrderBookService.Application.Config;
using OrderBookService.Application.Exceptions;
using OrderBookService.Application.Misc;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Repositories.Mongo.OrderBooks;
using Shouldly;
using StackExchange.Redis;

namespace OrderBookService.IntegrationTests.Domain.Repositories;

public class OrderBookRepositoryTests : IDisposable
{

	private readonly IMongoRunner         _runner;
	private readonly IOrderBookRepository _repo;
	private readonly Fixture              _fixture;
	private readonly IMongoDatabase       _readBackDb;

	public OrderBookRepositoryTests()
	{
		_fixture = new();


		_fixture.Customize<GuidValue>(c => c.With(gv => gv.Value, Guid.NewGuid().ToString()));
		// Needed because TimeStamp and Datetime have different precisions in nanos and id is actually a guid
		_fixture.Customize<OrderEntity>(c => c.With(o => o.EffectiveTime, DateTime.UtcNow.Date)
											  .With(o => o.Id, () => Guid.NewGuid().ToString()));


		MongoRunnerOptions options = new()
									 {
										 UseSingleNodeReplicaSet = true,                     // Default: false
										 StandardOuputLogger     = Console.WriteLine,        // Default: null
										 StandardErrorLogger     = Console.WriteLine,        // Default: null
										 ConnectionTimeout       = TimeSpan.FromSeconds(10), // Default: 30 seconds
										 ReplicaSetSetupTimeout  = TimeSpan.FromSeconds(5),  // Default: 10 seconds
										 AdditionalArguments     = "--quiet",                // Default: null
									 };

		string databaseName = Guid.NewGuid().ToString();

		_runner     = MongoRunner.Run(options);
		_readBackDb = new MongoClient(_runner.ConnectionString).GetDatabase(databaseName);

		IOptions<MongoDbSettings> mongoOptions = Options.Create(new MongoDbSettings
																{
																	ConnectionString = _runner.ConnectionString,
																	DatabaseName     = databaseName
																});

		// This could do with tests for the caching too
		_repo = new OrderBookRepository(mongoOptions, NullLogger<OrderBookRepository>.Instance, Substitute.For<IConnectionMultiplexer>());
	}

	[Fact]
	public async Task CanAddANewDocument()
	{
		OrderBookEntity obe = _fixture.Create<OrderBookEntity>();

		_ = await _repo.UpsertSingleAsync(obe);

		OrderBookEntity? readBackDocument = await ReadBackDocumentFromDb(obe);

		readBackDocument.ShouldNotBeNull();
		readBackDocument.ShouldBeEquivalentTo(obe);
	}

	[Fact]
	public async Task CreatesNewCollectionForNewAssetClass()
	{
		OrderBookEntity obe = _fixture.Create<OrderBookEntity>();

		_ = await _repo.UpsertSingleAsync(obe);

		BsonDocument                filter      = new("name", obe.UnderlyingAsset.Class.ToString());
		IAsyncCursor<BsonDocument>? collections = await _readBackDb.ListCollectionsAsync(new ListCollectionsOptions {Filter = filter});

		(await collections.AnyAsync()).ShouldBeTrue();
	}

	[Fact]
	public async Task CanUpdateAnExistingDocument()
	{
		OrderBookEntity extantDoc = _fixture.Create<OrderBookEntity>();
		await GetReadBackDb(extantDoc).InsertOneAsync(extantDoc);

		OrderBookEntity modifiedDoc = extantDoc with {Orders = new()};

		ReplaceOneResult res = await _repo.UpsertSingleAsync(modifiedDoc);
		res.ModifiedCount.ShouldBe(1);

		OrderBookEntity? readBackDocument = await ReadBackDocumentFromDb(extantDoc);

		readBackDocument.ShouldNotBeNull();
		readBackDocument.ShouldBeEquivalentTo(modifiedDoc);
	}

	[Fact]
	public async Task CanGetASingleDocument()
	{
		OrderBookEntity extantDoc = _fixture.Create<OrderBookEntity>();
		await GetReadBackDb(extantDoc).InsertOneAsync(extantDoc);

		OrderBookEntity res = await _repo.GetSingleAsync(extantDoc.UnderlyingAsset);

		res.ShouldBeEquivalentTo(extantDoc);
	}

	[Fact]
	public async Task GivenOrderBookExistsThenICanAddAnOrderToIt()
	{
		OrderBookEntity extantDoc = _fixture.Create<OrderBookEntity>();
		await GetReadBackDb(extantDoc).InsertOneAsync(extantDoc);

		OrderEntity newOrder = _fixture.Create<OrderEntity>();
		await _repo.AddOrderToOrderBook(extantDoc.UnderlyingAsset, newOrder);

		List<OrderEntity> expectedOrderList = extantDoc.Orders.Append(newOrder).ToList();

		OrderBookEntity? readBackDocument = await ReadBackDocumentFromDb(extantDoc);

		readBackDocument.ShouldNotBeNull();
		readBackDocument.Orders.ShouldBeEquivalentTo(expectedOrderList);
	}

	[Fact]
	public async Task GivenOrderBookDoesntExistThenICreateOneWithTheNewOrder()
	{
		OrderEntity     newOrder        = _fixture.Create<OrderEntity>();
		AssetDefinition assetDefinition = _fixture.Create<AssetDefinition>();

		List<OrderEntity> expectedOrderList = new() {newOrder};

		await _repo.AddOrderToOrderBook(assetDefinition, newOrder);

		OrderBookEntity? readBackDocument = await ReadBackDocumentFromDb(assetDefinition);

		readBackDocument.ShouldNotBeNull();
		readBackDocument.Orders.ShouldBeEquivalentTo(expectedOrderList);
	}

	[Fact]
	public async Task GivenOrderExistsThenICanModifyItInPlace()
	{
		OrderBookEntity extantDoc = _fixture.Create<OrderBookEntity>();
		await GetReadBackDb(extantDoc).InsertOneAsync(extantDoc);

		List<OrderEntity> modifiedOrders = extantDoc.Orders.Select(o => o with {Amount = o.Amount/2}).ToList();

		foreach (OrderEntity orderEntity in modifiedOrders)
		{
			await _repo.ModifyOrderInOrderBook(extantDoc.UnderlyingAsset, orderEntity);
		}

		OrderBookEntity? readBackDocument = await ReadBackDocumentFromDb(extantDoc);

		readBackDocument.ShouldNotBeNull();
		readBackDocument.Orders.ShouldBeEquivalentTo(modifiedOrders);
	}

	[Fact]
	public async Task GivenOrderBookExistsAndOrderDoesNotWhenITryToModifyOrderThenIThrowFailedToDeleteException()
	{
		OrderBookEntity extantDoc = _fixture.Create<OrderBookEntity>() with {Orders = new()};
		await GetReadBackDb(extantDoc).InsertOneAsync(extantDoc);

		OrderEntity modifiedOrder = _fixture.Create<OrderEntity>();

		FailedToModifyOrDeleteOrderException exc = await Should.ThrowAsync<FailedToModifyOrDeleteOrderException>(async () => await _repo.ModifyOrderInOrderBook(extantDoc.UnderlyingAsset, modifiedOrder));
		exc.Message.ShouldBe(StaticStrings.FailedToModifyOrDeleteOrderIdNonExistent);
	}

	[Fact]
	public async Task GivenOrderBookDoesntExistWhenITryToModifyOrderThenIThrowFailedToDeleteException()
	{
		OrderBookEntity extinctDoc    = _fixture.Create<OrderBookEntity>() with {Orders = new()};
		OrderEntity     modifiedOrder = _fixture.Create<OrderEntity>();

		FailedToModifyOrDeleteOrderException exc = await Should.ThrowAsync<FailedToModifyOrDeleteOrderException>(async () => await _repo.ModifyOrderInOrderBook(extinctDoc.UnderlyingAsset, modifiedOrder));
		exc.Message.ShouldBe(StaticStrings.FailedToModifyOrDeleteNoOrderBookMessage);
	}

	[Fact]
	public async Task GivenOrderExistsThenICanRemoveIt()
	{
		OrderBookEntity extantDoc = _fixture.Create<OrderBookEntity>();
		await GetReadBackDb(extantDoc).InsertOneAsync(extantDoc);

		int         removalIndex  = Random.Shared.Next(extantDoc.Orders.Count - 1);
		OrderEntity orderToRemove = extantDoc.Orders[removalIndex];

		List<OrderEntity> expectedOrders = extantDoc.Orders.Where(o => o != orderToRemove).ToList();

		await _repo.RemoveOrderFromBook(extantDoc.UnderlyingAsset, orderToRemove.Id);

		OrderBookEntity? readBackDocument = await ReadBackDocumentFromDb(extantDoc);

		readBackDocument.ShouldNotBeNull();
		readBackDocument.Orders.ShouldBeEquivalentTo(expectedOrders);
	}

	[Fact]
	public async Task GivenOrderBookExistsAndOrderDoesNotWhenITryToRemoveOrderThenIThrowFailedToDeleteException()
	{
		OrderBookEntity extantDoc = _fixture.Create<OrderBookEntity>() with {Orders = new()};
		await GetReadBackDb(extantDoc).InsertOneAsync(extantDoc);

		OrderEntity extinctOrder = _fixture.Create<OrderEntity>();

		FailedToModifyOrDeleteOrderException exc = await Should.ThrowAsync<FailedToModifyOrDeleteOrderException>(async () => await _repo.RemoveOrderFromBook(extantDoc.UnderlyingAsset, extinctOrder.Id));
		exc.Message.ShouldBe(StaticStrings.FailedToModifyOrDeleteOrderIdNonExistent);
	}

	[Fact]
	public async Task GivenOrderBookDoesntExistThenWhenITryToRemoveOrderIThrowFailedToDeleteException()
	{
		OrderBookEntity extinctDoc   = _fixture.Create<OrderBookEntity>() with {Orders = new()};
		OrderEntity     extinctOrder = _fixture.Create<OrderEntity>();

		FailedToModifyOrDeleteOrderException exc = await Should.ThrowAsync<FailedToModifyOrDeleteOrderException>(async () => await _repo.RemoveOrderFromBook(extinctDoc.UnderlyingAsset, extinctOrder.Id));
		exc.Message.ShouldBe(StaticStrings.FailedToModifyOrDeleteNoOrderBookMessage);
	}


	private async Task<OrderBookEntity?> ReadBackDocumentFromDb(OrderBookEntity obe)   => await ReadBackDocumentFromDb(obe.UnderlyingAsset);
	private async Task<OrderBookEntity?> ReadBackDocumentFromDb(AssetDefinition asset) => await (await GetReadBackDb(asset).FindAsync(o => o.UnderlyingAsset == asset)).SingleOrDefaultAsync();

	private IMongoCollection<OrderBookEntity> GetReadBackDb(OrderBookEntity obe)   => GetReadBackDb(obe.UnderlyingAsset);
	private IMongoCollection<OrderBookEntity> GetReadBackDb(AssetDefinition asset) => _readBackDb.GetCollection<OrderBookEntity>(asset.Class.ToString());

	public void Dispose()
	{
		_runner.Dispose();
	}
}
