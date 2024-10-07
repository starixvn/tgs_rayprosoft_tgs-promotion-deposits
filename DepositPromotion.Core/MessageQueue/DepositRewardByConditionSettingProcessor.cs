using System.Text.Json;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using Tgs.Calculate.Clients;
using Tgs.Promotion.Clients.Protos;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Promotion.Deposits.MessageQueue;

internal class DepositRewardByConditionSettingProcessor(
	ITgsRepository tgsRepository,
	ICalculateClient calculateClient,
	ILogger<DepositRewardByConditionSettingProcessor> logger)
	: IMessageReplyHandler<CampaignDetailRequest, CampaignDetailResponse>
{
	public async Task<CampaignDetailResponse> Handle(string subject, string? queueGroup, NatsHeaders? natsHeaders,
		CampaignDetailRequest? message,
		CancellationToken cancellationToken)
	{
		var calculateId = await tgsRepository.GetPromotionCalculateIdAsync(
			message.PromotionId,
			cancellationToken).ConfigureAwait(false);

		if (calculateId is null)
		{
			calculateId = Guid.NewGuid();

			_ = await calculateClient.CreateCalculateAsync(
				calculateId.Value,
				cancellationToken).ConfigureAwait(false);

			await tgsRepository.SetPromotionCalculateIdAsync(
				message.PromotionId,
				calculateId.Value,
				cancellationToken).ConfigureAwait(false);
		}

		var settings = JsonSerializer.Deserialize<DepositRewardByConditionSettings>(
			               message.Settings,
			               PromotionUtility.SerializerOptions)
		               ?? throw new NullReferenceException("Deserialize DepositRewardSetting is null");

		logger.LogDebug("Account deposit setting : {settings}", settings);

		settings = settings with
		{
			BucketTypes = settings.BucketTypes?.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray(),
			PlayerConditions = settings.PlayerConditions?.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray()
		};

		var response = new CampaignDetailResponse
		{
			CampaignDetail = JsonSerializer.Serialize(
				settings,
				PromotionUtility.SerializerOptions),
			BonusAmountLimit = settings.BonusAmountLimit,
			Percentage = settings.Percentage,
			TurnoverMultiple = settings.TurnoverMultiple
		};

		return response;
	}
}
