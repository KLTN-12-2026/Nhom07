using EzCondo.BackEndServer.Infrastructure.Subscriptions.Options;
using EzCondo.BackEndServer.Services.SubscriptionRenewals;
using Microsoft.Extensions.Options;

namespace EzCondo.BackEndServer.Infrastructure.Subscriptions
{
	public class SubscriptionLifecycleWorker(
		IServiceScopeFactory scopeFactory,
		IOptions<SubscriptionLifecycleOptions> optionsAccessor,
		ILogger<SubscriptionLifecycleWorker> logger)
		: BackgroundService
	{
		protected override async Task ExecuteAsync(
			CancellationToken stoppingToken)
		{
			await RunOnceSafeAsync(stoppingToken);

			if (stoppingToken.IsCancellationRequested)
			{
				return;
			}

			var interval = TimeSpan.FromMinutes(
				optionsAccessor.Value.CheckIntervalMinutes);

			using var timer = new PeriodicTimer(interval);

			try
			{
				while(await timer.WaitForNextTickAsync(stoppingToken))
				{
					await RunOnceSafeAsync(stoppingToken);
				}
			}
			catch(OperationCanceledException)
				when (stoppingToken.IsCancellationRequested)
			{
			}
		}

		private async Task RunOnceSafeAsync(
			CancellationToken cancellationToken)
		{
			try
			{
				await using var scope = scopeFactory.CreateAsyncScope();

				var processor = scope
					.ServiceProvider
						.GetRequiredService<ISubscriptionLifecycleProcessor>();

				var result = await processor.ProcessAsync(cancellationToken);

				logger.LogInformation(
					"Subscription lifecycle processed. " +
					"Scanned: {Scanned}; " +
					"NoticeSent: {NoticeSent}; " +
					"NoticeFailed: {NoticeFailed}; " +
					"Expired: {Expired}",
					result.ScannedCount,
					result.NoticeSentCount,
					result.NoticeFailedCount,
					result.ExpiredCount);
			}
			catch(OperationCanceledException)
				when (cancellationToken.IsCancellationRequested)
			{
			}
			catch(Exception exception)
			{
				logger.LogError(
					exception,
					"Xử lý subscription lifecycle thất bại.");
			}
		}
	}
}
