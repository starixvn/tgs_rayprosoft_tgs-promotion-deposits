using Tgs.ServiceUtility;

namespace Tgs.Promotion.Deposits.ApplicationServices;

internal class GetPlayerTotalCountQueryService(IFirstLevelRepository firstLevelRepository)
	: IGetService<GetPlayerTotalCountQuery, PlayerTotalInfo>
{
	private readonly string _key = "Tgs:Promotion:Deposits:DepositRewardV2:{0}:{1}:Total";

	public async ValueTask<PlayerTotalInfo> GetAsync(GetPlayerTotalCountQuery query, CancellationToken cancellationToken = default)
	{
		var result = await firstLevelRepository
			.GetPlayerDepositByConditionTotalCountAsync(query.PromotionId, query.PlayerCode, cancellationToken)
			.ConfigureAwait(false);

		return result is null
			? new PlayerTotalInfo(null, null, null)
			: new PlayerTotalInfo(
				Key: string.Format(_key, query.PromotionId, query.PlayerCode),
				Processed: result.Processed,
				Count: result.Count);
	}
}