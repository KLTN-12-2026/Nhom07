
using System.Collections.Concurrent;

namespace EzCondo.BackEndServer.Infrastructure.Subscriptions
{
	public sealed class TenantBackgroundJobCoordinator
		: ITenantBackgroundJobCoordinator,
		IDisposable
	{
		private readonly ConcurrentDictionary<
			Guid, 
			CancellationTokenSource> tokens = new();

		public void Dispose()
		{
			foreach(var source in tokens.Values)
			{
				source.Dispose();
			}

			tokens.Clear();
		}

		public CancellationToken GetToken(Guid tenantId)
		{
			var source = tokens.GetOrAdd(
				tenantId,
				_ => new CancellationTokenSource());

			return source.Token;
		}

		public bool IsStopped(Guid tenantId)
		{
			return tokens.TryGetValue(tenantId, out var source) &&
				source.IsCancellationRequested;
		}

		public void Resume(Guid tenantId)
		{
			if(tokens.TryRemove(tenantId, out var source))
			{
				source.Dispose();
			}
		}

		public void Stop(Guid tenantId)
		{
			var source = tokens.GetOrAdd(
				tenantId,
				_ => new CancellationTokenSource());

			source.Cancel();
		}
	}
}
