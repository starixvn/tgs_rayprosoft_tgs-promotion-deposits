using System.Text.Json;
using ExpectedObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Tgs.BrandCertificate.Clients;
using Tgs.Calculate.Clients;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
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
            ["RealName", "Birthdate"],
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
                "playerConditions": ["RealName", "Birthdate"],
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

    [Fact]
    public async Task 整合測試_儲值送檢查事件處理()
    {
        // Arrange
        var subject = $"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.check";

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeTimeProvider = Substitute.For<TimeProvider>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        await using var web = PromotionDepositsTestApplication.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePromotionServiceClient)
                    .AddSingleton(fakeBrandServiceClient)
                    .AddSingleton(fakeTimeProvider)
                    .AddSingleton(fakePlayerServiceClient)
                    .AddKeyedSingleton("Redis", fakeRedisDatabase)
                    .AddMessageQueue()
                    .AddDirectMessageQueue(directConfiguration => directConfiguration
                        .AddProcessor(
                            ProcessorTypes.DepositRewardByConditionFlowProcessorType,
                            subject))
                    .ClearAllExchanges()
                    .AddDirectGlobPatternExchange("*")));

        var messageSender = web.Services.GetRequiredService<IMessageSender>();

        var promotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var currentTime = DateTimeOffset.ParseExact(
            "2024-03-25T13:27:00.0000000+08:00",
            "o",
            null).ToUniversalTime();

        var request = new CheckPromotionRequest
        {
            PromotionId = promotionId,
            PlayerCode = "test1"
        };

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(
            Arg.Is(request.PlayerCode),
            Arg.Any<CancellationToken>())
            .Returns(new PlayerInfo
            {
                PlayerCode = request.PlayerCode
            });

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = promotionId,
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 480
            });

        _ = fakeTimeProvider.GetUtcNow()
            .Returns(currentTime);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(new DepositRewardByConditionSettings(
                1,
                2,
                3,
                [],
                null,
                null,
                null,
                null,
                null,
                null));
        // Act
        var response = await messageSender.RequestAsync<CheckPromotionRequest, ReadOnlyMemory<byte>>(subject, request);
        var actual = CheckPromotionResponse.Parser.ParseFrom(response.Span);

        // Assert
        Assert.True(actual.Result);
    }

    [Fact]
    public async Task 整合測試_儲值送標記事件處理()
    {
        // Arrange
        var subject = $"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.tag";

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeTimeProvider = Substitute.For<TimeProvider>();
        var fakeCheckService = Substitute.For<IDepositRewardByConditionCheckService>();

        await using var web = PromotionDepositsTestApplication.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePromotionServiceClient)
                    .AddSingleton(fakeBrandServiceClient)
                    .AddSingleton(fakeTimeProvider)
                    .AddSingleton(fakeCheckService)
                    .AddMessageQueue()
                    .AddDirectMessageQueue(directConfiguration => directConfiguration
                        .AddHandler(
                            ProcessorTypes.DepositRewardByConditionFlowProcessorType,
                            subject))
                    .ClearAllExchanges()
                    .AddDirectGlobPatternExchange("*")));

        var messageSender = web.Services.GetRequiredService<IMessageSender>();

        var promotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var currentTime = DateTimeOffset.ParseExact(
            "2024-03-25T13:27:00.0000000+08:00",
            "o",
            null).ToUniversalTime();

        var request = new CheckPromotionRequest
        {
            PromotionId = promotionId,
            PlayerCode = "test1"
        };

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = promotionId,
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 480
            });

        _ = fakeTimeProvider.GetUtcNow()
            .Returns(currentTime);

        // Act
        await messageSender.SendAsync(subject, request);

        // Assert
        _ = fakeCheckService.Received(1)
            .TagDepositAsync(
                Arg.Is(promotionId),
                Arg.Is(request.PlayerCode),
                Arg.Is(DateTimeOffset.ParseExact(
                    "2024-03-25T00:00:00.0000000+08:00",
                    "o",
                    null)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_儲值送取消標記事件處理()
    {
        // Arrange
        var subject = $"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.rollback";

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeTimeProvider = Substitute.For<TimeProvider>();
        var fakeCheckService = Substitute.For<IDepositRewardByConditionCheckService>();

        await using var web = PromotionDepositsTestApplication.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePromotionServiceClient)
                    .AddSingleton(fakeBrandServiceClient)
                    .AddSingleton(fakeTimeProvider)
                    .AddSingleton(fakeCheckService)
                    .AddMessageQueue()
                    .AddDirectMessageQueue(directConfiguration => directConfiguration
                        .AddHandler(
                            ProcessorTypes.DepositRewardByConditionFlowProcessorType,
                            subject))
                    .ClearAllExchanges()
                    .AddDirectGlobPatternExchange("*")));

        var messageSender = web.Services.GetRequiredService<IMessageSender>();

        var promotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var currentTime = DateTimeOffset.ParseExact(
            "2024-03-25T13:27:00.0000000+08:00",
            "o",
            null).ToUniversalTime();

        var request = new CheckPromotionRequest
        {
            PromotionId = promotionId,
            PlayerCode = "test1"
        };

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = promotionId,
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 480
            });

        _ = fakeTimeProvider.GetUtcNow()
            .Returns(currentTime);

        // Act
        await messageSender.SendAsync(subject, request);

        // Assert
        _ = fakeCheckService.Received(1)
            .RollbackTagAsync(
                Arg.Is(promotionId),
                Arg.Is(request.PlayerCode),
                Arg.Is(DateTimeOffset.ParseExact(
                    "2024-03-25T00:00:00.0000000+08:00",
                    "o",
                    null)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 整合測試_儲值送Commit事件_什麼都不會做()
    {
        // Arrange
        var subject = $"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.commit";

        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeTimeProvider = Substitute.For<TimeProvider>();
        var fakeCheckService = Substitute.For<IDepositRewardByConditionCheckService>();

        await using var web = PromotionDepositsTestApplication.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePromotionServiceClient)
                    .AddSingleton(fakeBrandServiceClient)
                    .AddSingleton(fakeTimeProvider)
                    .AddSingleton(fakeCheckService)
                    .AddMessageQueue()
                    .AddDirectMessageQueue(directConfiguration => directConfiguration
                        .AddHandler(
                            ProcessorTypes.DepositRewardByConditionFlowProcessorType,
                            subject))
                    .ClearAllExchanges()
                    .AddDirectGlobPatternExchange("*")));

        var messageSender = web.Services.GetRequiredService<IMessageSender>();

        var promotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var currentTime = DateTimeOffset.ParseExact(
            "2024-03-25T13:27:00.0000000+08:00",
            "o",
            null).ToUniversalTime();

        var request = new CheckPromotionRequest
        {
            PromotionId = promotionId,
            PlayerCode = "test1"
        };

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(
            Arg.Is(promotionId),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = promotionId,
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            Arg.Is(brandId),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 480
            });

        _ = fakeTimeProvider.GetUtcNow()
            .Returns(currentTime);

        // Act
        await messageSender.SendAsync(subject, request);
    }
}
