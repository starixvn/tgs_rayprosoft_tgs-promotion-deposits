using Microsoft.EntityFrameworkCore;
using Tgs.Promotion.Deposits.Repositories.Entities;

namespace Tgs.Promotion.Deposits.Repositories;

internal class TgsRepository(TgsContext dbContext) : ITgsRepository
{
	public async ValueTask<Guid?> GetPromotionCalculateIdAsync(Guid promotionId, CancellationToken cancellationToken = default)
	{
		var row = await dbContext.DepositPromotionCalculatePairs.FindAsync(
			[promotionId],
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return row?.CalculateId;
	}

	public async ValueTask SetPromotionCalculateIdAsync(Guid promotionId, Guid calculateId, CancellationToken cancellationToken = default)
	{
		_ = await dbContext.DepositPromotionCalculatePairs
			.AddAsync(
				new DepositPromotionCalculatePair
				{
					PromotionId = promotionId,
					CalculateId = calculateId
				},
				cancellationToken)
			.ConfigureAwait(false);

		_ = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<DepositCountInfo> GetDepositCountInfoAsync(
		Guid promotionId,
		string playerCode,
		DateTime date,
		CancellationToken cancellationToken = default)
	{
		var statistics = await dbContext.DepositPromotionStatistics.FindAsync(
			[promotionId, playerCode],
			cancellationToken).ConfigureAwait(false)
			?? (await dbContext.AddAsync(
				new DepositPromotionStatistic
				{
					PromotionId = promotionId,
					PlayerCode = playerCode,
					ProcessedDate = DateTime.MinValue,
					TotalDepositCount = 0
				},
				cancellationToken).ConfigureAwait(false)).Entity;

		if (statistics.ProcessedDate < date)
		{
			var notCounted = await dbContext.PromotionDistributes
				.Where(pd => pd.PromotionId == promotionId
					&& pd.PlayerCode == playerCode
					&& pd.CreatedOn >= statistics.ProcessedDate
					&& pd.CreatedOn < date)
				.CountAsync(cancellationToken)
				.ConfigureAwait(false);

			statistics.ProcessedDate = date;
			statistics.TotalDepositCount += notCounted;

			_ = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}

		return new DepositCountInfo(
			statistics.ProcessedDate,
			statistics.TotalDepositCount);
	}
}
