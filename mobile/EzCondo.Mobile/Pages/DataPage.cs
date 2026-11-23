using EzCondo.Mobile.Services;

namespace EzCondo.Mobile.Pages;

public abstract class DataPage : ContentPage
{
    protected readonly VerticalStackLayout Body = new() { Spacing = 18, Padding = new Thickness(16, 16, 16, 28) };
    private bool loading;
    protected DataPage(string title, bool residentHeader = false)
    {
        Title = title;
        BackgroundColor = Ui.Background;
        var scroll = new ScrollView { Content = Body };
        if (residentHeader && App.Api.IsResident)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            var root = new Grid { RowDefinitions = [new(GridLength.Auto), new(GridLength.Star)] };
            root.Add(Header());
            root.Add(scroll, 0, 1);
            Content = root;
        }
        else
        {
            Content = scroll;
            ToolbarItems.Add(new ToolbarItem("Tải lại", null, async () => await Reload()));
        }
    }
    private View Header()
    {
        var user = App.Api.Account!;
        var identity = Ui.Text(user.TenantName + " · " + user.ApartmentCode, 12, bold: true);
        identity.MaxLines = 1;
        identity.LineBreakMode = LineBreakMode.TailTruncation;
        identity.VerticalOptions = LayoutOptions.Center;
        var logo = Ui.IconBadge("\ue7f1", Colors.White, size: 36);
        var unit = new Border { BackgroundColor = Ui.Soft.WithAlpha(.5f), StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 },
            Padding = new Thickness(10, 8), Content = identity, VerticalOptions = LayoutOptions.Center };
        var refresh = Ui.Tap(Ui.IconBadge("\ue5d5", Ui.Background, Ui.Ink), "Tải lại", Reload);
        var avatar = Ui.Tap(Ui.IconBadge("\ue7fd", Ui.Soft, Ui.Primary), "Hồ sơ cư dân", () => Open(new ProfilePage()));
        avatar.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 30 };
        var header = new Grid { Padding = new Thickness(16, 6), ColumnSpacing = 8,
            ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto)] };
        header.Add(logo);
        header.Add(unit, 1);
        header.Add(refresh, 2);
        header.Add(avatar, 3);
        return header;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Reload();
    }
    public async Task Reload()
    {
        if (loading) return;
        loading = true;
        Body.Clear();
        Body.Add(new ActivityIndicator { IsRunning = true, Color = Ui.Primary });
        Body.Add(Ui.Text("Đang tải dữ liệu…", color: Ui.Muted));
        try { await Load(); }
        catch (Exception error)
        {
            Body.Clear();
            Body.Add(Ui.Card(Ui.Section("Chưa tải được dữ liệu"), Ui.Text(error.Message),
                error is SessionExpiredException ? Ui.Action("Đăng nhập lại", () => { App.ShowLogin(); return Task.CompletedTask; })
                : Ui.Action("Thử lại", Reload)));
        }
        finally { loading = false; }
    }
    protected abstract Task Load();
    protected void Empty(string message) => Body.Add(Ui.Card(Ui.Text(message, color: Ui.Muted)));
    protected async Task Open(Page page)
    {
        if (Application.Current!.Windows[0].Page is TabbedPage tabs
            && tabs.Children.OfType<NavigationPage>().FirstOrDefault(n => n.RootPage.GetType() == page.GetType()) is { } target)
        {
            tabs.CurrentPage = target;
            await target.PopToRootAsync(false);
            return;
        }
        await Navigation.PushAsync(page);
    }
}
