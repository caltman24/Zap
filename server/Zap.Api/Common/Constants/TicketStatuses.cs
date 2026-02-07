namespace Zap.Api.Common.Constants;

public static class TicketStatuses
{
    public const string New = "New";
    public const string InDevelopment = "In Development";
    public const string Testing = "Testing";
    public const string Resolved = "Resolved";

    public static IReadOnlyList<string> ToList()
    {
        var l = new List<string>
        {
            New,
            InDevelopment,
            Testing,
            Resolved
        };
        return l.AsReadOnly();
    }
}