using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;
using StackExchange.Redis;

namespace Tgs.Promotion.Deposits.Repositories;

internal class RedisFirstLevelRepository(
	IDatabase redisDatabase)
	: IFirstLevelRepository
{
	private static readonly RedisKey _KeyPrefix = "Tgs:Promotion:Deposits:";

	public async ValueTask<long?> GetPlayerDepositByConditionDailyCountAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var dataKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");

		var result = await redisDatabase.StringGetAsync(
			dataKey).ConfigureAwait(false);

		return result.HasValue ? (long)result : null;
	}

	public async ValueTask IncrementPlayerDepositByConditionAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var dataKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");

		var expireDate = date.CeilingMonthDay().AddDays(1).ToDateTime(TimeOnly.MinValue);
		_ = await redisDatabase.StringSetAsync(
			dataKey,
			0L,
			expiry: expireDate - date.ToDateTime(TimeOnly.MinValue),
			when: When.NotExists).ConfigureAwait(false);

		_ = await redisDatabase.StringIncrementAsync(
			dataKey).ConfigureAwait(false);
	}

	public async ValueTask DecrementPlayerDepositByConditionAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var dataKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");

		var expireDate = date.CeilingMonthDay().AddDays(1).ToDateTime(TimeOnly.MinValue);
		_ = await redisDatabase.StringSetAsync(
			dataKey,
			1L,
			expiry: expireDate - date.ToDateTime(TimeOnly.MinValue),
			when: When.NotExists).ConfigureAwait(false);

		var result = await redisDatabase.StringDecrementAsync(
			  dataKey).ConfigureAwait(false);

		if (result < 0L)
		{
			await IncrementPlayerDepositByConditionAsync(
				promotionId,
				playerCode,
				date,
				cancellationToken).ConfigureAwait(false);
		}
	}

	public async ValueTask<long?> GetPlayerDepositByConditionMonthlyCountAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var dataKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Month:{date.FloorMonthDay()}");

		var json = await redisDatabase.StringGetAsync(
			dataKey).ConfigureAwait(false);

		var result = new DepositCountSnapshot(0L, DateOnly.MinValue);

		if (json.HasValue)
		{
			result = JsonSerializer.Deserialize<DepositCountSnapshot>((string)json!)
				?? new DepositCountSnapshot(0L, DateOnly.MinValue);
		}

		if (result.SnapshotDate < date.AddDays(-1))
		{
			var startDate = result.SnapshotDate < date.FloorMonthDay()
				? date.FloorMonthDay()
				: result.SnapshotDate.AddDays(1);

			var totalCount = result.Count;

			foreach (var processDate in Enumerable.Range(0, 31)
				.Select(startDate.AddDays)
				.Where(processDate => processDate < date))
			{
				var dailyCount = await GetPlayerDepositByConditionDailyCountAsync(
					promotionId,
					playerCode,
					processDate,
					cancellationToken).ConfigureAwait(false);

				totalCount += dailyCount ?? 0L;
			}

			result = new DepositCountSnapshot(totalCount, date.AddDays(-1));

			var expireDate = date.CeilingMonthDay().AddDays(1).ToDateTime(TimeOnly.MinValue);
			await redisDatabase.StringSetAsync(
				dataKey,
				JsonSerializer.Serialize(result),
				expiry: expireDate - date.ToDateTime(TimeOnly.MinValue)).ConfigureAwait(false);
		}

		return result.Count;
	}

	public async ValueTask<long?> GetPlayerDepositByConditionWeeklyCountAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var dataKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Week:{date.FloorWeekDay()}");

		var json = await redisDatabase.StringGetAsync(
			dataKey).ConfigureAwait(false);

		var result = new DepositCountSnapshot(0L, DateOnly.MinValue);

		if (json.HasValue)
		{
			result = JsonSerializer.Deserialize<DepositCountSnapshot>((string)json!)
				?? new DepositCountSnapshot(0L, DateOnly.MinValue);
		}

		if (result.SnapshotDate < date.AddDays(-1))
		{
			var startDate = result.SnapshotDate < date.FloorWeekDay()
				? date.FloorWeekDay()
				: result.SnapshotDate.AddDays(1);

			var totalCount = result.Count;

			foreach (var processDate in Enumerable.Range(0, 7)
				.Select(startDate.AddDays)
				.Where(processDate => processDate < date))
			{
				var dailyCount = await GetPlayerDepositByConditionDailyCountAsync(
					promotionId,
					playerCode,
					processDate,
					cancellationToken).ConfigureAwait(false);

				totalCount += dailyCount ?? 0L;
			}

			result = new DepositCountSnapshot(totalCount, date.AddDays(-1));

			var expireDate = date.CeilingMonthDay().AddDays(1).ToDateTime(TimeOnly.MinValue);
			await redisDatabase.StringSetAsync(
				dataKey,
				JsonSerializer.Serialize(result),
				expiry: expireDate - date.ToDateTime(TimeOnly.MinValue)).ConfigureAwait(false);
		}

		return result.Count;
	}

	public async ValueTask<DepositCountInfo?> GetPlayerDepositByConditionTotalCountAsync(
		Guid promotionId,
		string playerCode,
		CancellationToken cancellationToken = default)
	{
		var dataKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Total");

		var result = await redisDatabase.StringGetAsync(
			dataKey).ConfigureAwait(false);

		return result.HasValue
			? JsonSerializer.Deserialize<DepositCountInfo>((string)result!, PromotionUtility.SerializerOptions)
			: null;
	}

	public async ValueTask SetPlayerDepositByConditionTotalCountAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		DepositCountInfo countInfo,
		CancellationToken cancellationToken = default)
	{
		var dataKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Total");
		var expireDate = date.CeilingMonthDay().AddDays(1).ToDateTime(TimeOnly.MinValue);

		_ = await redisDatabase.StringSetAsync(
			dataKey,
			JsonSerializer.Serialize(countInfo, PromotionUtility.SerializerOptions),
			expiry: expireDate - date.ToDateTime(TimeOnly.MinValue)).ConfigureAwait(false);
	}

	public async IAsyncEnumerable<string> SearchKeysAsync(
		string dataKey,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var keys = await redisDatabase.ScriptEvaluateAsync(LuaScript.Prepare(
			   //Redis的keys模糊查询：
			   " local res = redis.call('KEYS', @keypattern) " +
			   " return res "), new { @keypattern = $"{dataKey}" })
			.ConfigureAwait(false);

		if (keys is null)
			yield break;

		var result = ((string[])keys).ToArray();

		foreach (var key in result)
			yield return key;
	}

	public async ValueTask<long?> GetDailyCountAsync(string key, CancellationToken cancellationToken = default)
	{
		var result = await redisDatabase
			.StringGetAsync(key)
			.ConfigureAwait(false);

		return result.HasValue ? (long)result : null;
	}	

	public async ValueTask<JsonDocument> GetKeyInfoAsync(string key, CancellationToken cancellationToken = default)
	{
		var json = await redisDatabase
			.StringGetAsync(key)
			.ConfigureAwait(false);

		return json.HasValue
			? JsonDocument.Parse((string)json!)
			: JsonDocument.Parse("{}");
	}	
}