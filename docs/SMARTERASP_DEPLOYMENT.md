# Triển khai EzCondo trên SmarterASP.NET

> Cập nhật: 11/11/2026.
> Phạm vi hiện tại: triển khai backend `EzCondo.BackEndServer` của source `EzCondo_Integration` để kiểm tra Swagger và các chức năng Platform đã xây dựng. Đây là môi trường demo/staging, chưa phải xác nhận hệ thống đủ an toàn để phát hành production.

## 1. Quyết định triển khai

- SmarterASP.NET là nhà cung cấp Windows hosting/IIS, không phải ASP.NET Core middleware.
- Runtime của backend là .NET 8; hình thức publish ưu tiên là Web Deploy từ Visual Studio 2022 bằng publish profile tải từ Hosting Control Panel.
- Database là PostgreSQL do SmarterASP.NET cấp. Ứng dụng dùng EF Core/Npgsql và hiện tự chạy migration cùng seed khi khởi động.
- Dùng một site hoặc subdomain riêng cho API, ví dụ `api-dev.<domain>`; kiểm tra trước bằng temporary URL của hosting rồi mới trỏ domain.
- Giữ `ASPNETCORE_ENVIRONMENT=Production`. Swagger chỉ được mở bằng cấu hình riêng `Swagger__Enabled=true`; không chuyển toàn bộ ứng dụng sang `Development` chỉ để hiển thị Swagger.
- Secret và thông tin kết nối không ghi vào `appsettings.json`, publish profile hoặc Git. Cấu hình bằng Environment Variables của application pool hoặc cơ chế secret tương đương trong Hosting Control Panel.
- Site API nên dùng application pool riêng nếu tài khoản cho phép, vì biến môi trường trên SmarterASP.NET có phạm vi application pool.
- Bật HTTPS bằng Managed Let's Encrypt sau khi domain/subdomain đã trỏ đúng.

## 2. Trạng thái source ảnh hưởng đến lần deploy đầu

`EzCondo.BackEndServer/Program.cs` hiện có các đặc điểm sau:

1. Swagger chỉ chạy trong `Development`, nên publish Release/Production nguyên trạng sẽ không có `/swagger`.
2. Startup bắt buộc có `ConnectionStrings:DefaultConnection` và `Jwt:SigningKey` tối thiểu 32 byte.
3. Startup chạy `Database.MigrateAsync()`, seed permission/role và seed tài khoản Platform. Database phải truy cập được, user database phải có quyền tạo/thay đổi schema, và cấu hình tài khoản seed phải hợp lệ.
4. `PlatformAccounts:Accounts:0:Password` hiện không được cấu hình trong `appsettings.json`; nếu giữ phần tử account hiện có thì production phải cấp password qua secret/environment variable, nếu không ứng dụng dừng khi startup.
5. Source chưa đăng ký authentication/authorization policy và controller hiện chưa được bảo vệ. Swagger công khai đồng nghĩa các endpoint thay đổi dữ liệu cũng đang công khai. Chỉ dùng site này làm demo/staging có kiểm soát cho đến khi hoàn thiện bảo mật.

## 3. Thay đổi code tối thiểu để bật Swagger an toàn hơn

Giữ đăng ký hiện có:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

Thêm cờ cấu hình trước `builder.Build()` hoặc đọc trực tiếp sau khi build:

```csharp
var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");
```

Đổi đoạn pipeline Swagger thành:

```csharp
if (app.Environment.IsDevelopment() || swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EzCondo API v1");
        options.RoutePrefix = "swagger";
    });
}
```

Trên hosting đặt `Swagger__Enabled=true`. Khi không còn cần Swagger công khai, đổi thành `false` hoặc xóa biến.

`__` là cách ASP.NET Core ánh xạ biến môi trường vào key cấu hình phân cấp: `Swagger__Enabled` tương đương `Swagger:Enabled`.

## 4. Biến môi trường cần cấu hình

Giá trị bên dưới là tên key, không phải giá trị mẫu để sao chép nguyên xi.

| Biến | Ý nghĩa |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Swagger__Enabled` | `true` trong giai đoạn demo Swagger |
| `ConnectionStrings__DefaultConnection` | Chuỗi kết nối PostgreSQL lấy từ Hosting Control Panel; phải dùng đúng host và port được cấp |
| `Jwt__SigningKey` | Secret ngẫu nhiên, ít nhất 32 byte UTF-8 |
| `Jwt__Issuer` | URL HTTPS của API hoặc issuer đã thống nhất |
| `Jwt__Audience` | Audience đã thống nhất cho client EzCondo |
| `PlatformAccounts__Accounts__0__Email` | Email tài khoản quản trị Platform cần seed |
| `PlatformAccounts__Accounts__0__FullName` | Họ tên tài khoản seed |
| `PlatformAccounts__Accounts__0__CitizenId` | Định danh theo thiết kế hiện tại; xem là dữ liệu nhạy cảm |
| `PlatformAccounts__Accounts__0__Password` | Mật khẩu seed đáp ứng Identity policy hiện tại |
| `Cors__AllowedOrigins__0` | Origin frontend đầy đủ, ví dụ `https://app.<domain>`; không thêm dấu `/` cuối |

Lưu ý: cấu hình .NET không phân biệt hoa/thường theo mặc định, nhưng nên thống nhất `Cors` thay vì trộn `cors` để dễ quản lý.

Chuỗi kết nối Npgsql thường có hình dạng:

```text
Host=<host>;Port=<port>;Database=<database>;Username=<username>;Password=<password>;SSL Mode=Require
```

Phải dùng thông tin và yêu cầu SSL thực tế của tài khoản SmarterASP.NET; không mặc định PostgreSQL chạy cổng 5432. Chỉ thêm `Trust Server Certificate=true` nếu SmarterASP.NET xác nhận chứng thư của server cần cấu hình này, vì tùy chọn đó làm giảm mức kiểm tra chứng thư TLS.

## 5. Quy trình triển khai lần đầu

### Bước A — Tạo tài nguyên hosting

1. Tạo/đăng nhập tài khoản SmarterASP.NET và chọn Windows ASP.NET hosting có .NET 8 và PostgreSQL.
2. Trong Hosting Control Panel, tạo site riêng cho API. Giai đoạn đầu dùng temporary URL do hosting cấp.
3. Nếu plan hỗ trợ, tạo application pool riêng rồi chuyển site API vào pool đó.
4. Vào `Databases > PostgreSQL > + Database`, chọn version, đặt tên database, mật khẩu và quota.
5. Sao chép chính xác hostname, port, database, username và password từ Control Panel. Có thể kiểm tra kết nối trước bằng pgAdmin.

### Bước B — Chuẩn bị ứng dụng

1. Áp dụng thay đổi bật Swagger bằng `Swagger:Enabled` ở mục 3.
2. Build và chạy Release tại local với một PostgreSQL test trước khi publish.
3. Đảm bảo migration hiện tại chạy thành công trên database trống.
4. Không đưa password, JWT signing key hoặc connection string thật vào Git.

### Bước C — Cấu hình site

1. Vào `Advanced Tools > Pool Manager > Actions > Environment Variables` của đúng pool.
2. Khai báo các biến ở mục 4.
3. Vì biến có phạm vi application pool, không dùng chung pool với site cần giá trị khác. Nếu buộc dùng chung, cân nhắc cấu hình riêng trong `web.config` được tạo khi publish, nhưng không commit secret.
4. Recycle application pool sau khi thay đổi biến.

### Bước D — Web Deploy bằng Visual Studio 2022

1. Mở solution `EzCondo_Solution2.slnx` từ source `EzCondo_Integration`.
2. Chọn đúng project `EzCondo.BackEndServer`, không publish toàn solution, `EzCondo.Portal` hay frontend.
3. Trong SmarterASP.NET: `WEBSITES > site API > Manage Website > VS Webdeploy`; bật Web Deploy và tải publish profile XML.
4. Trong Visual Studio: chuột phải `EzCondo.BackEndServer > Publish > Import Profile`, chọn file vừa tải.
5. Kiểm tra `Configuration = Release`, `Target Framework = net8.0`, `Deployment Mode = Framework-dependent`, `Target Runtime = Portable` (SmarterASP.NET công bố có .NET 8). Chỉ chuyển sang self-contained `win-x64` khi runtime server thực tế không phù hợp.
6. Validate Connection rồi Publish. Không commit file `.pubxml`/`.publishsettings` vì chứa thông tin triển khai nhạy cảm.
7. Lần đầu startup có thể lâu hơn vì EF Core chạy migration và seed. Không publish song song nhiều instance trong lần khởi tạo đầu.

### Bước E — Kiểm tra temporary URL

Theo thứ tự:

1. `https://<temporary-url>/swagger/v1/swagger.json` phải trả JSON.
2. `https://<temporary-url>/swagger` phải mở Swagger UI.
3. Kiểm tra `GET /WeatherForecast` và các API đọc trước.
4. Chỉ thử API ghi dữ liệu trên database demo và xác nhận dữ liệu tạo đúng tenant/scope.
5. Kiểm tra bảng `__EFMigrationsHistory`, permission, role và tài khoản Platform seed trong PostgreSQL.

Nếu site lỗi 500/timeout, ưu tiên kiểm tra log ứng dụng/IIS, biến môi trường, kết nối PostgreSQL, quyền migration và recycle app pool. Site Diagnostic của SmarterASP.NET giúp phân biệt lỗi application với DNS/SSL.

## 6. Gắn domain và HTTPS

Khuyến nghị dùng `api-dev.<domain>` trong giai đoạn này.

1. Thêm domain/subdomain vào site API trong Hosting Control Panel.
2. Trỏ DNS bằng nameserver SmarterASP.NET (`NS1.SITE4NOW.NET`, `NS2.SITE4NOW.NET`, `NS3.SITE4NOW.NET`) hoặc tạo A/CNAME theo thông tin IP/target mà Control Panel cấp.
3. Chờ DNS propagate và kiểm tra temporary URL vẫn hoạt động.
4. Vào `SSL > Request Free SSL`, chọn domain/subdomain để cấp Managed Let's Encrypt.
5. Đổi `Jwt__Issuer`, CORS và các URL client sang HTTPS domain chính thức; recycle pool.
6. Kiểm tra lại `https://api-dev.<domain>/swagger`.

## 7. Tiêu chí hoàn tất giai đoạn Swagger

- Site chạy ở `Production`; Swagger chỉ mở bởi `Swagger__Enabled=true`.
- Temporary URL và domain HTTPS đều mở được `/swagger`.
- `/swagger/v1/swagger.json` trả 200.
- Migration/seed chạy đúng một cách lặp lại được khi recycle.
- Không có secret trong repository, publish profile hoặc ảnh chụp màn hình chia sẻ.
- Đã ghi nhận rõ site là demo/staging và endpoint chưa an toàn để phát hành công khai cho đến khi hoàn thiện authentication/authorization, login/logout, permission policy và retest toàn bộ endpoint Platform.

## 8. Công việc sau giai đoạn này

1. Hoàn thiện các chức năng Quản trị viên đang ưu tiên.
2. Quay lại authentication/authorization, login/logout, JWT bearer, permission policy và retest toàn bộ endpoint Platform.
3. Bảo vệ hoặc tắt Swagger ngoài môi trường cần thiết.
4. Tách migration khỏi startup hoặc thiết kế quy trình migration có kiểm soát trước khi chạy nhiều instance/production thật.
5. Thiết lập backup PostgreSQL và kiểm thử restore.
6. Sau khi review xong chức năng Khôi phục tenant, hoàn thiện phần gửi thông báo tạm ngưng và background worker chuyển tenant sang `Suspended`; chức năng Tạm ngưng hiện chưa được coi là hoàn tất đầy đủ.

## 9. Tài liệu SmarterASP.NET tham chiếu

- ASP.NET Core hosting và phiên bản hỗ trợ: https://www.smarterasp.net/asp.net_core_hosting
- Publish ASP.NET Core bằng Visual Studio 2022: https://www.smarterasp.net/support/kb/a2286/how-to-publish-asp_net-core-web-app-visual-studio-2022.aspx
- Web Deploy profile: https://www.smarterasp.net/support/kb/a2429/where-can-i-find-my-web-deploy-login-information.aspx
- Tạo PostgreSQL: https://www.smarterasp.net/support/kb/a2449/how-can-i-create-a-postgresql-database-from-the-hosting-control-panel.aspx
- Kết nối PostgreSQL/pgAdmin: https://www.smarterasp.net/support/kb/a2452/how-to-connect-with-a-postgresql-client-application.aspx
- Environment Variables: https://www.smarterasp.net/support/kb/a2156/how-to-set-environment-variable-for-your-account.aspx
- Domain/DNS: https://www.smarterasp.net/support/kb/a2090/how-to-point-domains-to-my-hosting-account.aspx
- Managed Let's Encrypt: https://www.smarterasp.net/support/kb/a1885/do-you-support-lets-encrypt-free-ssl-tls-certificates-in-shared-hosting.aspx
- Site Diagnostic: https://www.smarterasp.net/support/kb/a2484/how-to-troubleshoot-a-website-that-is-not-working-with-site-diagnostic.aspx
