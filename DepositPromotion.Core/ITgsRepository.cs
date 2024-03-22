namespace Tgs.Promotion.Deposits;

public interface ITgsRepository
{
	ValueTask<Guid?> GetPromotionCalculateIdAsync(Guid promotionId, CancellationToken cancellationToken = default);

	ValueTask SetPromotionCalculateIdAsync(Guid promotionId, Guid calculateId, CancellationToken cancellationToken = default);

	ValueTask<DepositCountInfo> GetDepositCountInfoAsync(Guid promotionId, string playerCode, DateTime date, CancellationToken cancellationToken = default);
}
