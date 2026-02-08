namespace RaceOverlay.Core.Services;

public class DriverSessionInfo
{
    public int CarIdx { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string CarNumber { get; init; } = string.Empty;
    public string CarClassShortName { get; init; } = string.Empty;
    public int CarClassColor { get; init; }
    public int IRating { get; init; }
    public string LicString { get; init; } = string.Empty;
    public int LicColor { get; init; }
    public string CarScreenNameShort { get; init; } = string.Empty;
    public bool IsSpectator { get; init; }
}
