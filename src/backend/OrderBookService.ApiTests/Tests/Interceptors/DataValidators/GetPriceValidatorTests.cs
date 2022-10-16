using AutoFixture;
using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests.Tests.Interceptors;

public class PriceRequestValidatorTests: ApiTestBase
{
	private const int NumTests = 100;

	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient _client;
	
	public PriceRequestValidatorTests(OrderBookTestFixture testFixture, ITestOutputHelper outputHelper) : base(testFixture, outputHelper)
	{
		_client     = new(Channel);
	}

	[Theory]
	[MemberData(nameof(NegativeAndZeroAmountRequestGenerator), NumTests)]
	public async Task FailsWhenNegativeOrZeroAmountProvided(GetPriceRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.GetPriceAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Amount));
	}

	[Theory]
	[MemberData(nameof(InvalidDecimalValueRequestGenerator), NumTests)]
	public async Task FailsWhenInvalidDecimalValueInAmount(GetPriceRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.GetPriceAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Amount));
	}

	[Fact]
	public async Task FailsIfAssetClassIsInvalid()
	{
		GetPriceRequest request = AutoFix.Create<GetPriceRequest>();
		request.AssetDefinition.Class = (AssetClass)int.MaxValue;

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.GetPriceAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.AssetDefinition.Class));
	}
	
	[Fact]
	public async Task FailsIfAssetClassSymbolIsEmpty()
	{
		GetPriceRequest request = AutoFix.Create<GetPriceRequest>();
		request.AssetDefinition.Symbol = "";
		
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.GetPriceAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.AssetDefinition.Symbol));
	}

	[Fact]
	public async Task FailsWhenOrderActionIsInvalid()
	{
		GetPriceRequest request = AutoFix.Create<GetPriceRequest>();
		request.OrderAction = (OrderAction)int.MaxValue;

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.GetPriceAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.OrderAction));
	}




	public static IEnumerable<object[]> InvalidDecimalValueRequestGenerator(int num)
	=> AutoFix.Build<GetPriceRequest>().With(p => p.Amount, () =>
																					 {
																						 int nanos = Random.Shared.Next(int.MinValue, int.MaxValue);
																						 return new()
																								{
																									Units = -(long)nanos,
																									Nanos = nanos
																								};
																					 }).CreateMany(num).Select(r => new object[] {r});
		
	
	public static IEnumerable<object[]> NegativeAndZeroAmountRequestGenerator(int num)
	{
		GetPriceRequest zeroAmountRequest = AutoFix.Build<GetPriceRequest>()
													   .With(p => p.Amount, 0)
													   .Create();

		IEnumerable<GetPriceRequest>? negativeAmountRequests = AutoFix.Build<GetPriceRequest>()
																		  .With(p => p.Amount, () => (DecimalValue)NegativeDecimal())
																		  .CreateMany(num - 1);
		
		return negativeAmountRequests.Append(zeroAmountRequest).Select(r => new object[] {r});
	}
		

	private static decimal PositiveDecimal() => decimal.Abs(AutoFix.Create<decimal>());
	private static decimal NegativeDecimal() => -PositiveDecimal();


}
