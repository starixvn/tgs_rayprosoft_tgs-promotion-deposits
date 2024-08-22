using System.Text.Json;

namespace Tgs.Promotion.Deposits.ApplicationServices;
public record PlayerWeeklyInfo(
	string Key,
	JsonDocument Info);