using Google.Protobuf;
using NATS.Client.Core;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Clients.Protos;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Promotion.Deposits.MessageQueue;

internal class DepositRewardByConditionFlowProcessor(
	IPromotionServiceClient promotionServiceClient,
	IBrandServiceClient brandServiceClient,
	IDepositRewardByConditionCheckService depositRewardByConditionCheckService,
	TimeProvider timeProvider)
	: IMessageReplyHandler<CheckPromotionRequest, ReadOnlyMemory<byte>>
{
	private static readonly ReadOnlyMemory<byte> _EmptyResponse = Array.Empty<byte>();

	public async Task<ReadOnlyMemory<byte>> Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders,
		CheckPromotionRequest? message,
		CancellationToken cancellationToken)
	{
		var action = subject.Split('.').Last();

		if (message.PromotionId == null || message.PromotionId.ToGuid() == Guid.Empty)
			return _EmptyResponse;

		var promotionInfo = await promotionServiceClient.GetPromotionMainInfoAsync(
			message.PromotionId.ToGuid(),
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
						message.PromotionId,
						message.PlayerCode,
						currentDate,
						cancellationToken).ConfigureAwait(false)
				}.ToByteArray();

			case "commit":
				return _EmptyResponse;

			case "rollback":
				await depositRewardByConditionCheckService.RollbackTagAsync(
					message.PromotionId,
					message.PlayerCode,
					currentDate,
					cancellationToken).ConfigureAwait(false);
				return _EmptyResponse;

			case "tag":
				await depositRewardByConditionCheckService.TagDepositAsync(
					message.PromotionId,
					message.PlayerCode,
					currentDate,
					cancellationToken).ConfigureAwait(false);
				return _EmptyResponse;
		}

		return _EmptyResponse;
	}
}