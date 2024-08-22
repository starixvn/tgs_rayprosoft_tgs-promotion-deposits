using System.Runtime.CompilerServices;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.Deposits.ApplicationServices;

internal class ListPlayerDailyCountQueryService(
	IFirstLevelRepository firstLevelRepository)
	: IQueryService<ListPlayerDailyCountQuery, PlayerDailyInfo>
{
	private static readonly string _Key = "Tgs:Promotion:Deposits:DepositRewardV2:{0}:{1}:Day:*";

	public async IAsyncEnumerable<PlayerDailyInfo> QueryAsync(
		ListPlayerDailyCountQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var keys = await firstLevelRepository
			.SearchKeysAsync(string.Format(_Key, query.PromotionId, query.PlayerCode), cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		foreach (var key in keys)
		{
			var count = await firstLevelRepository
				.GetDailyCountAsync(key, cancellationToken)
				.ConfigureAwait(false);

			yield return new PlayerDailyInfo(key, count);
		}
	}
}