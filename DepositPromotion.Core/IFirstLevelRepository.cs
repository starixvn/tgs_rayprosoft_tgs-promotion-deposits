namespace Tgs.Promotion.Deposits;

public interface IFirstLevelRepository
{
	ValueTask<long?> GetPlayerDepositByConditionDailyCountAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);

	ValueTask IncrementPlayerDepositByConditionAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);

	ValueTask DecrementPlayerDepositByConditionAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);

	ValueTask<long?> GetPlayerDepositByConditionMonthlyCountAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);

	ValueTask<long?> GetPlayerDepositByConditionWeeklyCountAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);
}
