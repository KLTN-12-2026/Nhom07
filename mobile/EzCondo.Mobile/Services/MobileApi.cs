using System.Net;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EzCondo.Mobile.Contracts;

namespace EzCondo.Mobile.Services;

public sealed class MobileApi
{
    private readonly HttpClient client;

    public MobileApi(HttpClient? client = null)
    {
        this.client = client ?? new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
    }
    public Account? Account { get; private set; }
    public bool IsResident => Account?.Role == "resident";
    public string BaseUrl { get; private set; } = EmulatorAddress(Preferences.Default.Get("shared-api-url", "http://127.0.0.1:5275/"));

    public void Configure(string url)
    {
        if (!Uri.TryCreate(url.Trim().TrimEnd('/') + "/", UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(uri.UserInfo))
            throw new InvalidOperationException("Địa chỉ API cần bắt đầu bằng http:// hoặc https://.");
        BaseUrl = EmulatorAddress(uri.AbsoluteUri);
        Preferences.Default.Set("shared-api-url", BaseUrl);
    }

    private static string EmulatorAddress(string url)
    {
        // Android emulator loopback belongs to Android; 10.0.2.2 reaches the host without adb reverse.
        if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.DeviceType == DeviceType.Virtual
            && Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsLoopback)
            return new UriBuilder(uri) { Host = "10.0.2.2" }.Uri.AbsoluteUri;
        return url;
    }
    public async Task SignIn(string email, string password)
    {
        var session = await Post<SessionResponse>("sessions", new LoginRequest(email.Trim(), password));
        Account = session.Account;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
    }
    public async Task SignOutCurrent()
    {
        try { await Send<JsonElement>(HttpMethod.Delete, "sessions/current", null, empty: true); }
        catch (SessionExpiredException) { }
        SignOut();
    }

    public void SignOut()
    {
        Account = null;
        client.DefaultRequestHeaders.Authorization = null;
    }
    public Task<T> Get<T>(string path) => Send<T>(HttpMethod.Get, path, null);

    public async Task<ServerInfo> GetServerInfo()
    {
        var clock = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                var info = await Get<ServerInfo>("server");
                if (info.Backend != "EzCondo.BackEndServer")
                    throw new InvalidOperationException("Máy chủ không phải backend dùng chung EzCondo.");
                return info;
            }
            // F5 can launch the app before the local API finishes its cold startup.
            catch (InvalidOperationException error) when (clock.Elapsed < TimeSpan.FromSeconds(30)
                && (error.InnerException is HttpRequestException or WebException or TaskCanceledException))
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
    public Task<T> Post<T>(string path, object body) => Send<T>(HttpMethod.Post, path, body);
    public async Task Patch(string path, object body) => _ = await Send<JsonElement>(HttpMethod.Patch, path, body, true);

    private async Task<T> Send<T>(HttpMethod method, string path, object? body, bool empty = false, bool anonymous = false)
    {
        try
        {
            var route = path.StartsWith("api/", StringComparison.Ordinal) ? path : "api/mobile/" + path;
            using var request = new HttpRequestMessage(method, new Uri(new Uri(BaseUrl), route));
            if (body != null) request.Content = JsonContent.Create(body);
            using var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized && path != "sessions" && !anonymous)
                throw new SessionExpiredException();
            if (!response.IsSuccessStatusCode)
            {
                var message = "Không thể thực hiện thao tác. Vui lòng thử lại.";
                try
                {
                    var error = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (error.TryGetProperty("message", out var text)) message = text.GetString() ?? message;
                    else if (error.TryGetProperty("detail", out var detail)) message = detail.GetString() ?? message;
                }
                catch (JsonException) { }
                throw new InvalidOperationException(message);
            }
            if (empty) return default!;
            return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("API trả về dữ liệu trống.");
        }
        catch (TaskCanceledException error) { throw new InvalidOperationException("Kết nối quá thời gian. Kiểm tra API và thử lại.", error); }
        catch (HttpRequestException error) { throw new InvalidOperationException("Không kết nối được máy chủ. Kiểm tra API local và kết nối Android.", error); }
        // AndroidMessageHandler on .NET 9 uses WebException for transport failures.
        catch (WebException error) { throw new InvalidOperationException("Không kết nối được máy chủ. Kiểm tra API local và kết nối Android.", error); }
    }

    public Task<ActivatedInvitationDto> AcceptInvitation(string link, string password, string confirmPassword)
    {
        if (!Uri.TryCreate(link.Trim(), UriKind.Absolute, out var uri) || !uri.Fragment.StartsWith("#token=", StringComparison.Ordinal))
            throw new InvalidOperationException("Hãy dán liên kết lời mời đầy đủ trong email.");
        var token = uri.Fragment[7..];
        if (token.Length != 43 || token.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_')))
            throw new InvalidOperationException("Liên kết lời mời không hợp lệ.");
        var invitationOrigin = new Uri(EmulatorAddress(uri.AbsoluteUri)).GetLeftPart(UriPartial.Authority);
        if (uri.UserInfo.Length != 0 || !string.Equals(invitationOrigin, new Uri(BaseUrl).GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Liên kết lời mời không thuộc máy chủ EzCondo đang kết nối.");

        return Send<ActivatedInvitationDto>(HttpMethod.Post, "api/invitations/accept",
            new ActivateInvitationDto(token, password, confirmPassword), anonymous: true);
    }
}

public sealed class SessionExpiredException() : Exception("Phiên đã hết hạn. Vui lòng đăng nhập lại.");
