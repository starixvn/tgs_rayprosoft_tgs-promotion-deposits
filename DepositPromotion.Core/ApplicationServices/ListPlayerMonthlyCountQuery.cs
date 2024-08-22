namespace Tgs.Promotion.Deposits.ApplicationServices;
public record ListPlayerMonthlyCountQuery(
	Guid PromotionId,
	string PlayerCode);