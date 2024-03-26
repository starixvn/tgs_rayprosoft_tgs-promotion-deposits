using Tgs.Promotion.Clients;

namespace Tgs.Promotion.Deposits;

internal class DepositRewardByConditionCheckService(
	IFirstLevelRepository firstLevelRepository,
	IPromotionServiceClient promotionServiceClient,
	ITgsRepository tgsRepository)
	: IDepositRewardByConditionCheckService
{
	public async ValueTask<bool> CheckAsync(
		Guid promotionId,
		string playerCode,
		DateTimeOffset date,
		CancellationToken cancellationToken = default)
	{
		var promotionSettings = await promotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(
			promotionId,
			cancellationToken).ConfigureAwait(false);

		if (promotionSettings is null)
			return false;

		var processDateOnly = DateOnly.FromDateTime(date.DateTime);

		await firstLevelRepository.IncrementPlayerDepositByConditionAsync(
			promotionId,
			playerCode,
			processDateOnly,
			cancellationToken).ConfigureAwait(false);

		var dailyCount = await firstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
			promotionId,
			playerCode,
			processDateOnly,
			cancellationToken).ConfigureAwait(false);

		if (promotionSettings.DailyDepositLimitByPlayer is not null)
		{
			if (promotionSettings.DailyDepositLimitByPlayer < dailyCount)
				return false;
		}

		if (promotionSettings.WeeklyDepositLimitByPlayer is not null)
		{
			var weeklyCount = await firstLevelRepository.GetPlayerDepositByConditionWeeklyCountAsync(
				promotionId,
				playerCode,
				processDateOnly,
				cancellationToken).ConfigureAwait(false);

			if (promotionSettings.WeeklyDepositLimitByPlayer < weeklyCount + dailyCount)
				return false;
		}

		if (promotionSettings.MonthlyDepositLimitByPlayer is not null)
		{
			var monthlyCount = await firstLevelRepository.GetPlayerDepositByConditionMonthlyCountAsync(
				promotionId,
				playerCode,
				processDateOnly,
				cancellationToken).ConfigureAwait(false);

			if (promotionSettings.MonthlyDepositLimitByPlayer < monthlyCount + dailyCount)
				return false;
		}

		if (promotionSettings.TotalDepositLimitByPlayer is not null)
		{
			var totalCountInfo = await firstLevelRepository.GetPlayerDepositByConditionTotalCountAsync(
				promotionId,
				playerCode,
				cancellationToken).ConfigureAwait(false);

			if (totalCountInfo is null || totalCountInfo.Processed < date.UtcDateTime)
			{
				totalCountInfo = await tgsRepository.GetDepositCountInfoAsync(
					promotionId,
					playerCode,
					date.UtcDateTime,
					cancellationToken).ConfigureAwait(false);

				await firstLevelRepository.SetPlayerDepositByConditionTotalCountAsync(
					promotionId,
					playerCode,
					processDateOnly,
					totalCountInfo,
					cancellationToken).ConfigureAwait(false);
			}

			if (promotionSettings.TotalDepositLimitByPlayer < totalCountInfo.Count + dailyCount)
				return false;
		}

		await firstLevelRepository.DecrementPlayerDepositByConditionAsync(
			promotionId,
			playerCode,
			DateOnly.FromDateTime(date.DateTime),
			cancellationToken).ConfigureAwait(false);

		return true;
	}

	public ValueTask RollbackTagAsync(
		Guid promotionId,
		string playerCode,
		DateTimeOffset date,
		CancellationToken cancellationToken = default)
		=> firstLevelRepository.DecrementPlayerDepositByConditionAsync(
			promotionId,
			playerCode,
			DateOnly.FromDateTime(date.DateTime),
			cancellationToken);

	public ValueTask TagDepositAsync(
		Guid promotionId,
		string playerCode,
		DateTimeOffset date,
		CancellationToken cancellationToken = default)
		=> firstLevelRepository.IncrementPlayerDepositByConditionAsync(
			promotionId,
			playerCode,
			DateOnly.FromDateTime(date.DateTime),
			cancellationToken);
}
