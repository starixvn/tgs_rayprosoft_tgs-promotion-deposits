using System.Text.Json;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using Tgs.Promotion.Clients;
using TGS.Core.Infrastructures.Nats.Interface;

namespace Tgs.Promotion.Deposits.MessageQueue;

public class DetailReplyProcessor(ILogger<DetailReplyProcessor> logger, IPromotionServiceClient promotionServiceClient)
    : IMessageReplyHandler<DistributedDetailRequest, DistributedDetailResponse>
{
    public async Task<DistributedDetailResponse> Handle(string subject, string? queueGroup,
        NatsHeaders? natsHeaders, DistributedDetailRequest? message,
        CancellationToken cancellationToken)
	{
		var distribution =
			await promotionServiceClient.GetPromotionDistributeByReferenceIdAsync(message.ReferenceId,
				cancellationToken).ConfigureAwait(false);

		var response = new DistributedDetailResponse
		{
			Detail = distribution != null
				? JsonSerializer.Serialize(
					new
					{
						distribution.BonusAmount
					},
					PromotionUtility.SerializerOptions)
				: null
		};

		return response;
	}
}