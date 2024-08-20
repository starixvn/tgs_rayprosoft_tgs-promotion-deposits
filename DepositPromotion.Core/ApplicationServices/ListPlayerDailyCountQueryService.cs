using System.Runtime.CompilerServices;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.Deposits.ApplicationServices;

internal class ListPlayerDailyCountQueryService(
	IFirstLevelRepository firstLevelRepository)
	: IQueryService<ListPlayerDailyCountQuery, PlayerDailyInfo>
{
	public async IAsyncEnumerable<PlayerDailyInfo> QueryAsync(
		ListPlayerDailyCountQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var keys = await firstLevelRepository
			.GetPlayerDailyKeysAsync(query.PromotionId, query.PlayerCode, cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		foreach (var key in keys)
		{
			var count = await firstLevelRepository
				.GetDailyCountAsync(key)
				.ConfigureAwait(false);

			yield return new PlayerDailyInfo(key, count);
		}
	}
}