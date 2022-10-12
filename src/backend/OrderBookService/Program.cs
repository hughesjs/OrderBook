using OrderBookService.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();

if (app.Environment.IsDevelopment())
{
	app.MapGrpcReflectionService();
}

app.Run();

