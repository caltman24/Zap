namespace Zap.Api.Common.Constants;

public static class Priorities
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Urgent = "Urgent";

    public static IReadOnlyList<string> ToList()
    {
        var l = new List<string>
        {
            Low,
            Medium,
            High,
            Urgent
        };
        return l.AsReadOnly();
    }
}