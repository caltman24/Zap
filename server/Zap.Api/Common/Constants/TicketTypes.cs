namespace Zap.Api.Common.Constants;

public static class TicketTypes
{
    public const string Defect = "Defect";
    public const string Feature = "Feature";
    public const string GeneralTask = "General Task";
    public const string WorkTask = "Work Task";
    public const string ChangeRequest = "Change Request";
    public const string Enhanecment = "Enhancement";

    public static IReadOnlyList<string> ToList()
    {
        var l = new List<string>
        {
            Defect,
            Feature,
            GeneralTask,
            WorkTask,
            ChangeRequest,
            Enhanecment
        };
        return l.AsReadOnly();
    }
}