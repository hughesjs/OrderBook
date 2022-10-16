using AutoFixture;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using OrderBookProtos.CustomTypes;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class ApiTestBase : IClassFixture<OrderBookTestFixture>, IDisposable
{
	private OrderBookTestFixture TestFixture { get; }
	
	private   LoggerFactory        LoggerFactory { get; }
	private   GrpcChannel?         _channel;

	protected static Fixture AutoFix { get; } = new();

	static ApiTestBase()
	{
		AutoFix.Customize<GuidValue>(c => c.With(p => p.Value, () => Guid.NewGuid().ToString()));
	}
	
	
	protected ApiTestBase(OrderBookTestFixture testFixture, ITestOutputHelper outputHelper)
	{
		LoggerFactory = new();
		TestFixture       = testFixture;
	}
	
	protected GrpcChannel Channel => _channel ??= CreateChannel();

	private GrpcChannel CreateChannel()
	{
		return GrpcChannel.ForAddress("http://localhost", new()
														  {
															  LoggerFactory = LoggerFactory,
															  HttpHandler   = TestFixture.Handler
														  });
	}

	public void Dispose()
	{
		_channel = null;
	}
}
