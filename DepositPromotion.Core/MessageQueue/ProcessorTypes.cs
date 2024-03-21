namespace Tgs.Promotion.Deposits.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type DepositFormStatusChangedHandlerType = typeof(DepositFormStatusChangedHandler);
	public static readonly Type DepositRewardByConditionFlowProcessorType = typeof(DepositRewardByConditionFlowProcessor);
}
