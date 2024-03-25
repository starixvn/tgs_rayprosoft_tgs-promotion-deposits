using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Deposits;
using Xunit;

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

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

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

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

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
            null);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(Arg.Is(promotionId))
            .Returns(settings);

        // Act
        var actual = await sut.CheckAsync(promotionId, playerCode, date);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_每日次數超過設定值回傳False()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
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

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

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

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

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

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

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
    public async Task DepositRewardByConditionCheckService_檢查是否可以使用儲值送活動_需要通過所有設定的檢查才會回傳True()
    {
        // Arrange
        var fakeFirstLevelRepository = Substitute.For<IFirstLevelRepository>();
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact("2024-03-25T00:00:00.0000000+08:00", "o", null);

        var settings = new DepositRewardByConditionSettings(
            1M,
            1M,
            10M,
            [],
            null,
            2,
            2,
            2,
            2);

        _ = fakePromotionServiceClient.GetPromotionRuleAsync<DepositRewardByConditionSettings>(Arg.Is(promotionId))
            .Returns(settings);

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

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

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

        var sut = new DepositRewardByConditionCheckService(
            fakeFirstLevelRepository,
            fakePromotionServiceClient,
            fakeTgsRepository);

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
}
