namespace Tgs.Promotion.Deposits;
public record DepositRewardByConditionSettings(
	decimal Percentage,
	decimal BonusAmountLimit,
	decimal TurnoverMultiple,
	int[] VipLevels,
	string[]? BucketTypes,
	int? DailyDepositLimitByPlayer,
	int? WeeklyDepositLimitByPlayer,
	int? MonthlyDepositLimitByPlayer,
	int? TotalDepositLimitByPlayer);
