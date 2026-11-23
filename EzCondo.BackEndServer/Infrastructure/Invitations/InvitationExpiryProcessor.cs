using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.BackEndServer.Infrastructure.Invitations
{
	public class InvitationExpiryProcessor(
		ApplicationDbContext dbcontext,
		TimeProvider timeProvider,
		ILogger<InvitationExpiryProcessor> logger)
		: IInvitationExpiryProcessor
	{
		public async Task<int> ExpireAsync(
			CancellationToken cancellationToken = default)
		{
			var now = timeProvider.GetUtcNow();

			var expiredCount = await dbcontext.Invitations
				.Where(
					x =>
						!x.IsDeleted &&
						x.ExpiresAt <= now &&
						(
							x.Status == InvitationStatusEnum.PendingDelivery ||
							x.Status == InvitationStatusEnum.Sent
						))
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(
							x => x.Status,
							InvitationStatusEnum.Expired),
					cancellationToken);

			if(expiredCount > 0)
			{
				logger.LogInformation(
					"Đã chuyển {Count} invitation sang Expired lúc {Now}.",
					expiredCount,
					now);
			}

			return expiredCount;
		}
	}
}
