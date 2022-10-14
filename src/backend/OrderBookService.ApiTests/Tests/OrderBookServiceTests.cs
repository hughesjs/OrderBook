using AutoFixture;
using OrderBookProtos.ServiceBases;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class OrderBookServiceTests: ApiTestBase
{
	public OrderBookServiceTests(OrderBookTestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }
	
	[Fact]
	public async Task AddOrderTest()
	{
		OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client = new(Channel);

		AddOrModifyOrderRequest req = AutoFixture.Create<AddOrModifyOrderRequest>();

		OrderBookModificationResponse? response = await client.AddOrderAsync(req);

		response.ShouldNotBeNull();
		response.Status.IsSuccess.ShouldBe(true);
	}
	
	[Fact]
	public async Task ModifyOrderTest()
	{
		OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client = new(Channel);
				
		AddOrModifyOrderRequest req = AutoFixture.Create<AddOrModifyOrderRequest>();

		OrderBookModificationResponse? response = await client.ModifyOrderAsync(req);

		response.ShouldNotBeNull();
		response.Status.IsSuccess.ShouldBe(false);
		response.Status.Message.ShouldBe("Not Yet Implemented");
	}
	
	[Fact]
	public async Task RemoveOrderTest()
	{
		OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client = new(Channel);
        
		RemoveOrderRequest req = AutoFixture.Create<RemoveOrderRequest>();

		OrderBookModificationResponse? response = await client.RemoveOrderAsync(req);

		response.ShouldNotBeNull();
		response.Status.IsSuccess.ShouldBe(false);
		response.Status.Message.ShouldBe("Not Yet Implemented");
	}
	
	[Fact]
	public async Task GetPriceTest()
	{
		OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client = new(Channel);
        
		GetPriceRequest req = AutoFixture.Create<GetPriceRequest>();

		PriceResponse? response = await client.GetPriceAsync(req);

		response.ShouldNotBeNull();
		response.Status.IsSuccess.ShouldBe(false);
		response.Status.Message.ShouldBe("Not Yet Implemented");
	}
}
