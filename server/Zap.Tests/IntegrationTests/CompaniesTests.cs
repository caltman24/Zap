using System.Net.Http.Headers;
using Zap.Api.Features.Companies.Services;

namespace Zap.Tests.IntegrationTests;

public class CompaniesTests
{
    [Fact]
    public async Task Register_Company_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);

        var client = app.CreateClient(userId);

        var registerRequest = new RegisterCompanyRequest("Test Company", "Description");
        var res = await client.PostAsJsonAsync("/company/register", registerRequest);

        Assert.True(res.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Register_Company_With_Existing_Relation_Returns400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);

        var client = app.CreateClient(userId);

        var registerRequest = new RegisterCompanyRequest("Test Company", "Description");
        var res = await client.PostAsJsonAsync("/company/register", registerRequest);
        Assert.True(res.IsSuccessStatusCode);

        var badRes = await client.PostAsJsonAsync("/company/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, badRes.StatusCode);
    }

    [Fact]
    public async Task Get_Company_Info_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);

        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CreateTestCompany(db, userId, user);

        var client = app.CreateClient(userId);
        var res = await client.GetFromJsonAsync<CompanyInfoDto>("/company/info");

        Assert.NotNull(res);
        Assert.Equal(company.Name, res.Name);
        Assert.Equal(company.Description, res.Description);
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
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);

        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var client = app.CreateClient(userId);
        var res = await client.GetAsync("/company/info");

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Get_Company_Projects_Unarchived_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);

        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var companyId = Guid.NewGuid().ToString();
        var projectId = Guid.NewGuid().ToString();
        var projectId2 = Guid.NewGuid().ToString();
        await CreateTestCompany(db, userId, user, [
            new Project()
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
            new Project()
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
        ], companyId);

        var client = app.CreateClient(userId);

        var res = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects?isArchived=false");

        Assert.NotNull(res);
        Assert.Null(res.FirstOrDefault(x => x.Id == projectId && x.IsArchived));
        Assert.NotNull(res.FirstOrDefault(x => x.Id == projectId2 && x.IsArchived == false));
    }

    [Fact]
    public async Task Get_Company_Projects_Archived_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);

        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var companyId = Guid.NewGuid().ToString();
        var projectId = Guid.NewGuid().ToString();
        var projectId2 = Guid.NewGuid().ToString();
        await CreateTestCompany(db, userId, user, [
            new Project()
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
            new Project()
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
        ], companyId);

        var client = app.CreateClient(userId);

        var res = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects?isArchived=true");

        Assert.NotNull(res);

        Assert.NotNull(res.FirstOrDefault(x => x.Id == projectId && x.IsArchived));
        Assert.Null(res.FirstOrDefault(x => x.Id == projectId2 && x.IsArchived == false));
    }

    [Fact]
    public async Task Get_Company_Projects_All_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);

        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var companyId = Guid.NewGuid().ToString();
        var projectId = Guid.NewGuid().ToString();
        var projectId2 = Guid.NewGuid().ToString();
        await CreateTestCompany(db, userId, user, [
            new Project()
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
            new Project()
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
        ], companyId);

        var client = app.CreateClient(userId);

        var res = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects");

        Assert.NotNull(res);
        Assert.NotNull(res.FirstOrDefault(x => x.Id == projectId && x.IsArchived));
        Assert.NotNull(res.FirstOrDefault(x => x.Id == projectId2 && x.IsArchived == false));
    }

    [Fact]
    public async Task Update_Company_Without_Image_As_Admin_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId, role: RoleNames.Admin);

        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CreateTestCompany(db, userId, user);

        var client = app.CreateClient(userId, role: RoleNames.Admin);

        // Create multipart form data content
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("New Company Name"), "Name");
        content.Add(new StringContent("New Description"), "Description");
        content.Add(new StringContent("false"), "RemoveLogo");
        content.Add(new StringContent("https://example.com"), "WebsiteUrl");
        // Make sure content type is set correctly
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var res = await client.PostAsync("/company/info", content);

        Assert.True(res.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Update_Company_Without_Image_As_ProjectManager_Returns_Forbidden()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId, role: RoleNames.ProjectManager);

        var user = await db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CreateTestCompany(db, userId, user);

        var client = app.CreateClient(userId, role: RoleNames.ProjectManager);

        // Create multipart form data content
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("New Company Name"), "Name");
        content.Add(new StringContent("New Description"), "Description");
        content.Add(new StringContent("false"), "RemoveLogo");
        content.Add(new StringContent("https://example.com"), "WebsiteUrl");
        // Make sure content type is set correctly
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var res = await client.PostAsync("/company/info", content);

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
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