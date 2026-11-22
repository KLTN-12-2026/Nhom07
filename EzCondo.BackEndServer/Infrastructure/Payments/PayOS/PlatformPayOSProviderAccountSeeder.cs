using EzCondo.BackEndServer.Data;
using EzCondo.BackEndServer.Data.Entities.SchemaPayment;
using EzCondo.BackEndServer.Data.Enums;
using EzCondo.BackEndServer.Infrastructure.Payments.PayOS.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EzCondo.BackEndServer.Infrastructure.Payments.PayOS
{
	public sealed class PlatformPayOSProviderAccountSeeder(
		ApplicationDbContext dbcontext,
		IOptions<PayOSIntegrationOptions> options,
		ILogger<PlatformPayOSProviderAccountSeeder> logger)
	{
		private const long SeederAdvisoryLockId = 7_398_260_110_100_003;

		private readonly PayOSIntegrationOptions _options = options.Value;

		public async Task SeedAsync(
			CancellationToken cancellationToken = default)
		{
			var accountCode = NormalizeAccountCode(
				_options.ProviderAccountCode);

			await using var transaction = await dbcontext.Database
				.BeginTransactionAsync(cancellationToken);

			await AcquireSeederLockAsync(cancellationToken);

			var providerAccount = await dbcontext.ProviderAccounts
				.SingleOrDefaultAsync(
					x =>
						x.OwnerScope == ProviderOwnerScope.Platform &&
						x.Code == accountCode,
					cancellationToken);

			var wasCreated = providerAccount is null;

			if(providerAccount is null)
			{
				providerAccount = new ProviderAccount
				{
					Id = Guid.NewGuid(),
					TenantId = null,
					WebhookKey = Guid.NewGuid(),
					Code = accountCode,
					Provider = PayOSProviderConstants.ProviderCode,
					OwnerScope = ProviderOwnerScope.Platform,
					MerchantCode = null,

					PaymentCredentialsRef = PayOSProviderConstants.PaymentCredentialsReference,

					PayoutCredentialsRef = null,
					SupportsCollection = true,
					SupportsPayout = false,
					Settings = "{}",
					IsActive = true
				};

				dbcontext.ProviderAccounts.Add(providerAccount);
			}
			else
			{
				SynchronizeProviderAccount(providerAccount);
			}

			await dbcontext.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			logger.LogInformation(
				"Đã {Operation} kênh PayOS Platform "
					+ "{ProviderAccountCode}. "
					+ "ProviderAccountId: {ProviderAccountId}",
				wasCreated ? "tạo" : "đồng bộ",
				providerAccount.Code,
				providerAccount.Id);
		}

		private static void SynchronizeProviderAccount(ProviderAccount providerAccount)
		{
			if(providerAccount.TenantId is not null)
			{
				throw new InvalidOperationException(
					"Kênh thu tiền PayOS của Platform không được gắn với TenantId.");
			}

			if(providerAccount.WebhookKey == Guid.Empty)
			{
				providerAccount.WebhookKey = Guid.NewGuid();
			}

			providerAccount.Provider = PayOSProviderConstants.ProviderCode;

			providerAccount.OwnerScope = ProviderOwnerScope.Platform;

			providerAccount.PaymentCredentialsRef = PayOSProviderConstants.PaymentCredentialsReference;

			providerAccount.PayoutCredentialsRef = null;
			providerAccount.SupportsCollection = true;
			providerAccount.SupportsPayout = false;
			providerAccount.IsActive = true;

			if (string.IsNullOrWhiteSpace(providerAccount.Settings))
			{
				providerAccount.Settings = "{}";
			}
		}

		private async Task AcquireSeederLockAsync(CancellationToken cancellationToken)
		{
			if(dbcontext.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
			{
				return;
			}

			await dbcontext.Database.ExecuteSqlRawAsync(
				$"""
				SELECT pg_advisory_xact_lock(
					{SeederAdvisoryLockId})
				""",
				cancellationToken);
				
		}

		private static string NormalizeAccountCode(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidOperationException(
					"PayOS:ProviderAccountCode không được để trống.");
			}

			var normalizedValue = value.Trim().ToUpperInvariant();

			if(normalizedValue.Length > 80)
			{
				throw new InvalidOperationException(
					"PayOS:ProviderAccountCode không được vượt quá 80 ký tự.");
			}

			return normalizedValue;
		}
	}
}