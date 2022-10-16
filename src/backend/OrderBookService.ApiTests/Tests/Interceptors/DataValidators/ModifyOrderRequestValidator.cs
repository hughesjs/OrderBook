using AutoFixture;
using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.ApiTests.TestInfrastructure;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests.Tests.Interceptors.DataValidators;

public class ModifyOrderRequestValidator: ApiTestBase
{
	private const int NumTests = 100;

	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient _client;
	
	public ModifyOrderRequestValidator(OrderBookTestFixture testFixture, ITestOutputHelper outputHelper) : base(testFixture, outputHelper)
	{
		_client     = new(Channel);
	}

	[Theory]
	[MemberData(nameof(NegativeAndZeroAmountRequestGenerator), NumTests)]
	public async Task FailsWhenNegativeOrZeroAmountProvided(ModifyOrderRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Amount));
	}

	[Theory]
	[MemberData(nameof(InvalidDecimalValueAmountRequestGenerator), NumTests)]
	public async Task FailsWhenInvalidDecimalValueInAmount(ModifyOrderRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Amount));
	}
	
	[Theory]
	[MemberData(nameof(NegativeAndZeroPriceRequestGenerator), NumTests)]
	public async Task FailsWhenNegativeOrZeroPriceProvided(ModifyOrderRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Price));
	}

	[Theory]
	[MemberData(nameof(InvalidDecimalValuePriceRequestGenerator), NumTests)]
	public async Task FailsWhenInvalidDecimalInPrice(ModifyOrderRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Price));
	}

	[Fact]
	public async Task FailsIfAssetClassIsInvalid()
	{
		ModifyOrderRequest request = AutoFix.Create<ModifyOrderRequest>();
		request.AssetDefinition.Class = (AssetClass)int.MaxValue;

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.AssetDefinition.Class));
	}
	
	[Fact]
	public async Task FailsIfAssetClassSymbolIsEmpty()
	{
		ModifyOrderRequest request = AutoFix.Create<ModifyOrderRequest>();
		request.AssetDefinition.Symbol = "";
		
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.AssetDefinition.Symbol));
	}

	[Fact]
	public async Task FailsWhenOrderActionIsInvalid()
	{
		ModifyOrderRequest request = AutoFix.Create<ModifyOrderRequest>();
		request.OrderAction = (OrderAction)int.MaxValue;

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.OrderAction));
	}
	
	[Fact]
	public async Task FailsWhenNoIdempotencyKeyIsProvided()
	{
		ModifyOrderRequest request = AutoFix.Build<ModifyOrderRequest>()
										 .With(r => r.IdempotencyKey, (GuidValue)null!)
										 .Create();

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.IdempotencyKey));
	}

	[Fact]
	public async Task FailsWhenNoOrderIdIsProvided()
	{
		ModifyOrderRequest request = AutoFix.Build<ModifyOrderRequest>()
										 .With(r => r.IdempotencyKey, (GuidValue)null!)
										 .Create();

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.ModifyOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.IdempotencyKey));
	}


	
	public static IEnumerable<object[]> InvalidDecimalValueAmountRequestGenerator(int num)
	=> AutoFix.Build<ModifyOrderRequest>().With(p => p.Amount, () =>
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
		ModifyOrderRequest zeroAmountRequest = AutoFix.Build<ModifyOrderRequest>()
												   .With(p => p.Amount, 0)
												   .Create();

		IEnumerable<ModifyOrderRequest>? negativeAmountRequests = AutoFix.Build<ModifyOrderRequest>()
																	  .With(p => p.Amount, () => (DecimalValue)NegativeDecimal())
																	  .CreateMany(num - 1);
		
		return negativeAmountRequests.Append(zeroAmountRequest).Select(r => new object[] {r});
	}
	
	public static IEnumerable<object[]> InvalidDecimalValuePriceRequestGenerator(int num)
		=> AutoFix.Build<ModifyOrderRequest>().With(p => p.Price, () =>
															   {
																   int nanos = Random.Shared.Next(int.MinValue, int.MaxValue);
																   return new()
																		  {
																			  Units = -(long)nanos,
																			  Nanos = nanos
																		  };
															   }).CreateMany(num).Select(r => new object[] {r});
		
	
	public static IEnumerable<object[]> NegativeAndZeroPriceRequestGenerator(int num)
	{
		ModifyOrderRequest zeroAmountRequest = AutoFix.Build<ModifyOrderRequest>()
												   .With(p => p.Price, 0)
												   .Create();

		IEnumerable<ModifyOrderRequest>? negativeAmountRequests = AutoFix.Build<ModifyOrderRequest>()
																	  .With(p => p.Price, () => (DecimalValue)NegativeDecimal())
																	  .CreateMany(num - 1);
		
		return negativeAmountRequests.Append(zeroAmountRequest).Select(r => new object[] {r});
	}
		

	private static decimal PositiveDecimal() => decimal.Abs(AutoFix.Create<decimal>());
	private static decimal NegativeDecimal() => -PositiveDecimal();
}

