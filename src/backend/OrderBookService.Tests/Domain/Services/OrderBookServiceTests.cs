using AutoFixture;
using AutoMapper;
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
	private static readonly OrderBookModificationResponse _successResponse = new() {Status = new() {IsSuccess = true, Message = "Success"}};

	private readonly IOrderBookService    _orderBookService;
	private readonly IOrderBookRepository _mockOrderBookRepository;


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
	[MemberData(nameof(GetPriceTestData), NumTests)]
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

	// [Theory]
	// [MemberData(nameof(GetPriceTestData), NumTests)]
	// internal async Task GivenAnOrderBookIfOrderCannotBeSatisfiedThenReturnBestMatch(GetPriceTestCase testCase)
	// {
	// 	
	// }

	[Theory]
	[MemberData(nameof(GetModifyOrderRequests), NumTests)]
	public async Task WhenOrderIsAddedSuccessfullyThenItReturnsSuccess(AddOrModifyOrderRequest request)
	{
		_ = _mockOrderBookRepository.AddOrderToOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		OrderBookModificationResponse res = await _orderBookService.AddOrder(request);

		res.Status.IsSuccess.ShouldBe(true);
	}

	[Theory]
	[MemberData(nameof(GetModifyOrderRequests), NumTests)]
	public async Task WhenOrderIsAddedThenEffectiveTimeIsSetToUtcNow(AddOrModifyOrderRequest request)
	{
		_ = _mockOrderBookRepository.AddOrderToOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		OrderBookModificationResponse res = await _orderBookService.AddOrder(request);

		(DateTime.UtcNow - res.EffectiveFrom.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
	}

	[Theory]
	[MemberData(nameof(GetModifyOrderRequests), NumTests)]
	public async Task WhenOrderIsModifiedSuccessfullyThenItReturnsSuccess(AddOrModifyOrderRequest request)
	{
		_ = _mockOrderBookRepository.ModifyOrderInOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		OrderBookModificationResponse res = await _orderBookService.ModifyOrder(request);

		res.Status.IsSuccess.ShouldBe(true);
	}

	[Theory]
	[MemberData(nameof(GetModifyOrderRequests), NumTests)]
	public async Task WhenOrderIsModifiedSuccessfullyThenEffectiveTimeIsSetToUtcNow(AddOrModifyOrderRequest request)
	{
		_ = _mockOrderBookRepository.ModifyOrderInOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		OrderBookModificationResponse res = await _orderBookService.ModifyOrder(request);

		(DateTime.UtcNow - res.EffectiveFrom.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
	}

	[Theory]
	[MemberData(nameof(GetRemoveOrderRequests), NumTests)]
	public async Task WhenOrderIsRemovedSuccessfullyThenItReturnsSuccess(RemoveOrderRequest request)
	{
		_ = _mockOrderBookRepository.ModifyOrderInOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		OrderBookModificationResponse res = await _orderBookService.RemoveOrder(request);

		res.Status.IsSuccess.ShouldBe(true);
	}

	[Theory]
	[MemberData(nameof(GetRemoveOrderRequests), NumTests)]
	public async Task WhenOrderIsRemovedSuccessfullyThenEffectiveTimeIsSetToUtcNow(RemoveOrderRequest request)
	{
		_ = _mockOrderBookRepository.ModifyOrderInOrderBook(Arg.Any<AssetDefinition>(), Arg.Any<OrderEntity>()).Returns(Task.CompletedTask);

		OrderBookModificationResponse res = await _orderBookService.RemoveOrder(request);

		(DateTime.UtcNow - res.EffectiveFrom.ToDateTime()).ShouldBeLessThan(TimeSpan.FromSeconds(10));
	}

	public static IEnumerable<object[]> GetModifyOrderRequests(int num) => Fixture.CreateMany<AddOrModifyOrderRequest>(num).Select(or => new object[] {or});
	public static IEnumerable<object[]> GetRemoveOrderRequests(int num) => Fixture.CreateMany<RemoveOrderRequest>(num).Select(or => new object[] {or});

	public static IEnumerable<object[]> GetPriceTestData(int num)
	{
		AssetDefinition assetDefinition = Fixture.Create<AssetDefinition>();

		List<GetPriceTestCase> testCases = new();
		
		// This is admittedly more complex than I'd usually make a test-case
		// But I was having fun, and it's mathematically rigourous (see readme)
		// I did start off simple and iterate
		for (int i = 0; i < num; i++)
		{
			OrderAction action             = Fixture.Create<OrderAction>();
			decimal     amount             = PositiveDecimal();
			decimal     lowestPrice        = PositiveDecimal();
			decimal[]   amountCoefficients = GetAmountCoefficients();
			decimal[]   priceCoefficients  = GetPriceCoefficients(action == OrderAction.Buy);

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

	private static decimal[] GetAmountCoefficients()
	{
		decimal thetaOne   = (decimal)Random.Shared.NextDouble();                   
		decimal thetaTwo   = (1m - thetaOne) * (decimal)Random.Shared.NextDouble();
		decimal thetaThree = 1m - thetaOne - thetaTwo;
		return new[] {thetaOne, thetaTwo, thetaThree};
	}

	private static decimal[] GetPriceCoefficients(bool isBuy) => isBuy ? new[] {PositiveDecimal(), PositiveDecimal(), PositiveDecimal()}.OrderBy(d => d).ToArray()
																	 : new [] {PositiveDecimal(), PositiveDecimal(), PositiveDecimal()}.OrderByDescending(d => d).ToArray();

	private static decimal DotProduct(decimal[] a, decimal[] b) => a.Select((t, i) => t*b[i]).Sum();


	internal class GetPriceTestCase
	{
		public required OrderBookEntity OrderBookEntity { get; init; }
		public required OrderAction     OrderAction     { get; init; }
		public required decimal         ExpectedPrice   { get; init; }
		public required decimal         AmountWanted       { get; init; }
	}
}
