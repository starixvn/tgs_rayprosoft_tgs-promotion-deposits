using System.Runtime.InteropServices;
using Isopoh.Cryptography.Blake2b;
using Isopoh.Cryptography.SecureArray;

namespace Tgs.Promotion.Deposits;

internal class DepositRewardByConditionHasher : IConditionHasher
{
	public long ComputeHash(Guid depositFormId)
	{
		var hasher = Blake2B.Create(
			 new Blake2BConfig { OutputSizeInBits = 64 },
			 SecureArray.DefaultCall);

		var hash = hasher
			.AsHashAlgorithm()
			.ComputeHash(depositFormId.ToByteArray());

		return MemoryMarshal.AsRef<long>(hash);
	}
}