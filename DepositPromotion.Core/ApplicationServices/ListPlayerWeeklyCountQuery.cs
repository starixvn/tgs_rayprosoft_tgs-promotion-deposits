namespace Tgs.Promotion.Deposits.ApplicationServices;
public record ListPlayerWeeklyCountQuery(
	Guid PromotionId,
	string PlayerCode);