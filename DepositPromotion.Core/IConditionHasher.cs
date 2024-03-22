namespace Tgs.Promotion.Deposits;

internal interface IConditionHasher
{
	long ComputeHash(ReadOnlySpan<byte> data);
}
