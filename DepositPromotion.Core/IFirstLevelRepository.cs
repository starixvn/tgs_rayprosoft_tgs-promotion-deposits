using System.Text.Json;

namespace Tgs.Promotion.Deposits;

public interface IFirstLevelRepository
{
	ValueTask<long?> GetPlayerDepositByConditionDailyCountAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default);

	ValueTask<DepositCountInfo?> GetPlayerDepositByConditionTotalCountAsync(
		Guid promotionId,
		string playerCode,
		CancellationToken cancellationToken = default);

	ValueTask SetPlayerDepositByConditionTotalCountAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		DepositCountInfo countInfo,
		CancellationToken cancellationToken = default);

	ValueTask IncrementPlayerDepositByConditionAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default);

	ValueTask DecrementPlayerDepositByConditionAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default);

	ValueTask<long?> GetPlayerDepositByConditionMonthlyCountAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default);

	ValueTask<long?> GetPlayerDepositByConditionWeeklyCountAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default);

	IAsyncEnumerable<string> SearchKeysAsync(
		string dataKey,
		CancellationToken cancellationToken = default);

	ValueTask<long?> GetDailyCountAsync(
		string key,
		CancellationToken cancellationToken = default);

	ValueTask<JsonDocument> GetKeyInfoAsync(
		string key,
		CancellationToken cancellationToken = default);
}