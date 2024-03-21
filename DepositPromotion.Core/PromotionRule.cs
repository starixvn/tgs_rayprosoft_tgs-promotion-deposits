using System.Text.Json.Serialization;

namespace Tgs.Promotion.Deposits;
public record PromotionRule<TSettings>(
	[property: JsonPropertyName("compaignDetail")] TSettings CampaignDetail);
