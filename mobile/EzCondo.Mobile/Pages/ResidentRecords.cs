using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class InvoicesPage() : DataPage("Hóa đơn")
{
    protected override async Task Load()
    {
        var invoices = await App.Api.Get<List<InvoiceDto>>("invoices");
        Body.Clear();
        Body.Add(Ui.Section("Chi phí căn hộ"));
        Body.Add(Ui.Text("Tra cứu phí và lịch sử hóa đơn của bạn.", color: Ui.Muted));
        if (invoices.Count == 0) Empty("Bạn chưa có hóa đơn.");
        foreach (var invoice in invoices)
            Body.Add(Ui.Card(Ui.Status(invoice.Status), Ui.Text($"Tháng {invoice.Month}", 20, bold: true),
                Ui.Text(Ui.Money(invoice.Total), 26, Ui.Teal, true), Ui.Text($"Hạn thanh toán {invoice.DueDate}", 13, Ui.Muted),
                Ui.Action("Chi tiết " + invoice.Code, () => Open(new InvoicePage(invoice)), true)));
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
        foreach (var item in invoice.Items)
            Body.Add(Ui.Card(Ui.Text(item.Description, bold: true),
                Ui.Text($"{item.Quantity} {item.Unit} × {Ui.Money(item.UnitPrice)}", 13, Ui.Muted),
                Ui.Text(Ui.Money(item.Amount), 19, Ui.Teal, true)));
        Body.Add(Ui.Card(Ui.Text("Tổng cộng", bold: true), Ui.Text(Ui.Money(invoice.Total), 28, Ui.Teal, true),
            Ui.Text($"Hạn thanh toán {invoice.DueDate}", 14, Ui.Muted)));
        if (invoice.Status != "paid") Body.Add(Ui.Text("Liên hệ ban quản lý để được hướng dẫn thanh toán. Thanh toán trực tuyến chưa được cấu hình trong bản local.", 14, Ui.Muted));
        return Task.CompletedTask;
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

public sealed class ShiftsPage() : DataPage("Lịch trực")
{
    protected override async Task Load()
    {
        var shifts = await App.Api.Get<List<ShiftDto>>("shifts");
        Body.Clear();
        Body.Add(Ui.Section("Lịch phân công của bạn"));
        if (shifts.Count == 0) Empty("Bạn chưa có lịch trực được phân công.");
        foreach (var s in shifts)
            Body.Add(Ui.Card(Ui.Text(s.Date, 14, Ui.Teal, true), Ui.Text(s.ShiftName, 23, bold: true),
                Ui.Text(s.Department), Ui.Text(s.Notes ?? "Không có ghi chú", 14, Ui.Muted)));
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
        if (App.Api.IsResident)
        {
            Body.Add(Ui.Section("Phương tiện"));
            if (p.Vehicles.Count == 0) Empty("Chưa có phương tiện đăng ký.");
            foreach (var vehicle in p.Vehicles) Body.Add(Ui.Card(Ui.Text(vehicle.Plate, 20, bold: true), Ui.Text(vehicle.Type)));
        }
        Body.Add(Ui.Text("EzCondo · Phiên bản local 1.0.0", 12, Ui.Muted));
        Body.Add(Ui.Action("Đăng xuất", async () =>
        {
            if (await DisplayAlert("Đăng xuất", "Kết thúc phiên sử dụng trên thiết bị này?", "Đăng xuất", "Ở lại")) App.ShowLogin();
        }, true));
    }
}
