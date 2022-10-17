using System.Net;
using AutoFixture;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Repositories.Mongo.OrderBooks;
using OrderBookService.Domain.Services;
using Shouldly;

namespace OrderBookService.Tests.Domain.Services;

public class OrderBookServiceTests
{
	private const           int                           NumTests         = 100;
	private static readonly Fixture                       Fixture          = new(); // Static so data gens can access

	private readonly IOrderBookService    _orderBookService;
	private readonly IOrderBookRepository _mockOrderBookRepository;
	
	static OrderBookServiceTests()
	{
		Fixture.Customize<GuidValue>(c => c.With(p => p.Value, Guid.NewGuid().ToString()));
	}

	public OrderBookServiceTests()
	{
		_mockOrderBookRepository = Substitute.For<IOrderBookRepository>();
		_orderBookService        = new OrderBookService.Domain.Services.OrderBookService(GetMapper(), _mockOrderBookRepository);
	}

	[Fact]
	internal async Task GivenTheExampleOrderBookThenPriceIsSuccessfullyCalculated()
	{
		AssetDefinition BtcUsd = new()
								 {
									 Class  = AssetClass.CoinPair,
									 Symbol = "BTCUSD"
								 };
		OrderBookEntity obe = new()
							  {
								  UnderlyingAsset = BtcUsd,
								  Orders = new()
										   {
											   new()
											   {
												   Price         = 30000,
												   Amount        = 200,
												   EffectiveTime = DateTime.UtcNow,
												   Id            = Guid.NewGuid().ToString(),
												   OrderAction   = OrderAction.Sell
											   },
											   new()
											   {
												   Price         = 31000,
												   Amount        = 400,
												   EffectiveTime = DateTime.UtcNow,
												   Id            = Guid.NewGuid().ToString(),
												   OrderAction   = OrderAction.Sell
											   },
											   new()
											   {
												   Price         = 32000,
												   Amount        = 600,
												   EffectiveTime = DateTime.UtcNow,
												   Id            = Guid.NewGuid().ToString(),
												   OrderAction   = OrderAction.Sell
											   },
											   new()
											   {
												   Price         = 29000,
												   Amount        = 200,
												   EffectiveTime = DateTime.UtcNow,
												   Id            = Guid.NewGuid().ToString(),
												   OrderAction   = OrderAction.Buy
											   },
											   new()
											   {
												   Price         = 28000,
												   Amount        = 400,
												   EffectiveTime = DateTime.UtcNow,
												   Id            = Guid.NewGuid().ToString(),
												   OrderAction   = OrderAction.Buy
											   },
											   new()
											   {
												   Price         = 27000,
												   Amount        = 600,
												   EffectiveTime = DateTime.UtcNow,
												   Id            = Guid.NewGuid().ToString(),
												   OrderAction   = OrderAction.Buy
											   }
										   }
							  };
		
		_ = _mockOrderBookRepository.GetSingleAsync(Arg.Any<AssetDefinition>()).Returns(obe);
		
		GetPriceRequest req = new()
							  {
								  Amount = 300,
								  AssetDefinition = new()
													{
														Class  = AssetClass.CoinPair,
														Symbol = "BTCUSD"
													},
								  OrderAction = OrderAction.Buy
							  };
		
		PriceResponse res = await _orderBookService.GetPrice(req);
		
		((decimal)res.Price).ShouldBe(30333.33m,0.01m);
	}

	[Theory]
	[MemberData(nameof(GetPriceTestData))]
	internal async Task GivenAnOrderBookThenPriceIsSuccessfullyCalculated(GetPriceTestCase testCase)
	{
		_ = _mockOrderBookRepository.GetSingleAsync(Arg.Any<AssetDefinition>()).Returns(testCase.OrderBookEntity);

		GetPriceRequest req = new()
							  {
								  Amount = testCase.AmountWanted,
								  AssetDefinition = new()
													{
														Class  = testCase.OrderBookEntity.UnderlyingAsset.Class,
														Symbol = testCase.OrderBookEntity.UnderlyingAsset.Symbol
													},
								  OrderAction = testCase.OrderAction
							  };
		
		PriceResponse res = await _orderBookService.GetPrice(req);
		
		((decimal)res.Price).ShouldBe(testCase.ExpectedPrice, 0.1m);
	}

	[Theory]
	[MemberData(nameof(GetAddOrderRequests))]
	public async Task WhenOrderIsAddedSuccessfullyThenItReturnsSuccess(AddOrderRequest request)
	{
		_ = _mockOrderBookRepository.AddOrderToOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		AddOrderResponse res = await _orderBookService.AddOrder(request);

		res.Status.Code.ShouldBe((int)StatusCode.OK);
	}

	[Theory]
	[MemberData(nameof(GetAddOrderRequests))]
	public async Task WhenOrderIsAddedThenEffectiveTimeIsSetToUtcNow(AddOrderRequest request)
	{
		_ = _mockOrderBookRepository.AddOrderToOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		AddOrderResponse res = await _orderBookService.AddOrder(request);

		(DateTime.UtcNow - res.EffectiveFrom.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
	}
	
	[Theory]
	[MemberData(nameof(GetAddOrderRequests))]
	public async Task WhenOrderIsAddedThenOrderIdIsReturned(AddOrderRequest request)
	{
		_ = _mockOrderBookRepository.AddOrderToOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		AddOrderResponse res = await _orderBookService.AddOrder(request);

		Guid.TryParse(res.OrderId.Value, out Guid guid).ShouldBeTrue();
		guid.ShouldNotBe(Guid.Empty);
	}

	[Theory]
	[MemberData(nameof(GetModifyOrderRequests))]
	public async Task WhenOrderIsModifiedSuccessfullyThenItReturnsSuccess(ModifyOrderRequest request)
	{
		_ = _mockOrderBookRepository.ModifyOrderInOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		ModifyOrderResponse res = await _orderBookService.ModifyOrder(request);

		res.Status.Code.ShouldBe((int)StatusCode.OK);
	}

	[Theory]
	[MemberData(nameof(GetModifyOrderRequests))]
	public async Task WhenOrderIsModifiedSuccessfullyThenEffectiveTimeIsSetToUtcNow(ModifyOrderRequest request)
	{
		_ = _mockOrderBookRepository.ModifyOrderInOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		ModifyOrderResponse res = await _orderBookService.ModifyOrder(request);

		(DateTime.UtcNow - res.EffectiveFrom.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
	}

	[Theory]
	[MemberData(nameof(GetRemoveOrderRequests))]
	public async Task WhenOrderIsRemovedSuccessfullyThenItReturnsSuccess(RemoveOrderRequest request)
	{
		_ = _mockOrderBookRepository.ModifyOrderInOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		ModifyOrderResponse res = await _orderBookService.RemoveOrder(request);

		res.Status.Code.ShouldBe((int)StatusCode.OK);
	}

	[Theory]
	[MemberData(nameof(GetRemoveOrderRequests))]
	public async Task WhenOrderIsRemovedSuccessfullyThenEffectiveTimeIsSetToUtcNow(RemoveOrderRequest request)
	{
		_ = _mockOrderBookRepository.ModifyOrderInOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		ModifyOrderResponse res = await _orderBookService.RemoveOrder(request);

		(DateTime.UtcNow - res.EffectiveFrom.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
	}

	public static IEnumerable<object[]> GetAddOrderRequests() => Fixture.CreateMany<AddOrderRequest>(NumTests).Select(or => new object[] {or});
	public static IEnumerable<object[]> GetModifyOrderRequests() => Fixture.CreateMany<ModifyOrderRequest>(NumTests).Select(or => new object[] {or});
	public static IEnumerable<object[]> GetRemoveOrderRequests() => Fixture.CreateMany<RemoveOrderRequest>(NumTests).Select(or => new object[] {or});

	public static IEnumerable<object[]> GetPriceTestData()
	{
		AssetDefinition assetDefinition = Fixture.Create<AssetDefinition>();

		List<GetPriceTestCase> testCases = new();
		
		// This is admittedly more complex than I'd usually make a test-case
		// But I was having fun, and it's mathematically rigourous (see readme)
		// I did start off simple and iterate
		for (int i = 0; i < NumTests; i++)
		{
			int         numCoefficients    = (Fixture.Create<int>() % 10) + 1;
			OrderAction action             = Fixture.Create<OrderAction>();
			decimal     amount             = PositiveDecimal();
			decimal     lowestPrice        = PositiveDecimal();
			decimal[]   amountCoefficients = GetAmountCoefficients(numCoefficients);
			decimal[]   priceCoefficients  = GetPriceCoefficients(numCoefficients, action == OrderAction.Buy);

			decimal priceMux = DotProduct(amountCoefficients, priceCoefficients);

			List<OrderEntity> orders = amountCoefficients.Select((t, j) => new OrderEntity
																		   {
																			   Id            = Guid.NewGuid().ToString(),
																			   Amount        = j != amountCoefficients.Length - 1 ? amount * t : amount * t + 0.5m, // Ensure there's a surplus
																			   EffectiveTime = DateTime.Today,
																			   OrderAction   = action == OrderAction.Buy ? OrderAction.Sell : OrderAction.Buy,
																			   Price         = lowestPrice*priceCoefficients[j]
																		   })
														 .ToList();

			OrderBookEntity orderBookEntity = new()
											  {
												  UnderlyingAsset = assetDefinition,
												  Orders          = orders
											  };
			testCases.Add(new()
						  {
							  AmountWanted    = amount,
							  ExpectedPrice   = priceMux * lowestPrice,
							  OrderBookEntity = orderBookEntity,
							  OrderAction     = action
						  });
		}

		return testCases.Select(tc => new object[] {tc});
	}
	

	private IMapper GetMapper()
	{
		// This just saves manually building the IMapper in tests
		ServiceCollection services = new();
		services.AddAutoMapper(typeof(Program).Assembly);
		return services.BuildServiceProvider().GetRequiredService<IMapper>();
	}

	private static decimal PositiveDecimal() => decimal.Abs(Fixture.Create<decimal>());

	private static decimal[] GetAmountCoefficients(int n)
	{
		if (n == 1)
		{
			return new[] {1m};
		}
		
		List<decimal> thetas = new();

		decimal thetaOne = (decimal)Random.Shared.NextDouble();
		thetas.Add(thetaOne);
		
		decimal remaining = 1m - thetaOne;
		
		for (int i = 1; i < n-1; i++)
		{
			decimal thetaK = remaining * (decimal)Random.Shared.NextDouble();
			remaining -= thetaK;
			thetas.Add(thetaK);
		}
		
		decimal thetaN = remaining;
		thetas.Add(thetaN);

		return thetas.ToArray();
	}

	private static decimal[] GetPriceCoefficients(int n, bool isBuy) => isBuy ? Fixture.CreateMany<decimal>(n).Select(decimal.Abs).OrderBy(d => d).ToArray()
																	 : Fixture.CreateMany<decimal>(n).Select(decimal.Abs).OrderByDescending(d => d).ToArray();

	private static decimal DotProduct(decimal[] a, decimal[] b)
	{
		a.Length.ShouldBe(b.Length);
		return a.Length == 1 ? a[0]* b[0] : a.Select((t, i) => t*b[i]).Sum();
	} 


	internal class GetPriceTestCase
	{
		public required OrderBookEntity OrderBookEntity { get; init; }
		public required OrderAction     OrderAction     { get; init; }
		public required decimal         ExpectedPrice   { get; init; }
		public required decimal         AmountWanted       { get; init; }
	}
}
