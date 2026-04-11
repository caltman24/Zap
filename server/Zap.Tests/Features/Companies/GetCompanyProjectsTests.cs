namespace Zap.Tests.Features.Companies;

public sealed class GetCompanyProjectsTests : IntegrationTestBase
{
    [Fact]
    public async Task Get_Company_Projects_Unarchived_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var companyId = Guid.NewGuid().ToString();
        var archivedProjectId = Guid.NewGuid().ToString();
        var activeProjectId = Guid.NewGuid().ToString();

        await CompanyTestData.CreateTestCompanyAsync(_db, userId, user,
        [
            new Project
            {
                Id = archivedProjectId,
                Name = "Test Project",
                Description = "Test Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = true
            },
            new Project
            {
                Id = activeProjectId,
                Name = "Test Project",
                Description = "Test Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = false
            }
        ], companyId);

        var client = _app.CreateClient(userId);
        var response = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects?isArchived=false");

        Assert.NotNull(response);
        Assert.Null(response.FirstOrDefault(project => project.Id == archivedProjectId && project.IsArchived));
        Assert.NotNull(response.FirstOrDefault(project => project.Id == activeProjectId && !project.IsArchived));
    }

    [Fact]
    public async Task Get_Company_Projects_Archived_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var companyId = Guid.NewGuid().ToString();
        var archivedProjectId = Guid.NewGuid().ToString();
        var activeProjectId = Guid.NewGuid().ToString();

        await CompanyTestData.CreateTestCompanyAsync(_db, userId, user,
        [
            new Project
            {
                Id = archivedProjectId,
                Name = "Test Project",
                Description = "Test Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = true
            },
            new Project
            {
                Id = activeProjectId,
                Name = "Test Project",
                Description = "Test Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = false
            }
        ], companyId);

        var client = _app.CreateClient(userId);
        var response = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects?isArchived=true");

        Assert.NotNull(response);
        Assert.NotNull(response.FirstOrDefault(project => project.Id == archivedProjectId && project.IsArchived));
        Assert.Null(response.FirstOrDefault(project => project.Id == activeProjectId && !project.IsArchived));
    }

    [Fact]
    public async Task Get_Company_Projects_Without_Filter_Returns_Unarchived_Only()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var companyId = Guid.NewGuid().ToString();
        var archivedProjectId = Guid.NewGuid().ToString();
        var activeProjectId = Guid.NewGuid().ToString();

        await CompanyTestData.CreateTestCompanyAsync(_db, userId, user,
        [
            new Project
            {
                Id = archivedProjectId,
                Name = "Test Project",
                Description = "Test Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = true
            },
            new Project
            {
                Id = activeProjectId,
                Name = "Test Project",
                Description = "Test Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = false
            }
        ], companyId);

        var client = _app.CreateClient(userId);
        var response = await client.GetFromJsonAsync<List<CompanyProjectDto>>("/company/projects");

        Assert.NotNull(response);
        Assert.Null(response.FirstOrDefault(project => project.Id == archivedProjectId && project.IsArchived));
        Assert.NotNull(response.FirstOrDefault(project => project.Id == activeProjectId && !project.IsArchived));
    }

    [Fact]
    public async Task Get_Company_Projects_As_Developer_Returns_Forbidden()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        await CompanyTestData.CreateTestCompanyAsync(_db, userId, user, role: RoleNames.Developer);
        var client = _app.CreateClient(userId, RoleNames.Developer);

        var response = await client.GetAsync("/company/projects?isArchived=false");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}