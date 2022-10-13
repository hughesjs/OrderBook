using OrderBookService.Protos.ServiceBases;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class GreeterServiceTests : ApiTestBase
{
    public GreeterServiceTests(OrderBookTestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper) { }


    [Fact]
    public async Task SayHelloTest()
    {
        Greeter.GreeterClient client = new(Channel);
        
        HelloReply? response = await client.SayHelloAsync(new() { Name = "Bob" });
        
        response.Message.ShouldBe("Hello Bob");
    }
}
