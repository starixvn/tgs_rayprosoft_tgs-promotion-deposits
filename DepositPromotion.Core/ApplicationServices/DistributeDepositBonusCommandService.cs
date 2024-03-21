using Tgs.Calculate.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.Deposits.ApplicationServices;

internal class DistributeDepositBonusCommandService(
	IPromotionServiceClient promotionServiceClient,
	ITgsRepository tgsRepository,
	ICalculateClient calculateClient) : ICommandService<DistributeDepositBonusCommand>
{
	public async ValueTask ExecuteAsync(DistributeDepositBonusCommand command, CancellationToken cancellationToken = default)
	{
		if (command.DepositForm.PromotionId is null)
			return;

		var promotionId = command.DepositForm.PromotionId.Value;

		var promotionMainInfo = await promotionServiceClient.GetPromotionMainInfoAsync(
			command.DepositForm.PromotionId.Value,
			cancellationToken).ConfigureAwait(false);

		var rule = await promotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardSettings>>(
			promotionId,
			cancellationToken).ConfigureAwait(false);

		if (rule is null || promotionMainInfo is null || promotionMainInfo.PromotionType is not DepositPromotionConsts.DepositRewardByConditionTypeName)
			return;

		var (bonusAmount, bucketAmount) = CalculateAmount(
			rule.CampaignDetail,
			command.DepositForm.ApplyAmount);

		var calculateId = await tgsRepository.GetPromotionCalculateIdAsync(
			promotionMainInfo.Id,
			cancellationToken).ConfigureAwait(false);

		var postUsedAmount = await calculateClient.ExchangeAsync(
			calculateId!.Value,
			bonusAmount,
			cancellationToken).ConfigureAwait(false);

		if (promotionMainInfo.PromotionLimitAmount is not null
			&& postUsedAmount.PostBalance - bonusAmount >= promotionMainInfo.PromotionLimitAmount.Value)
		{
			_ = await calculateClient.ExchangeAsync(
				calculateId!.Value,
				-bonusAmount,
				cancellationToken).ConfigureAwait(false);

			return;
		}

		var distPlan = await promotionServiceClient.GetPromotionDistributePlanAsync(
			promotionId,
			0L,
			cancellationToken).ConfigureAwait(false);

		await promotionServiceClient.DistributePromotionBonusAsync(
			distPlan.Id,
			command.DepositForm.PlayerCode,
			bonusAmount,
			bucketAmount,
			null,
			command.DepositForm.Id,
			rule.CampaignDetail.BucketTypes,
			cancellationToken).ConfigureAwait(false);

		await promotionServiceClient.UpdatePromotionUsedAmountAsync(
			promotionMainInfo.Id,
			postUsedAmount.PostBalance,
			postUsedAmount.Id,
			cancellationToken).ConfigureAwait(false);
	}

	private static (decimal bonusAmount, decimal bucketAmount) CalculateAmount(
		DepositRewardSettings setting,
		decimal depositAmount)
	{
		var depositBonusAmount = depositAmount;

		var bonusAmount = setting.Percentage * depositAmount / 100M;

		bonusAmount = bonusAmount > setting.BonusAmountLimit
			? setting.BonusAmountLimit
			: bonusAmount;

		depositBonusAmount += bonusAmount;

		var bucketAmount = depositBonusAmount * setting.TurnoverMultiple - depositAmount;

		return (bonusAmount, bucketAmount);
	}
}
