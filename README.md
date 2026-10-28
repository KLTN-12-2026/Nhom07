Nhom07

Xây dựng nền tảng SaaS Multi-tenant EzCondo quản lý vận hành chung cư tích hợp AI - GVHD: ThS. Phan Long

## Giới thiệu

EzCondo được xây dựng nhằm số hóa hoạt động quản lý và vận hành chung cư. Hệ thống phân tách hai phạm vi độc lập:

- **Platform:** quản lý nền tảng SaaS, tenant, gói dịch vụ, subscription, thanh toán thương mại và hỗ trợ vận hành.
- **Tenant:** quản lý hoạt động nội bộ của từng chung cư như căn hộ, cư dân, hợp đồng, dịch vụ, hóa đơn, nhân sự, tiền lương và khiếu nại.

Dự án đang trong quá trình phát triển phục vụ khóa luận tốt nghiệp.

## Công nghệ sử dụng

- C# và .NET 8
- ASP.NET Core Web API và MVC
- Entity Framework Core 8
- PostgreSQL và Npgsql
- ASP.NET Core Identity và JWT
- Permission-based RBAC
- Swagger / OpenAPI

## Cấu trúc solution

- `EzCondo.BackEndServer`: Web API và database model.
- `EzCondo.Portal`: ứng dụng ASP.NET Core MVC.
- `EzCondo.ViewModel`: các model dùng chung.
- `docs`: tài liệu và ngữ cảnh dự án.
- `EzCondo_Solution2.slnx`: solution chính.

## Vai trò nghiệp vụ

### Platform

- Quản trị viên nền tảng.

### Tenant

- `TENANT_ADMIN`: Chủ chung cư.
- `MANAGER`: Quản lý.
- `STAFF`: Nhân viên.
- `RESIDENT`: Cư dân.

Platform RBAC và Tenant RBAC là hai phạm vi phân quyền độc lập. Quản trị viên Platform không mặc nhiên có quyền truy cập dữ liệu nghiệp vụ của Tenant.

## Yêu cầu môi trường

- .NET SDK 8.x
- PostgreSQL
- Visual Studio 2026 hoặc IDE hỗ trợ .NET 8
- Git

## Khởi chạy dự án

```powershell
git clone https://github.com/KLTN-12-2026/Nhom07.git
cd Nhom07
dotnet restore
dotnet build
dotnet run --project EzCondo.BackEndServer