namespace Tgs.Promotion.Deposits;

public interface ITgsRepository
{
	ValueTask<Guid?> GetPromotionCalculateIdAsync(Guid promotionId, CancellationToken cancellationToken = default);

	ValueTask SetPromotionCalculateIdAsync(Guid promotionId, Guid calculateId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the deposit count information asynchronous.
	/// </summary>
	/// <param name="promotionId">The promotion identifier.</param>
	/// <param name="playerCode">The player code.</param>
	/// <param name="date">查詢的日期，需要傳入 Brand 時區的零時整點時間，如果傳錯會導致計算錯誤</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>傳回指定日期的前天統計，或者是之前統計好的資料</returns>
	ValueTask<DepositCountInfo> GetDepositCountInfoAsync(
		Guid promotionId,
		string playerCode,
		DateTime date,
		CancellationToken cancellationToken = default);
}
