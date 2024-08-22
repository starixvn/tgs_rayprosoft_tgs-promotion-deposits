namespace Tgs.Promotion.Deposits.ApplicationServices;
public record CancelPlayerDailyTagCommand(
	Guid PromotionId,
	string PlayerCode);