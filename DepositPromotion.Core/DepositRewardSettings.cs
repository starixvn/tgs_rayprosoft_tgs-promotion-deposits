namespace Tgs.Promotion.Deposits;
public record DepositRewardSettings(
	decimal Percentage,
	decimal BonusAmountLimit,
	decimal TurnoverMultiple,
	int[] VipLevels,
	string[]? BucketTypes);
