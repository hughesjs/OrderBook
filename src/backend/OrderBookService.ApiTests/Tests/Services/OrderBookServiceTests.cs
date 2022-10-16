using AutoFixture;
using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

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

	[Fact]
	public async Task GivenCollectionExistsThenWeCanAddOrdersToIt()
	{
		
		AssetDefinitionValue asset = new()
								{
									Class  = AssetClass.CoinPair,
									Symbol = "USDETH"
								};

		for (int i = 0; i < NumTestEntries; i++)
		{
			AddOrderRequest req = AutoFix.Create<AddOrderRequest>();
			req.AssetDefinition = asset;

			AddOrderResponse? res = await _client.AddOrderAsync(req);
			res.Status.Code.ShouldBe((int)StatusCode.OK);
		}
	}
	
	[Fact]
	public async Task GivenIHaveAddedAnOrderThenIGetAnOrderIdBack()
	{
		
		AssetDefinitionValue asset = new()
									 {
										 Class  = AssetClass.CoinPair,
										 Symbol = "USDETH"
									 };

		for (int i = 0; i < NumTestEntries; i++)
		{
			AddOrderRequest req = AutoFix.Create<AddOrderRequest>();
			req.AssetDefinition = asset;

			AddOrderResponse? res = await _client.AddOrderAsync(req);
			Guid.TryParse(res.OrderId.Value, out Guid guid).ShouldBeTrue();
			guid.ShouldNotBe(Guid.Empty);
		}
	}

	[Fact]
	public async Task GivenCollectionDoesNotExistThenAddingAnOrderCreatesIt()
	{
		AssetDefinitionValue asset = AutoFix.Create<AssetDefinitionValue>();
		
		for (int i = 0; i < NumTestEntries; i++)
		{
			AddOrderRequest req = AutoFix.Create<AddOrderRequest>();
			req.AssetDefinition = asset;

			AddOrderResponse? res = await _client.AddOrderAsync(req);
			res.Status.Code.ShouldBe((int)StatusCode.OK);
		}
	}
	
	
	[Fact]
	public async Task GivenOrderExistsThenWeCanModifyIt()
	{
		AssetDefinitionValue    asset = AutoFix.Create<AssetDefinitionValue>();
		
		for (int i = 0; i < NumTestEntries; i++)
		{
			AddOrderRequest addReq = AutoFix.Create<AddOrderRequest>();
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
	}
	
	[Fact]
	public async Task GivenOrderExistsTheWeCanRemoveIt()
	{
		AssetDefinitionValue asset = AutoFix.Create<AssetDefinitionValue>();
		
		for (int i = 0; i < NumTestEntries; i++)
		{
			AddOrderRequest addReq = AutoFix.Create<AddOrderRequest>();
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
	}
	
	[Fact]
	public async Task GivenOrderIsSatisfiableThenWeCanGetAPrice()
	{
		for (int i = 0; i < NumTestEntries; i++)
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
																								  Amount          = j      != amountCoefficients.Length - 1 ? amount* t : amount* t + 0.5m, // Ensure there's a surplus
																								  AssetDefinition = asset,
																								  IdempotencyKey  = AutoFix.Create<GuidValue>(),
																								  Price           = basePrice*priceCoefficients[j]

																							  }).ToList();
			
			foreach (AddOrderRequest request in orderRequests)
			{
				AddOrderResponse? res = await _client.AddOrderAsync(request);
				res.OrderId.ShouldNotBeNull();
			}
			
			GetPriceRequest req = new()
								  {
									  AssetDefinition = asset,
									  Amount          = amount,
									  OrderAction     = action
								  };

			PriceResponse? priceRes = await _client.GetPriceAsync(req);
			priceRes.Status.Code.ShouldBe((int)StatusCode.OK);
			((decimal)priceRes.Price).ShouldBe(expectedPrice, 0.1m);
			(DateTime.UtcNow - priceRes.ValidAt.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
		}
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
