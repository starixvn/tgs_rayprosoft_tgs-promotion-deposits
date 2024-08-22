using Google.Protobuf;
using Tgs.Promotion.Clients.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Promotion.Deposits.ApplicationServices;

internal class CancelPlayerDailyTagCommandService(IMessageSender messageSender)
	: ICommandService<CancelPlayerDailyTagCommand>
{
	public ValueTask ExecuteAsync(CancelPlayerDailyTagCommand command, CancellationToken cancellationToken = default)
	{
		var request = new CheckPromotionRequest
		{
			PlayerCode = command.PlayerCode,
			PromotionId = command.PromotionId,
		};

		return messageSender.SendAsync(
			string.Format(PromotionCheckConsts.RollbackSubjectTemplate, "DepositRewardV2".ToLowerInvariant()),
			request.ToByteArray(),
			cancellationToken);
	}
}