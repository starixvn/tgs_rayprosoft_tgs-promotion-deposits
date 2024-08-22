using System.Runtime.CompilerServices;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.Deposits.ApplicationServices;

internal class ListPlayerMonthlyCountQueryService(IFirstLevelRepository firstLevelRepository)
	: IQueryService<ListPlayerMonthlyCountQuery, PlayerMonthlyInfo>
{
	private static readonly string _Key = "Tgs:Promotion:Deposits:DepositRewardV2:{0}:{1}:Month:*";

	public async IAsyncEnumerable<PlayerMonthlyInfo> QueryAsync(
		ListPlayerMonthlyCountQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var keys = await firstLevelRepository
			.SearchKeysAsync(string.Format(_Key, query.PromotionId, query.PlayerCode), cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		foreach (var key in keys)
		{
			var info = await firstLevelRepository
				.GetKeyInfoAsync(key, cancellationToken)
				.ConfigureAwait(false);

			yield return new PlayerMonthlyInfo(key, info);
		}
	}
}