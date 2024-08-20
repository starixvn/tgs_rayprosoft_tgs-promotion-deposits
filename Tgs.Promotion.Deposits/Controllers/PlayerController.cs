using Microsoft.AspNetCore.Mvc;
using Tgs.Promotion.Deposits.ApplicationServices;
using Tgs.Promotion.Deposits.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.Deposits.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
	/// <summary>
	/// 查詢玩家在活動裡的每日次數
	/// </summary>
	/// <param name="queryService"></param>
	/// <param name="promotionId"></param>
	/// <param name="playerCode"></param>
	/// <returns></returns>
	[HttpGet("daily")]
	public async IAsyncEnumerable<PlayerDailyViewModel> GetPlayerDailyCountAsync(
		[FromServices] IQueryService<ListPlayerDailyCountQuery, PlayerDailyInfo> queryService,
		[FromQuery] Guid promotionId,
		[FromQuery] string playerCode)
	{
		var result = await queryService
			.QueryAsync(new ListPlayerDailyCountQuery(promotionId, playerCode), HttpContext.RequestAborted)
			.ToArrayAsync(HttpContext.RequestAborted)
			.ConfigureAwait(false);

		foreach (var item in result)
		{
			yield return new PlayerDailyViewModel
			{
				Key = item.Key,
				Count = item.Count,
			};
		}
	}
}