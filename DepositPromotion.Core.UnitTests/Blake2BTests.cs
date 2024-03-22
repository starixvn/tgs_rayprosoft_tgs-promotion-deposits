using System.Runtime.InteropServices;
using Isopoh.Cryptography.Blake2b;
using Isopoh.Cryptography.SecureArray;

namespace DepositPromotion.Core.UnitTests;

public class Blake2BTests
{
    [Fact]
    public void Blake2B_對Guid產生64位元雜湊()
    {
        // Arrange
        var data = Guid.NewGuid();
        var config = new Blake2BConfig()
        {
            OutputSizeInBits = 64
        };
        var sut = Blake2B.Create(config, SecureArray.DefaultCall);

        // Act
        var actual = sut.AsHashAlgorithm().ComputeHash(data.ToByteArray());

        // Assert
        Assert.Equal(8, actual.Length);
    }

    [Fact]
    public void Blake2B_雜湊值運算結果會永遠一樣()
    {
        // Arrange
        var data = Guid.NewGuid();
        var config = new Blake2BConfig()
        {
            OutputSizeInBits = 64
        };
        var sut = Blake2B.Create(config, SecureArray.DefaultCall);

        // Act
        var hash1 = sut.AsHashAlgorithm().ComputeHash(data.ToByteArray());
        var actual1 = MemoryMarshal.AsRef<long>(hash1);
        var hash2 = sut.AsHashAlgorithm().ComputeHash(data.ToByteArray());
        var actual2 = MemoryMarshal.AsRef<long>(hash2);

        // Assert
        Assert.Equal(actual1, actual2);
    }

    [Fact]
    public void Blake2B_同樣設定產生的Hasher對同樣的資料要產生同樣的雜湊()
    {
        // Arrange
        var data = Guid.NewGuid();
        var config1 = new Blake2BConfig()
        {
            OutputSizeInBits = 64
        };
        var config2 = new Blake2BConfig()
        {
            OutputSizeInBits = 64
        };
        var sut1 = Blake2B.Create(config1, SecureArray.DefaultCall);
        var sut2 = Blake2B.Create(config2, SecureArray.DefaultCall);

        // Act
        var hash1 = sut1.AsHashAlgorithm().ComputeHash(data.ToByteArray());
        var actual1 = MemoryMarshal.AsRef<long>(hash1);

        var hash2 = sut2.AsHashAlgorithm().ComputeHash(data.ToByteArray());
        var actual2 = MemoryMarshal.AsRef<long>(hash2);

        // Assert
        Assert.Equal(actual1, actual2);
    }
}
