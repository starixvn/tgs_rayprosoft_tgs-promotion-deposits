using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Calculate.Clients;
using Tgs.Promotion.Clients.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Promotion.Deposits.MessageQueue;

internal class DepositRewardByConditionSettingProcessor(
	ITgsRepository tgsRepository,
	ICalculateClient calculateClient,
	ILogger<DepositRewardByConditionSettingProcessor> logger)
	: IMessageProcessor<CampaignDetailRequest, CampaignDetailResponse>
{
	public async ValueTask<CampaignDetailResponse> HandleAsync(
		string subject,
		CampaignDetailRequest data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var calculateId = await tgsRepository.GetPromotionCalculateIdAsync(
			data.PromotionId,
			cancellationToken).ConfigureAwait(false);

		if (calculateId is null)
		{
			calculateId = Guid.NewGuid();

			_ = await calculateClient.CreateCalculateAsync(
				calculateId.Value,
				cancellationToken).ConfigureAwait(false);

			await tgsRepository.SetPromotionCalculateIdAsync(
				data.PromotionId,
				calculateId.Value,
				cancellationToken).ConfigureAwait(false);
		}

		var settings = JsonSerializer.Deserialize<DepositRewardByConditionSettings>(
			data.Settings,
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
