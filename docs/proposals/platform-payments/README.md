# Đề xuất entity và configuration thanh toán Subscription

Đây là bộ mã đề xuất, chưa đưa vào project Backend và chưa áp dụng migration/database. Bốn entity và bốn configuration dùng namespace của dự án, EF Core/Npgsql 8 và EntityConfigurationBase hiện có.

## Các quyết định của bản đề xuất

- Cả bốn bảng thuộc schema `platform`. Không có TenantId dư thừa; tenant khách hàng được xác định qua Order -> SubscriptionInvoice -> Subscription -> Tenant.
- Mỗi order bắt buộc có một SubscriptionInvoice; hóa đơn có thể có nhiều order. Không hỗ trợ thanh toán gộp nhiều hóa đơn trong một order ở bản này.
- Cấu hình provider thuộc EzCondo, không cần tenant giả. `SecretReference` chỉ là tham chiếu đến nơi lưu bí mật; không để secret thô trong Settings/Payload.
- Order, transaction và webhook gắn cùng cấu hình provider thông qua FK ghép. FK hóa đơn chỉ nhận SubscriptionInvoice, không nhận TenantInvoice.
- RequestedByUserId nullable để hỗ trợ job hệ thống. IdentityUser hiện tại còn có TenantId; cần hoàn thiện thiết kế tài khoản Platform và kiểm tra quyền người yêu cầu tại service. FK này không tự cấp quyền.
- Tất cả quan hệ dùng NoAction để không xóa dây chuyền dữ liệu thanh toán. Ngừng cấu hình bằng IsActive; giữ cấu hình cũ để đối soát.
- Đề xuất ban đầu dùng VND, có CHECK ở order vì SubscriptionInvoice hiện chưa có Currency. Nếu hỗ trợ đa tiền tệ, cần thêm Currency cho hóa đơn và kiểm tra khớp khi tạo order.
- AmountPaid là tổng thu đã xác minh; cho phép cao hơn Amount để không đánh mất giao dịch chuyển thừa. PaidAmount của hóa đơn là phần tiền được áp dụng cho hóa đơn, không vượt dư nợ. Phần thừa cần đối soát/hoàn tiền riêng; không cộng thẳng toàn bộ AmountPaid vào hóa đơn.
- Giữ các CLR enum PaymentOrderStatus/PaymentTransactionStatus nhưng chuyển sang varchar bằng HasConversion<string>(); lưu tên PascalCase, không dùng PostgreSQL enum `tenant.*`. PgName không đổi giá trị lưu trong cách mapping này. Nếu đổi enum phải cập nhật CHECK và dữ liệu tương ứng.

## Tích hợp vào dự án

1. Đưa các file `Entities/*.cs` vào `EzCondo.BackEndServer/Data/Entities/`.
2. Đưa các file `Configurations/*.cs` vào `EzCondo.BackEndServer/Infrastructure/Configurations/`.
3. Đăng ký sequence sau trong OnModelCreating trước khi tạo migration:

```csharp
b.HasSequence<long>("subscription_payment_order_code_seq", "platform")
    .StartsAt(100000)
    .IncrementsBy(1);
```

ApplyConfigurationsFromAssembly hiện có sẽ đọc bốn configuration. Không bắt buộc bổ sung DbSet vì có thể dùng Set<TEntity>(). Dùng sequence để phân bổ mã, không dùng Max(OrderCode) + 1. OrderCode unique trong bảng Platform; nếu hai luồng dùng chung merchant/provider account, phải dùng bộ cấp mã chung hoặc dải mã không trùng, thay vì hai sequence cùng bắt đầu 100000.

4. Rà soát các lỗi model sẵn có trước khi tạo migration, đặc biệt FK một cột đang trỏ principal key hai cột và PlatformPermission.Code đang Guid còn FK PermissionCode là string. Các lỗi đó không được sửa trong bộ đề xuất này.
5. Kiểm tra SQL migration; di chuyển order Subscription và giao dịch/webhook liên quan từ bảng cũ dựa trên dữ liệu xác thực. Không đoán liên kết hóa đơn từ số tiền hoặc thời điểm.

## Idempotency và webhook

- ProviderConfigId được server xác định từ endpoint/cấu hình nhận webhook; không tin giá trị do payload khai báo.
- Payload webhook lưu raw text để giữ được cả request có JSON sai. Đặt giới hạn kích thước tại endpoint; payload giao dịch chỉ lưu JSON hợp lệ và dữ liệu người trả đã che bớt.
- Webhook.OrderCode dùng long? để khớp Order.OrderCode, không dùng int?. Đây là dữ liệu nhận được; không thay thế FK PaymentOrderId.
- EventKey chỉ được thiết lập sau xác minh chữ ký. Khóa duy nhất (ProviderConfigId, EventKey) ngăn cùng sự kiện đã xác minh tạo nhiều bản ghi inbox. Một request giả mạo không được chiếm khóa trước request thật.
- EventKey phải được adapter chuẩn hóa từ định danh sự kiện ổn định; nếu không có định danh sự kiện, thiết kế khóa từ transaction reference và loại sự kiện, không dùng mỗi OrderCode. Khi không có khóa đáng tin, giữ null và dựa vào khóa giao dịch để chống ghi tiền trùng.
- Đây là inbox một sự kiện hợp lệ, không phải log mọi lần delivery. Nếu provider gửi lại, service bắt xung đột unique và xử lý bản ghi cũ; không ghi đè payload đã xác minh bằng request không hợp lệ.
- PaymentOrderId của webhook được phép null khi chưa tìm được order. ProcessedAt chỉ được đặt khi đã xử lý nghiệp vụ thành công; lỗi để ProcessingError và chưa đặt ProcessedAt. Worker chỉ lấy sự kiện đã xác minh, cần cơ chế nhận việc tránh nhiều worker xử lý đồng thời.
- ProviderTransactionId là định danh giao dịch ổn định do adapter chọn từ dữ liệu provider; không tạo UUID mới mỗi lần nhận webhook. Unique (ProviderConfigId, ProviderTransactionId) ngăn một giao dịch được gán vào nhiều order trong cùng cấu hình.
- Mỗi cấu hình đại diện một merchant/provider account ổn định. Khi xoay secret, giữ identity cấu hình; nếu tạo nhiều cấu hình cho cùng account phải chuẩn hóa định danh merchant để chống trùng xuyên cấu hình.
- FK webhook bảo đảm cấu hình khớp order; service phải kiểm tra OrderCode nhận được khớp order đã tìm. CHECK không kiểm tra chéo cột của hai bảng.
- Ghi transaction, áp dụng tiền vào hóa đơn, cập nhật order và đánh dấu webhook hoàn tất trong cùng transaction DB. Dùng khóa dòng hoặc optimistic concurrency/retry để hai order của cùng hóa đơn không cộng tiền vượt dư nợ. Idempotency không chỉ là kiểm tra AnyAsync trước insert.
- Không dựa vào ReturnUrl để đánh dấu đã thanh toán. Khi nhận thanh toán muộn của order hết hạn/hủy, vẫn ghi nhận tiền và đối soát.
- Enum Refunded không đủ để mô tả hoàn tiền một phần hoặc quy trình duyệt hoàn tiền. Các bảng này ghi nhận thu tiền; hoàn tiền cần nghiệp vụ/entity riêng khi triển khai.

## Các kiểm tra cần thực hiện khi tích hợp

Đã kiểm tra bộ tám class trong một project tạm dùng EF Core/Npgsql 8.0.0: biên dịch thành công, dựng model và sinh SQL DDL thành công; kiểm tra schema Platform, FK ghép, webhook nullable, không phát sinh shadow FK, NoAction và unique giao dịch. Kiểm tra dùng base configuration, interface và enum thật của dự án; SubscriptionInvoice và IdentityUser được thay bằng stub chỉ có Id để cô lập lỗi model hiện có. Chưa xác nhận tích hợp toàn bộ Backend hoặc chạy SQL trên PostgreSQL thật.

- Model build được; migration tạo đúng bốn bảng trong platform, sequence, unique index và FK ghép.
- Order không có hóa đơn hoặc tham chiếu hóa đơn không tồn tại bị từ chối.
- Hai order cho cùng hóa đơn được phép; transaction/webhook liên kết khác cấu hình bị từ chối.
- Lặp ProviderTransactionId trong cùng cấu hình bị từ chối, kể cả khi gán sang order khác.
- Webhook chưa xác minh không được có EventKey/PaymentOrderId; webhook chưa xác định order vẫn được lưu.
- Kiểm tra race khi nhận webhook lặp và thanh toán đồng thời hai order cho một hóa đơn trên PostgreSQL thật.

## Nguồn kỹ thuật

- EF Core: https://learn.microsoft.com/en-us/ef/core/modeling/relationships/foreign-and-principal-keys
- Npgsql enum mapping: https://www.npgsql.org/efcore/mapping/enum.html

Đây là đề xuất thiết kế; chưa thay đổi các quyết định đã chốt trong EZCONDO_PROJECT_CONTEXT.md.
