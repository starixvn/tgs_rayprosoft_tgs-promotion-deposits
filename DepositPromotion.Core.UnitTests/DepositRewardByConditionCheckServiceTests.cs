using System.Collections.Immutable;
using Microsoft.Extensions.Logging.Abstractions;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Deposits;

namespace DepositPromotion.Core.UnitTests;

public class DepositRewardByConditionCheckServiceTests
{
    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_如果沒有取得活動設定會回傳False()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_如果沒有開限制會回傳True()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            null,
            null,
            null,
            null);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_如果參加條件有RealName而玩家沒有RealName會回傳False ()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            ["RealName"],
            null,
            null,
            null,
            null);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string> { }.ToImmutableDictionary());

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_每日次數超過設定值回傳False()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            1,
            null,
            null,
            null);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(Arg.Is(promotionId))
            .Returns(settings);

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(2);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_每週次數超過設定值回傳False()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            null,
            1,
            null,
            null);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(Arg.Is(promotionId))
            .Returns(settings);

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionWeeklyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_每月次數超過設定值回傳False()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            null,
            null,
            1,
            null);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(Arg.Is(promotionId))
            .Returns(settings);

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionMonthlyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_總計次數超過設定值回傳False()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            null,
            null,
            null,
            1);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(Arg.Is(promotionId))
            .Returns(settings);

        _ = fakeTgsRepository.GetDepositCountInfoAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(date.UtcDateTime))
            .Returns(new DepositCountInfo(date.UtcDateTime, 1));

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_總計次數查詢會先查詢FirstLevelRepository()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            null,
            null,
            null,
            1);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(Arg.Is(promotionId))
            .Returns(settings);

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionTotalCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode))
            .Returns(new DepositCountInfo(date.UtcDateTime, 1));
        _ = fakeTgsRepository.GetDepositCountInfoAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(date.UtcDateTime))
            .Returns(new DepositCountInfo(date.UtcDateTime, 1));

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);

        _ = fakeTgsRepository.DidNotReceive()
            .GetDepositCountInfoAsync(
                Arg.Is(promotionId),
                Arg.Is(playerCode),
                Arg.Is(date.UtcDateTime));
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_總計次數查詢查到的FirstLevel資料過時會去查DB更新資料()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            null,
            null,
            null,
            1);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionTotalCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode))
            .Returns(new DepositCountInfo(date.UtcDateTime.AddDays(-1D), 0));
        _ = fakeTgsRepository.GetDepositCountInfoAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(date.UtcDateTime))
            .Returns(new DepositCountInfo(date.UtcDateTime, 1));

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
        _ = fakeFirstLevelRepository.Received(1)
            .SetPlayerDepositByConditionTotalCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")),
            Arg.Any<DepositCountInfo>());
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_總計次數查詢查不到FirstLevel資料過時會去查DB更新資料()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            null,
            null,
            null,
            1);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        _ = fakeTgsRepository.GetDepositCountInfoAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(date.UtcDateTime))
            .Returns(new DepositCountInfo(date.UtcDateTime, 1));

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
        _ = fakeFirstLevelRepository.Received(1)
            .SetPlayerDepositByConditionTotalCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")),
            Arg.Any<DepositCountInfo>());
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_需要通過所有設定的檢查才會回傳True()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            null,
            2,
            2,
            2,
            2);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionWeeklyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionMonthlyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        _ = fakeTgsRepository.GetDepositCountInfoAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(date.UtcDateTime))
            .Returns(new DepositCountInfo(date.UtcDateTime, 1));

        _ = fakeFirstLevelRepository.GetPlayerDepositByConditionDailyCountAsync(
            Arg.Is(promotionId),
            Arg.Is(playerCode),
            Arg.Is(DateOnly.Parse("2024-03-25")))
            .Returns(1);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_標記使用活動()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        // Act
        await sut.TagDepositAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeFirstLevelRepository.Received(1)
            .IncrementPlayerDepositByConditionAsync(
                Arg.Is(promotionId),
                Arg.Is(playerCode),
                Arg.Is(DateOnly.Parse("2024-03-25")));
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_取消標記使用活動()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        // Act
        await sut.RollbackTagAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeFirstLevelRepository.Received(1)
            .DecrementPlayerDepositByConditionAsync(
                Arg.Is(promotionId),
                Arg.Is(playerCode),
                Arg.Is(DateOnly.Parse("2024-03-25")));
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_如果查不到玩家資料回傳False()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            ["RealName"],
            null,
            null,
            null,
            null);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(Arg.Is(promotionId))
            .Returns(settings);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_查詢玩家資料RealName是否有設定()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            ["RealName"],
            null,
            null,
            null,
            null);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                ["RealName"] = "測試者"
            }.ToImmutableDictionary());

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_查詢玩家資料Phone是否有設定()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            ["Phone"],
            null,
            null,
            null,
            null);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                ["AreaCode"] = "111",
                ["Phone"] = "1234567"
            }.ToImmutableDictionary());

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_查詢玩家資料Birthdate是否有設定()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            ["Birthdate"],
            null,
            null,
            null,
            null);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                ["Birthdate"] = "2024-03-28"
            }.ToImmutableDictionary());

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_查詢玩家資料的條件要全部通過才行()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository,
            fakePlayerServiceClient,
            NullLogger<DepositRewardByConditionCheckService>.Instance);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            ["RealName", "Phone", "Birthdate"],
            null,
            null,
            null,
            null);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(playerCode))
            .Returns(new PlayerInfo
            {
                PlayerCode = playerCode
            });

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionRule<DepositRewardByConditionSettings>>(Arg.Is(promotionId))
            .Returns(new PromotionRule<DepositRewardByConditionSettings>(settings));

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(new Dictionary<string, string>
            {
                ["RealName"] = "測試者",
                ["AreaCode"] = "111",
                ["Phone"] = "1234567",
                ["Birthdate"] = "2024-03-28"
            }.ToImmutableDictionary());

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.True(actual);
    }
}