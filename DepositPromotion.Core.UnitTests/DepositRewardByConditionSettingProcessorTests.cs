using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Calculate.Clients;
using Tgs.Promotion.Clients.Protos;
using Tgs.Promotion.Deposits;
using Tgs.Promotion.Deposits.MessageQueue;

namespace DepositPromotion.Core.UnitTests;

public class DepositRewardByConditionSettingProcessorTests
{
    [Fact]
    public async Task DepositRewardByConditionSettingProcessor_處理設定值變更要求()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeCalculateClient = Substitute.For<ICalculateClient>();

        var sut = new DepositRewardByConditionSettingProcessor(
            fakeTgsRepository,
            fakeCalculateClient,
            NullLogger<DepositRewardByConditionSettingProcessor>.Instance);

        var request = new CampaignDetailRequest
        {
            PromotionId = Guid.NewGuid(),
            Settings = "{}"
        };

        // Act
        var actual = await sut.Handle("test", "test", null, request, default);

        // Assert
        Assert.NotNull(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionSettingProcessor_處理設定值變更要求_如果還未建立累計值會進行建立()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeCalculateClient = Substitute.For<ICalculateClient>();

        var sut = new DepositRewardByConditionSettingProcessor(
            fakeTgsRepository,
            fakeCalculateClient,
            NullLogger<DepositRewardByConditionSettingProcessor>.Instance);

        var promotionId = Guid.NewGuid();
        var request = new CampaignDetailRequest
        {
            PromotionId = promotionId,
            Settings = "{}"
        };

        // Act
        _ = await sut.Handle("test", "test", null, request, default);

        // Assert
        _ = fakeCalculateClient.Received(1)
            .CreateCalculateAsync(Arg.Any<Guid>());
        _ = fakeTgsRepository.Received(1)
            .SetPromotionCalculateIdAsync(Arg.Is(promotionId), Arg.Any<Guid>());
    }

    [Fact]
    public async Task DepositRewardByConditionSettingProcessor_處理設定值變更要求_如果已經建立累計值_不會再進行建立()
    {
        // Arrange
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeCalculateClient = Substitute.For<ICalculateClient>();

        var sut = new DepositRewardByConditionSettingProcessor(
            fakeTgsRepository,
            fakeCalculateClient,
            NullLogger<DepositRewardByConditionSettingProcessor>.Instance);

        var promotionId = Guid.NewGuid();
        var request = new CampaignDetailRequest
        {
            PromotionId = promotionId,
            Settings = "{}"
        };

        _ = fakeTgsRepository.GetPromotionCalculateIdAsync(Arg.Is(promotionId))
            .Returns(Guid.NewGuid());

        // Act
        _ = await sut.Handle("test", "test", null, request, default);

        // Assert
        _ = fakeCalculateClient.Received(0)
            .CreateCalculateAsync(Arg.Any<Guid>());
        _ = fakeTgsRepository.Received(0)
            .SetPromotionCalculateIdAsync(Arg.Is(promotionId), Arg.Any<Guid>());
    }
}
