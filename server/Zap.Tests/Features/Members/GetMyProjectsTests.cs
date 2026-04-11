namespace Zap.Tests.Features.Members;

public sealed class GetMyProjectsTests : IntegrationTestBase
{
    [Fact]
    public async Task Get_My_Projects_As_Developer_Returns_Assigned_Unarchived_Projects_Only()
    {
        var adminUserId = Guid.NewGuid().ToString();
        var developerUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(adminUserId);
        await _app.CreateUserAsync(developerUserId);

        var adminUser = await _db.Users.FindAsync(adminUserId);
        Assert.NotNull(adminUser);

        var companyId = Guid.NewGuid().ToString();
        var assignedProjectId = Guid.NewGuid().ToString();
        var hiddenProjectId = Guid.NewGuid().ToString();
        var archivedProjectId = Guid.NewGuid().ToString();

        await CompanyTestData.CreateTestCompanyAsync(_db, adminUserId, adminUser,
        [
            new Project
            {
                Id = assignedProjectId,
                Name = "Assigned Project",
                Description = "Assigned Project",
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
            },
            new Project
            {
                Id = archivedProjectId,
                Name = "Archived Project",
                Description = "Archived Project",
                Priority = "Urgent",
                CompanyId = companyId,
                DueDate = DateTime.Now.AddDays(1),
                IsArchived = true
            }
        ], companyId);

        var developerMember = new CompanyMember
        {
            UserId = developerUserId,
            CompanyId = companyId,
            RoleId = await _db.CompanyRoles.Where(role => role.Name == RoleNames.Developer).Select(role => role.Id)
                .FirstAsync()
        };

        developerMember.AssignedProjects.Add(await _db.Projects.FirstAsync(project => project.Id == assignedProjectId));
        developerMember.AssignedProjects.Add(await _db.Projects.FirstAsync(project => project.Id == archivedProjectId));
        _db.CompanyMembers.Add(developerMember);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(developerUserId, RoleNames.Developer);
        var response =
            await client.GetFromJsonAsync<List<CompanyProjectDto>>($"/members/{developerMember.Id}/myprojects");

        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal(assignedProjectId, response[0].Id);
    }
}