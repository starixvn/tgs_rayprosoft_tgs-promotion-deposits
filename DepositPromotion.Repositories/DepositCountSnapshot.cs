using System.Text.Json.Serialization;

namespace Tgs.Promotion.Deposits;
record DepositCountSnapshot(
	[property: JsonPropertyName("count")] long Count,
	[property: JsonPropertyName("snapshot")] DateOnly SnapshotDate);
