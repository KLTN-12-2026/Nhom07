using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class BookingsPage() : DataPage("Đặt tiện ích")
{
    protected override async Task Load()
    {
        var bookings = await App.Api.Get<List<BookingDto>>("bookings");
        Body.Clear();
        Body.Add(Ui.Section("Không gian dành cho bạn"));
        Body.Add(Ui.Action("Đăng ký tiện ích", () => Open(new BookingForm())));
        if (bookings.Count == 0) Empty("Bạn chưa có đăng ký tiện ích.");
        foreach (var b in bookings)
            Body.Add(Ui.Card(Ui.Status(b.Status), Ui.Text(b.FacilityName, 20, bold: true),
                Ui.Text(b.Date + " · " + b.TimeSlot), Ui.Text($"{b.Guests} người tham gia", 14, Ui.Muted)));
    }
}

public sealed class BookingForm : ContentPage
{
    public BookingForm()
    {
        Title = "Đăng ký tiện ích";
        BackgroundColor = Ui.Background;
        var facility = Ui.Picker("Tiện ích", Choices.Facilities);
        var date = new DatePicker { Date = DateTime.Today.AddDays(1), MinimumDate = DateTime.Today, MaximumDate = DateTime.Today.AddDays(90), Format = "dd/MM/yyyy", TextColor = Ui.Ink };
        var slot = Ui.Picker("Khung giờ", Choices.TimeSlots);
        var guests = Ui.Entry("Số người từ 1 đến 20", 2);
        guests.Keyboard = Keyboard.Numeric;
        guests.Text = "2";
        var body = Ui.Stack(Ui.Section("Đăng ký sử dụng tiện ích"), Ui.Text("Ban quản lý sẽ kiểm tra và xác nhận đăng ký của bạn.", color: Ui.Muted),
            Ui.Field("Tiện ích", facility), Ui.Field("Ngày sử dụng", date), Ui.Field("Khung giờ", slot), Ui.Field("Số người", guests),
            Ui.Action("Gửi đăng ký", async () =>
            {
                if (!int.TryParse(guests.Text, out var count) || count is < 1 or > 20) throw new InvalidOperationException("Số người cần từ 1 đến 20.");
                await App.Api.Post<BookingDto>("bookings", new CreateBooking(Choices.Facilities[facility.SelectedIndex],
                    date.Date.ToString("yyyy-MM-dd"), Choices.TimeSlots[slot.SelectedIndex], count));
                await DisplayAlert("Đã gửi đăng ký", "Bạn có thể theo dõi trạng thái tại mục Đặt tiện ích.", "Đóng");
                await Navigation.PopAsync();
            }));
        body.Padding = 20;
        Content = new ScrollView { Content = body };
    }
}

public sealed class ServicesPage() : DataPage("Yêu cầu dịch vụ")
{
    protected override async Task Load()
    {
        var services = await App.Api.Get<List<ServiceDto>>("services");
        Body.Clear();
        Body.Add(Ui.Section("Dịch vụ tại căn hộ"));
        Body.Add(Ui.Action("Tạo yêu cầu dịch vụ", () => Open(new ServiceForm())));
        if (services.Count == 0) Empty("Bạn chưa có yêu cầu dịch vụ.");
        foreach (var s in services)
            Body.Add(Ui.Card(Ui.Status(s.Status), Ui.Text(s.Type, 20, bold: true),
                Ui.Text(s.Code + " · " + s.Date, 13, Ui.Muted), Ui.Text(s.Details)));
    }
}

public sealed class ServiceForm : ContentPage
{
    public ServiceForm()
    {
        Title = "Tạo yêu cầu dịch vụ";
        BackgroundColor = Ui.Background;
        var type = Ui.Picker("Loại dịch vụ", Choices.ServiceTypes);
        var date = new DatePicker { Date = DateTime.Today.AddDays(1), MinimumDate = DateTime.Today, MaximumDate = DateTime.Today.AddDays(90), Format = "dd/MM/yyyy", TextColor = Ui.Ink };
        var details = Ui.Editor("Mô tả nhu cầu và thời gian thuận tiện để liên hệ…", 2000);
        var body = Ui.Stack(Ui.Section("Bạn cần dịch vụ gì?"), Ui.Text("Thông tin liên hệ và căn hộ lấy từ tài khoản của bạn.", color: Ui.Muted),
            Ui.Field("Loại dịch vụ", type), Ui.Field("Ngày mong muốn", date), Ui.Field("Chi tiết yêu cầu", details),
            Ui.Action("Gửi yêu cầu", async () =>
            {
                await App.Api.Post<ServiceDto>("services", new CreateService(Choices.ServiceTypes[type.SelectedIndex],
                    date.Date.ToString("yyyy-MM-dd"), Ui.Required(details.Text, 10, "Chi tiết")));
                await DisplayAlert("Đã gửi yêu cầu", "Ban quản lý sẽ xem xét và liên hệ với bạn.", "Đóng");
                await Navigation.PopAsync();
            }));
        body.Padding = 20;
        Content = new ScrollView { Content = body };
    }
}
