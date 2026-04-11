namespace Zap.Tests.Features.Projects;

public sealed class GetProjectTests : IntegrationTestBase
{
    [Fact]
    public async Task Get_Project_As_Developer_Outside_Project_Returns_Forbidden()
    {
        var adminUserId = Guid.NewGuid().ToString();
        var developerUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(adminUserId);
        await _app.CreateUserAsync(developerUserId);

        var adminUser = await _db.Users.FindAsync(adminUserId);
        Assert.NotNull(adminUser);

        var companyId = Guid.NewGuid().ToString();
        var visibleProjectId = Guid.NewGuid().ToString();
        var hiddenProjectId = Guid.NewGuid().ToString();

        await CompanyTestData.CreateTestCompanyAsync(_db, adminUserId, adminUser,
        [
            new Project
            {
                Id = visibleProjectId,
                Name = "Visible Project",
                Description = "Visible Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = false
            },
            new Project
            {
                Id = hiddenProjectId,
                Name = "Hidden Project",
                Description = "Hidden Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = false
            }
        ], companyId);

        var developerMember = new CompanyMember
        {
            UserId = developerUserId,
            CompanyId = companyId,
            RoleId = await _db.CompanyRoles.Where(role => role.Name == RoleNames.Developer).Select(role => role.Id)
                .FirstAsync()
        };

        developerMember.AssignedProjects.Add(await _db.Projects.FirstAsync(project => project.Id == visibleProjectId));
        _db.CompanyMembers.Add(developerMember);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(developerUserId, RoleNames.Developer);

        var visibleResponse = await client.GetAsync($"/projects/{visibleProjectId}");
        var hiddenResponse = await client.GetAsync($"/projects/{hiddenProjectId}");

        Assert.Equal(HttpStatusCode.OK, visibleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, hiddenResponse.StatusCode);
    }
}