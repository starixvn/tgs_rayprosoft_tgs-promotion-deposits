using Xunit;

namespace DepositPromotion.Core.UnitTests;

public class DateTimeTests
{
    [Fact]
    public void DateOnly_取得一週的第一天()
    {
        // Arrange
        var date = DateOnly.Parse("2024-03-20");

        var expected = DateOnly.Parse("2024-03-18");

        // Act
        var actual = date.FloorWeekDay();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DateOnly_取得一週的第一天_當日期就是第一天時_回傳結果會相同()
    {
        // Arrange
        var date = DateOnly.Parse("2024-03-18");

        var expected = DateOnly.Parse("2024-03-18");

        // Act
        var actual = date.FloorWeekDay();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DateOnly_取得下週的第一天()
    {
        // Arrange
        var date = DateOnly.Parse("2024-03-20");

        var expected = DateOnly.Parse("2024-03-25");

        // Act
        var actual = date.CeilingWeekDay();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DateOnly_取得該月的第一天()
    {
        // Arrange
        var date = DateOnly.Parse("2024-03-20");

        var expected = DateOnly.Parse("2024-03-01");

        // Act
        var actual = date.FloorMonthDay();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DateOnly_取得該月的第一天_如果日期就是第一天_回傳結果相同()
    {
        // Arrange
        var date = DateOnly.Parse("2024-03-01");

        var expected = DateOnly.Parse("2024-03-01");

        // Act
        var actual = date.FloorMonthDay();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DateOnly_取得下月的第一天()
    {
        // Arrange
        var date = DateOnly.Parse("2024-03-20");

        var expected = DateOnly.Parse("2024-04-01");

        // Act
        var actual = date.CeilingMonthDay();

        // Assert
        Assert.Equal(expected, actual);
    }
}
