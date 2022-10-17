using AutoFixture;
using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.ApiTests.TestInfrastructure;
using OrderBookService.Application.Misc;
using OrderBookService.Domain.Entities;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests.Tests.Services;

public class OrderBookServiceTests: ApiTestBase
{
	// This is rubbish, issue 25 looks at avoiding relying on extant db state
	//TODO: Once #17 is sorted add unhappy handling
	
	private const int NumTestEntries = 25;
	
	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient _client;

	public OrderBookServiceTests(OrderBookTestFixture testFixture, ITestOutputHelper outputHelper) : base(testFixture, outputHelper)
	{
		_client = new(Channel);
	}

	[Theory]
	[MemberData(nameof(AddOrderRequestGenerator), NumTestEntries)]
	public async Task GivenCollectionExistsThenWeCanAddOrdersToIt(AddOrderRequest req)
	{
		
		AssetDefinitionValue asset = new()
								{
									Class  = AssetClass.CoinPair,
									Symbol = "USDETH"
								};
		
		req.AssetDefinition = asset;

		AddOrderResponse? res = await _client.AddOrderAsync(req);
		res.Status.Code.ShouldBe((int)StatusCode.OK);
	}


	[Theory]
	[MemberData(nameof(AddOrderRequestGenerator), NumTestEntries)]
	public async Task GivenIHaveAddedAnOrderThenIGetAnOrderIdBack(AddOrderRequest req)
	{
		
		AssetDefinitionValue asset = new()
									 {
										 Class  = AssetClass.CoinPair,
										 Symbol = "USDETH"
									 };

		req.AssetDefinition = asset;

		AddOrderResponse? res = await _client.AddOrderAsync(req);
		Guid.TryParse(res.OrderId.Value, out Guid guid).ShouldBeTrue();
		guid.ShouldNotBe(Guid.Empty);
	}

	[Theory]
	[MemberData(nameof(AddOrderRequestGenerator), NumTestEntries)]
	public async Task GivenCollectionDoesNotExistThenAddingAnOrderCreatesIt(AddOrderRequest req)
	{
		AssetDefinitionValue asset = AutoFix.Create<AssetDefinitionValue>();
		
		req.AssetDefinition = asset;

		AddOrderResponse? res = await _client.AddOrderAsync(req);
		res.Status.Code.ShouldBe((int)StatusCode.OK);
	}
	
	
	[Theory]
	[MemberData(nameof(AddOrderRequestGenerator), NumTestEntries)]
	public async Task GivenOrderExistsThenWeCanModifyIt(AddOrderRequest addReq)
	{
		AssetDefinitionValue    asset = AutoFix.Create<AssetDefinitionValue>();
		
		addReq.AssetDefinition = asset;

		AddOrderResponse? addRes = await _client.AddOrderAsync(addReq);
		addRes.Status.Code.ShouldBe((int)StatusCode.OK);

		ModifyOrderRequest modReq = AutoFix.Build<ModifyOrderRequest>()
										   .With(r => r.AssetDefinition, addReq.AssetDefinition)
										   .With(r => r.OrderId, addRes.OrderId)
										   .With(r => r.Price, () => Random.Shared.Next())
										   .Create();

		ModifyOrderResponse modRes         = await _client.ModifyOrderAsync(modReq);
		
		modRes.Status.Code.ShouldBe((int)StatusCode.OK);
	}
	
	[Theory]
	[MemberData(nameof(AddOrderRequestGenerator), NumTestEntries)]
	public async Task GivenOrderExistsTheWeCanRemoveIt(AddOrderRequest addReq)
	{
		AssetDefinitionValue asset = AutoFix.Create<AssetDefinitionValue>();
		
		addReq.AssetDefinition = asset;

		AddOrderResponse? addRes = await _client.AddOrderAsync(addReq);
		addRes.Status.Code.ShouldBe((int)StatusCode.OK);
		
		RemoveOrderRequest remReq = AutoFix.Build<RemoveOrderRequest>()
										   .With(r => r.AssetDefinition, addReq.AssetDefinition)
										   .With(r => r.OrderId,         addRes.OrderId)
										   .Create();
		
		ModifyOrderResponse remRes       = await _client.RemoveOrderAsync(remReq);
		
		remRes.Status.Code.ShouldBe((int)StatusCode.OK);
	}
	
	[Theory]
	[MemberData(nameof(PossibleTradeDataGenerator), NumTestEntries)]
	public async Task GivenOrderIsSatisfiableThenWeCanGetAPrice(GetPriceTestData testData)
	{
		foreach (AddOrderRequest request in testData.OrderRequests)
		{
			AddOrderResponse? res = await _client.AddOrderAsync(request);
			res.OrderId.ShouldNotBeNull();
		}
		
		GetPriceRequest req = new()
							  {
								  AssetDefinition = testData.OrderRequests.First().AssetDefinition,
								  Amount          = testData.Amount,
								  OrderAction     = testData.Action
							  };

		PriceResponse? priceRes = await _client.GetPriceAsync(req);
		priceRes.Status.Code.ShouldBe((int)StatusCode.OK);
		((decimal)priceRes.Price).ShouldBe(testData.ExpectedPrice, 0.1m);
		(DateTime.UtcNow - priceRes.ValidAt.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
	}

	public static IEnumerable<object[]> PossibleTradeDataGenerator(int num)
	{
		List<GetPriceTestData> testData = new();

		for (int i = 0; i < num; i++)
		{
			AssetDefinitionValue asset = AutoFix.Create<AssetDefinitionValue>();

			OrderAction action             = AutoFix.Create<OrderAction>();
			decimal     amount             = PositiveDecimal();
			decimal     basePrice          = PositiveDecimal();
			decimal[]   amountCoefficients = GetAmountCoefficients();
			decimal[]   priceCoefficients  = GetPriceCoefficients(action == OrderAction.Buy);

			decimal priceMux      = DotProduct(amountCoefficients, priceCoefficients);
			decimal expectedPrice = priceMux*basePrice;

			List<AddOrderRequest> orderRequests = amountCoefficients.Select((t, j) => new AddOrderRequest
																					  {
																						  OrderAction     = action == OrderAction.Buy ? OrderAction.Sell : OrderAction.Buy,
																						  Amount          = j      != amountCoefficients.Length - 1 ? amount*t : amount*t + 0.5m, // Ensure there's a surplus
																						  AssetDefinition = asset,
																						  IdempotencyKey  = AutoFix.Create<GuidValue>(),
																						  Price           = basePrice*priceCoefficients[j]

																					  }).ToList();
			testData.Add(new()
						 {
							 Action        = action,
							 Amount        = amount,
							 ExpectedPrice = expectedPrice,
							 OrderRequests = orderRequests
						 });
		}
		return testData.Select(td => new[] {td});
	}

	public static IEnumerable<object[]> AddOrderRequestGenerator(int num) => AutoFix.CreateMany<AddOrderRequest>(num).Select(r => new [] {r});

	public class GetPriceTestData
	{
		public required List<AddOrderRequest> OrderRequests        { get; init; }
		public required decimal               Amount        { get; init; }
		public required decimal               ExpectedPrice { get; init; }
		public required OrderAction           Action        { get; init; }
	}

	private static decimal PositiveDecimal() => decimal.Abs(AutoFix.Create<decimal>());

	private static decimal[] GetAmountCoefficients()
	{
		decimal thetaOne   = (decimal)Random.Shared.NextDouble();                   
		decimal thetaTwo   = (1m - thetaOne)* (decimal)Random.Shared.NextDouble();
		decimal thetaThree = 1m - thetaOne - thetaTwo;
		return new[] {thetaOne, thetaTwo, thetaThree};
	}

	private static decimal[] GetPriceCoefficients(bool isBuy) => isBuy ? new[] {PositiveDecimal(), PositiveDecimal(), PositiveDecimal()}.OrderBy(d => d).ToArray()
																	 : new [] {PositiveDecimal(), PositiveDecimal(), PositiveDecimal()}.OrderByDescending(d => d).ToArray();

	private static decimal DotProduct(decimal[] a, decimal[] b) => a.Select((t, i) => t*b[i]).Sum();
	
}
