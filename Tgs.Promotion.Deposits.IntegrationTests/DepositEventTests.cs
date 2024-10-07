using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.Promotion.Deposits.ApplicationServices;
using Tgs.Promotion.Deposits.MessageQueue;
using Tgs.ServiceUtility;
using Xunit.Abstractions;

namespace Tgs.Promotion.Deposits.IntegrationTests;

public class DepositEventTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task 整合測試_儲值表單變更事件處理()
    {
        // Arrange
        var subject = "tgs.payment.deposit.status_ex";

        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakeDistributeDepositBonusCommandService = Substitute.For<ICommandService<DistributeDepositBonusCommand>>();

        await using var web = PromotionDepositsTestApplication.Instance
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakeDistributeDepositBonusCommandService)
                    .AddFakeNatsMessageQueue()));

        var depositFormId = Guid.NewGuid();

        var request = new FormEventEx
        {
            BrandId = Guid.NewGuid(),
            FormId = depositFormId,
            Status = PaymentFlowState.WaitApprove.ToString()
        };

        var depositFormInfo = new DepositFormInfo
        {
            Id = depositFormId,
            PromotionId = Guid.NewGuid(),
            Status = PaymentFlowState.Approved
        };

        _ = fakePaymentServiceClient.GetDepositFormAsync(
            Arg.Is(depositFormId),
            Arg.Any<CancellationToken>())
            .Returns(depositFormInfo);

        var sut = new DepositFormStatusChangedHandler(fakePaymentServiceClient,
            fakeDistributeDepositBonusCommandService);
        // Act
        await sut.Handle(subject, subject, null, request);

        // Assert
        _ = fakeDistributeDepositBonusCommandService.Received(1)
            .ExecuteAsync(
                Arg.Any<DistributeDepositBonusCommand>(),
                Arg.Any<CancellationToken>());
    }
}
