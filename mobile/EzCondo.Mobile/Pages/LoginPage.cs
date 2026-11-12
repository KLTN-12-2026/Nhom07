using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class LoginPage : DataPage
{
    public LoginPage() : base("EzCondo") { NavigationPage.SetHasNavigationBar(this, false); }
    protected override async Task Load()
    {
        Body.Clear();
        Body.Add(new BoxView { HeightRequest = 24, Color = Colors.Transparent });
        Body.Add(Ui.Text("EzCondo", 36, Ui.Teal, true));
        Body.Add(Ui.Text("Kết nối cuộc sống\ntại chung cư của bạn", 26, bold: true));
        Body.Add(Ui.Text("Cư dân & nhân viên", 16, Ui.Muted));
        Body.Add(Ui.Card(Ui.Text("BẢN LOCAL · TÀI KHOẢN DEMO", 12, Ui.Teal, true),
            Ui.Text("Chọn tài khoản có sẵn để trải nghiệm dữ liệu chung cư. Phiên demo chỉ dùng khi chạy local.", 14, Ui.Muted)));
        var url = Ui.Entry("Địa chỉ API", 250);
        url.Text = App.Api.BaseUrl;
        url.Keyboard = Keyboard.Url;
        Body.Add(Ui.Field("Máy chủ API", url));
        Body.Add(Ui.Action("Kết nối máy chủ", async () => { App.Api.Configure(url.Text ?? ""); await Reload(); }, true));
        try
        {
            var accounts = await App.Api.Get<List<Account>>("demo-accounts");
            if (accounts.Count == 0) { Body.Add(Ui.Text("Chưa có tài khoản cư dân hoặc nhân viên hợp lệ.")); return; }
            var picker = Ui.Picker("Chọn tài khoản", accounts.Select(a => $"{a.Name} · {(a.Role == "resident" ? "Cư dân" : "Nhân viên")}"));
            Body.Add(Ui.Field("Tài khoản", picker));
            Body.Add(Ui.Action("Đăng nhập demo", async () =>
            {
                App.Api.Configure(url.Text ?? "");
                await App.Api.SignIn(accounts[picker.SelectedIndex].Id);
                App.ShowHome();
            }));
        }
        catch (Exception error)
        {
            Body.Add(Ui.Card(Ui.Text(error.Message, color: Color.FromArgb("#AD4736")), Ui.Action("Thử lại", Reload)));
        }
    }
}
