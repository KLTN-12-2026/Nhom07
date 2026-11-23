using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class ComplaintsPage() : DataPage(App.Api.IsResident ? "Phản ánh" : "Công việc", residentHeader: true)
{
    private string filter = "all";
    private List<ComplaintDto> complaints = [];
    protected override async Task Load()
    {
        complaints = await App.Api.Get<List<ComplaintDto>>("complaints");
        Render();
    }
    private void Render()
    {
        Body.Clear();
        Body.Add(Ui.Text(App.Api.IsResident ? "Khiếu nại & Kỹ thuật" : "Công việc được giao", 23, bold: true));
        Body.Add(Ui.Text(App.Api.IsResident ? "Theo dõi tiến độ xử lý sự cố cho căn " + App.Api.Account!.ApartmentCode
            : "Theo dõi và cập nhật công việc của bạn", 13, Ui.Muted));
        if (App.Api.IsResident)
        {
            var banner = Ui.Hero(Ui.Leading(Ui.IconBadge("\ue745", Color.FromArgb("#326CA3"), Colors.White),
                Ui.Stack(Ui.Text("Tạo phiếu báo sự cố mới", 16, Colors.White, true),
                    Ui.Text("Mô tả tình trạng cần hỗ trợ", 12, Ui.BlueLight))));
            Body.Add(Ui.Tap(banner, "Gửi phản ánh mới", () => Open(new CreateComplaintPage())));
        }
        var filters = new[] { ("all", "Tất cả"), ("in_progress", "Đang xử lý"), ("new", "Mới tiếp nhận"), ("done", "Đã xử lý") };
        Body.Add(Ui.Filters(filters.Select(f => Ui.Chip(f.Item2 + "  " + complaints.Count(c => Matches(c, f.Item1)), filter == f.Item1, () =>
        {
            filter = f.Item1;
            Render();
            return Task.CompletedTask;
        }))));
        var visible = complaints.Where(c => Matches(c, filter)).ToList();
        if (visible.Count == 0) Empty("Chưa có phản ánh phù hợp.");
        foreach (var c in visible)
        {
            var title = Ui.Text(c.Title, 17, bold: true);
            title.MaxLines = 3;
            title.LineBreakMode = LineBreakMode.TailTruncation;
            var summary = Ui.Text(c.Description, 13, Ui.Muted);
            summary.MaxLines = 2;
            summary.LineBreakMode = LineBreakMode.TailTruncation;
            var picture = new Grid { WidthRequest = 76, HeightRequest = 84 };
            var fallback = Ui.IconBadge("\ue869", size: 76);
            fallback.HeightRequest = 84;
            picture.Add(fallback);
            if (c.Images?.FirstOrDefault() is string image && Uri.TryCreate(image, UriKind.Absolute, out var uri)
                && uri.Scheme == "https")
                picture.Add(new Border { StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    Content = new Image { Source = image, Aspect = Aspect.AspectFill } });
            var assignee = Ui.Stack(Ui.Text(c.AssignedTo ?? "Ban quản lý", 13, bold: true),
                Ui.Text(c.AssignedTo == null ? "Đang chờ phân công" : "Nhân viên phụ trách", 12, Ui.Muted));
            assignee.Spacing = 2;
            var assigned = Ui.Card(Ui.Leading(Ui.IconBadge("\uea3d", Color.FromArgb("#DBE2FF"), size: 32), assignee));
            assigned.BackgroundColor = Ui.Background;
            assigned.StrokeThickness = 0;
            assigned.Padding = 10;
            var card = Ui.Card(Ui.Row(Ui.Pill("#" + c.Code), Ui.Status(c.Status)), Ui.Text(c.Category, 12, Ui.Muted),
                Ui.Leading(picture, Ui.Stack(title, summary)), assigned,
                Ui.Text("Gửi lúc · " + Ui.Date(c.CreatedAt), 12, Ui.Muted),
                Ui.Text("Chi tiết phiếu  ›", 13, Ui.Primary, true));
            Body.Add(Ui.Tap(card, "Xem phản ánh " + c.Code, () => Open(new ComplaintPage(c.Id))));
        }
    }
    private static bool Matches(ComplaintDto complaint, string value) => value switch
    {
        "all" => true, "done" => complaint.Status is "resolved" or "closed",
        "in_progress" => complaint.Status is "in_progress" or "waiting_resident", _ => complaint.Status == value
    };
}

public sealed class CreateComplaintPage : ContentPage
{
    public CreateComplaintPage()
    {
        Title = "Gửi phản ánh";
        BackgroundColor = Ui.Background;
        var title = Ui.Entry("Ví dụ · Nước chảy yếu trong căn hộ");
        var description = Ui.Editor("Mô tả tình trạng, vị trí và thời gian xảy ra…", 3000);
        var body = Ui.Stack(Ui.Section("Bạn cần hỗ trợ điều gì?"),
            Ui.Text("Ban quản lý sẽ tiếp nhận và cập nhật tiến độ tại mục Phản ánh.", color: Ui.Muted),
            Ui.Field("Tiêu đề", title),
            Ui.Field("Nội dung", description), Ui.Action("Gửi phản ánh", async () =>
            {
                var result = await App.Api.Post<ComplaintDto>("complaints", new CreateComplaint(Ui.Required(title.Text, 5, "Tiêu đề"),
                    "", "", Ui.Required(description.Text, 10, "Nội dung")));
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
        Body.Add(Ui.Card(Ui.Text(c.Description),
            Ui.Text("Tạo lúc " + c.CreatedAt, 13, Ui.Muted), Ui.Text("Phụ trách · " + (c.AssignedTo ?? "Đang chờ phân công"), 14),
            Ui.Text("Phản ánh căn hộ", 14, Ui.Muted)));
        if (!App.Api.IsResident)
        {
            var targets = c.Status == "in_progress" ? new[] { "resolved" } : Array.Empty<string>();
            foreach (var status in targets) Body.Add(Ui.Action("Cập nhật · " + Choices.Status(status), async () =>
            {
                if (!await DisplayAlert("Cập nhật công việc", "Chuyển sang " + Choices.Status(status) + "?", "Xác nhận", "Hủy")) return;
                await App.Api.Patch("complaints/" + id + "/status", new ChangeStatus(c.Status, status));
                await Reload();
            }));
        }
    }
}
