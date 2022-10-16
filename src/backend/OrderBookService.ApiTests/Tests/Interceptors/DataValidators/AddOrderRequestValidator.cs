using AutoFixture;
using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.ApiTests.TestInfrastructure;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests.Tests.Interceptors.DataValidators;

public class AddOrderRequestValidator: ApiTestBase
{
	private const int NumTests = 100;

	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient _client;
	
	public AddOrderRequestValidator(OrderBookTestFixture testFixture, ITestOutputHelper outputHelper) : base(testFixture, outputHelper)
	{
		_client     = new(Channel);
	}

	[Theory]
	[MemberData(nameof(NegativeAndZeroAmountRequestGenerator), NumTests)]
	public async Task FailsWhenNegativeOrZeroAmountProvided(AddOrderRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Amount));
	}

	[Theory]
	[MemberData(nameof(InvalidDecimalValueAmountRequestGenerator), NumTests)]
	public async Task FailsWhenInvalidDecimalValueInAmount(AddOrderRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Amount));
	}
	
	[Theory]
	[MemberData(nameof(NegativeAndZeroPriceRequestGenerator), NumTests)]
	public async Task FailsWhenNegativeOrZeroPriceProvided(AddOrderRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Price));
	}

	[Theory]
	[MemberData(nameof(InvalidDecimalValuePriceRequestGenerator), NumTests)]
	public async Task FailsWhenInvalidDecimalInPrice(AddOrderRequest request)
	{
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request)); 
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.Price));
	}

	[Fact]
	public async Task FailsIfAssetClassIsInvalid()
	{
		AddOrderRequest request = AutoFix.Create<AddOrderRequest>();
		request.AssetDefinition.Class = (AssetClass)int.MaxValue;

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.AssetDefinition.Class));
	}
	
	[Fact]
	public async Task FailsIfAssetClassSymbolIsEmpty()
	{
		AddOrderRequest request = AutoFix.Create<AddOrderRequest>();
		request.AssetDefinition.Symbol = "";
		
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.AssetDefinition.Symbol));
	}

	[Fact]
	public async Task FailsWhenOrderActionIsInvalid()
	{
		AddOrderRequest request = AutoFix.Create<AddOrderRequest>();
		request.OrderAction = (OrderAction)int.MaxValue;

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.OrderAction));
	}
	
	[Fact]
	public async Task FailsWhenNoIdempotencyKeyIsProvided()
	{
		AddOrderRequest request = AutoFix.Build<AddOrderRequest>()
										 .With(r => r.IdempotencyKey, (GuidValue)null!)
										 .Create();

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.IdempotencyKey));
	}

	[Fact]
	public async Task FailsWhenNoOrderIdIsProvided()
	{
		AddOrderRequest request = AutoFix.Build<AddOrderRequest>()
										 .With(r => r.IdempotencyKey, (GuidValue)null!)
										 .Create();

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.AddOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.IdempotencyKey));
	}


	
	public static IEnumerable<object[]> InvalidDecimalValueAmountRequestGenerator(int num)
	=> AutoFix.Build<AddOrderRequest>().With(p => p.Amount, () =>
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
		AddOrderRequest zeroAmountRequest = AutoFix.Build<AddOrderRequest>()
												   .With(p => p.Amount, 0)
												   .Create();

		IEnumerable<AddOrderRequest>? negativeAmountRequests = AutoFix.Build<AddOrderRequest>()
																	  .With(p => p.Amount, () => (DecimalValue)NegativeDecimal())
																	  .CreateMany(num - 1);
		
		return negativeAmountRequests.Append(zeroAmountRequest).Select(r => new object[] {r});
	}
	
	public static IEnumerable<object[]> InvalidDecimalValuePriceRequestGenerator(int num)
		=> AutoFix.Build<AddOrderRequest>().With(p => p.Price, () =>
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
		AddOrderRequest zeroAmountRequest = AutoFix.Build<AddOrderRequest>()
												   .With(p => p.Price, 0)
												   .Create();

		IEnumerable<AddOrderRequest>? negativeAmountRequests = AutoFix.Build<AddOrderRequest>()
																	  .With(p => p.Price, () => (DecimalValue)NegativeDecimal())
																	  .CreateMany(num - 1);
		
		return negativeAmountRequests.Append(zeroAmountRequest).Select(r => new object[] {r});
	}
		

	private static decimal PositiveDecimal() => decimal.Abs(AutoFix.Create<decimal>());
	private static decimal NegativeDecimal() => -PositiveDecimal();
}

