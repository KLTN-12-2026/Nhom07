using System;
using System.Collections.Generic;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities;

public class SubscriptionPaymentOrder : IAuditableEntity
{
    public Guid Id { get; set; }
    public long OrderCode { get; set; }
    // Chưa có link khi order vừa được lưu hoặc lời gọi provider thất bại.
    public string? ProviderPaymentLinkId { get; set; }
    public PaymentOrderStatus Status { get; set; } = PaymentOrderStatus.Pending;
    public decimal Amount { get; set; }
    // Tổng tiền thu đã xác minh; có thể vượt Amount nếu khách chuyển thừa.
    public decimal AmountPaid { get; set; }
    public string Currency { get; set; } = "VND";
    public string? CheckoutUrl { get; set; }
    public string? QrCode { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid SubscriptionInvoiceId { get; set; }
    public SubscriptionInvoice SubscriptionInvoice { get; set; } = null!;
    public Guid ProviderConfigId { get; set; }
    public PlatformPaymentProviderConfig ProviderConfig { get; set; } = null!;

    // Null khi job hệ thống tạo; FK tài khoản không tự cấp quyền tạo order.
    public Guid? RequestedByUserId { get; set; }
    public IdentityUser? RequestedByUser { get; set; }

    public ICollection<SubscriptionPaymentTransaction> PaymentTransactions { get; set; }
        = new List<SubscriptionPaymentTransaction>();
    public ICollection<SubscriptionPaymentWebhookEvent> WebhookEvents { get; set; }
        = new List<SubscriptionPaymentWebhookEvent>();
}
