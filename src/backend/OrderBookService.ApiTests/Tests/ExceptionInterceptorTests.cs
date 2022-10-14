using AutoFixture;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class ExceptionInterceptorTests: ApiTestBase
{
	private const int NumTests = 100;

	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient client;

	public ExceptionInterceptorTests(OrderBookTestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
	{
		client = new(Channel);
	}

	[Theory]
	[MemberData(nameof(GetRemoveOrderRequests), NumTests)]
	public async Task IfIThrowAnExceptionThenTheMessageIsReturned(RemoveOrderRequest req)
	{
		OrderBookModificationResponse? res = await client.RemoveOrderAsync(req);
		res.Status.IsSuccess.ShouldBe(false);
		res.Status.Message.ShouldBe(StaticStrings.FailedToDeleteNoOrderBookMessage);
	}
	
	public static IEnumerable<object[]> GetRemoveOrderRequests(int num) => AutoFixture.CreateMany<RemoveOrderRequest>(num).Select(or => new object[] {or});
}
