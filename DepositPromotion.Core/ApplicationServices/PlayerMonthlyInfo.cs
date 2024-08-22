using System.Text.Json;

namespace Tgs.Promotion.Deposits.ApplicationServices;
public record PlayerMonthlyInfo(
	string Key,
	JsonDocument Info);