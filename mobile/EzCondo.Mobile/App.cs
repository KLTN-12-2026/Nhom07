using EzCondo.Mobile.Pages;
using EzCondo.Mobile.Services;

namespace EzCondo.Mobile;

public sealed class App : Application
{
    public static MobileApi Api { get; } = new();

    public App()
    {
        UserAppTheme = AppTheme.Light;
        Resources.Add(new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextColorProperty, Value = Ui.Ink },
            new Setter { Property = Label.FontFamilyProperty, Value = "Jakarta" },
            new Setter { Property = Label.FontSizeProperty, Value = 15d } } });
        Resources.Add(new Style(typeof(Button)) { Setters = { new Setter { Property = Button.BackgroundColorProperty, Value = Ui.Primary },
            new Setter { Property = Button.FontFamilyProperty, Value = "Jakarta" },
            new Setter { Property = Button.TextColorProperty, Value = Colors.White }, new Setter { Property = Button.CornerRadiusProperty, Value = 12 },
            new Setter { Property = Button.MinimumHeightRequestProperty, Value = 50d }, new Setter { Property = Button.FontSizeProperty, Value = 15d },
            new Setter { Property = Button.FontAttributesProperty, Value = FontAttributes.Bold } } });
        Resources.Add(new Style(typeof(Entry)) { Setters = { new Setter { Property = Entry.FontFamilyProperty, Value = "Jakarta" } } });
        Resources.Add(new Style(typeof(Editor)) { Setters = { new Setter { Property = Editor.FontFamilyProperty, Value = "Jakarta" } } });
        Resources.Add(new Style(typeof(Picker)) { Setters = { new Setter { Property = Picker.FontFamilyProperty, Value = "Jakarta" } } });
        Resources.Add(new Style(typeof(DatePicker)) { Setters = { new Setter { Property = DatePicker.FontFamilyProperty, Value = "Jakarta" } } });
        Resources.Add(new Style(typeof(NavigationPage)) { Setters = { new Setter { Property = NavigationPage.BarBackgroundColorProperty, Value = Colors.White },
            new Setter { Property = NavigationPage.BarTextColorProperty, Value = Ui.Ink } } });
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(Api.Account == null
        ? new NavigationPage(new LoginPage()) : new MainTabs());
    public static void ShowLogin()
    {
        Api.SignOut();
        Current!.Windows[0].Page = new NavigationPage(new LoginPage());
    }
    public static void ShowHome() => Current!.Windows[0].Page = new MainTabs();
}
