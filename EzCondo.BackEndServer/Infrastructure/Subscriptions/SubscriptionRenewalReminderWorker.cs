using EzCondo.BackEndServer.Infrastructure.Subscriptions.Options;
using EzCondo.BackEndServer.Services.SubscriptionRenewals;
using Microsoft.Extensions.Options;
using System.Threading;

namespace EzCondo.BackEndServer.Infrastructure.Subscriptions
{
	public sealed class SubscriptionRenewalReminderWorker(
		IServiceScopeFactory scopeFactory,
		IOptions<SubscriptionRenewalReminderOptions> optionsAccessor,
		ILogger<SubscriptionRenewalReminderWorker> logger)
		: BackgroundService
	{
		protected override async Task ExecuteAsync(
			CancellationToken stoppingToken)
		{
			await RunOnceSafelyAsync(stoppingToken);

			if (stoppingToken.IsCancellationRequested)
			{
				return;
			}

			var interval = TimeSpan.FromHours(
				optionsAccessor.Value.CheckIntervalHours);

			using var timer = new PeriodicTimer(interval);

			try
			{
				while(await timer.WaitForNextTickAsync(stoppingToken))
				{
					await RunOnceSafelyAsync(stoppingToken);
				}
			}
			catch(OperationCanceledException)
				when (stoppingToken.IsCancellationRequested)
			{
				//
			}
		}

		private async Task RunOnceSafelyAsync(
			CancellationToken cancellationToken)
		{
			try
			{
				await using var scope = scopeFactory.CreateAsyncScope();

				var processor = scope.ServiceProvider
					.GetRequiredService<ISubscriptionRenewalReminderProcessor>();

				var result = await processor.ProcessAsync(cancellationToken);

				logger.LogInformation(
					"Đã xử lý reminder gia hạn. " +
					"Scanned: {ScannedCount}; " +
					"Eligible: {EligibleCount}; " +
					"Created: {CreatedCount}; " +
					"AlreadyExists: {AlreadyExistsCount}; " +
					"SkippedState: {SkippedStateCount}; " +
					"MissingTenantAdmin: {MissingTenantAdminCount}; ",
					result.ScannedCount,
					result.EligibleCount,
					result.CreatedCount,
					result.AlreadyExistsCount,
					result.SkippedStateCount,
					result.MissingTenantAdminCount);
			}
			catch(OperationCanceledException)
				when (cancellationToken.IsCancellationRequested)
			{
				//
			}
			catch(Exception exception)
			{
				logger.LogError(
					exception,
					"Lượt xử lý reminder gia hạn thất bại.");
			}
		}
	}
}
