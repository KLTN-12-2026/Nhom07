using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class PayrollPage() : DataPage("Bảng lương")
{
    protected override async Task Load()
    {
        var payrolls = await App.Api.Get<List<PayrollDto>>("payrolls");
        Body.Clear();
        if (payrolls.Count == 0) Empty("Bạn chưa có bảng lương đã phát hành.");

        foreach (var item in payrolls)
        {
            var card = Ui.Card(Ui.Section(item.Title),
                Ui.Row(Ui.Text("Lương cơ bản"), Ui.Text(Ui.Money(item.BaseSalary))),
                Ui.Row(Ui.Text("Phụ cấp"), Ui.Text(Ui.Money(item.Allowance))),
                Ui.Row(Ui.Text("Khấu trừ"), Ui.Text(Ui.Money(item.Deduction))),
                Ui.Row(Ui.Text("Lương sau khấu trừ"), Ui.Text(Ui.Money(item.NetAmount), 18, Ui.Primary, true)));
            if (item.PublishedAt is { } publishedAt && card.Content is VerticalStackLayout content)
                content.Insert(1, Ui.Text($"Phát hành {publishedAt.ToLocalTime():dd/MM/yyyy}", 13, Ui.Muted));
            Body.Add(card);
        }
    }
}
