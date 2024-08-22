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
	/// 查詢Redis裡玩家在活動的每日次數
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

	/// <summary>
	/// 查詢Redis裡玩家在活動的每周資訊
	/// </summary>
	/// <param name="queryService"></param>
	/// <param name="promotionId"></param>
	/// <param name="playerCode"></param>
	/// <returns></returns>
	[HttpGet("weekly")]
	public async IAsyncEnumerable<PlayerWeeklyViewModel> GetPlayerWeeklyInfoAsync(
		[FromServices] IQueryService<ListPlayerWeeklyCountQuery, PlayerWeeklyInfo> queryService,
		[FromQuery] Guid promotionId,
		[FromQuery] string playerCode)
	{
		var result = await queryService
			.QueryAsync(new ListPlayerWeeklyCountQuery(promotionId, playerCode), HttpContext.RequestAborted)
			.ToArrayAsync(HttpContext.RequestAborted)
			.ConfigureAwait(false);

		foreach (var item in result)
		{
			yield return new PlayerWeeklyViewModel
			{
				Key = item.Key,
				Info = item.Info,
			};
		}
	}

	/// <summary>
	/// 查詢Redis裡玩家在活動的每月資訊
	/// </summary>
	/// <param name="queryService"></param>
	/// <param name="promotionId"></param>
	/// <param name="playerCode"></param>
	/// <returns></returns>
	[HttpGet("monthly")]
	public async IAsyncEnumerable<PlayerMonthlyViewModel> GetPlayerMonthlyInfoAsync(
		[FromServices] IQueryService<ListPlayerMonthlyCountQuery, PlayerMonthlyInfo> queryService,
		[FromQuery] Guid promotionId,
		[FromQuery] string playerCode)
	{
		var result = await queryService
			.QueryAsync(new ListPlayerMonthlyCountQuery(promotionId, playerCode), HttpContext.RequestAborted)
			.ToArrayAsync(HttpContext.RequestAborted)
			.ConfigureAwait(false);

		foreach (var item in result)
		{
			yield return new PlayerMonthlyViewModel
			{
				Key = item.Key,
				Info = item.Info,
			};
		}
	}

	/// <summary>
	/// 查詢Redis裡玩家在活動的總次數資訊
	/// </summary>
	/// <param name="getService"></param>
	/// <param name="promotionId"></param>
	/// <param name="playerCode"></param>
	/// <returns></returns>
	[HttpGet("total")]
	public async ValueTask<PlayerTotalViewModel> GetPlayerTotalInfoAsync(
		[FromServices] IGetService<GetPlayerTotalCountQuery, PlayerTotalInfo> getService,
		[FromQuery] Guid promotionId,
		[FromQuery] string playerCode)
	{
		var result = await getService
			.GetAsync(new GetPlayerTotalCountQuery(promotionId, playerCode), HttpContext.RequestAborted)
			.ConfigureAwait(false);

		return new PlayerTotalViewModel
		{
			Key = result.Key,
			Processed = result.Processed,
			Count = result.Count,
		};
	}

	/// <summary>
	/// 扣除Redis裡玩家在活動的每日次數
	/// </summary>
	/// <param name="untagService"></param>
	/// <param name="promotionId"></param>
	/// <param name="playerCode"></param>
	/// <returns></returns>
	[HttpDelete("cancel-tag/{promotionId}/{playerCode}")]
	public ValueTask CancelTagAsync(
		[FromServices] ICommandService<CancelPlayerDailyTagCommand> untagService,		
		Guid promotionId,
		string playerCode)
	{
		var command = new CancelPlayerDailyTagCommand(promotionId, playerCode);

		return untagService.ExecuteAsync(command, HttpContext.RequestAborted);
	}
}