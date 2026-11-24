namespace EzCondo.Mobile.Contracts;

public record Account(string Id, string Name, string Role, string TenantName, string? ApartmentCode, bool CanInviteResidents = false);
public record SessionRequest(string AccountId);
public record LoginRequest(string Email, string Password);
public record ActivateInvitationDto(string Token, string Password, string ConfirmPassword);
public record ActivatedInvitationDto(string Email);
public record InviteResidentDto(string FullName, string Email);
public record ResidentInvitationDto(Guid Id, DateTimeOffset ExpiresAt);
public record ServerInfo(string Backend, string Database);
public record ServiceCatalogDto(string Id, string Code, string Name, decimal Price);
public record WorkRecordDto(string Month, decimal StandardDays, decimal PaidLeaveDays, decimal UnpaidLeaveDays,
    decimal OvertimeHours, string? Note);
public record PayrollDto(string Title, DateTimeOffset? PublishedAt, decimal BaseSalary, decimal Allowance,
    decimal Deduction, decimal NetAmount);
public record SessionResponse(string Token, Account Account);
public record Profile(Account Account, string Phone, string Email, string Address, string? Department,
    string? JobTitle, List<VehicleDto> Vehicles);
public record VehicleDto(string Type, string Plate);
public record Overview(int OpenComplaints, int PendingParcels, decimal AmountDue, int ShiftCount);
public record InvoiceDto(string Id, string Code, string Month, string DueDate, string Status, decimal Total,
    List<InvoiceLine> Items);
public record InvoiceLine(string Description, double Quantity, string Unit, decimal UnitPrice, decimal Amount);
public record ContractDto(string Number, string Type, string StartDate, string EndDate, string Status,
    long? MonthlyRent, long? Deposit);
public record ParcelDto(string TrackingCode, string Courier, string ArrivedAt, string Status, string? Locker);
public record ComplaintDto(string Id, string Code, string Title, string Description, string Category,
    string ApartmentCode, string Status, string Priority, string CreatedAt, string? AssignedTo, string? Deadline,
    List<MessageDto> Messages, List<string>? Images = null);
public record MessageDto(string Id, string SenderName, string SenderRole, string Content, string Timestamp);
public record CreateComplaint(string Title, string Category, string Priority, string Description);
public record CreateMessage(string Content);
public record ChangeStatus(string ExpectedStatus, string Status);
public record BookingDto(string Id, string FacilityName, string Date, string TimeSlot, int Guests, string Status);
public record CreateBooking(string FacilityName, string Date, string TimeSlot, int Guests);
public record ServiceDto(string Id, string Code, string Type, string Date, string Details, string Status);
public record CreateService(string Type, string Date, string Details);
public record ShiftDto(string Date, string ShiftName, string? Notes, string Department);

public static class Choices
{
    public static readonly string[] Categories = ["Kỹ thuật - Nước", "Kỹ thuật - Điện", "An ninh - Trật tự", "Vệ sinh môi trường", "Thang máy", "Khác"];
    public static readonly string[] Priorities = ["low", "medium", "high", "urgent"];
    public static readonly string[] Facilities = ["Khu tiệc nướng BBQ", "Sân Tennis", "Hồ bơi vô cực"];
    public static readonly string[] TimeSlots = ["06:00 - 08:00", "08:00 - 10:00", "16:00 - 18:00", "18:00 - 21:00"];
    public static readonly string[] ServiceTypes = ["Sửa chữa điện nước", "Vệ sinh căn hộ", "Đăng ký chuyển đồ", "Đăng ký khách lưu trú"];
    public static string Status(string value) => value switch
    {
        "new" => "Mới tiếp nhận", "in_progress" => "Đang xử lý", "waiting_resident" => "Chờ cư dân",
        "resolved" => "Đã xử lý", "closed" => "Đã đóng", "paid" => "Đã thanh toán",
        "unpaid" => "Chưa thanh toán", "overdue" => "Quá hạn", "active" => "Đang hiệu lực",
        "expired" => "Hết hạn", "expiring_soon" => "Sắp hết hạn", "terminated" => "Đã chấm dứt",
        "low" => "Thấp", "medium" => "Bình thường", "high" => "Cao", "urgent" => "Khẩn cấp", _ => value
    };
}
