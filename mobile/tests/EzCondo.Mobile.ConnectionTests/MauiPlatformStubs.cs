// The HTTP client is tested on the host; these replace only MAUI's device/preferences APIs.
public sealed class Preferences
{
    public static Preferences Default { get; } = new();
    private readonly Dictionary<string, string> values = new();
    public string Get(string key, string defaultValue) => values.GetValueOrDefault(key, defaultValue);
    public void Set(string key, string value) => values[key] = value;
}

public enum DevicePlatform { Android, WinUI }
public enum DeviceType { Virtual, Physical }
public static class DeviceInfo
{
    public static DevicePlatform Platform => DevicePlatform.WinUI;
    public static DeviceType DeviceType => global::DeviceType.Physical;
}
