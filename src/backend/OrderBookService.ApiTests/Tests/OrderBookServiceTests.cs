using AutoFixture;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class OrderBookServiceTests: ApiTestBase
{
	// This is rubbish, issue 25 looks at avoiding relying on extant db state
	//TODO: Once #25 is sorted, consider reading out at end of all tests to check the content of the db
	
	private const int NumTestEntries = 25;
	
	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client;

	public OrderBookServiceTests(OrderBookTestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
	{
		client = new(Channel);
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
			AddOrModifyOrderRequest req = AutoFixture.Create<AddOrModifyOrderRequest>();
			req.AssetDefinition = asset;

			OrderBookModificationResponse? res = await client.AddOrderAsync(req);
			res.Status.IsSuccess.ShouldBe(true);
		}
		
		//TODO: Read back

	}

	[Fact]
	public async Task GivenCollectionDoesNotExistThenAddingAnOrderCreatesIt()
	{
		AssetDefinitionValue asset = AutoFixture.Create<AssetDefinitionValue>();
		
		for (int i = 0; i < NumTestEntries; i++)
		{
			AddOrModifyOrderRequest req = AutoFixture.Create<AddOrModifyOrderRequest>();
			req.AssetDefinition = asset;

			OrderBookModificationResponse? res = await client.AddOrderAsync(req);
			res.Status.IsSuccess.ShouldBe(true);
		}
		
		//TODO: Read back
	}
	
	
	[Fact]
	public async Task GivenOrderExistsThenWeCanModifyIt()
	{
		AssetDefinitionValue    asset = AutoFixture.Create<AssetDefinitionValue>();
		
		for (int i = 0; i < NumTestEntries; i++)
		{
			AddOrModifyOrderRequest req = AutoFixture.Create<AddOrModifyOrderRequest>();
			req.AssetDefinition = asset;

			OrderBookModificationResponse? res = await client.AddOrderAsync(req);
			res.Status.IsSuccess.ShouldBe(true);

			req.Price = AutoFixture.Create<DecimalValue>();
			res = await client.ModifyOrderAsync(req);
			
			res.Status.IsSuccess.ShouldBe(true);
		}
		
		//TODO: Read back
	}
	
	[Fact]
	public async Task GivenOrderExistsTheWeCanRemoveIt()
	{
		AssetDefinitionValue asset = AutoFixture.Create<AssetDefinitionValue>();
		
		for (int i = 0; i < NumTestEntries; i++)
		{
			AddOrModifyOrderRequest req = AutoFixture.Create<AddOrModifyOrderRequest>();
			req.AssetDefinition = asset;

			OrderBookModificationResponse? res = await client.AddOrderAsync(req);
			res.Status.IsSuccess.ShouldBe(true);

			RemoveOrderRequest remReq = new()
										{
											AssetDefinition = asset,
											IdempotencyKey  = AutoFixture.Create<GuidValue>(),
											OrderId         = req.OrderId
										};
			
			res       = await client.RemoveOrderAsync(remReq);
			
			res.Status.IsSuccess.ShouldBe(true);
		}
		
		//TODO: Read back
	}
	
	[Fact]
	public async Task GivenOrderIsSatisfiableThenWeCanGetAPrice()
	{
		for (int i = 0; i < NumTestEntries; i++)
		{
			AssetDefinitionValue asset = AutoFixture.Create<AssetDefinitionValue>();
			
			OrderAction action             = AutoFixture.Create<OrderAction>();
			decimal     amount             = PositiveDecimal();
			decimal     basePrice          = PositiveDecimal();
			decimal[]   amountCoefficients = GetAmountCoefficients();
			decimal[]   priceCoefficients  = GetPriceCoefficients(action == OrderAction.Buy);
			
			decimal priceMux      = DotProduct(amountCoefficients, priceCoefficients);
			decimal expectedPrice = priceMux*basePrice;

			List<AddOrModifyOrderRequest> orderRequests = amountCoefficients.Select((t, j) => new AddOrModifyOrderRequest
																							  {
																								  OrderAction     = action == OrderAction.Buy ? OrderAction.Sell : OrderAction.Buy,
																								  Amount          = j      != amountCoefficients.Length - 1 ? amount* t : amount* t + 0.5m, // Ensure there's a surplus
																								  AssetDefinition = asset,
																								  IdempotencyKey  = new(){Value = Guid.NewGuid().ToString()},
																								  OrderId         = new(){Value = Guid.NewGuid().ToString()},
																								  Price           = basePrice*priceCoefficients[j]

																							  }).ToList();

			foreach (var request in orderRequests)
			{
				await client.AddOrderAsync(request);
			}
			
			GetPriceRequest req = new()
								  {
									  AssetDefinition = asset,
									  Amount          = amount,
									  OrderAction     = action
								  };

			PriceResponse? priceRes = await client.GetPriceAsync(req);
			priceRes.Status.IsSuccess.ShouldBe(true);
			((decimal)priceRes.Price).ShouldBe(expectedPrice, 0.1m);
			(DateTime.UtcNow - priceRes.ValidAt.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
		}
	}
	
	private static decimal PositiveDecimal() => decimal.Abs(AutoFixture.Create<decimal>());

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
