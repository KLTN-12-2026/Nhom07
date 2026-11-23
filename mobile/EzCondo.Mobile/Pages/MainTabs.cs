namespace EzCondo.Mobile.Pages;

public sealed class MainTabs : TabbedPage
{
    public MainTabs()
    {
        BarBackgroundColor = Colors.White;
        SelectedTabColor = Ui.Primary;
        UnselectedTabColor = Ui.Muted;
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(
            this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
        Add(new HomePage(), "Trang chủ", "\ue88a");
        if (App.Api.IsResident) Add(new InvoicesPage(), "Hóa đơn", "\uef6e");
        Add(new ComplaintsPage(), App.Api.IsResident ? "Phản ánh" : "Công việc", "\uf083");
        if (App.Api.IsResident) Add(new ServicesHubPage(), "Tiện ích", "\uea40");
        else
        {
            Add(new ShiftsPage(), "Bảng công", "\uebcc");
            Add(new ProfilePage(), "Cá nhân", "\ue7ff");
        }
    }
    private void Add(Page page, string title, string glyph) => Children.Add(new NavigationPage(page)
    {
        Title = title, IconImageSource = new FontImageSource { FontFamily = "MaterialIcons", Glyph = glyph, Size = 25, Color = Ui.Primary }
    });
}
