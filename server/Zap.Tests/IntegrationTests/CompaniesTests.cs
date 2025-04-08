using System.Net.Http.Headers;
using Zap.Api.Features.Companies.Services;

namespace Zap.Tests.IntegrationTests;

public class CompaniesTests
{
    private async Task<(ZapApplication app, AppDbContext db, string userId, AppUser user, HttpClient client)>
        SetupTestEnvironment(string role = RoleNames.Admin)
    {
        var userId = Guid.NewGuid().ToString();

        var app = new ZapApplication();
        var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId, role: role);

        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var client = app.CreateClient(userId, role: role);

        return (app, db, userId, user!, client);
    }

    [Fact]
    public async Task Register_Company_Returns_Success()
    {
        var (app, db, userId, _, client) = await SetupTestEnvironment();

        var registerRequest = new RegisterCompanyRequest("Test Company", "Description");
        var res = await client.PostAsJsonAsync("/company/register", registerRequest);

        Assert.True(res.IsSuccessStatusCode);

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Register_Company_With_Existing_Relation_Returns400_BadRequest()
    {
        var (app, db, userId, _, client) = await SetupTestEnvironment();

        var registerRequest = new RegisterCompanyRequest("Test Company", "Description");
        var res = await client.PostAsJsonAsync("/company/register", registerRequest);
        Assert.True(res.IsSuccessStatusCode);

        var badRes = await client.PostAsJsonAsync("/company/register", registerRequest);
        Assert.Equal(HttpStatusCode.BadRequest, badRes.StatusCode);

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Get_Company_Info_Returns_Success()
    {
        var (app, db, userId, user, client) = await SetupTestEnvironment();

        var company = await CreateTestCompany(db, userId, user);

        var res = await client.GetFromJsonAsync<CompanyInfoDto>("/company/info");

        Assert.NotNull(res);
        Assert.Equal(company.Name, res.Name);
        Assert.Equal(company.Description, res.Description);

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Get_Company_Info_Unauthorized_Returns_401_Unauthorized()
    {
        await using var app = new ZapApplication();
        var client = app.CreateClient();

        var res = await client.GetAsync("/company/info");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Get_Company_Info_With_No_Relation_Returns_400_BadRequest()
    {
        var (app, db, userId, _, client) = await SetupTestEnvironment();

        var res = await client.GetAsync("/company/info");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    private Task<(List<Project> projects, string companyId)> CreateTestProjects(AppUser user)
    {
        var companyId = Guid.NewGuid().ToString();
        var projectId = Guid.NewGuid().ToString();
        var projectId2 = Guid.NewGuid().ToString();

        var projects = new List<Project>
        {
            new Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                AssignedMembers = [user],
                IsArchived = true
            },
            new Project
            {
                Id = projectId2,
                Name = "Test Project",
                Description = "Test Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                AssignedMembers = [user],
                IsArchived = false
            }
        };

        return Task.FromResult((projects, companyId));
    }

    [Fact]
    public async Task Get_Company_Projects_Unarchived_Returns_Success()
    {
        var (app, db, userId, user, client) = await SetupTestEnvironment();
        var (projects, companyId) = await CreateTestProjects(user);

        await CreateTestCompany(db, userId, user, projects, companyId);

        var res = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects?isArchived=false");

        Assert.NotNull(res);
        Assert.Null(res.FirstOrDefault(x => x.Id == projects[0].Id && x.IsArchived));
        Assert.NotNull(res.FirstOrDefault(x => x.Id == projects[1].Id && x.IsArchived == false));

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Get_Company_Projects_Archived_Returns_Success()
    {
        var (app, db, userId, user, client) = await SetupTestEnvironment();
        var (projects, companyId) = await CreateTestProjects(user);

        await CreateTestCompany(db, userId, user, projects, companyId);

        var res = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects?isArchived=true");

        Assert.NotNull(res);
        Assert.NotNull(res.FirstOrDefault(x => x.Id == projects[0].Id && x.IsArchived));
        Assert.Null(res.FirstOrDefault(x => x.Id == projects[1].Id && x.IsArchived == false));

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Get_Company_Projects_All_Returns_Success()
    {
        var (app, db, userId, user, client) = await SetupTestEnvironment();
        var (projects, companyId) = await CreateTestProjects(user);

        await CreateTestCompany(db, userId, user, projects, companyId);

        var res = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects");

        Assert.NotNull(res);
        Assert.NotNull(res.FirstOrDefault(x => x.Id == projects[0].Id && x.IsArchived));
        Assert.NotNull(res.FirstOrDefault(x => x.Id == projects[1].Id && x.IsArchived == false));

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    private MultipartFormDataContent CreateCompanyUpdateContent()
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("New Company Name"), "Name");
        content.Add(new StringContent("New Description"), "Description");
        content.Add(new StringContent("false"), "RemoveLogo");
        content.Add(new StringContent("https://example.com"), "WebsiteUrl");
        return content;
    }

    [Fact]
    public async Task Update_Company_Admin_Returns_Success()
    {
        var (app, db, userId, user, client) = await SetupTestEnvironment(role: RoleNames.Admin);
        await CreateTestCompany(db, userId, user);

        using var content = CreateCompanyUpdateContent();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var res = await client.PostAsync("/company/info", content);

        Assert.True(res.IsSuccessStatusCode);

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Update_Company_ProjectManager_Returns_Forbidden()
    {
        var (app, db, userId, user, client) = await SetupTestEnvironment(role: RoleNames.ProjectManager);
        await CreateTestCompany(db, userId, user);

        using var content = CreateCompanyUpdateContent();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var res = await client.PostAsync("/company/info", content);

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Update_Company_Unauthorized_Returns_401_Unauthorized()
    {
        await using var app = new ZapApplication();
        var client = app.CreateClient();

        using var content = CreateCompanyUpdateContent();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var res = await client.PostAsync("/company/info", content);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Update_Company_Bad_Info_Returns_400_BadRequest()
    {
        var (app, db, userId, user, client) = await SetupTestEnvironment(role: RoleNames.Admin);

        // Don't create a company relation
        using var content = CreateCompanyUpdateContent();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var res = await client.PostAsync("/company/info", content);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    private static async Task<Company> CreateTestCompany(AppDbContext db, string userId, AppUser user,
        List<Project>? projects = null, string? companyId = null)
    {
        var company = new Company()
        {
            Id = companyId ?? Guid.NewGuid().ToString(),
            Name = "Test Company",
            Description = "Test Description",
            OwnerId = userId,
            Members = [user]
        };

        if (projects != null)
        {
            company.Projects = projects;
        }

        var c = db.Companies.Add(company);
        await db.SaveChangesAsync();

        return c.Entity;
    }
}

internal record RegisterCompanyRequest(string Name, string Description);

internal record UpdateCompanyRequest(string Name, string Description, bool RemoveLogo, string? WebsiteUrl);