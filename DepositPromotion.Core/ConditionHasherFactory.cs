namespace Tgs.Promotion.Deposits;

internal static class ConditionHasherFactory
{
	public static IConditionHasher CreateHasher(string promotionType)
	{
		return promotionType switch
		{
			DepositPromotionConsts.DepositRewardByConditionTypeName => new DepositRewardByConditionHasher(),
			_ => throw new NotImplementedException()
		};
	}
}
