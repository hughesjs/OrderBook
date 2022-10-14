using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Google.Protobuf.WellKnownTypes;
using NSubstitute;
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

	private IMapper GetMapper()
	{
		// This just saves manually building the IMapper in tests
		ServiceCollection services = new();
		services.AddAutoMapper(typeof(Program).Assembly);
		return services.BuildServiceProvider().GetRequiredService<IMapper>();
	}
}
