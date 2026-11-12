using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Services;

public sealed class MobileApi
{
    private readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(12) };
    public Account? Account { get; private set; }
    public bool IsResident => Account?.Role == "resident";
    public string BaseUrl { get; private set; } = Preferences.Default.Get("api-url", "http://127.0.0.1:5275/");

    public void Configure(string url)
    {
        if (!Uri.TryCreate(url.Trim().TrimEnd('/') + "/", UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(uri.UserInfo))
            throw new InvalidOperationException("Địa chỉ API cần bắt đầu bằng http:// hoặc https://.");
        BaseUrl = uri.AbsoluteUri;
        Preferences.Default.Set("api-url", BaseUrl);
    }
    public async Task SignIn(string id)
    {
        var session = await Post<SessionResponse>("sessions", new SessionRequest(id));
        Account = session.Account;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
    }
    public void SignOut()
    {
        Account = null;
        client.DefaultRequestHeaders.Authorization = null;
    }
    public Task<T> Get<T>(string path) => Send<T>(HttpMethod.Get, path, null);
    public Task<T> Post<T>(string path, object body) => Send<T>(HttpMethod.Post, path, body);
    public async Task Patch(string path, object body) => _ = await Send<JsonElement>(HttpMethod.Patch, path, body, true);

    private async Task<T> Send<T>(HttpMethod method, string path, object? body, bool empty = false)
    {
        try
        {
            using var request = new HttpRequestMessage(method, new Uri(new Uri(BaseUrl), "api/mobile/" + path));
            if (body != null) request.Content = JsonContent.Create(body);
            using var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new SessionExpiredException();
            if (!response.IsSuccessStatusCode)
            {
                var message = "Không thể thực hiện thao tác. Vui lòng thử lại.";
                try
                {
                    var error = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (error.TryGetProperty("message", out var text)) message = text.GetString() ?? message;
                }
                catch (JsonException) { }
                throw new InvalidOperationException(message);
            }
            if (empty) return default!;
            return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("API trả về dữ liệu trống.");
        }
        catch (TaskCanceledException) { throw new InvalidOperationException("Kết nối quá thời gian. Kiểm tra API và thử lại."); }
        catch (HttpRequestException) { throw new InvalidOperationException("Không kết nối được máy chủ. Kiểm tra API local và kết nối Android."); }
        // AndroidMessageHandler on .NET 9 uses WebException for transport failures.
        catch (WebException) { throw new InvalidOperationException("Không kết nối được máy chủ. Kiểm tra API local và kết nối Android."); }
    }
}

public sealed class SessionExpiredException() : Exception("Phiên đã hết hạn. Vui lòng đăng nhập lại.");
