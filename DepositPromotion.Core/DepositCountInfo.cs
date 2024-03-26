using System.Text.Json.Serialization;

namespace Tgs.Promotion.Deposits;
public record DepositCountInfo(
	DateTime Processed,
	int Count);
