namespace Zap.Api.Features.Demo;

public static class DemoRoleKeys
{
    public const string Admin = "admin";
    public const string ProjectManager = "projectManager";
    public const string Developer = "developer";
    public const string Submitter = "submitter";

    public static IReadOnlyList<string> ToList()
    {
        return [Admin, ProjectManager, Developer, Submitter];
    }
}