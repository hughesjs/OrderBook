using Calzolari.Grpc.AspNetCore.Validation;
using JetBrains.Annotations;
using OrderBookService.Application.Interceptors;
using OrderBookService.DependencyInjection;
using OrderBookService.Services.ProtosServices;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(c =>
						 {
							 c.EnableMessageValidation();
							 c.Interceptors.Add<ExceptionInterceptor>();
							 c.Interceptors.Add<IdempotencyInterceptor>();
						 }
						);
builder.Services.AddGrpcReflection();

builder.Services.AddOrderBookServices(builder.Configuration);

builder.ConfigureOrderBookServices();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrderBookProtosService>();

if (app.Environment.IsDevelopment())
{
	app.MapGrpcReflectionService();
}

app.Run();

namespace OrderBookService
{
	[UsedImplicitly]
	public partial class Program { }
}
