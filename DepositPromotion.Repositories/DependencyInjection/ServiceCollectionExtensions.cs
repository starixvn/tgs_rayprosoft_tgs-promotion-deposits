using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Tgs.Promotion.Deposits;
using Tgs.Promotion.Deposits.Repositories;
using Tgs.Promotion.Deposits.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPromotionDepositsRepositories(
		this IServiceCollection services,
		Action<DbContextOptionsBuilder> dbConfigure,
		Func<IServiceProvider, IDatabase> resolveRedisDatabase)
		=> services
			.AddDbContext<TgsContext>(dbConfigure)
			.AddSingleton<IFirstLevelRepository>(
				sp => ActivatorUtilities.CreateInstance<RedisFirstLevelRepository>(
					sp,
					resolveRedisDatabase(sp)))
			.AddScoped<ITgsRepository, TgsRepository>();
}
