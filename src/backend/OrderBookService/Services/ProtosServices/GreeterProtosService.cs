using Grpc.Core;
using OrderBookService.Protos.ServiceBases;

namespace OrderBookService.Services.ProtosServices;

public class GreeterProtosService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterProtosService> _logger;
    public GreeterProtosService(ILogger<GreeterProtosService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
