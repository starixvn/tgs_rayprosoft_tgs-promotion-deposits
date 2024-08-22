using System.Runtime.CompilerServices;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.Deposits.ApplicationServices;

internal class ListPlayerWeeklyCountQueryService(IFirstLevelRepository firstLevelRepository)
	: IQueryService<ListPlayerWeeklyCountQuery, PlayerWeeklyInfo>
{
	private static readonly string _Key = "Tgs:Promotion:Deposits:DepositRewardV2:{0}:{1}:Week:*";

	public async IAsyncEnumerable<PlayerWeeklyInfo> QueryAsync(
		ListPlayerWeeklyCountQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var keys = await firstLevelRepository
			.SearchKeysAsync(string.Format(_Key, query.PromotionId, query.PlayerCode), cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		foreach (var key in keys)
		{
			var weeklyInfo = await firstLevelRepository
				.GetKeyInfoAsync(key, cancellationToken)
				.ConfigureAwait(false);

			yield return new PlayerWeeklyInfo(key, weeklyInfo);
		}
	}
}