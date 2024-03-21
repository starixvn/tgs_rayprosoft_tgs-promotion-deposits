using System.Text.Json;

namespace Tgs.Promotion.Deposits;

public static class PromotionUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};
}
