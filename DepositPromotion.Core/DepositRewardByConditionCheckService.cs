using Microsoft.Extensions.Logging;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;

namespace Tgs.Promotion.Deposits;

internal class DepositRewardByConditionCheckService(
	IFirstLevelRepository firstLevelRepository,
	IPromotionServiceClient promotionServiceClient,
	ITgsRepository tgsRepository,
	IPlayerServiceClient playerServiceClient,
	ILogger<DepositRewardByConditionCheckService> logger)
	: IDepositRewardByConditionCheckService
{
	public async ValueTask<bool> CheckAsync(
		Guid promotionId,
		string playerCode,
		DateTimeOffset date,
		CancellationToken cancellationToken = default)
	{
		var rule = await promotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(
			promotionId,
			cancellationToken).ConfigureAwait(false);

		var promotionSettings = rule?.CampaignDetail;

		if (promotionSettings is null)
			return false;

		logger.LogInformation("PromotionId:{promotionId}, PromotionSettings.PlayerConditions:{playerConditions}.", promotionId, promotionSettings.PlayerConditions);

		if (promotionSettings.PlayerConditions is not null)
		{
			var playerInfo = await playerServiceClient.GetPlayerInfoAsync(
				playerCode,
				cancellationToken).ConfigureAwait(false);

			if (playerInfo is null)
				return false;

			var playerProps = await playerServiceClient.GetPlayerPropertiesAsync(
				playerCode,
				cancellationToken).ConfigureAwait(false);

			if (promotionSettings.PlayerConditions.Any(propName => propName == "RealName"))
			{
				playerProps.TryGetValue("RealName", out var realName);

				logger.LogInformation("PlayerCode:{playerCode}, RealName:{realName}.", playerCode, realName);

				if (string.IsNullOrWhiteSpace(realName))
					return false;
			}

			if (promotionSettings.PlayerConditions.Any(propName => propName == "Birthdate"))
			{
				playerProps.TryGetValue("Birthdate", out var birthdate);

				logger.LogInformation("PlayerCode:{playerCode}, Birthdate:{birthdate}.", playerCode, birthdate);

				if (string.IsNullOrWhiteSpace(birthdate))
					return false;
			}

			if (promotionSettings.PlayerConditions.Any(propName => propName == "Phone"))
			{
				playerProps.TryGetValue("Phone", out var phone);

				logger.LogInformation("PlayerCode:{playerCode}, Phone:{phone}.", playerCode, phone);

				if (string.IsNullOrWhiteSpace(phone))
					return false;
			}
		}

		var processDateOnly = DateOnly.FromDateTime(date.DateTime);

		//await firstLevelRepository.IncrementPlayerDepositByConditionAsync(
		//	promotionId,
		//	playerCode,
		//	processDateOnly,
		//	cancellationToken).ConfigureAwait(false);

		var dailyCount = await firstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
			promotionId,
			playerCode,
			processDateOnly,
			cancellationToken).ConfigureAwait(false);

		logger.LogInformation("PlayerCode:{playerCode}, ProcessDate:{processDateOnly}, DailyCount:{dailyCount}.", playerCode, processDateOnly, dailyCount);

		if (promotionSettings.DailyDepositLimitByPlayer is not null)
		{
			logger.LogInformation("DailyDepositLimitByPlayer:{DailyDepositLimitByPlayer}.", promotionSettings.DailyDepositLimitByPlayer);
			if (promotionSettings.DailyDepositLimitByPlayer < dailyCount + 1)
			{
				return false;
			}
		}

		if (promotionSettings.WeeklyDepositLimitByPlayer is not null)
		{
			var weeklyCount = await firstLevelRepository.GetPlayerDepositByConditionWeeklyCountAsync(
				promotionId,
				playerCode,
				processDateOnly,
				cancellationToken).ConfigureAwait(false);

			logger.LogInformation("WeeklyDepositLimitByPlayer:{WeeklyDepositLimitByPlayer}, WeeklyCount:{weeklyCount}.",
				promotionSettings.WeeklyDepositLimitByPlayer, weeklyCount);
			if (promotionSettings.WeeklyDepositLimitByPlayer < weeklyCount + dailyCount + 1)
			{
				return false;
			}
		}

		if (promotionSettings.MonthlyDepositLimitByPlayer is not null)
		{
			var monthlyCount = await firstLevelRepository.GetPlayerDepositByConditionMonthlyCountAsync(
				promotionId,
				playerCode,
				processDateOnly,
				cancellationToken).ConfigureAwait(false);

			logger.LogInformation("MonthlyDepositLimitByPlayer:{MonthlyDepositLimitByPlayer}, MonthlyCount:{monthlyCount}.",
				promotionSettings.MonthlyDepositLimitByPlayer, monthlyCount);
			if (promotionSettings.MonthlyDepositLimitByPlayer < monthlyCount + dailyCount + 1)
			{
				return false;
			}
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

			logger.LogInformation("TotalDepositLimitByPlayer:{TotalDepositLimitByPlayer}, TotalCountInfo.Count:{totalCountInfo}.",
				promotionSettings.TotalDepositLimitByPlayer, totalCountInfo.Count);
			if (promotionSettings.TotalDepositLimitByPlayer < totalCountInfo.Count + dailyCount + 1)
			{
				return false;
			}
		}

		//await firstLevelRepository.DecrementPlayerDepositByConditionAsync(
		//	promotionId,
		//	playerCode,
		//	DateOnly.FromDateTime(date.DateTime),
		//	cancellationToken).ConfigureAwait(false);

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