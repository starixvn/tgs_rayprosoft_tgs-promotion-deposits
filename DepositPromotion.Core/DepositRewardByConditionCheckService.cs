using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Promotion.Clients;

namespace Tgs.Promotion.Deposits;

internal class DepositRewardByConditionCheckService(
	IFirstLevelRepository firstLevelRepository,
	IPromotionServiceClient promotionServiceClient)
	: IDepositRewardByConditionCheckService
{
	public async ValueTask<bool> CheckAsync(
		Guid promotionId,
		string playerCode,
		DateOnly date,
		CancellationToken cancellationToken = default)
	{
		var promotionSettings = await promotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(
			promotionId,
			cancellationToken).ConfigureAwait(false);

		if (promotionSettings is null)
			return false;

		await firstLevelRepository.IncrementPlayerDepositByConditionAsync(
			promotionId,
			playerCode,
			date,
			cancellationToken).ConfigureAwait(false);

		var dailyCount = await firstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
			promotionId,
			playerCode,
			date,
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
				date,
				cancellationToken).ConfigureAwait(false);

			if (promotionSettings.WeeklyDepositLimitByPlayer < weeklyCount + dailyCount)
				return false;
		}

		if (promotionSettings.MonthlyDepositLimitByPlayer is not null)
		{
			var monthlyCount = await firstLevelRepository.GetPlayerDepositByConditionMonthlyCountAsync(
				promotionId,
				playerCode,
				date,
				cancellationToken).ConfigureAwait(false);

			if (promotionSettings.MonthlyDepositLimitByPlayer < monthlyCount + dailyCount)
				return false;
		}

		await firstLevelRepository.DecrementPlayerDepositByConditionAsync(
			promotionId,
			playerCode,
			date,
			cancellationToken).ConfigureAwait(false);

		return true;
	}

	public ValueTask RollbackTagAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default) => throw new NotImplementedException();

	public ValueTask TagDepositAsync(Guid promotionId, string playerCode, DateOnly date, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
