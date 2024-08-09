using System.Text.Json;
using StackExchange.Redis;
using Tgs.Promotion.Deposits;
using Tgs.Promotion.Deposits.Repositories;

namespace DepositPromotion.Repositories.UnitTests;

public class RedisFirstLevelRepositoryTests
{
    private static readonly RedisKey _KeyPrefix = "Tgs:Promotion:Deposits:";

    [Fact]
    public void IncrementPlayerDepositByConditionAsync_expiry計算()
    {
        var queryDate = new DateTimeOffset(2024, 8, 8, 13, 0, 0, TimeSpan.FromHours(8d));

        var promotionId = Guid.Parse("019121ad-6fae-0000-0001-07e8593a0694");
        var playerCode = "4umzzymax6vv";

        var date = DateOnly.FromDateTime(queryDate.DateTime);

        var dataKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");

        var expireDate = date.CeilingMonthDay().AddDays(1).ToDateTime(TimeOnly.MinValue);

        var expiry = expireDate - date.ToDateTime(TimeOnly.MinValue);

        Assert.Equal($"Tgs:Promotion:Deposits:DepositRewardV2:019121ad-6fae-0000-0001-07e8593a0694:4umzzymax6vv:Day:{date}", dataKey);
        Assert.Equal(new DateTime(2024, 9, 2, 0, 0, 0, DateTimeKind.Unspecified), expireDate);
        Assert.Equal(TimeSpan.FromDays(25), expiry);
    }

    [Fact]
    public void GetPlayerDepositByConditionTotalCountAsync_JsonSerializer_Deserialize()
    {
        var redisValue = """
            {    "processed": "2024-08-07T16:00:00Z",    "count": 1}
            """;

        var countInfo = JsonSerializer.Deserialize<DepositCountInfo>((string)redisValue!, PromotionUtility.SerializerOptions);

        Assert.NotNull(countInfo);
        Assert.Equal(new DateTime(2024, 8, 7, 16, 0, 0, DateTimeKind.Utc), countInfo.Processed);
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每日玩家儲值送活動存款次數()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");

        // Act
        _ = await sut.GetPlayerDepositByConditionDailyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringGetAsync(Arg.Is(expectedKey));
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得玩家儲值送活動總次數()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Total");

        // Act
        _ = await sut.GetPlayerDepositByConditionTotalCountAsync(promotionId, playerCode);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringGetAsync(Arg.Is(expectedKey));
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每週玩家儲值送活動存款次數()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Week:{DateOnly.Parse("2024-03-18")}");

        // Act
        _ = await sut.GetPlayerDepositByConditionWeeklyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringGetAsync(Arg.Is(expectedKey));
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每週玩家儲值送活動存款次數_資料庫沒有資料的話_會加總這週到昨天為止的每日總數()
    {
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Week:{DateOnly.Parse("2024-03-18")}");

        var keyFormat = "DepositRewardV2:{0}:{1}:Day:{2}";
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-18")))))
            .Returns(10L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-19")))))
            .Returns(20L);

        // Act
        var actual = await sut.GetPlayerDepositByConditionWeeklyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringGetAsync(Arg.Is(expectedKey));
        Assert.Equal(30L, actual);
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每週玩家儲值送活動存款次數_資料庫資料統計過時的話_會更新加總()
    {
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Week:{DateOnly.Parse("2024-03-18")}");

        _ = fakeRedisDatabase.StringGetAsync(Arg.Is(expectedKey))
            .Returns("""
            {
                "count": 10,
                "snapshot": "2024-03-18"
            }
            """);

        var keyFormat = "DepositRewardV2:{0}:{1}:Day:{2}";
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-18")))))
            .Returns(10L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-19")))))
            .Returns(20L);

        // Act
        var actual = await sut.GetPlayerDepositByConditionWeeklyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringGetAsync(Arg.Is(expectedKey));
        Assert.Equal(30L, actual);
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每週玩家儲值送活動存款次數_會將更新的資料存回資料庫()
    {
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedExpiry = DateOnly.Parse("2024-04-02").ToDateTime(TimeOnly.MinValue) - date.ToDateTime(TimeOnly.MinValue);
        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Week:{DateOnly.Parse("2024-03-18")}");

        _ = fakeRedisDatabase.StringGetAsync(Arg.Is(expectedKey))
            .Returns("""
            {
                "count": 10,
                "snapshot": "2024-03-18"
            }
            """);

        var keyFormat = "DepositRewardV2:{0}:{1}:Day:{2}";
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-18")))))
            .Returns(10L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-19")))))
            .Returns(20L);

        // Act
        _ = await sut.GetPlayerDepositByConditionWeeklyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringSetAsync(
                Arg.Is(expectedKey),
                Arg.Any<RedisValue>(),
                expiry: Arg.Is(expectedExpiry));
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每月玩家儲值送活動存款次數()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Month:{DateOnly.Parse("2024-03-01")}");

        // Act
        _ = await sut.GetPlayerDepositByConditionMonthlyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringGetAsync(Arg.Is(expectedKey));
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每月玩家儲值送活動存款次數_如果資料庫沒有資料的話_會加總月初到統計日前一天的每日次數()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-04");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Month:{DateOnly.Parse("2024-03-01")}");

        var keyFormat = "DepositRewardV2:{0}:{1}:Day:{2}";
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-01")))))
            .Returns(10L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-02")))))
            .Returns(20L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-03")))))
            .Returns(30L);

        // Act
        var actual = await sut.GetPlayerDepositByConditionMonthlyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringGetAsync(Arg.Is(expectedKey));
        Assert.Equal(60L, actual);
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每月玩家儲值送活動存款次數_如果資料庫資料統計過時_會更新加總()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-04");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Month:{DateOnly.Parse("2024-03-01")}");

        _ = fakeRedisDatabase.StringGetAsync(Arg.Is(expectedKey))
            .Returns("""
            {
                "count": 30,
                "snapshot": "2024-03-02"
            }
            """);

        var keyFormat = "DepositRewardV2:{0}:{1}:Day:{2}";
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-01")))))
            .Returns(10L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-02")))))
            .Returns(20L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-03")))))
            .Returns(30L);

        // Act
        var actual = await sut.GetPlayerDepositByConditionMonthlyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringGetAsync(Arg.Is(expectedKey));
        Assert.Equal(60L, actual);
    }

    [Fact]
    public async Task RedisFirstLevelRepository_取得每月玩家儲值送活動存款次數_當資料更新會存回資料庫()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-04");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Month:{DateOnly.Parse("2024-03-01")}");
        var expectedExpiry = DateOnly.Parse("2024-04-02").ToDateTime(TimeOnly.MinValue) - date.ToDateTime(TimeOnly.MinValue);

        _ = fakeRedisDatabase.StringGetAsync(Arg.Is(expectedKey))
            .Returns("""
            {
                "count": 30,
                "snapshot": "2024-03-02"
            }
            """);

        var keyFormat = "DepositRewardV2:{0}:{1}:Day:{2}";
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-01")))))
            .Returns(10L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-02")))))
            .Returns(20L);
        _ = fakeRedisDatabase.StringGetAsync(
            Arg.Is(_KeyPrefix.Append(string.Format(
                keyFormat,
                promotionId,
                playerCode,
                DateOnly.Parse("2024-03-03")))))
            .Returns(30L);

        // Act
        _ = await sut.GetPlayerDepositByConditionMonthlyCountAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringSetAsync(
                Arg.Is(expectedKey),
                Arg.Any<RedisValue>(),
                expiry: Arg.Is(expectedExpiry));
    }

    [Fact]
    public async Task RedisFirstLevelRepository_遞增每日玩家儲值送活動次數_第一次未設置TTL_會設置TTL()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");

        // Act
        await sut.IncrementPlayerDepositByConditionAsync(promotionId, playerCode, date);

        _ = fakeRedisDatabase.KeyTimeToLiveAsync(Arg.Is(expectedKey))
            .Returns(new TimeSpan?());

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringIncrementAsync(Arg.Is(expectedKey));

        var expireDate = date.CeilingMonthDay().AddDays(1).ToDateTime(TimeOnly.MinValue);
        var expiry = expireDate - date.ToDateTime(TimeOnly.MinValue);

        _ = fakeRedisDatabase.Received(1)
            .KeyExpireAsync(
                Arg.Is(expectedKey),
                Arg.Is(expiry));
    }

    [Fact]
    public async Task RedisFirstLevelRepository_遞增每日玩家儲值送活動次數_設置過TTL_則不再設置()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");

        _ = fakeRedisDatabase.KeyTimeToLiveAsync(Arg.Is(expectedKey))
            .Returns(TimeSpan.FromDays(1));

        // Act
        await sut.IncrementPlayerDepositByConditionAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringIncrementAsync(Arg.Is(expectedKey));

        _ = fakeRedisDatabase.Received(0)
            .KeyExpireAsync(
                Arg.Is(expectedKey),
                Arg.Any<TimeSpan?>());
    }

    [Fact]
    public async Task RedisFirstLevelRepository_遞減每日玩家儲值送活動次數()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");
        var expectedExpiry = DateOnly.Parse("2024-04-02").ToDateTime(TimeOnly.MinValue) - date.ToDateTime(TimeOnly.MinValue);

        // Act
        await sut.DecrementPlayerDepositByConditionAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringSetAsync(
                Arg.Is(expectedKey),
                Arg.Is<RedisValue>(1L),
                expiry: Arg.Is(expectedExpiry),
                when: Arg.Is(When.NotExists));
        _ = fakeRedisDatabase.Received(1)
            .StringDecrementAsync(Arg.Is(expectedKey));
    }

    [Fact]
    public async Task RedisFirstLevelRepository_遞減每日玩家儲值送活動次數_如果遞減後結果為負數會透過遞增補回零()
    {
        // Arrange
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var sut = new RedisFirstLevelRepository(fakeRedisDatabase);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateOnly.Parse("2024-03-20");

        var expectedKey = _KeyPrefix.Append($"DepositRewardV2:{promotionId}:{playerCode}:Day:{date}");
        var expectedExpiry = DateOnly.Parse("2024-04-02").ToDateTime(TimeOnly.MinValue) - date.ToDateTime(TimeOnly.MinValue);

        _ = fakeRedisDatabase.StringDecrementAsync(Arg.Is(expectedKey))
            .Returns(-1L);

        // Act
        await sut.DecrementPlayerDepositByConditionAsync(promotionId, playerCode, date);

        // Assert
        _ = fakeRedisDatabase.Received(1)
            .StringSetAsync(
                Arg.Is(expectedKey),
                Arg.Is<RedisValue>(1L),
                expiry: Arg.Is(expectedExpiry),
                when: Arg.Is(When.NotExists));
        _ = fakeRedisDatabase.Received(1)
            .StringIncrementAsync(Arg.Is(expectedKey));
    }
}