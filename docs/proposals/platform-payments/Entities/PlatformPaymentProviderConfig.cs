using System;
using System.Collections.Generic;

namespace EzCondo.BackEndServer.Data.Entities;

/// <summary>Cấu hình nhận tiền của EzCondo. Không thuộc một tenant khách hàng.</summary>
public class PlatformPaymentProviderConfig : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Provider { get; set; } = "payos";
    public string? MerchantCode { get; set; }
    public string SecretReference { get; set; } = null!;
    public string Settings { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<SubscriptionPaymentOrder> PaymentOrders { get; set; }
        = new List<SubscriptionPaymentOrder>();
    public ICollection<SubscriptionPaymentTransaction> PaymentTransactions { get; set; }
        = new List<SubscriptionPaymentTransaction>();
    public ICollection<SubscriptionPaymentWebhookEvent> WebhookEvents { get; set; }
        = new List<SubscriptionPaymentWebhookEvent>();
}
