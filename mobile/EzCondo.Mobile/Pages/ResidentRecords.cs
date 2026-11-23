using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class InvoicesPage() : DataPage("Hóa đơn", residentHeader: true)
{
    private List<InvoiceDto> invoices = [];
    private string? selectedId;
    protected override async Task Load()
    {
        invoices = await App.Api.Get<List<InvoiceDto>>("invoices");
        Render();
    }
    private void Render()
    {
        Body.Clear();
        Body.Add(Ui.Stack(Ui.Text("HÓA ĐƠN & DỊCH VỤ", 11, Ui.Muted),
            Ui.Text("Căn hộ " + App.Api.Account!.ApartmentCode, 24, bold: true)));
        if (invoices.Count == 0) { Empty("Bạn chưa có hóa đơn."); return; }
        var invoice = invoices.FirstOrDefault(i => i.Id == selectedId) ?? invoices[0];
        selectedId = invoice.Id;
        Body.Add(Ui.BillHero(invoice, "Sao chép mã hóa đơn", async () =>
        {
            await Clipboard.Default.SetTextAsync(invoice.Code);
            await DisplayAlert("Đã sao chép", invoice.Code, "Đóng");
        }));
        Body.Add(Ui.Heading("Kỳ thanh toán", invoices.Count + " kỳ"));
        Body.Add(Ui.Filters(invoices.Select(i => Ui.Chip("Tháng " + i.Month, i.Id == selectedId, () =>
        {
            selectedId = i.Id;
            Render();
            return Task.CompletedTask;
        }))));
        Body.Add(Ui.Heading("Bảng kê chi tiết chi phí", invoice.Items.Count + " khoản mục"));
        foreach (var item in invoice.Items) Body.Add(InvoicePage.Line(item, invoice.Status));
        Body.Add(Ui.Card(Ui.Leading(Ui.IconBadge("\ue873"), Ui.Section("Thông tin hóa đơn")),
            Ui.Row(Ui.Text("Mã hóa đơn", 13, Ui.Muted), Ui.Text(invoice.Code, 13, Ui.Primary, true)),
            Ui.Row(Ui.Text("Hạn thanh toán", 13, Ui.Muted), Ui.Text(Ui.Date(invoice.DueDate), 13, bold: true)),
            Ui.Row(Ui.Text("Tổng cộng", 14), Ui.Text(Ui.Money(invoice.Total), 20, Ui.Primary, true)),
            Ui.Action("Chi tiết " + invoice.Code, () => Open(new InvoicePage(invoice)), true)));
        if (invoice.Status != "paid")
            Body.Add(Ui.Card(Ui.Text("Cần hỗ trợ thanh toán?", 15, bold: true),
                Ui.Text("Liên hệ ban quản lý " + App.Api.Account.TenantName + " và cung cấp mã hóa đơn để được hướng dẫn.", 13, Ui.Muted)));
    }
}

public sealed class InvoicePage(InvoiceDto invoice) : DataPage("Chi tiết hóa đơn")
{
    protected override Task Load()
    {
        Body.Clear();
        Body.Add(Ui.Text(invoice.Code, 13, Ui.Muted));
        Body.Add(Ui.Section($"Hóa đơn tháng {invoice.Month}"));
        Body.Add(Ui.Status(invoice.Status));
        foreach (var item in invoice.Items) Body.Add(Line(item, invoice.Status));
        Body.Add(Ui.Card(Ui.Text("Tổng cộng", bold: true), Ui.Text(Ui.Money(invoice.Total), 28, Ui.Primary, true),
            Ui.Text("Hạn thanh toán " + Ui.Date(invoice.DueDate), 14, Ui.Muted)));
        if (invoice.Status != "paid") Body.Add(Ui.Text("Liên hệ ban quản lý để được hướng dẫn thanh toán. Thanh toán trực tuyến chưa được cấu hình trong bản local.", 14, Ui.Muted));
        return Task.CompletedTask;
    }
    public static View Line(InvoiceLine item, string status)
    {
        var glyph = item.Description.Contains("nước", StringComparison.OrdinalIgnoreCase) ? "\ue798"
            : item.Description.Contains("điện", StringComparison.OrdinalIgnoreCase) ? "\uec1c"
            : item.Description.Contains("xe", StringComparison.OrdinalIgnoreCase) ? "\ue54f" : "\uea40";
        var calculation = Ui.Card(Ui.Columns(
            Ui.Stack(Ui.Text("Khối lượng", 11, Ui.Muted), Ui.Text($"{item.Quantity} {item.Unit}", 14)),
            Ui.Stack(Ui.Text("Đơn giá", 11, Ui.Muted), Ui.Text(Ui.Money(item.UnitPrice), 14))));
        calculation.BackgroundColor = Ui.Background;
        calculation.StrokeThickness = 0;
        calculation.Padding = 12;
        return Ui.Card(Ui.Leading(Ui.IconBadge(glyph), Ui.Text(item.Description, 15, bold: true)), calculation,
            Ui.Row(Ui.Status(status), Ui.Text(Ui.Money(item.Amount), 20, bold: true)));
    }
}

public sealed class ContractsPage() : DataPage("Hợp đồng căn hộ")
{
    protected override async Task Load()
    {
        var contracts = await App.Api.Get<List<ContractDto>>("contracts");
        Body.Clear();
        Body.Add(Ui.Section("Hợp đồng của bạn"));
        if (contracts.Count == 0) Empty("Chưa có hợp đồng liên kết với tài khoản của bạn.");
        foreach (var c in contracts)
        {
            var card = Ui.Card(Ui.Status(c.Status), Ui.Text(c.Number, 20, bold: true), Ui.Text(c.Type),
                Ui.Text($"Từ {c.StartDate} đến {c.EndDate}", 14, Ui.Muted));
            var content = (VerticalStackLayout)card.Content!;
            if (c.MonthlyRent is long rent) content.Add(Ui.Text("Tiền thuê / tháng · " + Ui.Money(rent), bold: true));
            if (c.Deposit is long deposit) content.Add(Ui.Text("Tiền cọc · " + Ui.Money(deposit)));
            Body.Add(card);
        }
    }
}

public sealed class ParcelsPage() : DataPage("Bưu kiện")
{
    protected override async Task Load()
    {
        var parcels = await App.Api.Get<List<ParcelDto>>("parcels");
        Body.Clear();
        Body.Add(Ui.Section("Bưu kiện của bạn"));
        if (parcels.Count == 0) Empty("Bạn chưa có bưu kiện.");
        foreach (var p in parcels)
            Body.Add(Ui.Card(Ui.Status(p.Status), Ui.Text(p.Courier, 20, bold: true), Ui.Text(p.TrackingCode),
                Ui.Text("Đến lúc " + p.ArrivedAt, 14, Ui.Muted),
                Ui.Text(string.IsNullOrEmpty(p.Locker) ? "Nhận tại quầy lễ tân" : "Tủ nhận hàng · " + p.Locker, 14)));
    }
}

public sealed class ShiftsPage() : DataPage("Bảng công")
{
    protected override async Task Load()
    {
        var shifts = await App.Api.Get<List<WorkRecordDto>>("work-records");
        Body.Clear();
        Body.Add(Ui.Section("Bảng công theo tháng"));
        if (shifts.Count == 0) Empty("Chưa có bảng công của bạn.");
        foreach (var s in shifts)
            Body.Add(Ui.Card(Ui.Text("Tháng " + s.Month, 20, Ui.Teal, true),
                Ui.Text($"Ngày công tiêu chuẩn: {s.StandardDays}"),
                Ui.Text($"Nghỉ có lương: {s.PaidLeaveDays} · Không lương: {s.UnpaidLeaveDays}"),
                Ui.Text($"Giờ làm thêm: {s.OvertimeHours}"), Ui.Text(s.Note ?? "Không có ghi chú", 14, Ui.Muted)));
    }
}

public sealed class ProfilePage() : DataPage("Cá nhân")
{
    protected override async Task Load()
    {
        var p = await App.Api.Get<Profile>("profile");
        Body.Clear();
        Body.Add(Ui.Text(p.Account.Name, 27, bold: true));
        Body.Add(Ui.Status(App.Api.IsResident ? "Cư dân" : "Nhân viên"));
        Body.Add(Ui.Card(Ui.Text(p.Account.TenantName, 20, bold: true), Ui.Text(p.Address, 14, Ui.Muted),
            Ui.Text(App.Api.IsResident ? "Căn hộ " + p.Account.ApartmentCode : p.JobTitle ?? "Nhân viên")));
        Body.Add(Ui.Card(Ui.Section("Thông tin liên hệ"), Ui.Text(p.Phone), Ui.Text(p.Email)));
        if (!string.IsNullOrEmpty(p.Department)) Body.Add(Ui.Card(Ui.Section("Bộ phận"), Ui.Text(p.Department)));
        Body.Add(Ui.Text("EzCondo · Phiên bản local 1.0.0", 12, Ui.Muted));
        Body.Add(Ui.Action("Đăng xuất", async () =>
        {
            if (await DisplayAlert("Đăng xuất", "Kết thúc phiên sử dụng trên thiết bị này?", "Đăng xuất", "Ở lại")) App.ShowLogin();
        }, true));
    }
}
