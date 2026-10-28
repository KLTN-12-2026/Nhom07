using System;

namespace EzCondo.BackEndServer.Data.Entities;

/// <summary>Inbox sự kiện webhook; không phải sổ tiền đã thanh toán.</summary>
public class SubscriptionPaymentWebhookEvent
{
    public Guid Id { get; set; }
    // Chỉ gán khóa chống trùng sau khi xác minh chữ ký thành công.
    public string? EventKey { get; set; }
    public long? OrderCode { get; set; }
    public bool IsSignatureValid { get; set; }
    // Lưu raw body dạng text để không mất request có JSON không hợp lệ.
    public string Payload { get; set; } = null!;
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int ProcessingAttempts { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? ProcessingError { get; set; }

    // Xác định từ endpoint/cấu hình server, không tin config ID trong payload.
    public Guid ProviderConfigId { get; set; }
    public PlatformPaymentProviderConfig ProviderConfig { get; set; } = null!;
    // Có thể chưa tìm được order tại thời điểm nhận webhook.
    public Guid? PaymentOrderId { get; set; }
    public SubscriptionPaymentOrder? PaymentOrder { get; set; }
}
