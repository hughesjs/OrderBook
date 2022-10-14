using AutoFixture;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class IdempotencyTests: ApiTestBase
{
	private const  int     NumTests = 100;
	
	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client;

	public IdempotencyTests(OrderBookTestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
	{
		client = new(Channel);
	}

	[Theory]
	[MemberData(nameof(GetModifyOrderRequests), NumTests)]
	public async Task IfISendTheSameIdempotentRequestTwiceThenItIsRejected(AddOrModifyOrderRequest req)
	
	{
		OrderBookModificationResponse? res = await client.AddOrderAsync(req);
		res.Status.IsSuccess.ShouldBe(true);
		res = await client.AddOrderAsync(req);
		res.Status.IsSuccess.ShouldBe(false);
		res.Status.Message.ShouldBe(StaticStrings.IdempotentOperationAlreadyCompleteMessage);
	}
	
	[Theory]
	[MemberData(nameof(GetModifyOrderRequests), NumTests)]
	public async Task IfIDontIncludeAKeyInAnIdempotentRequestTwiceThenItIsRejected(AddOrModifyOrderRequest req)

	{
		req.IdempotencyKey = null;
		OrderBookModificationResponse? res = await client.AddOrderAsync(req);
		res.Status.IsSuccess.ShouldBe(false);
		res.Status.Message.ShouldBe(StaticStrings.NoIdempotencyKeyProvidedMessage);
	}
	
	public static IEnumerable<object[]> GetModifyOrderRequests(int num) => AutoFixture.CreateMany<AddOrModifyOrderRequest>(num).Select(or => new object[] {or});
}
