using System.Globalization;
using EZCondoAPI.Data;
using EZCondoAPI.Models;
using EzCondo.Mobile.Contracts;
using Microsoft.EntityFrameworkCore;

namespace EzCondo.Mobile.Api;

public static class MobileEndpoints
{
    public static void Map(WebApplication app)
    {
        var api = app.MapGroup("/api/mobile");
        api.MapGet("/demo-accounts", async (EZCondoDbContext db) => await db.UserAccounts
            .Where(u => (u.Role == "resident" || u.Role == "staff") && u.Tenant != null
                && (u.Tenant.Status == "active" || u.Tenant.Status == "trial"))
            .Select(u => new Account(u.Id, u.Name, u.Role, u.Tenant!.Name, u.ApartmentCode)).ToListAsync());
        api.MapPost("/sessions", async (SessionRequest request, MobileAccess access) =>
            await access.Load(request.AccountId)
                ? Results.Ok(new SessionResponse(access.Issue(request.AccountId), access.Account()))
                : Results.Unauthorized());
        var secured = api.MapGroup("").AddEndpointFilter<SessionFilter>();
        secured.MapGet("/profile", async (MobileAccess a, EZCondoDbContext db) =>
        {
            var vehicles = a.IsResident ? await db.Vehicles.Where(v => v.Resident != null
                && v.Resident.TenantId == a.User.TenantId && v.Resident.Name == a.User.Name
                && a.ApartmentCodes.Contains(v.Resident.ApartmentCode))
                .Select(v => new VehicleDto(v.Type, v.Plate)).ToListAsync() : [];
            return new Profile(a.Account(), a.User.Phone, a.User.Email, a.User.Tenant!.Address,
                a.Staff?.Department, a.Staff?.Role, vehicles);
        });
        secured.MapGet("/overview", async (MobileAccess a, EZCondoDbContext db) => new Overview(
            await a.Complaints().CountAsync(c => c.Status != "resolved" && c.Status != "closed"),
            a.IsResident ? await db.Parcels.CountAsync(p => a.ApartmentCodes.Contains(p.ApartmentCode)
                && p.RecipientName == a.User.Name && p.Status != "Đã nhận") : 0,
            a.IsResident ? await db.Invoices.Where(i => a.ApartmentCodes.Contains(i.ApartmentCode)
                && i.ResidentName == a.User.Name && i.Status != "paid").SumAsync(i => i.TotalAmount) : 0,
            a.Staff == null ? 0 : await db.ShiftAssignments.CountAsync(s => s.StaffId == a.Staff.Id)));
        secured.MapGet("/invoices", async (MobileAccess a, EZCondoDbContext db) =>
        {
            if (!a.IsResident) return Results.StatusCode(403);
            return Results.Ok(await db.Invoices.AsNoTracking().Where(i => a.ApartmentCodes.Contains(i.ApartmentCode)
                && i.ResidentName == a.User.Name).OrderByDescending(i => i.IssueDate)
                .Select(i => new InvoiceDto(i.Id, i.InvoiceCode, i.Month, i.DueDate, i.Status, i.TotalAmount,
                    i.Items.Select(x => new InvoiceLine(x.Description, x.Quantity, x.Unit, x.UnitPrice, x.Amount)).ToList())).ToListAsync());
        });
        secured.MapGet("/contracts", async (MobileAccess a, EZCondoDbContext db) =>
        {
            if (!a.IsResident) return Results.StatusCode(403);
            return Results.Ok(await db.Contracts.AsNoTracking().Where(c => a.ApartmentCodes.Contains(c.ApartmentCode)
                && c.ResidentName == a.User.Name).Select(c => new ContractDto(c.ContractNumber, c.Type,
                    c.StartDate, c.EndDate, c.Status, c.MonthlyRent, c.Deposit)).ToListAsync());
        });
        secured.MapGet("/parcels", async (MobileAccess a, EZCondoDbContext db) =>
        {
            if (!a.IsResident) return Results.StatusCode(403);
            return Results.Ok(await db.Parcels.AsNoTracking().Where(p => a.ApartmentCodes.Contains(p.ApartmentCode)
                && p.RecipientName == a.User.Name).OrderByDescending(p => p.ArrivedAt)
                .Select(p => new ParcelDto(p.TrackingCode, p.Courier, p.ArrivedAt, p.Status, p.LockerNumber)).ToListAsync());
        });
        secured.MapGet("/complaints", async (MobileAccess a) => (await a.Complaints().AsNoTracking()
            .Include(c => c.Messages).OrderByDescending(c => c.CreatedAt).ToListAsync()).Select(ToDto));
        secured.MapPost("/complaints", async (CreateComplaint r, MobileAccess a, EZCondoDbContext db) =>
        {
            if (!a.IsResident) return Results.StatusCode(403);
            if (!Text(r.Title, 5, 150) || !Text(r.Description, 10, 3000)
                || !Choices.Categories.Contains(r.Category) || !Choices.Priorities.Contains(r.Priority))
                return Bad("Vui lòng nhập tiêu đề 5–150 ký tự, nội dung 10–3000 ký tự và chọn phân loại hợp lệ.");
            var c = new Complaint { Id = Guid.NewGuid().ToString("N"), TicketCode = "KN-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
                Title = r.Title.Trim(), Description = r.Description.Trim(), Category = r.Category, Priority = r.Priority,
                ApartmentCode = a.User.ApartmentCode!, ResidentName = a.User.Name, ResidentPhone = a.User.Phone,
                Status = "new", CreatedAt = Now(), UpdatedAt = Now() };
            db.Complaints.Add(c);
            await db.SaveChangesAsync();
            return Results.Created($"/api/mobile/complaints/{c.Id}", ToDto(c));
        });
        secured.MapGet("/complaints/{id}", async (string id, MobileAccess a) =>
        {
            var c = await a.Complaints().AsNoTracking().Include(c => c.Messages).SingleOrDefaultAsync(c => c.Id == id);
            return c == null ? Results.NotFound() : Results.Ok(ToDto(c));
        });
        secured.MapPost("/complaints/{id}/messages", async (string id, CreateMessage r, MobileAccess a, EZCondoDbContext db) =>
        {
            if (!Text(r.Content, 1, 2000)) return Bad("Tin nhắn cần có từ 1 đến 2000 ký tự.");
            var c = await a.Complaints().SingleOrDefaultAsync(c => c.Id == id);
            if (c == null) return Results.NotFound();
            if (c.Status == "closed") return Bad("Phản ánh đã đóng, vui lòng tạo phản ánh mới.");
            var message = new ComplaintMessage { Id = Guid.NewGuid().ToString("N"), ComplaintId = id,
                SenderName = a.User.Name, SenderRole = a.IsResident ? "Cư dân" : "Nhân viên", Content = r.Content.Trim(), Timestamp = Now() };
            db.ComplaintMessages.Add(message);
            c.UpdatedAt = Now();
            await db.SaveChangesAsync();
            return Results.Ok(new MessageDto(message.Id, message.SenderName, message.SenderRole, message.Content, message.Timestamp));
        });
        secured.MapPatch("/complaints/{id}/status", async (string id, ChangeStatus r, MobileAccess a, EZCondoDbContext db) =>
        {
            if (a.IsResident) return Results.StatusCode(403);
            var allowed = (r.ExpectedStatus, r.Status) switch
            {
                ("new", "in_progress") => true,
                ("in_progress", "waiting_resident" or "resolved") => true,
                ("waiting_resident", "in_progress" or "resolved") => true,
                _ => false
            };
            if (!allowed) return Bad("Chuyển trạng thái không hợp lệ.");
            if (!await a.Complaints().AnyAsync(c => c.Id == id)) return Results.NotFound();
            var changed = await a.Complaints().Where(c => c.Id == id && c.Status == r.ExpectedStatus)
                .ExecuteUpdateAsync(set => set.SetProperty(c => c.Status, r.Status).SetProperty(c => c.UpdatedAt, Now()));
            return changed == 1 ? Results.Ok() : Results.Conflict(new { message = "Công việc đã được cập nhật. Vui lòng tải lại." });
        });
        secured.MapGet("/bookings", async (MobileAccess a, EZCondoDbContext db) =>
        {
            if (!a.IsResident) return Results.StatusCode(403);
            return Results.Ok(await db.FacilityBookings.AsNoTracking().Where(b => a.ApartmentCodes.Contains(b.ApartmentCode)
                && b.ResidentName == a.User.Name).OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingDto(b.Id, b.FacilityName, b.BookingDate, b.TimeSlot, b.GuestCount, b.Status)).ToListAsync());
        });
        secured.MapPost("/bookings", async (CreateBooking r, MobileAccess a, EZCondoDbContext db) =>
        {
            if (!a.IsResident) return Results.StatusCode(403);
            if (!Choices.Facilities.Contains(r.FacilityName) || !Choices.TimeSlots.Contains(r.TimeSlot)
                || r.Guests is < 1 or > 20 || !FutureDate(r.Date))
                return Bad("Chọn tiện ích, khung giờ, ngày trong 90 ngày tới và số người từ 1 đến 20.");
            // Serialize the duplicate check with the write so concurrent submissions cannot create duplicates.
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            if (await db.FacilityBookings.AnyAsync(b => b.ApartmentCode == a.User.ApartmentCode
                && b.ResidentName == a.User.Name && b.FacilityName == r.FacilityName && b.BookingDate == r.Date
                && b.TimeSlot == r.TimeSlot && b.Status != "Đã hủy"))
                return Results.Conflict(new { message = "Bạn đã đăng ký tiện ích trong khung giờ này." });
            var b = new FacilityBooking { Id = Guid.NewGuid().ToString("N"), ApartmentCode = a.User.ApartmentCode!,
                ResidentName = a.User.Name, FacilityName = r.FacilityName, BookingDate = r.Date, TimeSlot = r.TimeSlot,
                GuestCount = r.Guests, Status = "Chờ duyệt" };
            db.FacilityBookings.Add(b);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return Results.Created($"/api/mobile/bookings/{b.Id}", new BookingDto(b.Id, b.FacilityName, b.BookingDate, b.TimeSlot, b.GuestCount, b.Status));
        });
        secured.MapGet("/services", async (MobileAccess a, EZCondoDbContext db) =>
        {
            if (!a.IsResident) return Results.StatusCode(403);
            return Results.Ok(await db.ServiceRequests.AsNoTracking().Where(s => a.ApartmentCodes.Contains(s.ApartmentCode)
                && s.ResidentName == a.User.Name).OrderByDescending(s => s.CreatedAt)
                .Select(s => new ServiceDto(s.Id, s.RequestCode, s.Type, s.StartDate, s.Details, s.Status)).ToListAsync());
        });
        secured.MapPost("/services", async (CreateService r, MobileAccess a, EZCondoDbContext db) =>
        {
            if (!a.IsResident) return Results.StatusCode(403);
            if (!Choices.ServiceTypes.Contains(r.Type) || !FutureDate(r.Date) || !Text(r.Details, 10, 2000))
                return Bad("Chọn dịch vụ, ngày trong 90 ngày tới và nhập mô tả 10–2000 ký tự.");
            var s = new ServiceRequest { Id = Guid.NewGuid().ToString("N"), RequestCode = "DV-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
                ApartmentCode = a.User.ApartmentCode!, ResidentName = a.User.Name, ResidentPhone = a.User.Phone,
                Type = r.Type, StartDate = r.Date, Details = r.Details.Trim(), Status = "Chờ duyệt", CreatedAt = Now() };
            db.ServiceRequests.Add(s);
            await db.SaveChangesAsync();
            return Results.Created($"/api/mobile/services/{s.Id}", new ServiceDto(s.Id, s.RequestCode, s.Type, s.StartDate, s.Details, s.Status));
        });
        secured.MapGet("/shifts", async (MobileAccess a, EZCondoDbContext db) =>
        {
            if (a.IsResident) return Results.StatusCode(403);
            return Results.Ok(await db.ShiftAssignments.AsNoTracking().Where(s => s.StaffId == a.Staff!.Id && s.ShiftSchedule != null)
                .OrderByDescending(s => s.ShiftSchedule!.Date).Select(s => new ShiftDto(s.ShiftSchedule!.Date,
                    s.ShiftSchedule.ShiftName, s.ShiftSchedule.Notes, s.Department)).ToListAsync());
        });
    }

    private static bool Text(string? value, int min, int max) => value?.Trim().Length >= min && value.Trim().Length <= max;
    private static DateOnly Today() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "SE Asia Standard Time").DateTime);
    private static bool FutureDate(string? date) => DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
        DateTimeStyles.None, out var parsed) && parsed >= Today() && parsed <= Today().AddDays(90);
    private static string Now() => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "SE Asia Standard Time").ToString("yyyy-MM-dd HH:mm:ss");
    private static IResult Bad(string message) => Results.BadRequest(new { message });
    private static ComplaintDto ToDto(Complaint c) => new(c.Id, c.TicketCode, c.Title, c.Description, c.Category,
        c.ApartmentCode, c.Status, c.Priority, c.CreatedAt, c.AssignedTo, c.Deadline,
        c.Messages.OrderBy(m => m.Timestamp).ThenBy(m => m.Id).Select(m => new MessageDto(m.Id, m.SenderName, m.SenderRole, m.Content, m.Timestamp)).ToList());
}
