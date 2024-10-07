using Google.Protobuf;
using TGS.Core.Infrastructures.Nats.Interface;
using Tgs.Promotion.Clients.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Promotion.Deposits.ApplicationServices;

internal class CancelPlayerDailyTagCommandService(IMessageManager messageSender)
	: ICommandService<CancelPlayerDailyTagCommand>
{
	public ValueTask ExecuteAsync(CancelPlayerDailyTagCommand command, CancellationToken cancellationToken = default)
	{
		var request = new CheckPromotionRequest
		{
			PlayerCode = command.PlayerCode,
			PromotionId = command.PromotionId,
		};

		return messageSender.PublishAsync(
			string.Format(PromotionCheckConsts.RollbackSubjectTemplate, "DepositRewardV2".ToLowerInvariant()),
			request.ToByteArray(),
			cancellationToken: cancellationToken);
	}
}