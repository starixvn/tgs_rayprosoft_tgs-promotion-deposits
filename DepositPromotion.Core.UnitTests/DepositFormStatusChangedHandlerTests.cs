using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.Promotion.Deposits.ApplicationServices;
using Tgs.Promotion.Deposits.MessageQueue;
using Tgs.ServiceUtility;

namespace DepositPromotion.Core.UnitTests;

public class DepositFormStatusChangedHandlerTests
{
    [Fact]
    public async Task DepositFormStatusChangedHandler_處理儲值表單變更事件()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeDistributeDepositBonusCommandService = Substitute.For<ICommandService<DistributeDepositBonusCommand>>();

        var sut = new DepositFormStatusChangedHandler(
            fakePaymentServiceClient,
            fakeDistributeDepositBonusCommandService);

        var depositFormId = Guid.NewGuid();

        var formEvent = new FormEventEx
        {
            BrandId = Guid.NewGuid(),
            FormId = depositFormId,
            Status = nameof(PaymentFlowState.WaitApprove)
        };

        var depositFormInfo = new DepositFormInfo
        {
            Id = depositFormId,
            PromotionId = Guid.NewGuid(),
            Status = PaymentFlowState.Approved
        };

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(depositFormId))
            .Returns(depositFormInfo);

        // Act
        await sut.HandleAsync("test", formEvent, null);

        // Assert
        _ = fakeDistributeDepositBonusCommandService.Received(1)
            .ExecuteAsync(Arg.Is(new DistributeDepositBonusCommand(depositFormInfo)));
    }

    [Fact]
    public async Task DepositFormStatusChangedHandler_處理儲值表單變更事件_如果表單狀態不是變更為Approved不會動作()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeDistributeDepositBonusCommandService = Substitute.For<ICommandService<DistributeDepositBonusCommand>>();

        var sut = new DepositFormStatusChangedHandler(
            fakePaymentServiceClient,
            fakeDistributeDepositBonusCommandService);

        var depositFormId = Guid.NewGuid();

        var formEvent = new FormEventEx
        {
            BrandId = Guid.NewGuid(),
            FormId = depositFormId,
            Status = nameof(PaymentFlowState.WaitApprove)
        };

        var depositFormInfo = new DepositFormInfo
        {
            Id = depositFormId,
            PromotionId = Guid.NewGuid(),
            Status = PaymentFlowState.Reject
        };

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(depositFormId))
            .Returns(depositFormInfo);

        // Act
        await sut.HandleAsync("test", formEvent, null);

        // Assert
        _ = fakeDistributeDepositBonusCommandService.DidNotReceive()
            .ExecuteAsync(Arg.Any<DistributeDepositBonusCommand>());
    }

    [Fact]
    public async Task DepositFormStatusChangedHandler_處理儲值表單變更事件_如果沒有參加活動不會動作()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeDistributeDepositBonusCommandService = Substitute.For<ICommandService<DistributeDepositBonusCommand>>();

        var sut = new DepositFormStatusChangedHandler(
            fakePaymentServiceClient,
            fakeDistributeDepositBonusCommandService);

        var depositFormId = Guid.NewGuid();

        var formEvent = new FormEventEx
        {
            BrandId = Guid.NewGuid(),
            FormId = depositFormId,
            Status = nameof(PaymentFlowState.WaitApprove)
        };

        var depositFormInfo = new DepositFormInfo
        {
            Id = depositFormId,
            Status = PaymentFlowState.Approved
        };

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(depositFormId))
            .Returns(depositFormInfo);

        // Act
        await sut.HandleAsync("test", formEvent, null);

        // Assert
        _ = fakeDistributeDepositBonusCommandService.DidNotReceive()
            .ExecuteAsync(Arg.Any<DistributeDepositBonusCommand>());
    }

    [Fact]
    public async Task DepositFormStatusChangedHandler_處理儲值表單變更事件_找不到儲值單不會動作()
    {
        // Arrange
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeDistributeDepositBonusCommandService = Substitute.For<ICommandService<DistributeDepositBonusCommand>>();

        var sut = new DepositFormStatusChangedHandler(
            fakePaymentServiceClient,
            fakeDistributeDepositBonusCommandService);

        var depositFormId = Guid.NewGuid();

        var formEvent = new FormEventEx
        {
            BrandId = Guid.NewGuid(),
            FormId = depositFormId,
            Status = nameof(PaymentFlowState.WaitApprove)
        };

        // Act
        await sut.HandleAsync("test", formEvent, null);

        // Assert
        _ = fakeDistributeDepositBonusCommandService.DidNotReceive()
            .ExecuteAsync(Arg.Any<DistributeDepositBonusCommand>());
    }
}
