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
            new Setter { Property = Label.FontSizeProperty, Value = 15d } } });
        Resources.Add(new Style(typeof(Button)) { Setters = { new Setter { Property = Button.BackgroundColorProperty, Value = Ui.Teal },
            new Setter { Property = Button.TextColorProperty, Value = Colors.White }, new Setter { Property = Button.CornerRadiusProperty, Value = 12 },
            new Setter { Property = Button.MinimumHeightRequestProperty, Value = 50d }, new Setter { Property = Button.FontSizeProperty, Value = 15d },
            new Setter { Property = Button.FontAttributesProperty, Value = FontAttributes.Bold } } });
        Resources.Add(new Style(typeof(NavigationPage)) { Setters = { new Setter { Property = NavigationPage.BarBackgroundColorProperty, Value = Colors.White },
            new Setter { Property = NavigationPage.BarTextColorProperty, Value = Ui.Ink } } });
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new NavigationPage(new LoginPage()));
    public static void ShowLogin()
    {
        Api.SignOut();
        Current!.Windows[0].Page = new NavigationPage(new LoginPage());
    }
    public static void ShowHome() => Current!.Windows[0].Page = new MainTabs();
}
