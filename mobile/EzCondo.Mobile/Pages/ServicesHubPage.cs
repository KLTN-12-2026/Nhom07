using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Pages;

public sealed class ServicesHubPage() : DataPage("Dịch vụ", residentHeader: true)
{
    protected override async Task Load()
    {
        var services = await App.Api.Get<List<ServiceCatalogDto>>("service-catalog");
        Body.Clear();
        Body.Add(Ui.Hero(Ui.Text("DỊCH VỤ CHUNG CƯ", 12, Ui.BlueLight),
            Ui.Text("Dịch vụ tại " + App.Api.Account!.TenantName, 24, Colors.White, true),
            Ui.Text("Danh mục và đơn giá do ban quản lý cung cấp", 14, Colors.White)));
        Body.Add(Ui.Heading("Danh mục dịch vụ", services.Count + " dịch vụ"));
        if (services.Count == 0) Empty("Ban quản lý chưa công bố dịch vụ.");
        foreach (var service in services)
            Body.Add(Ui.Card(Ui.Leading(Ui.IconBadge("\uea40"), Ui.Text(service.Name, 18, bold: true)),
                Ui.Text(service.Code, 12, Ui.Muted), Ui.Text(Ui.Money(service.Price), 20, Ui.Primary, true)));
    }
}
