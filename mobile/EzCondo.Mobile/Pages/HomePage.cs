using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class HomePage() : DataPage("EzCondo")
{
    protected override async Task Load()
    {
        var overview = await App.Api.Get<Overview>("overview");
        var user = App.Api.Account!;
        Body.Clear();
        Body.Add(Ui.Text(user.TenantName.ToUpperInvariant(), 12, Ui.Teal, true));
        Body.Add(Ui.Text($"Xin chào,\n{user.Name}", 28, bold: true));
        Body.Add(Ui.Text(App.Api.IsResident ? $"Căn hộ {user.ApartmentCode} · Cư dân" : "Nhân viên · Không gian làm việc", color: Ui.Muted));
        if (App.Api.IsResident)
        {
            var bill = Ui.Card(Ui.Text("Tổng phí chưa thanh toán", 14, Ui.Muted), Ui.Text(Ui.Money(overview.AmountDue), 32, Ui.Teal, true),
                Ui.Action("Xem hóa đơn", () => Open(new InvoicesPage())));
            Body.Add(bill);
        }
        Body.Add(Ui.Card(Ui.Section(App.Api.IsResident ? "Phản ánh của bạn" : "Công việc cần xử lý"),
            Ui.Text($"{overview.OpenComplaints} phản ánh đang mở", 16),
            Ui.Action(App.Api.IsResident ? "Theo dõi phản ánh" : "Xem công việc được giao", () => Open(new ComplaintsPage()), true)));
        Body.Add(Ui.Section(App.Api.IsResident ? "Dịch vụ chung cư" : "Lịch làm việc"));
        if (App.Api.IsResident)
        {
            Body.Add(Ui.Card(Ui.Text($"{overview.PendingParcels} bưu kiện chờ nhận", 17, bold: true),
                Ui.Action("Bưu kiện của tôi", () => Open(new ParcelsPage()), true)));
            Body.Add(Ui.Action("Đặt tiện ích", () => Open(new BookingsPage()), true));
            Body.Add(Ui.Action("Yêu cầu dịch vụ", () => Open(new ServicesPage()), true));
            Body.Add(Ui.Action("Hợp đồng căn hộ", () => Open(new ContractsPage()), true));
        }
        else
        {
            Body.Add(Ui.Card(Ui.Text($"{overview.ShiftCount} lịch trực đã phân công", 17, bold: true),
                Ui.Action("Xem lịch trực", () => Open(new ShiftsPage()), true)));
        }
    }
}
