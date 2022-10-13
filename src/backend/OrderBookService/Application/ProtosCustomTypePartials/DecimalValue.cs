// MS recommended implementation: https://learn.microsoft.com/en-us/aspnet/core/grpc/protobuf?view=aspnetcore-6.0

// This isn't going to line-up because it's autogenned proto code I'm hooking into
// ReSharper disable once CheckNamespace
namespace OrderBookProtos.CustomTypes;


public partial class DecimalValue
	{
		private const decimal NanoFactor = 1_000_000_000;
		public DecimalValue(long units, int nanos)
		{
			Units = units;
			Nanos = nanos;
		}

		public static implicit operator decimal(DecimalValue grpcDecimal)
		{
			return grpcDecimal.Units + grpcDecimal.Nanos/ NanoFactor;
		}

		public static implicit operator DecimalValue(decimal value)
		{
			long units = decimal.ToInt64(value);
			int nanos = decimal.ToInt32((value - units)* NanoFactor);
			return new(units, nanos);
		}
}

