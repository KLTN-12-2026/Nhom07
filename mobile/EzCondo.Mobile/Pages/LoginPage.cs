using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class LoginPage(string? prefilledEmail = null) : DataPage("EzCondo")
{
    protected override async Task Load()
    {
        NavigationPage.SetHasNavigationBar(this, false);
        Body.Clear();
        Body.Add(new BoxView { HeightRequest = 24, Color = Colors.Transparent });
        Body.Add(Ui.Text("EzCondo", 36, Ui.Primary, true));
        Body.Add(Ui.Text("Kết nối cuộc sống\ntại chung cư của bạn", 26, bold: true));
        Body.Add(Ui.Text("Cư dân & nhân viên", 16, Ui.Muted));
        Body.Add(Ui.Card(Ui.Text("TÀI KHOẢN EZCONDO", 12, Ui.Primary, true),
            Ui.Text("Đăng nhập bằng email và mật khẩu cư dân hoặc nhân viên của chung cư.", 14, Ui.Muted)));
        var url = Ui.Entry("Địa chỉ API", 250);
        url.Text = App.Api.BaseUrl;
        url.Keyboard = Keyboard.Url;
        Body.Add(Ui.Field("Máy chủ API", url));
        Body.Add(Ui.Action("Kết nối máy chủ", async () => { App.Api.Configure(url.Text ?? ""); await Reload(); }, true));
        var connecting = Ui.Text("Đang chờ máy chủ local sẵn sàng…", 14, Ui.Muted);
        Body.Add(connecting);
        try
        {
            await App.Api.GetServerInfo();
            var email = Ui.Entry("Email", 254);
            email.Text = prefilledEmail;
            email.Keyboard = Keyboard.Email;
            var password = Ui.Entry("Mật khẩu", 256);
            password.IsPassword = true;
            password.IsTextPredictionEnabled = false;
            Body.Add(Ui.Field("Email", email));
            Body.Add(Ui.Field("Mật khẩu", password));
            Body.Add(Ui.Action("Đăng nhập", async () =>
            {
                App.Api.Configure(url.Text ?? "");
                if (string.IsNullOrEmpty(password.Text)) throw new InvalidOperationException("Vui lòng nhập mật khẩu.");
                await App.Api.SignIn(Ui.Required(email.Text, 3, "Email"), password.Text);
                password.Text = "";
                App.ShowHome();
            }));
            Body.Add(Ui.Action("Kích hoạt tài khoản từ lời mời", () => Open(new ActivateInvitationPage()), true));
        }
        catch (Exception error)
        {
            Body.Add(Ui.Card(Ui.Text(error.Message, color: Color.FromArgb("#AD4736")), Ui.Action("Thử lại", Reload)));
        }
        finally { Body.Remove(connecting); }
    }
}
