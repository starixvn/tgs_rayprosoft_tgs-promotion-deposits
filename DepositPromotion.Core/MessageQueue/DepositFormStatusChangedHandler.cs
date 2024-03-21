using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.Promotion.Deposits.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Promotion.Deposits.MessageQueue;

internal class DepositFormStatusChangedHandler(
	IPaymentServiceClient paymentServiceClient,
	ICommandService<DistributeDepositBonusCommand> distributeService)
	: IMessageHandler<FormEventEx>
{
	public async ValueTask HandleAsync(
		string subject,
		FormEventEx data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var previousStatus = Enum.Parse<PaymentFlowState>(data.Status);

		var depositForm = await paymentServiceClient.GetDepositFormAsync(
			data.FormId,
			cancellationToken).ConfigureAwait(false);

		if (depositForm is null || depositForm.PromotionId is null)
			return;

		if (previousStatus != depositForm.Status
			&& depositForm.Status == PaymentFlowState.Approved)
		{
			var command = new DistributeDepositBonusCommand(depositForm);

			await distributeService.ExecuteAsync(
				command,
				cancellationToken).ConfigureAwait(false);
		}
	}
}
