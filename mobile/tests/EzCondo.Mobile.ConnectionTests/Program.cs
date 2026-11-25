using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EzCondo.Mobile.Contracts;
using EzCondo.Mobile.Services;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("PASS " + message);
}

static async Task Fails<T>(Func<Task> action) where T : Exception
{
    try { await action(); }
    catch (T) { return; }
    throw new Exception("Expected " + typeof(T).Name);
}

static HttpResponseMessage Accounts() => new(HttpStatusCode.OK)
{
    Content = JsonContent.Create(new ServerInfo("EzCondo.BackEndServer", "PostgreSQL"))
};

foreach (var failure in new Exception[]
    { new HttpRequestException("API starting"), new WebException("Android transport"), new TaskCanceledException("Timeout") })
{
    var calls = 0;
    using var client = new HttpClient(new StubHandler(_ =>
    {
        calls++;
        return calls < 3 ? Task.FromException<HttpResponseMessage>(failure) : Task.FromResult(Accounts());
    }));
    var api = new MobileApi(client);
    var accounts = await api.GetServerInfo();
    Check(calls == 3 && accounts.Backend == "EzCondo.BackEndServer", "Startup recovers from " + failure.GetType().Name);
}

foreach (var status in new[] { HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.Unauthorized })
{
    var calls = 0;
    using var client = new HttpClient(new StubHandler(_ =>
    {
        calls++;
        return Task.FromResult(new HttpResponseMessage(status) { Content = JsonContent.Create(new { message = "Server error" }) });
    }));
    var api = new MobileApi(client);
    if (status == HttpStatusCode.Unauthorized) await Fails<SessionExpiredException>(() => api.GetServerInfo());
    else await Fails<InvalidOperationException>(() => api.GetServerInfo());
    Check(calls == 1, "HTTP " + (int)status + " is reported without retry");
}

{
    var calls = 0;
    using var client = new HttpClient(new StubHandler(_ =>
    {
        calls++;
        return Task.FromException<HttpResponseMessage>(new HttpRequestException("Connection lost"));
    }));
    var api = new MobileApi(client);
    await Fails<InvalidOperationException>(() => api.Post<SessionResponse>("sessions", new LoginRequest("resident@test.local", "password")));
    Check(calls == 1, "POST is never automatically replayed");
    var clock = Stopwatch.StartNew();
    await Fails<InvalidOperationException>(() => api.GetServerInfo());
    Check(clock.Elapsed >= TimeSpan.FromSeconds(29) && clock.Elapsed < TimeSpan.FromSeconds(45),
        "Offline startup stops retrying within its deadline");
}

{
    var calls = 0;
    using var client = new HttpClient(new StubHandler(_ =>
    {
        calls++;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("invalid JSON") });
    }));
    var api = new MobileApi(client);
    await Fails<JsonException>(() => api.GetServerInfo());
    Check(calls == 1, "Invalid JSON is reported without retry");
}

{
    using var client = new HttpClient(new StubHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        { Content = JsonContent.Create(new { message = "Invalid credentials" }) })));
    var api = new MobileApi(client);
    await Fails<InvalidOperationException>(() => api.SignIn("resident@test.local", "incorrect"));
    Check(api.Account is null, "Invalid login does not create a session");
}

{
    var calls = 0;
    using var client = new HttpClient(new StubHandler(_ =>
    {
        calls++;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new ServerInfo("OldDemo", "SQLite")) });
    }));
    await Fails<InvalidOperationException>(() => new MobileApi(client).GetServerInfo());
    Check(calls == 1, "Wrong backend is rejected without retry");
}

{
    var calls = 0;
    using var client = new HttpClient(new StubHandler(async request =>
    {
        calls++;
        var body = await request.Content!.ReadFromJsonAsync<ActivateInvitationDto>();
        Check(request.RequestUri?.AbsolutePath == "/api/invitations/accept" && body?.Token == new string('a', 43),
            "activation submits only the token to the existing acceptance API");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new ActivatedInvitationDto("resident@test.local")) };
    }));
    var api = new MobileApi(client);
    api.Configure("https://ezcondo.test/");
    var token = new string('a', 43);
    await Fails<InvalidOperationException>(() => api.AcceptInvitation("https://other.test/api/mobile/invitation-activation#token=" + token,
        "Example!2026test", "Example!2026test"));
    Check(calls == 0, "activation never sends credentials to another configured origin");
    await api.AcceptInvitation("https://ezcondo.test/api/mobile/invitation-activation#token=" + token,
        "Example!2026test", "Example!2026test");
}

sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => send(request);
}
