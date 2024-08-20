namespace Tgs.Promotion.Deposits.ApplicationServices;
public record ListPlayerDailyCountQuery(
	Guid PromotionId,
	string PlayerCode);