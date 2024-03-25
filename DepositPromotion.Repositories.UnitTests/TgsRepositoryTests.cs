using Microsoft.EntityFrameworkCore;
using Tgs.Promotion.Deposits.Repositories;
using Tgs.Promotion.Deposits.Repositories.Entities;
using Xunit;

namespace DepositPromotion.Repositories.UnitTests;

public class TgsRepositoryTests
{
    [Fact]
    public async Task TgsRepository_取得累計發放金額Id()
    {
        // Arrange
        var dbContext = new TgsContext(new DbContextOptionsBuilder<TgsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var sut = new TgsRepository(dbContext);

        var calculateId = Guid.NewGuid();
        var promotionId = Guid.NewGuid();

        _ = await dbContext.DepositPromotionCalculatePairs.AddAsync(new DepositPromotionCalculatePair
        {
            PromotionId = promotionId,
            CalculateId = calculateId
        });

        _ = await dbContext.SaveChangesAsync();

        // Act
        var actual = await sut.GetPromotionCalculateIdAsync(promotionId);

        // Assert
        Assert.Equal(calculateId, actual);
    }

    [Fact]
    public async Task TgsRepository_設定活動的累計發放金額Id()
    {
        // Arrange
        var dbContext = new TgsContext(new DbContextOptionsBuilder<TgsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var sut = new TgsRepository(dbContext);

        var calculateId = Guid.NewGuid();
        var promotionId = Guid.NewGuid();

        // Act
        await sut.SetPromotionCalculateIdAsync(promotionId, calculateId);
        var actual = await dbContext.DepositPromotionCalculatePairs.FindAsync(promotionId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(calculateId, actual.CalculateId);
    }

    [Fact]
    public async Task TgsRepository_取得玩家儲值送活動存款總次數_會統計活動到前一天的總發放筆數()
    {
        // Arrange
        var dbContext = new TgsContext(new DbContextOptionsBuilder<TgsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var sut = new TgsRepository(dbContext);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact(
            "2024-03-25T00:00:00.0000000+08:00",
            "o",
            null).UtcDateTime;

        await dbContext.PromotionDistributes.AddRangeAsync(
            [
                new PromotionDistribute{
                    Id = Guid.NewGuid(),
                    PromotionId = promotionId,
                    PlayerCode = playerCode,
                    CreatedOn = date.AddDays(-3D)
                },
                new PromotionDistribute{
                    Id = Guid.NewGuid(),
                    PromotionId = promotionId,
                    PlayerCode = playerCode,
                    CreatedOn = date.AddDays(-1D)
                },
                new PromotionDistribute{
                    Id = Guid.NewGuid(),
                    PromotionId = promotionId,
                    PlayerCode = playerCode,
                    CreatedOn = date
                }
            ]);
        _ = await dbContext.SaveChangesAsync();

        // Act
        var actual = await sut.GetDepositCountInfoAsync(promotionId, playerCode, date);
        var actualRow = await dbContext.DepositPromotionStatistics.FirstAsync();

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.NotNull(actualRow);
        Assert.Equal(date, actualRow.ProcessedDate);
        Assert.Equal(2, actualRow.TotalDepositCount);
    }

    [Fact]
    public async Task TgsRepository_取得玩家儲值送活動存款總次數_如果統計資料過時會查詢差異資料來更新()
    {
        // Arrange
        var dbContext = new TgsContext(new DbContextOptionsBuilder<TgsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var sut = new TgsRepository(dbContext);

        var promotionId = Guid.NewGuid();
        var playerCode = "test1";
        var date = DateTimeOffset.ParseExact(
            "2024-03-25T00:00:00.0000000+08:00",
            "o",
            null).UtcDateTime;

        await dbContext.PromotionDistributes.AddRangeAsync(
            [
                new PromotionDistribute{
                    Id = Guid.NewGuid(),
                    PromotionId = promotionId,
                    PlayerCode = playerCode,
                    CreatedOn = date.AddDays(-3D)
                },
                new PromotionDistribute{
                    Id = Guid.NewGuid(),
                    PromotionId = promotionId,
                    PlayerCode = playerCode,
                    CreatedOn = date.AddDays(-2D)
                },
                new PromotionDistribute{
                    Id = Guid.NewGuid(),
                    PromotionId = promotionId,
                    PlayerCode = playerCode,
                    CreatedOn = date.AddDays(-1D)
                }
            ]);

        _ = await dbContext.DepositPromotionStatistics.AddAsync(
            new DepositPromotionStatistic
            {
                PromotionId = promotionId,
                PlayerCode = playerCode,
                ProcessedDate = date.AddDays(-2D),
                TotalDepositCount = 1
            });

        _ = await dbContext.SaveChangesAsync();

        // Act
        var actual = await sut.GetDepositCountInfoAsync(promotionId, playerCode, date);
        var actualRow = await dbContext.DepositPromotionStatistics.FirstAsync();

        // Assert
        Assert.Equal(3, actual.Count);
        Assert.NotNull(actualRow);
        Assert.Equal(date, actualRow.ProcessedDate);
        Assert.Equal(3, actualRow.TotalDepositCount);
    }
}
