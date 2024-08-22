namespace Tgs.Promotion.Deposits.ApplicationServices;
public record GetPlayerTotalCountQuery(
	Guid PromotionId,
	string PlayerCode);