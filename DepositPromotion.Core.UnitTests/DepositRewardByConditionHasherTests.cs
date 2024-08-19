using Tgs.Promotion.Deposits;

namespace DepositPromotion.Core.UnitTests;

public class DepositRewardByConditionHasherTests
{
    [Fact]
    public void ComputeHash()
    {
        // Arrange
        var data = Guid.Parse("427fc749-8101-0000-0001-000071e942d8");
        var sut = new DepositRewardByConditionHasher();

        // Act
        var actual = sut.ComputeHash(data);

        // Assert
        Assert.Equal(1409717553195448819L, actual);
    }
}