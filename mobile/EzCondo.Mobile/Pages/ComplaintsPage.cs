using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class ComplaintsPage() : DataPage(App.Api.IsResident ? "Phản ánh" : "Công việc")
{
    private readonly Picker filter = Ui.Picker("Trạng thái", ["Tất cả", "Đang mở", "Đã xử lý"]);
    private List<ComplaintDto> complaints = [];
    protected override async Task Load()
    {
        complaints = await App.Api.Get<List<ComplaintDto>>("complaints");
        filter.SelectedIndexChanged -= FilterChanged;
        filter.SelectedIndexChanged += FilterChanged;
        Render();
    }
    private void FilterChanged(object? sender, EventArgs e) => Render();
    private void Render()
    {
        Body.Clear();
        Body.Add(Ui.Section(App.Api.IsResident ? "Lắng nghe & hỗ trợ" : "Công việc được giao"));
        if (App.Api.IsResident) Body.Add(Ui.Action("Gửi phản ánh mới", () => Open(new CreateComplaintPage())));
        Body.Add(filter);
        var visible = complaints.Where(c => filter.SelectedIndex switch
        {
            1 => c.Status is not ("resolved" or "closed"), 2 => c.Status is "resolved" or "closed", _ => true
        }).ToList();
        if (visible.Count == 0) Empty("Chưa có phản ánh phù hợp.");
        foreach (var c in visible)
            Body.Add(Ui.Card(Ui.Status(c.Status), Ui.Text(c.Title, 19, bold: true),
                Ui.Text($"{c.Code} · {c.ApartmentCode}", 13, Ui.Muted),
                Ui.Text($"{c.Category} · Ưu tiên {Choices.Status(c.Priority)}", 13, Ui.Muted),
                Ui.Action("Xem phản ánh " + c.Code, () => Open(new ComplaintPage(c.Id)), true)));
    }
}

public sealed class CreateComplaintPage : ContentPage
{
    public CreateComplaintPage()
    {
        Title = "Gửi phản ánh";
        BackgroundColor = Ui.Background;
        var title = Ui.Entry("Ví dụ · Nước chảy yếu trong căn hộ");
        var category = Ui.Picker("Phân loại", Choices.Categories);
        var priority = Ui.Picker("Mức ưu tiên", Choices.Priorities.Select(Choices.Status));
        priority.SelectedIndex = 1;
        var description = Ui.Editor("Mô tả tình trạng, vị trí và thời gian xảy ra…", 3000);
        var body = Ui.Stack(Ui.Section("Bạn cần hỗ trợ điều gì?"),
            Ui.Text("Ban quản lý sẽ tiếp nhận và cập nhật tiến độ tại mục Phản ánh.", color: Ui.Muted),
            Ui.Field("Tiêu đề", title), Ui.Field("Phân loại", category), Ui.Field("Mức ưu tiên", priority),
            Ui.Field("Nội dung", description), Ui.Action("Gửi phản ánh", async () =>
            {
                var result = await App.Api.Post<ComplaintDto>("complaints", new CreateComplaint(Ui.Required(title.Text, 5, "Tiêu đề"),
                    Choices.Categories[category.SelectedIndex], Choices.Priorities[priority.SelectedIndex], Ui.Required(description.Text, 10, "Nội dung")));
                await DisplayAlert("Đã gửi phản ánh", "Mã tiếp nhận " + result.Code, "Xem phản ánh");
                Navigation.InsertPageBefore(new ComplaintPage(result.Id), this);
                await Navigation.PopAsync();
            }));
        body.Padding = 20;
        Content = new ScrollView { Content = body };
    }
}

public sealed class ComplaintPage(string id) : DataPage("Chi tiết phản ánh")
{
    protected override async Task Load()
    {
        var c = await App.Api.Get<ComplaintDto>("complaints/" + id);
        Body.Clear();
        Body.Add(Ui.Text(c.Code + " · " + c.ApartmentCode, 13, Ui.Muted));
        Body.Add(Ui.Text(c.Title, 25, bold: true));
        Body.Add(Ui.Status(c.Status));
        Body.Add(Ui.Card(Ui.Text(c.Description), Ui.Text($"{c.Category} · {Choices.Status(c.Priority)}", 13, Ui.Muted),
            Ui.Text("Tạo lúc " + c.CreatedAt, 13, Ui.Muted), Ui.Text("Phụ trách · " + (c.AssignedTo ?? "Đang chờ phân công"), 14),
            Ui.Text("Hẹn xử lý · " + (c.Deadline ?? "Chưa xác định"), 14)));
        if (!App.Api.IsResident)
        {
            var targets = c.Status switch
            {
                "new" => new[] { "in_progress" }, "in_progress" => new[] { "waiting_resident", "resolved" },
                "waiting_resident" => new[] { "in_progress", "resolved" }, _ => Array.Empty<string>()
            };
            foreach (var status in targets) Body.Add(Ui.Action("Cập nhật · " + Choices.Status(status), async () =>
            {
                if (!await DisplayAlert("Cập nhật công việc", "Chuyển sang " + Choices.Status(status) + "?", "Xác nhận", "Hủy")) return;
                await App.Api.Patch("complaints/" + id + "/status", new ChangeStatus(c.Status, status));
                await Reload();
            }));
        }
        Body.Add(Ui.Section("Trao đổi"));
        if (c.Messages.Count == 0) Empty("Chưa có tin nhắn. Bạn có thể bổ sung thông tin bên dưới.");
        foreach (var message in c.Messages)
            Body.Add(Ui.Card(Ui.Text(message.SenderName + " · " + message.SenderRole, 13, Ui.Teal, true),
                Ui.Text(message.Content), Ui.Text(message.Timestamp, 12, Ui.Muted)));
        if (c.Status != "closed")
        {
            var message = Ui.Editor("Nhập nội dung trao đổi…", 2000);
            Body.Add(Ui.Field("Tin nhắn mới", message));
            Body.Add(Ui.Action("Gửi tin nhắn", async () =>
            {
                await App.Api.Post<MessageDto>("complaints/" + id + "/messages", new CreateMessage(Ui.Required(message.Text, 1, "Tin nhắn")));
                await Reload();
            }));
        }
    }
}
