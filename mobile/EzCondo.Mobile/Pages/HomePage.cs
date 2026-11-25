using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class HomePage() : DataPage("EzCondo", residentHeader: true)
{
    protected override async Task Load()
    {
        var overview = await App.Api.Get<Overview>("overview");
        var user = App.Api.Account!;
        if (!App.Api.IsResident)
        {
            Body.Clear();
            Body.Add(Ui.Text(user.TenantName.ToUpperInvariant(), 12, Ui.Primary, true));
            Body.Add(Ui.Text($"Xin chào,\n{user.Name}", 28, bold: true));
            Body.Add(Ui.Text("Nhân viên · Không gian làm việc", color: Ui.Muted));
            Body.Add(Ui.Card(Ui.Section("Công việc cần xử lý"), Ui.Text($"{overview.OpenComplaints} phản ánh đang mở"),
                Ui.Action("Xem công việc được giao", () => Open(new ComplaintsPage()))));
            Body.Add(Ui.Card(Ui.Section("Bảng công"), Ui.Text("Tra cứu ngày công và giờ làm thêm theo tháng"),
                Ui.Action("Xem bảng công", () => Open(new ShiftsPage()), true)));
            Body.Add(Ui.Action("Tra cứu bảng lương", () => Open(new PayrollPage()), true));
            if (user.CanInviteResidents)
                Body.Add(Ui.Action("Mời cư dân", () => Open(new InviteResidentPage()), true));
            return;
        }

        var invoiceTask = App.Api.Get<List<InvoiceDto>>("invoices");
        var complaintTask = App.Api.Get<List<ComplaintDto>>("complaints");
        await Task.WhenAll(invoiceTask, complaintTask);
        var complaints = await complaintTask;
        Body.Clear();
        Body.Add(Ui.Stack(Ui.Text("CƯ DÂN THÂN THIẾT", 11, Ui.Primary, true),
            Ui.Text("Xin chào, " + user.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() + "!", 28, bold: true),
            Ui.Text($"Căn hộ {user.ApartmentCode} · {user.TenantName}", 12, Ui.Muted)));
        var invoice = (await invoiceTask).FirstOrDefault();
        if (invoice != null) Body.Add(Ui.BillHero(invoice, "Chi tiết hóa đơn  ›", () => Open(new InvoicesPage())));
        Body.Add(Ui.Heading("Thao tác cư dân"));
        Body.Add(Ui.Columns(
            Ui.QuickAction("Báo sự cố", "\ue869", () => Open(new CreateComplaintPage())),
            Ui.QuickAction("Hóa đơn", "\uef63", () => Open(new InvoicesPage())),
            Ui.QuickAction("Dịch vụ", "\uea40", () => Open(new ServicesHubPage())),
            Ui.QuickAction("Hợp đồng", "\ue873", () => Open(new ContractsPage()))));
        Body.Add(Ui.Heading("Sự cố đang xử lý", overview.OpenComplaints + " phiếu"));
        var open = complaints.Where(c => c.Status is not ("resolved" or "closed")).Take(2).ToList();
        if (open.Count == 0) Empty("Bạn chưa có sự cố đang mở.");
        foreach (var complaint in open)
        {
            var heading = Ui.Text(complaint.Title, 15, bold: true);
            heading.MaxLines = 2;
            heading.LineBreakMode = LineBreakMode.TailTruncation;
            var card = Ui.Card(Ui.Row(heading, Ui.Status(complaint.Status)),
                Ui.Text("Mã phiếu · " + complaint.Code, 12, Ui.Muted),
                Ui.Row(Ui.Text(complaint.AssignedTo ?? "Đang chờ phân công", 12, Ui.Primary), Ui.Icon("\ue5c8", 20)),
                Ui.Text("Gửi lúc " + Ui.Date(complaint.CreatedAt), 12, Ui.Muted));
            Body.Add(Ui.Tap(card, "Xem phản ánh " + complaint.Code, () => Open(new ComplaintPage(complaint.Id))));
        }
        Body.Add(Ui.Heading("Hoạt động gần đây"));
        foreach (var complaint in complaints.Take(2))
            Body.Add(Ui.Tap(Ui.Card(Ui.Row(Ui.Pill("Phản ánh căn hộ"), Ui.Text(Ui.Date(complaint.CreatedAt), 11, Ui.Muted)),
                Ui.Text(complaint.Title, 15, bold: true), Ui.Status(complaint.Status), Ui.Text("Xem nhật ký xử lý  ›", 13, Ui.Primary, true)),
                "Hoạt động " + complaint.Code, () => Open(new ComplaintPage(complaint.Id))));
        Body.Add(Ui.Tap(Ui.Card(Ui.Leading(Ui.IconBadge("\ue873"), Ui.Stack(Ui.Text("Hồ sơ & Hợp đồng căn hộ", 15, bold: true),
            Ui.Text("Thông tin cư dân và giấy tờ đã liên kết", 12, Ui.Muted)))), "Hồ sơ và hợp đồng", () => Open(new ContractsPage())));
    }
}
