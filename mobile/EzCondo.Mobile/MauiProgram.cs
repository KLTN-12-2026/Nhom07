using Microsoft.Extensions.Logging;

namespace EzCondo.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("PlusJakartaSans.ttf", "Jakarta");
                fonts.AddFont("MaterialIconsOutlined.otf", "MaterialIcons");
            });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
