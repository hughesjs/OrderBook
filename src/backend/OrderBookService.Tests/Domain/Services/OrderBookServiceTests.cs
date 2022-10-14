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
		
		((decimal)res.Price).ShouldBe(testCase.ExpectedPrice);
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
		int testsPerClass = num/4;

		AssetDefinition assetDefinition = Fixture.Create<AssetDefinition>();

		List<GetPriceTestCase> testCases = new();

		// Add simple buyside cases where everything is covered by one order
		for (int i = 0; i < testsPerClass; i++)
		{
			decimal buyAmount   = PositiveDecimal();
			decimal lowestPrice = PositiveDecimal();
			OrderBookEntity orderBookEntity = new()
											  {
												  UnderlyingAsset = assetDefinition,
												  Orders = new()
														   {
															   new()
															   {
																   Id            = Guid.NewGuid().ToString(),
																   Amount        = buyAmount + PositiveDecimal(),
																   EffectiveTime = DateTime.Today,
																   OrderAction   = OrderAction.Sell,
																   Price         = lowestPrice + PositiveDecimal()
															   },
															   new()
															   {
																   Id            = Guid.NewGuid().ToString(),
																   Amount        = buyAmount + PositiveDecimal(),
																   EffectiveTime = DateTime.Today,
																   OrderAction   = OrderAction.Sell,
																   Price         = lowestPrice
															   }
														   }
											  };
			testCases.Add(new()
						  {
							  AmountWanted = buyAmount,
							  ExpectedPrice = lowestPrice * buyAmount,
							  OrderBookEntity = orderBookEntity,
							  OrderAction = OrderAction.Buy
						  });
		
		}

		// Add simple sellside cases where everything is covered by one order
		for (int i = 0; i < testsPerClass; i++)
		{
			decimal sellAmount   = PositiveDecimal();
			decimal highestPrice = PositiveDecimal();
			OrderBookEntity orderBookEntity = new()
											  {
												  UnderlyingAsset = assetDefinition,
												  Orders = new()
														   {
															   new()
															   {
																   Id            = Guid.NewGuid().ToString(),
																   Amount        = sellAmount + PositiveDecimal(),
																   EffectiveTime = DateTime.Today,
																   OrderAction   = OrderAction.Buy,
																   Price         = highestPrice - PositiveDecimal()
															   },
															   new()
															   {
																   Id            = Guid.NewGuid().ToString(),
																   Amount        = sellAmount + PositiveDecimal(),
																   EffectiveTime = DateTime.Today,
																   OrderAction   = OrderAction.Buy,
																   Price         = highestPrice
															   }
														   }
											  };
			testCases.Add(new()
						  {
							  AmountWanted       = sellAmount,
							  ExpectedPrice   = highestPrice * sellAmount,
							  OrderBookEntity = orderBookEntity,
							  OrderAction     = OrderAction.Sell
						  });
		}

		// Add complex buyside cases where multiple orders are needed
		for (int i = 0; i < testsPerClass; i++) { }

		// Add complex sellside cases where multiple orders are needed
		for (int i = 0; i < testsPerClass; i++) { }

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


	internal class GetPriceTestCase
	{
		public required OrderBookEntity OrderBookEntity { get; init; }
		public required OrderAction     OrderAction     { get; init; }
		public required decimal         ExpectedPrice   { get; init; }
		public required decimal         AmountWanted       { get; init; }
	}
}
