FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["OrderBookService.Benchmarks/OrderBookService.Benchmarks.csproj", "OrderBookService.Benchmarks/"]
RUN dotnet restore "OrderBookService.Benchmarks/OrderBookService.Benchmarks.csproj"
COPY . .
WORKDIR "/src/OrderBookService.Benchmarks"
RUN dotnet build "OrderBookService.Benchmarks.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderBookService.Benchmarks.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderBookService.Benchmarks.dll"]
