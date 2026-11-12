using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace EzCondo.Mobile.Pages;

public static class Ui
{
    public static readonly Color Teal = Color.FromArgb("#087F78");
    public static readonly Color Ink = Color.FromArgb("#163832");
    public static readonly Color Muted = Color.FromArgb("#60756F");
    public static readonly Color Background = Color.FromArgb("#F3F6F4");
    public static Label Text(string text, double size = 15, Color? color = null, bool bold = false) => new()
    {
        Text = text, FontSize = size, TextColor = color ?? Ink,
        FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None, LineBreakMode = LineBreakMode.WordWrap
    };
    public static Border Card(params View[] children) => new()
    {
        BackgroundColor = Colors.White, Stroke = Color.FromArgb("#DFE8E3"), StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 16 }, Padding = 18,
        Content = Stack(children)
    };
    public static VerticalStackLayout Stack(params View[] children)
    {
        var stack = new VerticalStackLayout { Spacing = 12 };
        foreach (var child in children) stack.Add(child);
        return stack;
    }
    public static Button Action(string text, Func<Task> action, bool secondary = false)
    {
        var button = new Button { Text = text, AutomationId = text };
        if (secondary) { button.BackgroundColor = Color.FromArgb("#E3F1EC"); button.TextColor = Teal; }
        button.Clicked += async (_, _) =>
        {
            if (!button.IsEnabled) return;
            button.IsEnabled = false;
            try { await action(); }
            catch (Exception error)
            {
                var page = Application.Current!.Windows[0].Page!;
                await page.DisplayAlert("EzCondo", error.Message, "Đóng");
                if (error is Services.SessionExpiredException) App.ShowLogin();
            }
            finally { button.IsEnabled = true; }
        };
        return button;
    }
    public static View Status(string status) => new Border
    {
        StrokeThickness = 0, BackgroundColor = Color.FromArgb("#E7F2ED"), Padding = new Thickness(10, 6),
        StrokeShape = new RoundRectangle { CornerRadius = 8 }, HorizontalOptions = LayoutOptions.Start,
        Content = Text(Contracts.Choices.Status(status), 12, Teal, true)
    };
    public static string Money(long amount) => amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
    public static Label Section(string text) => Text(text, 20, bold: true);
    public static View Field(string title, View input) => Stack(Text(title, 14, bold: true), input);
    public static Entry Entry(string placeholder, int max = 150) => new()
    {
        Placeholder = placeholder, MaxLength = max, TextColor = Ink, PlaceholderColor = Muted,
        BackgroundColor = Colors.White, MinimumHeightRequest = 52, FontSize = 16
    };
    public static Picker Picker(string title, IEnumerable<string> items)
    {
        var picker = new Picker { Title = title, TextColor = Ink, BackgroundColor = Colors.White, MinimumHeightRequest = 52 };
        foreach (var item in items) picker.Items.Add(item);
        picker.SelectedIndex = 0;
        return picker;
    }
    public static Editor Editor(string placeholder, int max) => new()
    {
        Placeholder = placeholder, MaxLength = max, HeightRequest = 140, AutoSize = EditorAutoSizeOption.TextChanges,
        BackgroundColor = Colors.White, TextColor = Ink, PlaceholderColor = Muted, FontSize = 16
    };
    public static string Required(string? value, int min, string label)
    {
        if (value?.Trim().Length < min || string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{label} cần có ít nhất {min} ký tự.");
        return value.Trim();
    }
}
