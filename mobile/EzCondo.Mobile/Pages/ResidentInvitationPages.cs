using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class InviteResidentPage() : DataPage("Mời cư dân")
{
    protected override Task Load()
    {
        Body.Clear();
        var name = Ui.Entry("Họ và tên", 150);
        var email = Ui.Entry("Email", 254);
        email.Keyboard = Keyboard.Email;
        Body.Add(Ui.Field("Họ và tên cư dân", name));
        Body.Add(Ui.Field("Email nhận lời mời", email));
        Body.Add(Ui.Action("Gửi lời mời", async () =>
        {
            var invitation = await App.Api.Post<ResidentInvitationDto>("resident-invitations",
                new InviteResidentDto(Ui.Required(name.Text, 1, "Họ và tên"), Ui.Required(email.Text, 3, "Email")));
            name.Text = "";
            email.Text = "";
            await DisplayAlert("Đã gửi lời mời", $"Liên kết hết hạn lúc {invitation.ExpiresAt.ToLocalTime():HH:mm dd/MM/yyyy}.", "Đóng");
        }));
        return Task.CompletedTask;
    }
}

public sealed class ActivateInvitationPage() : DataPage("Kích hoạt tài khoản")
{
    protected override Task Load()
    {
        Body.Clear();
        var link = Ui.Entry("Liên kết trong email", 2048);
        link.Keyboard = Keyboard.Url;
        var password = Ui.Entry("Mật khẩu mới", 128);
        password.IsPassword = true;
        var confirmation = Ui.Entry("Nhập lại mật khẩu", 128);
        confirmation.IsPassword = true;
        Body.Add(Ui.Field("Liên kết lời mời", link));
        Body.Add(Ui.Field("Mật khẩu mới", password));
        Body.Add(Ui.Field("Nhập lại mật khẩu", confirmation));
        Body.Add(Ui.Text("Dùng 12–128 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt.", 13, Ui.Muted));
        Body.Add(Ui.Action("Đặt mật khẩu", async () =>
        {
            if (password.Text is not { Length: >= 12 } value || value != confirmation.Text)
                throw new InvalidOperationException("Mật khẩu cần ít nhất 12 ký tự và hai ô phải trùng nhau.");

            var activated = await App.Api.AcceptInvitation(link.Text ?? "", value, value);
            link.Text = password.Text = confirmation.Text = "";
            await DisplayAlert("Đã kích hoạt", "Bạn có thể đăng nhập bằng mật khẩu vừa đặt.", "Đăng nhập");
            App.ShowLogin(activated.Email);
        }));
        return Task.CompletedTask;
    }
}
