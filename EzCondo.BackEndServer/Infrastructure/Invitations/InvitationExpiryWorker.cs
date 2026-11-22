using EzCondo.BackEndServer.Infrastructure.Invitations.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EzCondo.BackEndServer.Infrastructure.Invitations
{
	public sealed class InvitationExpiryWorker(
		IServiceScopeFactory scopeFactory,
		IOptions<InvitationExpiryOptions> options,
		ILogger<InvitationExpiryWorker> logger)
		: BackgroundService
	{
		private readonly InvitationExpiryOptions _options = options.Value;

		protected override async Task ExecuteAsync(
			CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = scopeFactory.CreateAsyncScope();

					var processor = scope
						.ServiceProvider
							.GetRequiredService<IInvitationExpiryProcessor>();

					await processor.ExpireAsync(stoppingToken);
				}
				catch(OperationCanceledException)
					when (stoppingToken.IsCancellationRequested)
				{
					break;
				}
				catch(Exception exception)
				{
					logger.LogError(
						exception,
						"Invitation expiry worker gặp lỗi.");
				}

				try
				{
					await Task.Delay(
						TimeSpan.FromMinutes(_options.CheckIntervalMinutes),
						stoppingToken);
				}
				catch(OperationCanceledException)
					when (stoppingToken.IsCancellationRequested)
				{
					break;
				}
			}
		}
	}
}
