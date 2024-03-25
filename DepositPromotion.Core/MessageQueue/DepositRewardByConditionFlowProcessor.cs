using Google.Protobuf;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Clients.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Promotion.Deposits.MessageQueue;

internal class DepositRewardByConditionFlowProcessor(
	IPromotionServiceClient promotionServiceClient,
	IBrandServiceClient brandServiceClient,
	IDepositRewardByConditionCheckService depositRewardByConditionCheckService,
	TimeProvider timeProvider)
	: IMessageProcessor<CheckPromotionRequest, ReadOnlyMemory<byte>>
{
	private static readonly ReadOnlyMemory<byte> _EmptyResponse = Array.Empty<byte>();

	public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(
		string subject,
		CheckPromotionRequest data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var action = subject.Split('.').Last();
		var promotionInfo = await promotionServiceClient.GetPromotionMainInfoAsync(
			data.PromotionId,
			cancellationToken).ConfigureAwait(false);

		var brandInfo = await brandServiceClient.GetBrandInfoAsync(
			promotionInfo!.BrandId,
			cancellationToken).ConfigureAwait(false);
		var brandOffset = TimeSpan.FromMinutes(brandInfo!.TimeSpanMinutes ?? 480);
		var currentDate = new DateTimeOffset(timeProvider.GetUtcNow().ToOffset(brandOffset).Date, brandOffset);

		switch (action)
		{
			case "check":
				return new CheckPromotionResponse
				{
					Result = await depositRewardByConditionCheckService.CheckAsync(
						data.PromotionId,
						data.PlayerCode,
						currentDate,
						cancellationToken).ConfigureAwait(false)
				}.ToByteArray();

			case "commit":
				return _EmptyResponse;

			case "rollback":
				await depositRewardByConditionCheckService.RollbackTagAsync(
					data.PromotionId,
					data.PlayerCode,
					currentDate,
					cancellationToken).ConfigureAwait(false);
				return _EmptyResponse;

			case "tag":
				await depositRewardByConditionCheckService.TagDepositAsync(
					data.PromotionId,
					data.PlayerCode,
					currentDate,
					cancellationToken).ConfigureAwait(false);
				return _EmptyResponse;
		}

		return _EmptyResponse;
	}
}
