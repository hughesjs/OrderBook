using System.Text.Json;
using StackExchange.Redis;

namespace OrderBookService.Application.Caching;

public static class IDatabaseAsyncExtensions
{
	public static async Task SetData<T>(this IDatabaseAsync db, string key, T data)
	{
		await db.StringSetAsync(key, JsonSerializer.Serialize(data));
		await db.KeyExpireAsync(key, TimeSpan.FromMinutes(1));
	}

	public static async Task<T?> GetData<T>(this IDatabaseAsync db, string key)
	{
		RedisValue res = await db.StringGetAsync(key);
		return res.IsNull ? default : JsonSerializer.Deserialize<T>(res!);
	}
}
