using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using Tgs.MessageQueue.Nats.Serializers;
using Tgs.Promotion;
using Tgs.Promotion.Deposits;
using Tgs.Promotion.Deposits.MessageQueue;

var builder = WebApplication.CreateBuilder(args);
{
	_ = builder.Logging
		.AddOpenTelemetry(options =>
		{
			options.IncludeFormattedMessage = true;
			options.ParseStateValues = true;
			_ = options.AddFluentdForwardExporter(fluentOptions =>
			{
				var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
				fluentOptions.Host = nodeIp;
				fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
				fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
					"MachineName",
					() => Environment.MachineName));
			});
		});

	builder.Services
		.AddSingleton(TimeProvider.System)
		.AddKeyedSingleton<IConnectionMultiplexer>(
			"Redis",
			(sp, _) => ConnectionMultiplexer.Connect(
				builder.Configuration.GetConnectionString("Redis")!))
		.AddKeyedSingleton(
			"Redis",
			(sp, _) => sp
				.GetRequiredKeyedService<IConnectionMultiplexer>("Redis")
				.GetDatabase())
		.AddPromotionClientServices((opts, _) => opts.Url =
			builder.Configuration.GetValue<Uri>("PromotionDeposits:PromotionServiceBaseUrl")!)
		.AddPaymentServiceClient(httpBuilder => httpBuilder
			.ConfigureHttpClient(httpClient => httpClient.BaseAddress =
				builder.Configuration.GetValue<Uri>("PromotionDeposits:PaymentServiceBaseUrl")!))
		.AddCalculateClient(httpBuilder => httpBuilder
			.ConfigureHttpClient(httpClient => httpClient.BaseAddress =
				builder.Configuration.GetValue<Uri>("PromotionDeposits:CalculateServiceBaseUrl")!))
		.AddBrandServiceClient((opts, _) => opts.BrandCertificateBaseUrl =
			builder.Configuration.GetValue<Uri>("PromotionDeposits:BrandCertificationBaseUrl")!)
		.AddPlayerServiceClient((opts, _) => opts.BaseUrl =
			builder.Configuration.GetValue<Uri>("PromotionDeposits:PlayerServiceBaseUrl")!)
		.AddPromotionDeposits()
		.AddPromotionDepositsRepositories(
			dbBuilder => dbBuilder
				.UseNpgsql("name=Tgs"),
			sp => sp.GetRequiredKeyedService<IDatabase>("Redis"));

	builder.Services
		.AddKeyedSingleton(
			"Nats",
			(sp, _) => new NatsConnection(NatsOpts.Default with
			{
				Url = builder.Configuration.GetConnectionString("Nats")!,
				SerializerRegistry = TgsNatsSerializerRegistration.Default
			}))
		.AddMessageQueue()
		.AddNatsMessageQueue(natsConfiguration => natsConfiguration
			.ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats"))
			.AddProcessor(
				ProcessorTypes.DepositRewardByConditionSettingProcessorType,
				$"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.setting",
				$"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.setting")
			.AddProcessor(
				ProcessorTypes.DepositRewardByConditionFlowProcessorType,
				string.Format(
					PromotionCheckConsts.CheckSubjectTemplate,
					DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()),
				$"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.check")
			.AddReplyHandler(
				ProcessorTypes.DepositRewardByConditionFlowProcessorType,
				string.Format(
					PromotionCheckConsts.TagSubjectTemplate,
					DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()),
				$"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.tag")
			.AddReplyHandler(
				ProcessorTypes.DepositRewardByConditionFlowProcessorType,
				string.Format(
					PromotionCheckConsts.CommitSubjectTemplate,
					DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()),
				$"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.commit")
			.AddReplyHandler(
				ProcessorTypes.DepositRewardByConditionFlowProcessorType,
				string.Format(
					PromotionCheckConsts.RollbackSubjectTemplate,
					DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()),
				$"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.rollback")
			.AddHandler(
				ProcessorTypes.DepositFormStatusChangedHandlerType,
				"tgs.payment.deposit.status_ex",
				$"tgs.promotion.{DepositPromotionConsts.DepositRewardByConditionTypeName.ToLowerInvariant()}.deposit")
)
		.AddNatsGlobPatternExchange("*");

	var instrumentationExtenionFilters = new[] { "html", "ico", "png", "jpg" };
	_ = builder.Services		
		.AddHealthChecks()
		.Services
		.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
			sp => new ConfigureNamedOptions<OtlpExporterOptions>(
				Options.DefaultName,
				exporterOptions =>
				{
					var config = sp.GetRequiredService<IConfiguration>();
					var nodeIp = config.GetValue<string>("NODE_IP");

					if (!string.IsNullOrWhiteSpace(nodeIp))
						exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

					config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
				}))
		.AddOpenTelemetry()
		.WithTracing(tracingBuilder => tracingBuilder
			.SetResourceBuilder(ResourceBuilder.CreateDefault()
				.AddEnvironmentVariableDetector())
			.AddSource("Tgs.*")
			.AddSource("TGS.*")
			.AddSource("Valhalla.*")
			.AddAspNetCoreInstrumentation()			
			.AddNpgsql()			
			.AddProfileViewExporter()
			.AddOtlpExporter())
		.WithMetrics(meterBuiler => meterBuiler			
			.AddRuntimeInstrumentation()
			.AddAspNetCoreInstrumentation()
			.AddOtlpExporter())
		.Services
		.AddProfileViewer()
		.Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter =
			httpContext => !httpContext.Request.Path.HasValue
				|| !httpContext.Request.Path.StartsWithSegments("/swagger")
				&& !httpContext.Request.Path.StartsWithSegments("/healthz")
				&& !httpContext.Request.Path.StartsWithSegments("/metrics")
				&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
				&& !new[] { "html", "ico", "png", "jpg" }.Any(
					ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));
}

var app = builder.Build();
{
	_ = app.MapHealthChecks("/healthz");
	app.MapProfileViewer();
}

app.Run();
