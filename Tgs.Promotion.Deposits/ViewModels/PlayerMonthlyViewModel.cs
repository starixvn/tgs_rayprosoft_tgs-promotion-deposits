using System.Text.Json;

namespace Tgs.Promotion.Deposits.ViewModels;

public class PlayerMonthlyViewModel
{
	public string Key { get; set; } = default!;
	public JsonDocument Info { get; set; } = default!;
}