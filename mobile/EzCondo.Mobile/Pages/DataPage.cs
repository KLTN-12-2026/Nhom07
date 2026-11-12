using EzCondo.Mobile.Services;

namespace EzCondo.Mobile.Pages;

public abstract class DataPage : ContentPage
{
    protected readonly VerticalStackLayout Body = new() { Spacing = 16, Padding = new Thickness(20, 18, 20, 30) };
    private bool loading;
    protected DataPage(string title)
    {
        Title = title;
        BackgroundColor = Ui.Background;
        Content = new ScrollView { Content = Body };
        ToolbarItems.Add(new ToolbarItem("Tải lại", null, async () => await Reload()));
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
        Body.Add(new ActivityIndicator { IsRunning = true, Color = Ui.Teal });
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
    protected Task Open(Page page) => Navigation.PushAsync(page);
}
