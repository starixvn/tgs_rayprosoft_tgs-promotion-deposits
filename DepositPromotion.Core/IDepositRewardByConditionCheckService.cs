namespace Tgs.Promotion.Deposits;

public interface IDepositRewardByConditionCheckService
{
	ValueTask<bool> CheckAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);

	ValueTask RollbackTagAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);

	ValueTask TagDepositAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default);
}
