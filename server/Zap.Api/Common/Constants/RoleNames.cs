namespace Zap.Api.Common.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string ProjectManager = "Project Manager";
    public const string Developer = "Developer";
    public const string Submitter = "Submitter";

    public static IReadOnlyList<string> ToList()
    {
        var l = new List<string>
        {
            Admin,
            ProjectManager,
            Developer,
            Submitter
        };
        return l.AsReadOnly();
    }
}