using System;
using EzCondo.BackEndServer.Data.Enums;

namespace EzCondo.BackEndServer.Data.Entities;

public class SubscriptionPaymentTransaction
{
    public Guid Id { get; set; }
    // Adapter chuẩn hóa định danh giao dịch ổn định của provider vào trường này.
    public string ProviderTransactionId { get; set; } = null!;
    public string? ProviderReference { get; set; }
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Received;
    public decimal Amount { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string PayerMasked { get; set; } = "{}";
    public string ProviderPayload { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }

    // Cùng ProviderConfigId với order, được DB kiểm tra bằng FK ghép.
    public Guid ProviderConfigId { get; set; }
    public PlatformPaymentProviderConfig ProviderConfig { get; set; } = null!;
    public Guid PaymentOrderId { get; set; }
    public SubscriptionPaymentOrder PaymentOrder { get; set; } = null!;
}
