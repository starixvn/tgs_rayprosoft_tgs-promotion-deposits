using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Clients.Protos;
using Tgs.Promotion.Deposits;
using Tgs.Promotion.Deposits.MessageQueue;
using Xunit;

namespace DepositPromotion.Core.UnitTests;

public class DepositRewardByConditionFlowProcessorTests
{
    [Fact]
    public async Task DepositRewardByConditionFlowProcessor_檢查玩家是否超過儲值限制()
    {
        // Arrange
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeDepositByConditionCheckService = Substitute.For<IDepositRewardByConditionCheckService>();

        var sut = new DepositRewardByConditionFlowProcessor(
            fakePromotionServiceClient,
            fakeBrandServiceClient,
            fakeDepositByConditionCheckService,
            TimeProvider.System);

        var request = new CheckPromotionRequest
        {
            PlayerCode = "test1",
            PromotionId = Guid.NewGuid()
        };
        var brandId = Guid.NewGuid();

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(Arg.Is(request.PromotionId.ToGuid()))
            .Returns(new PromotionMainInfo
            {
                Id = request.PromotionId,
                PromotionType = DepositPromotionConsts.DepositRewardByConditionTypeName,
                EventDuring = (DateTime.Now.AddDays(-1D), DateTime.Now.AddDays(1D)),
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo
            {
                Id = brandId
            });

        _ = fakeDepositByConditionCheckService.CheckAsync(
            Arg.Is(request.PromotionId.ToGuid()),
            Arg.Is(request.PlayerCode),
            Arg.Any<DateTimeOffset>())
            .Returns(true);

        // Act
        var response = await sut.HandleAsync("check", request, null);
        var actual = CheckPromotionResponse.Parser.ParseFrom(response.Span);

        // Assert
        Assert.True(actual.Result);
    }

    [Fact]
    public async Task DepositRewardByConditionFlowProcessor_檢查玩家是否超過儲值限制_會使用Brand時區設定()
    {
        // Arrange
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeDepositByConditionCheckService = Substitute.For<IDepositRewardByConditionCheckService>();
        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var sut = new DepositRewardByConditionFlowProcessor(
            fakePromotionServiceClient,
            fakeBrandServiceClient,
            fakeDepositByConditionCheckService,
            fakeTimeProvider);

        var request = new CheckPromotionRequest
        {
            PlayerCode = "test1",
            PromotionId = Guid.NewGuid()
        };
        var brandId = Guid.NewGuid();
        var currentTime = DateTimeOffset.Parse("2024-03-18T23:00:00.0000000Z");
        var offset = TimeSpan.FromHours(2D);

        _ = fakeTimeProvider.GetUtcNow()
            .Returns(currentTime);

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(Arg.Is(request.PromotionId.ToGuid()))
            .Returns(new PromotionMainInfo
            {
                Id = request.PromotionId,
                PromotionType = DepositPromotionConsts.DepositRewardByConditionTypeName,
                EventDuring = (DateTime.Now.AddDays(-1D), DateTime.Now.AddDays(1D)),
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = (int)offset.TotalMinutes
            });

        _ = fakeDepositByConditionCheckService.CheckAsync(
            Arg.Is(request.PromotionId.ToGuid()),
            Arg.Is(request.PlayerCode),
            Arg.Is(DateTimeOffset.ParseExact("2024-03-19T00:00:00.0000000+02:00", "o", null)))
            .Returns(true);

        // Act
        var response = await sut.HandleAsync("check", request, null);
        var actual = CheckPromotionResponse.Parser.ParseFrom(response.Span);

        // Assert
        Assert.True(actual.Result);
    }

    [Fact]
    public async Task DepositRewardByConditionFlowProcessor_檢查玩家是否超過儲值限制_如果Brand沒有設定時區會用預設加8時區()
    {
        // Arrange
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeDepositByConditionCheckService = Substitute.For<IDepositRewardByConditionCheckService>();
        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var sut = new DepositRewardByConditionFlowProcessor(
            fakePromotionServiceClient,
            fakeBrandServiceClient,
            fakeDepositByConditionCheckService,
            fakeTimeProvider);

        var request = new CheckPromotionRequest
        {
            PlayerCode = "test1",
            PromotionId = Guid.NewGuid()
        };
        var brandId = Guid.NewGuid();
        var currentTime = DateTimeOffset.Parse("2024-03-18T16:00:00.0000000Z");

        _ = fakeTimeProvider.GetUtcNow()
            .Returns(currentTime);

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(Arg.Is(request.PromotionId.ToGuid()))
            .Returns(new PromotionMainInfo
            {
                Id = request.PromotionId,
                PromotionType = DepositPromotionConsts.DepositRewardByConditionTypeName,
                EventDuring = (DateTime.Now.AddDays(-1D), DateTime.Now.AddDays(1D)),
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo
            {
                Id = brandId
            });

        _ = fakeDepositByConditionCheckService.CheckAsync(
            Arg.Is(request.PromotionId.ToGuid()),
            Arg.Is(request.PlayerCode),
            Arg.Is(DateTimeOffset.ParseExact("2024-03-19T00:00:00.0000000+08:00", "o", null)))
            .Returns(true);

        // Act
        var response = await sut.HandleAsync("check", request, null);
        var actual = CheckPromotionResponse.Parser.ParseFrom(response.Span);

        // Assert
        Assert.True(actual.Result);
    }

    [Fact]
    public async Task DepositRewardByConditionFlowProcessor_處理玩家存款累計標記事件()
    {
        // Arrange
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeDepositByConditionCheckService = Substitute.For<IDepositRewardByConditionCheckService>();

        var sut = new DepositRewardByConditionFlowProcessor(
            fakePromotionServiceClient,
            fakeBrandServiceClient,
            fakeDepositByConditionCheckService,
            TimeProvider.System);

        var request = new CheckPromotionRequest
        {
            PlayerCode = "test1",
            PromotionId = Guid.NewGuid()
        };
        var brandId = Guid.NewGuid();

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(Arg.Is(request.PromotionId.ToGuid()))
            .Returns(new PromotionMainInfo
            {
                Id = request.PromotionId,
                PromotionType = DepositPromotionConsts.DepositRewardByConditionTypeName,
                EventDuring = (DateTime.Now.AddDays(-1D), DateTime.Now.AddDays(1D)),
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo
            {
                Id = brandId
            });

        _ = fakeDepositByConditionCheckService.CheckAsync(
            Arg.Is(request.PromotionId.ToGuid()),
            Arg.Is(request.PlayerCode),
            Arg.Any<DateTimeOffset>())
            .Returns(true);

        // Act
        await sut.HandleAsync("tag", request, null);

        // Assert
        _ = fakeDepositByConditionCheckService.Received(1)
            .TagDepositAsync(
                Arg.Is(request.PromotionId.ToGuid()),
                Arg.Is(request.PlayerCode),
                Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task DepositRewardByConditionFlowProcessor_處理回滾標記事件()
    {
        // Arrange
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeDepositByConditionCheckService = Substitute.For<IDepositRewardByConditionCheckService>();

        var sut = new DepositRewardByConditionFlowProcessor(
            fakePromotionServiceClient,
            fakeBrandServiceClient,
            fakeDepositByConditionCheckService,
            TimeProvider.System);

        var request = new CheckPromotionRequest
        {
            PlayerCode = "test1",
            PromotionId = Guid.NewGuid()
        };
        var brandId = Guid.NewGuid();

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(Arg.Is(request.PromotionId.ToGuid()))
            .Returns(new PromotionMainInfo
            {
                Id = request.PromotionId,
                PromotionType = DepositPromotionConsts.DepositRewardByConditionTypeName,
                EventDuring = (DateTime.Now.AddDays(-1D), DateTime.Now.AddDays(1D)),
                BrandId = brandId
            });

        _ = fakeBrandServiceClient.GetBrandInfoAsync(Arg.Is(brandId))
            .Returns(new BrandInfo
            {
                Id = brandId
            });

        _ = fakeDepositByConditionCheckService.CheckAsync(
            Arg.Is(request.PromotionId.ToGuid()),
            Arg.Is(request.PlayerCode),
            Arg.Any<DateTimeOffset>())
            .Returns(true);

        // Act
        await sut.HandleAsync("rollback", request, null);

        // Assert
        _ = fakeDepositByConditionCheckService.Received(1)
            .RollbackTagAsync(
                Arg.Is(request.PromotionId.ToGuid()),
                Arg.Is(request.PlayerCode),
                Arg.Any<DateTimeOffset>());
    }
}
