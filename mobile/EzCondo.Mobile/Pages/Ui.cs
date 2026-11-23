using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace EzCondo.Mobile.Pages;

public static class Ui
{
    public static readonly Color Primary = Color.FromArgb("#003E80");
    public static readonly Color Blue = Color.FromArgb("#0051D5");
    public static readonly Color Teal = Color.FromArgb("#00625A");
    public static readonly Color Ink = Color.FromArgb("#131B2E");
    public static readonly Color Muted = Color.FromArgb("#687080");
    public static readonly Color Background = Color.FromArgb("#FAF8FF");
    public static readonly Color Soft = Color.FromArgb("#EAEDFF");
    public static readonly Color BlueLight = Color.FromArgb("#B4CDFF");
    public static Label Text(string text, double size = 15, Color? color = null, bool bold = false) => new()
    {
        Text = text, FontSize = size, FontFamily = "Jakarta", TextColor = color ?? Ink,
        FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None, LineBreakMode = LineBreakMode.WordWrap
    };
    public static Border Card(params View[] children) => new()
    {
        BackgroundColor = Colors.White, Stroke = Color.FromArgb("#F0EDF7"), StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 18 }, Padding = 16,
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
        if (secondary) { button.BackgroundColor = Soft; button.TextColor = Primary; }
        button.Clicked += async (_, _) => await Invoke(button, action);
        return button;
    }
    public static T Tap<T>(T view, string description, Func<Task> action) where T : View
    {
        SemanticProperties.SetDescription(view, description);
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await Invoke(view, action);
        view.GestureRecognizers.Add(tap);
        return view;
    }
    private static async Task Invoke(View view, Func<Task> action)
    {
        if (!view.IsEnabled) return;
        view.IsEnabled = false;
        try { await action(); }
        catch (Exception error)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("EzCondo", error.Message, "Đóng");
            if (error is Services.SessionExpiredException) App.ShowLogin();
        }
        finally { view.IsEnabled = true; }
    }
    public static View Status(string status)
    {
        var (background, foreground) = status switch
        {
            "unpaid" or "overdue" or "urgent" or "Từ chối" => ("#FFE4E0", "#A42020"),
            "in_progress" or "waiting_resident" or "Chờ duyệt" => ("#FFF5DD", "#955400"),
            "paid" or "resolved" or "closed" or "Đã xác nhận" or "Đã nhận" or "Đã hoàn tất" => ("#E7FAF2", "#00624F"),
            _ => ("#E3E9FF", "#003E80")
        };
        return Pill(Contracts.Choices.Status(status), Color.FromArgb(background), Color.FromArgb(foreground));
    }
    public static Border Pill(string text, Color? background = null, Color? foreground = null) => new()
    {
        StrokeThickness = 0, BackgroundColor = background ?? Soft, Padding = new Thickness(10, 5),
        StrokeShape = new RoundRectangle { CornerRadius = 30 }, HorizontalOptions = LayoutOptions.Start,
        VerticalOptions = LayoutOptions.Center, Content = Text(text, 12, foreground ?? Primary, true)
    };
    public static Grid Row(View left, View right)
    {
        var grid = new Grid { ColumnSpacing = 10, ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)] };
        grid.Add(left);
        grid.Add(right, 1);
        return grid;
    }
    public static Grid Columns(params View[] children)
    {
        var grid = new Grid { ColumnSpacing = 10 };
        for (var i = 0; i < children.Length; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.Add(children[i], i);
        }
        return grid;
    }
    public static Grid Leading(View icon, View content)
    {
        var grid = new Grid { ColumnSpacing = 12, ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)] };
        grid.Add(icon);
        grid.Add(content, 1);
        return grid;
    }
    public static View Icon(string glyph, double size = 24, Color? color = null) => new Label
    {
        Text = glyph, FontFamily = "MaterialIcons", FontSize = size, TextColor = color ?? Primary,
        FontAutoScalingEnabled = false, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center,
        HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
    };
    public static Border IconBadge(string glyph, Color? background = null, Color? foreground = null, double size = 44) => new()
    {
        BackgroundColor = background ?? Soft, StrokeThickness = 0, WidthRequest = size, HeightRequest = size,
        StrokeShape = new RoundRectangle { CornerRadius = 12 }, HorizontalOptions = LayoutOptions.Start,
        Content = Icon(glyph, size * .55, foreground)
    };
    public static View Heading(string title, string? detail = null) => Row(Section(title), Text(detail ?? "", 12, Muted));
    public static View QuickAction(string title, string glyph, Func<Task> action)
    {
        var label = Text(title, 12, bold: true);
        label.HorizontalTextAlignment = TextAlignment.Center;
        var icon = IconBadge(glyph);
        icon.HorizontalOptions = LayoutOptions.Center;
        var card = Card(icon, label);
        card.Padding = new Thickness(4, 12);
        return Tap(card, title, action);
    }
    public static View ServiceTile(string title, string subtitle, string glyph, Func<Task> action)
    {
        var caption = Text(subtitle, 13, Muted);
        caption.MaxLines = 1;
        caption.LineBreakMode = LineBreakMode.TailTruncation;
        var card = Card(IconBadge(glyph), Text(title, 15, bold: true), caption);
        ((VerticalStackLayout)card.Content!).Spacing = 8;
        card.MinimumHeightRequest = 132;
        return Tap(card, title, action);
    }
    public static Border Hero(params View[] children)
    {
        var card = Card(children);
        card.Padding = 18;
        card.StrokeThickness = 0;
        card.Background = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0), EndPoint = new Point(1, 1),
            GradientStops = [new GradientStop(Primary, 0), new GradientStop(Color.FromArgb("#1E56A0"), 1)]
        };
        card.Shadow = new Shadow { Brush = Primary, Opacity = .15f, Radius = 8, Offset = new Point(0, 3) };
        return card;
    }
    public static View BillHero(Contracts.InvoiceDto invoice, string actionText, Func<Task> action)
    {
        var button = Action(actionText, action);
        button.BackgroundColor = Blue;
        return Hero(Row(Pill("Kỳ tháng " + invoice.Month, Color.FromArgb("#2B609C"), Colors.White), Status(invoice.Status)),
            Text("Tổng phí kỳ này", 13, BlueLight), Text(Money(invoice.Total), 32, Colors.White, true),
            Text(invoice.Status == "paid" ? "Hóa đơn đã được thanh toán" : "Hạn thanh toán · " + Date(invoice.DueDate), 13, BlueLight), button);
    }
    public static View Filters(IEnumerable<View> filters)
    {
        var stack = new HorizontalStackLayout { Spacing = 8 };
        foreach (var filter in filters) stack.Add(filter);
        return new ScrollView { Orientation = ScrollOrientation.Horizontal, HorizontalScrollBarVisibility = ScrollBarVisibility.Never, Content = stack };
    }
    public static Button Chip(string text, bool selected, Func<Task> action)
    {
        var button = Action(text, action, !selected);
        button.FontSize = 12;
        button.MinimumHeightRequest = 44;
        button.CornerRadius = 24;
        button.Padding = new Thickness(14, 8);
        return button;
    }
    public static string Date(string value) => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
        ? date.ToString(value.Contains(':') ? "dd/MM/yyyy · HH:mm" : "dd/MM/yyyy") : value;
    public static string Money(decimal amount) => amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
    public static Label Section(string text) => Text(text, 18, bold: true);
    public static View Field(string title, View input) => Stack(Text(title, 14, bold: true), new Border
    {
        BackgroundColor = Colors.White, Stroke = Color.FromArgb("#C2C6D3"), StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 12 }, Padding = new Thickness(10, 0), Content = input
    });
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
