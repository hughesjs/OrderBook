using AutoFixture;
using Grpc.Core;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class ExceptionInterceptorTests: ApiTestBase
{
	private const int NumTests = 100;

	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client;

	public ExceptionInterceptorTests(OrderBookTestFixture testFixture, ITestOutputHelper outputHelper) : base(testFixture, outputHelper)
	{
		client = new(Channel);
	}

	[Theory]
	[MemberData(nameof(GetRemoveOrderRequests), NumTests)]
	public async Task IfIThrowAnExceptionThenTheMessageIsReturned(RemoveOrderRequest req)
	{
		ModifyOrderResponse? res = await client.RemoveOrderAsync(req);
		res.Status.Code.ShouldBe((int)StatusCode.Internal);
		res.Status.Message.ShouldBe(StaticStrings.FailedToDeleteNoOrderBookMessage);
	}
	
	public static IEnumerable<object[]> GetRemoveOrderRequests(int num) => AutoFix.CreateMany<RemoveOrderRequest>(num).Select(or => new object[] {or});
}
