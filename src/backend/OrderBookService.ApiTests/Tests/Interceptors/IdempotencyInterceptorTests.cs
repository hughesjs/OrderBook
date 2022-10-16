using AutoFixture;
using Grpc.Core;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class IdempotencyTests: ApiTestBase
{
	private const  int     NumTests = 100;
	
	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client;

	public IdempotencyTests(OrderBookTestFixture testFixture, ITestOutputHelper outputHelper) : base(testFixture, outputHelper)
	{
		client = new(Channel);
	}

	[Theory]
	[MemberData(nameof(GetModifyOrderRequests), NumTests)]
	public async Task IfISendTheSameIdempotentRequestTwiceThenItIsRejected(AddOrderRequest req)
	
	{
		AddOrderResponse? res = await client.AddOrderAsync(req);
		res.Status.Code.ShouldBe((int)StatusCode.OK);
		res = await client.AddOrderAsync(req);
		res.Status.Code.ShouldBe((int)StatusCode.InvalidArgument);
		res.Status.Message.ShouldBe(StaticStrings.IdempotentOperationAlreadyCompleteMessage);
	}

	public static IEnumerable<object[]> GetModifyOrderRequests(int num) => AutoFix.CreateMany<AddOrderRequest>(num).Select(or => new object[] {or});
}
