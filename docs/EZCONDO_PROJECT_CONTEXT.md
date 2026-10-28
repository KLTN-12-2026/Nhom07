# EzCondo — Ngữ cảnh hệ thống và quyết định nghiệp vụ

> Cập nhật: 21/10/2026.
> Mục đích: Ngữ cảnh lâu dài để hỗ trợ thiết kế, lập trình, review, kiểm thử và viết tài liệu khóa luận EzCondo.
> Nguồn: Markdown ban đầu, năm sơ đồ phân rã chức năng và các quyết định của chủ dự án trong cuộc trao đổi.
> Tài liệu mô tả yêu cầu và định hướng; không xác nhận các chức năng đã được triển khai trong mã nguồn.

## 1. Thông tin dự án

- **Tên đề tài:** Xây dựng EzCondo – Hệ thống SaaS Multi-tenant quản lý vận hành chung cư đa nền tảng tích hợp AI.
- **Loại dự án:** Khóa luận tốt nghiệp; hệ thống B2B SaaS quản lý vận hành chung cư.
- **Mục tiêu:** Số hóa hoạt động quản lý, vận hành chung cư trên web/mobile; cung cấp quản trị nền tảng SaaS, thanh toán và AI hỗ trợ.
- Không mô tả EzCondo là hệ thống thương mại điện tử hoặc hệ thống mua bán căn hộ.

## 2. Hai scope độc lập

| Scope | Phạm vi | Tác nhân |
|---|---|---|
| `platform` | Nhà cung cấp SaaS EzCondo: tenant, gói dịch vụ, bản quyền/subscription, thanh toán thương mại, hỗ trợ và vận hành nền tảng | Quản trị viên |
| `tenant` | Hoạt động của một chung cư: căn hộ, cư dân, hợp đồng, dịch vụ, hóa đơn, nhân sự, lương, khiếu nại và tài liệu | Chủ chung cư, Quản lý, Nhân viên, Cư dân |

### 2.1. Ràng buộc đã xác lập

1. Platform RBAC và Tenant RBAC là hai hệ phân quyền độc lập; không gộp danh mục hoặc gán quyền chéo scope.
2. Quản trị viên thuộc Platform; các tác nhân còn lại thuộc Tenant.
3. Quản trị viên Platform không mặc nhiên có quyền xem hoặc thao tác dữ liệu nghiệp vụ Tenant, kể cả khi có quyền quản trị cao nhất ở Platform.
4. Quản trị viên chỉ được can thiệp nghiệp vụ của một tenant khi có sự cho phép hoặc yêu cầu hỗ trợ từ Chủ chung cư của tenant cụ thể đó.
5. Sự cho phép/yêu cầu hỗ trợ của tenant A không tạo quyền truy cập tenant B; không được suy diễn thành quyền Tenant thường trực cho Platform.
6. Quản lý hồ sơ SaaS, trạng thái tenant, gói dịch vụ và thanh toán thương mại trong Platform không đồng nghĩa với quyền truy cập dữ liệu vận hành nội bộ của tenant.
7. Báo cáo, giám sát toàn nền tảng không được trở thành đường vòng để truy cập dữ liệu nghiệp vụ Tenant ngoài điều kiện hỗ trợ đã nêu.

### 2.2. Cơ chế hỗ trợ cần thiết kế tiếp

Điều kiện phải có sự cho phép hoặc yêu cầu hỗ trợ của Chủ chung cư đã được chốt. Các chi tiết sau chưa được chốt: cách ghi nhận yêu cầu, danh tính người hỗ trợ, thao tác được phép, thời hạn, thu hồi và cơ chế thực thi tại database.

Định hướng đề xuất: một phiên hoặc bản ghi cấp quyền hỗ trợ gắn với tenant, người hỗ trợ, mục đích, phạm vi và thời hạn; có nhật ký thao tác và khả năng thu hồi. Một ticket hỗ trợ không nên tự động được hiểu là quyền truy cập toàn bộ dữ liệu. Đây là đề xuất thiết kế, không phải chức năng đã triển khai.

## 3. Multi-tenancy và cô lập dữ liệu

1. Mọi dữ liệu nghiệp vụ thuộc chung cư phải có `TenantId`.
2. Mỗi request xử lý nghiệp vụ Tenant chỉ được chạy trong một tenant context xác định.
3. Không tin `TenantId` từ client nếu chưa đối chiếu với membership, quyền và tenant context hiện tại; truy cập hỗ trợ phải có căn cứ cấp quyền tương ứng.
4. Một người thuộc hai tenant có thể có hai bản ghi nghiệp vụ trong cùng bảng với hai `TenantId` khác nhau để giảm độ phức tạp database. Quyết định này không mặc nhiên yêu cầu tạo hai tài khoản đăng nhập Identity.
5. Kiểm tra cô lập dữ liệu ở cả application layer và database layer.
6. PostgreSQL RLS được định hướng dùng với session context như `app.tenant_id` và `app.user_id`.
7. Tenant isolation áp dụng cho API, background job, báo cáo, xuất dữ liệu, truy vấn tài liệu AI và AI tool call.
8. Cùng tenant chưa đủ để được đọc mọi bản ghi: Cư dân còn phải đáp ứng điều kiện liên quan đến dữ liệu cá nhân, căn hộ, hóa đơn, hợp đồng và khiếu nại được phép truy cập.

## 4. Công nghệ

| Thành phần | Công nghệ/định hướng |
|---|---|
| Backend | C# / .NET 8.x; ASP.NET Core Web API 8.x và MVC 8.x |
| Database access | EF Core 8, Fluent API, Code First Migration |
| Database | PostgreSQL, Npgsql |
| Xác thực | ASP.NET Core Identity và JWT |
| Phân quyền | Permission-based RBAC |
| Client | Web; định hướng .NET MAUI/mobile |
| Thanh toán | PayOS .NET SDK |
| AI ban đầu | Google Gemini API |
| AI nâng cao | Vertex AI/AI Agent là hướng phát triển sau |
| Kiểm thử | xUnit |
| Tìm kiếm tài liệu | PostgreSQL `tsvector` và GIN index |

Không tự ý chuyển ví dụ hoặc kiến trúc sang Java/Spring.

## 5. Tác nhân và role

| Tác nhân | Scope | Role code | Trách nhiệm |
|---|---|---|---|
| Quản trị viên | Platform | Chưa chốt mã role | Quản trị và vận hành nền tảng SaaS |
| Chủ chung cư | Tenant | `TENANT_ADMIN` | Quản trị cao nhất trong tenant; phê duyệt nghiệp vụ tài chính được quy định |
| Quản lý | Tenant | `MANAGER` | Quản lý nhân sự, dịch vụ, báo cáo; lập và phát hành nghiệp vụ tài chính sau phê duyệt |
| Nhân viên | Tenant | `STAFF` | Thực hiện nghiệp vụ vận hành theo bộ quyền chung |
| Cư dân | Tenant | `RESIDENT` | Tự phục vụ, giới hạn theo dữ liệu được phép truy cập |

- Chủ chung cư chính là `TENANT_ADMIN`; không tạo thêm một role chủ sở hữu cao hơn role này chỉ vì khác tên gọi.
- Giữ một role `STAFF`; không yêu cầu chia thành role kế toán, lễ tân, kỹ thuật, bảo vệ, buồng phòng hoặc giữ xe.
- Tenant role không phải ASP.NET Identity Role. Role được tạo/copy theo từng tenant và có thể tùy chỉnh độc lập.
- Việc có role chung không loại bỏ kiểm tra dữ liệu sở hữu hoặc điều kiện nghiệp vụ.
- Không suy diễn `HierarchyLevel` thành cơ chế tự động kế thừa mọi permission hoặc bỏ qua quy trình phê duyệt; cơ chế kế thừa cụ thể chưa chốt.

## 6. Chức năng Platform — Quản trị viên

### 6.1. Quản lý chung cư/tenant

- Khởi tạo hồ sơ chung cư; nhập đại diện pháp lý, cấp mã và slug, cập nhật liên hệ.
- Thiết lập thời gian dùng thử.
- Kích hoạt, tạm ngưng và khôi phục tenant.
- Xem lịch sử trạng thái tenant.
- Mời Tenant Admin.

### 6.2. Bản quyền sử dụng và chính sách thương mại

- Theo dõi bản quyền/subscription; hủy bản quyền, xem lịch sử và gửi thông báo gia hạn.
- Tạo gói, cấu hình thông tin gói, ngừng kinh doanh gói.
- Xem tenant đang sử dụng từng gói.

### 6.3. Thanh toán thương mại SaaS

- Tra cứu theo tenant, hợp đồng hoặc kỳ thanh toán.
- Xử lý thanh toán thất bại; hoàn tiền hoặc hủy giao dịch thuộc Platform.
- Theo dõi công nợ; gửi nhắc nhở thanh toán.
- Xuất báo cáo doanh thu.

Thanh toán thương mại là quan hệ giữa nhà cung cấp EzCondo và khách hàng tenant. Quyền xử lý giao dịch Platform không tạo quyền xử lý hóa đơn hoặc tiền lương nội bộ của Tenant. Quy trình phê duyệt hoàn tiền Platform chưa được chốt.

### 6.4. Hỗ trợ và sự cố

- Xem yêu cầu hỗ trợ, trao đổi với tenant, cập nhật trạng thái và ghi phương án xử lý.
- Xem sự cố hệ thống, chỉ định Admin xử lý, cập nhật tiến độ, xem lịch sử.
- Đưa hệ thống vào chế độ bảo trì và thông báo tenant bị ảnh hưởng.
- Nếu việc hỗ trợ cần truy cập nghiệp vụ Tenant thì áp dụng điều kiện tại mục 2.

### 6.5. Giám sát và báo cáo toàn nền tảng

- Giám sát tenant mất kết nối/lỗi, bản quyền sắp hết hạn, email/SMS thất bại, import thất bại, đăng nhập bất thường, OTP quá nhiều, API lỗi, ticket quá hạn, sự cố tồn đọng và job nền thất bại.
- Dashboard: số tenant theo trạng thái, bản quyền sắp hết hạn, doanh thu theo thời gian/gói, tỷ lệ gia hạn, người dùng hoạt động, ticket và tỷ lệ xử lý đúng hạn, sự cố theo mức độ, đăng nhập thất bại, lượng OTP/email và mức dùng tài nguyên theo tenant.
- Các chỉ số vận hành không mặc nhiên cho phép mở nội dung hồ sơ, hợp đồng, hóa đơn, bảng lương hoặc tài liệu nội bộ Tenant.
- Liên hệ mua bán/kinh doanh là chức năng nêu trong Markdown ban đầu nhưng chưa được phân rã chi tiết trong các sơ đồ đã cung cấp.

## 7. Chức năng Tenant — Chủ chung cư

- Xem báo cáo lợi nhuận, thống kê nhân viên, biến động nhân khẩu và tình trạng cơ sở hạ tầng.
- Xem và cấu hình thông tin chung cư.
- Thiết lập nội quy và quản lý tài liệu pháp lý.
- Cập nhật quyền nhân sự cấp cao; đóng băng/mở quyền Quản lý.
- Phê duyệt thanh toán lương và hoàn tiền Tenant do Quản lý trình.
- Cho phép hoặc yêu cầu Platform hỗ trợ trong tenant của mình.
- AI hỗ trợ nâng cao; sơ đồ ghi chú tính năng Premium.

## 8. Chức năng Tenant — Quản lý

- Tra cứu thông tin nhân viên; cập nhật vai trò nhân viên.
- Tạo, tra cứu và thu hồi lời mời nhân sự.
- Tạo kỳ lương; lập nháp thanh toán lương, trình Chủ chung cư duyệt và phát hành sau khi được duyệt.
- Lập nháp hoàn tiền thuộc Tenant, trình Chủ chung cư duyệt và phát hành sau khi được duyệt.
- Báo cáo lại Chủ chung cư sau khi thay đổi vai trò nhân viên; không yêu cầu phê duyệt trước thao tác này.
- Xem báo cáo tài chính tháng/quý/năm, thống kê nhân viên, biến động nhân khẩu và cơ sở hạ tầng.
- Thêm, cập nhật dịch vụ; xem lịch sử thay đổi đơn giá.
- Markdown ban đầu còn nêu AI hỗ trợ công việc và báo cáo tình trạng khai thác căn hộ/vận hành, xử lý sự cố. Các chức năng này chưa được phân rã đầy đủ trong sơ đồ Quản lý; không mặc nhiên loại bỏ chỉ vì sơ đồ chưa thể hiện.

Quyền đổi vai trò nhân viên không được suy diễn thành quyền thay đổi Chủ chung cư hoặc tự nâng quyền Quản lý. Chi tiết giới hạn gán role và hình thức báo cáo sau thay đổi còn cần thiết kế.

## 9. Chức năng Tenant — Nhân viên

### 9.1. Truy cập nội bộ

- Tra cứu bảng lương.
- AI hướng dẫn quy trình làm việc.
- Phạm vi bảng lương được xem (cá nhân hay rộng hơn) chưa được chốt; không mặc nhiên cho mọi STAFF xem lương của toàn bộ nhân sự.

### 9.2. Hóa đơn và căn hộ

- Cập nhật hóa đơn; gửi thông báo hóa đơn; AI gắn cờ bất thường.
- Tra cứu và cập nhật thông tin căn hộ.
- Sơ đồ ghi chú cập nhật/gia hạn hợp đồng; cần thể hiện thành nghiệp vụ hợp đồng khi thiết kế chi tiết.

### 9.3. Cư dân và hợp đồng

- Nhập và đối soát dữ liệu cư dân.
- Gửi email kích hoạt tài khoản và xem danh sách email đã gửi.
- Import và xem hợp đồng căn hộ.

### 9.4. Dịch vụ và khiếu nại

- Tra cứu dịch vụ, xem dịch vụ căn hộ đang sử dụng.
- Hủy/tạm ngưng dịch vụ cho căn hộ.
- Xem khiếu nại dịch vụ.
- Tiếp nhận, xem danh sách và cập nhật tiến độ xử lý khiếu nại chung cư.

## 10. Chức năng Tenant — Cư dân

- Tra cứu và thanh toán hóa đơn được phép thanh toán.
- Xem hồ sơ cá nhân; đổi mật khẩu.
- Xem hợp đồng và thông tin căn hộ được phép truy cập.
- AI hỏi đáp.
- Tạo khiếu nại, theo dõi và xem lịch sử khiếu nại.
- Sơ đồ chưa nêu sửa hồ sơ cá nhân; không tự hiểu “quản lý thông tin cá nhân” là quyền sửa mọi trường hồ sơ.

Quan hệ chính xác giữa cư dân, căn hộ, người ở và bên đứng tên hợp đồng cần được thiết kế để kiểm tra quyền truy cập bản ghi; không chỉ dựa trên cùng `TenantId`.

## 11. Quy trình thanh toán lương và hoàn tiền Tenant

### 11.1. Quyết định đã chốt

1. Quản lý được phép lập bản nháp thanh toán lương và hoàn tiền.
2. Quản lý gửi bản nháp cho Chủ chung cư phê duyệt.
3. Quản lý chỉ được phát hành sau khi Chủ chung cư phê duyệt.
4. Chủ chung cư phê duyệt theo vai trò `TENANT_ADMIN`.
5. Các nghiệp vụ này thuộc Tenant, độc lập với xử lý giao dịch SaaS trong Platform.

### 11.2. Mô hình trạng thái đề xuất để triển khai

```text
Draft -> PendingApproval -> Approved -> Issued
                        -> Rejected -> chỉnh sửa và trình duyệt lại
```

- Các tên trạng thái và cách xử lý từ chối là đề xuất kỹ thuật; chưa xác nhận enum hoặc thiết kế database hiện có.
- Permission, trạng thái hợp lệ, tenant context và dữ liệu được duyệt đều cần được kiểm tra tại server.
- Đề xuất: sửa nội dung tài chính sau phê duyệt phải trình duyệt lại; lưu người lập, người duyệt, người phát hành và phiên bản/nội dung được duyệt.
- `Issued` chỉ thể hiện phát hành; không tự đồng nghĩa với giao dịch thanh toán thành công. Trạng thái xử lý của cổng thanh toán cần thiết kế riêng.

## 12. Permission-based RBAC

### 12.1. Nguyên tắc danh mục và seed

- Permission biểu diễn một khả năng nghiệp vụ có thể cấp/thu hồi riêng.
- `PermissionSeed(Code, Description)` là danh mục permission.
- `TenantRoleSeed(Code, Name, HierarchyLevel, PermissionCodes)` là role mẫu và danh sách quyền.
- Mọi code trong `TenantRoleSeed.PermissionCodes` phải tồn tại trong `TenantPermissions`.
- `TenantPermissions` có thể chứa permission chưa được role mặc định sử dụng để phục vụ mở rộng.
- Platform và Tenant có danh mục riêng; không gán chéo scope.
- Quy ước chung: `<scope>.<resource>.<action>`, ví dụ `tenant.invoices.read`, `platform.subscriptions.manage`.
- Không dùng một quyền `manage` bao trùm lập nháp, duyệt và phát hành khi các thao tác do các tác nhân khác nhau thực hiện.
- Permission không thay thế tenant isolation, kiểm tra quyền sở hữu, trạng thái nghiệp vụ hoặc quyền lợi gói SaaS.
- `HierarchyLevel` không được dùng một mình để cho phép gán role vượt thẩm quyền hoặc bỏ qua phê duyệt.

### 12.2. Nhóm quyền phải được bao phủ

| Scope | Nhóm tài nguyên/nghiệp vụ |
|---|---|
| Platform | Hồ sơ và trạng thái tenant; lời mời Tenant Admin |
| Platform | Subscription/bản quyền, gói dịch vụ và chính sách thương mại |
| Platform | Thanh toán SaaS, hoàn tiền/hủy giao dịch, công nợ và nhắc thanh toán |
| Platform | Yêu cầu hỗ trợ, sự cố, bảo trì, thông báo ảnh hưởng |
| Platform | Giám sát, dashboard và báo cáo nền tảng |
| Tenant | Thông tin chung cư, nội quy, tài liệu pháp lý |
| Tenant | Nhân sự, lời mời, gán vai trò, quyền nhân sự cấp cao |
| Tenant | Kỳ lương và thanh toán lương theo quy trình phê duyệt |
| Tenant | Căn hộ, cư dân, hợp đồng, import và gia hạn |
| Tenant | Hóa đơn, thanh toán cư dân, thông báo và hoàn tiền Tenant |
| Tenant | Danh mục dịch vụ, lịch sử giá và dịch vụ căn hộ |
| Tenant | Khiếu nại và tiến độ xử lý |
| Tenant | Báo cáo tài chính, lợi nhuận, nhân sự, nhân khẩu, khai thác và hạ tầng |
| Tenant | Hồ sơ cá nhân, dữ liệu tự phục vụ và AI |
| Tenant/Platform hỗ trợ | Điều kiện cho phép hỗ trợ tenant cụ thể; không gán chéo danh mục RBAC |

### 12.3. Mã quyền tài chính đề xuất

Các mã dưới đây là đề xuất thống nhất để thiết kế tiếp, chưa phải danh mục seed đã xác nhận tồn tại trong mã nguồn.

| Nhóm | Actions | Role thực hiện theo nghiệp vụ |
|---|---|---|
| `tenant.payroll_batches.*` | `read`, `create`, `update`, `submit`, `issue` | Quản lý; quyền đọc của Chủ chung cư phục vụ phê duyệt |
| `tenant.payroll_batches.*` | `approve`, `reject` | Chủ chung cư |
| `tenant.refunds.*` | `read`, `create`, `update`, `submit`, `issue` | Quản lý; quyền đọc của Chủ chung cư phục vụ phê duyệt |
| `tenant.refunds.*` | `approve`, `reject` | Chủ chung cư |

`*` ở đây là cách viết gọn bảng, không quy định hỗ trợ wildcard trong hệ thống. Quyền `reject` và mô hình hồ sơ thanh toán cụ thể vẫn thuộc phần đề xuất thiết kế.

Các mã như `tenant.payments.reconcile` hoặc `tenant.resident.tickets.create` từng xuất hiện dưới dạng ví dụ. Không vì vậy mà coi mọi nghiệp vụ đối soát thanh toán đã được chốt, hoặc tự đổi tên permission đã lưu trong database. Cần kiểm tra code/seed và thống nhất tên trước khi thay đổi.

## 13. AI và quyền truy cập

- Cư dân: hỏi đáp trong phạm vi được phép.
- Nhân viên: quy trình làm việc và hỗ trợ phát hiện bất thường hóa đơn.
- Quản lý: AI hỗ trợ công việc theo mô tả ban đầu; cần phân rã chi tiết tiếp.
- Chủ chung cư: AI hỗ trợ nâng cao, được định hướng Premium.
- AI không được mở rộng quyền của người sử dụng. Truy xuất tài liệu và tool call phải giữ tenant context và kiểm tra quyền trên dữ liệu/hành động.
- Không mặc nhiên cho AI thực hiện thanh toán, phê duyệt, phát hành hoặc thay đổi role vì có quyền hỏi đáp.
- Kiểm tra quyền lợi gói và permission người dùng là hai điều kiện riêng khi áp dụng tính năng Premium; chi tiết entitlement cần thiết kế tiếp.

## 14. Những điểm chưa chốt

Các điểm này không thay đổi những quyết định đã chốt; chỉ yêu cầu làm rõ khi xây dựng chức năng liên quan.

- Danh mục permission đầy đủ, tên code chính thức và ma trận gán quyền mặc định cho các role.
- Cơ chế hỗ trợ Tenant của Platform: cấp quyền, phạm vi, thời hạn, thu hồi, audit và thực thi RLS.
- Quy trình hoàn tiền giao dịch thương mại Platform.
- Phạm vi tra cứu bảng lương của STAFF.
- Giới hạn gán role của Quản lý và cơ chế báo cáo sau thay đổi vai trò.
- Quan hệ dữ liệu xác định hóa đơn, hợp đồng, căn hộ mà từng Cư dân được truy cập.
- Những chức năng chưa phân rã đầy đủ: liên hệ kinh doanh; AI Quản lý; khai thác căn hộ; ghi nhận hạ tầng/sự cố để lập báo cáo.
- Thao tác tạo/phát hành/hủy hóa đơn, chi tiết vòng đời hợp đồng, đăng ký/khôi phục dịch vụ và kết thúc khiếu nại chưa đủ thông tin để tự bổ sung thành yêu cầu đã chốt.
- Các mốc thời gian hiệu lực giá dịch vụ, nội dung bản duyệt và kết quả giao dịch thanh toán.

## 15. Nguyên tắc sử dụng tài liệu khi hỗ trợ dự án

- Dùng tài liệu này làm ngữ cảnh nghiệp vụ chung; ưu tiên quyết định mới được chủ dự án xác nhận rõ ràng nếu có thay đổi.
- Phân biệt yêu cầu đã chốt, đề xuất thiết kế và trạng thái triển khai thực tế.
- Không tự mở rộng quyền, thêm tác nhân hoặc thay đổi scope dựa trên chức danh quản trị.
- Không chia STAFF theo phòng ban nếu chưa có yêu cầu mới.
- Khi tài liệu và code khác nhau, nêu chênh lệch và đối chiếu yêu cầu hiện tại; không khẳng định code đã đáp ứng tài liệu khi chưa kiểm tra.
- Khi thay đổi nghiệp vụ được xác nhận, cập nhật tài liệu này để duy trì ngữ cảnh lâu dài.

## 16. Nguồn tham chiếu

- Markdown “EzCondo — AI Project Context” do chủ dự án cung cấp.
- Năm sơ đồ: `Nhân viên.drawio`, `Quản lý.drawio`, `Quản trị viên.drawio`, `Chủ chung cư.drawio`, `Cư Dân.drawio`.
- Đường dẫn sơ đồ tại thời điểm đọc: `G:/My Drive/MyData/DuyTanUniversity/Khoa_Luan_Tot_Nghiep/KLTN2/Sơ đồ/`.
- Các quyết định trong cuộc trao đổi: STAFF chung; Chủ chung cư = TENANT_ADMIN; Quản lý lập/trình/phát hành tài chính sau phê duyệt; đổi vai trò nhân viên rồi báo cáo; hai scope độc lập và Platform chỉ can thiệp Tenant khi Chủ chung cư cho phép/yêu cầu hỗ trợ.

Nội dung chức năng từ các sơ đồ đã được tổng hợp ở đây để giảm phụ thuộc vào đường dẫn nguồn bên ngoài repository.
