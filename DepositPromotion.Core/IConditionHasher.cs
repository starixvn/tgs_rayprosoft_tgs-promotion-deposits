namespace Tgs.Promotion.Deposits;

public interface IConditionHasher
{
	long ComputeHash(Guid depositFormId);
}