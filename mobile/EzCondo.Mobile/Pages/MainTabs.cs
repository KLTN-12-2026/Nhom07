namespace EzCondo.Mobile.Pages;

public sealed class MainTabs : TabbedPage
{
    public MainTabs()
    {
        BarBackgroundColor = Colors.White;
        SelectedTabColor = Ui.Teal;
        UnselectedTabColor = Ui.Muted;
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(
            this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
        Add(new HomePage(), "Trang chủ", "tab_home.png");
        if (App.Api.IsResident) Add(new InvoicesPage(), "Hóa đơn", "tab_invoice.png");
        Add(new ComplaintsPage(), App.Api.IsResident ? "Phản ánh" : "Công việc", "tab_chat.png");
        if (!App.Api.IsResident) Add(new ShiftsPage(), "Lịch trực", "tab_calendar.png");
        Add(new ProfilePage(), "Cá nhân", "tab_profile.png");
    }
    private void Add(Page page, string title, string icon) => Children.Add(new NavigationPage(page) { Title = title, IconImageSource = icon });
}
