using System.Text.Json;
using ExpectedObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tgs.Calculate.Clients;
using Tgs.Promotion.Clients.Protos;
using Tgs.Promotion.Deposits.MessageQueue;
using Valhalla.MessageQueue;
using Xunit.Abstractions;

namespace Tgs.Promotion.Deposits.IntegrationTests;

public class DepositRewardByConditionTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task 整合測試_儲值送設定事件處理()
    {
        // Arrange
        var subject = $"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.setting";

        var fakeCalculateService = Substitute.For<ICalculateClient>();

        await using var web = PromotionDepositsTestApplication.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakeCalculateService)
                    .AddMessageQueue()
                    .AddDirectMessageQueue(directConfiguration => directConfiguration
                        .AddProcessor(
                            ProcessorTypes.DepositRewardByConditionSettingProcessorType,
                            subject))
                    .ClearAllExchanges()
                    .AddDirectGlobPatternExchange("*")));

        var messageSender = web.Services.GetRequiredService<IMessageSender>();

        var settings = new DepositRewardByConditionSettings(
            1M,
            2M,
            3M,
            [],
            null,
            4,
            5,
            6,
            7);

        var request = new CampaignDetailRequest
        {
            PromotionId = Guid.NewGuid(),
            Settings = """
            {
                "percentage": 1,
                "bonusAmountLimit": 2,
                "turnoverMultiple": 3,
                "vipLevels": [],
                "dailyDepositLimitByPlayer": 4,
                "weeklyDepositLimitByPlayer": 5,
                "monthlyDepositLimitByPlayer": 6,
                "totalDepositLimitByPlayer": 7
            }
            """
        };

        // Act
        var actual = await messageSender.RequestAsync<CampaignDetailRequest, CampaignDetailResponse>(subject, request);
        var actualSettings = JsonSerializer.Deserialize<DepositRewardByConditionSettings>(
            actual.CampaignDetail,
            PromotionUtility.SerializerOptions);

        // Assert
        Assert.NotNull(actualSettings);
        settings.ToExpectedObject(
            ctx => ctx.IgnoreEqualsOverride())
            .ShouldMatch(actualSettings);
    }
}
