namespace Tgs.Promotion.Deposits.ApplicationServices;
public record PlayerTotalInfo(
	string? Key,
	DateTime? Processed,
	int? Count);