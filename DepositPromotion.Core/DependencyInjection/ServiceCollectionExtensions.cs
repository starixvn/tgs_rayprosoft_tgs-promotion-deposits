using System.Reflection;
using Tgs.Promotion.Deposits;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPromotionDeposits(this IServiceCollection services)
		=> services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddTransient<IDepositRewardByConditionCheckService, DepositRewardByConditionCheckService>();
}
