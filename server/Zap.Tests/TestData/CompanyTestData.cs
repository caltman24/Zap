namespace Zap.Tests.TestData;

internal static class CompanyTestData
{
    internal static async Task<Company> CreateTestCompanyAsync(AppDbContext db, string userId, AppUser user,
        List<Project>? projects = null, string? companyId = null, string? role = null)
    {
        var roleName = role ?? RoleNames.Admin;

        var company = new Company
        {
            Id = companyId ?? Guid.NewGuid().ToString(),
            Name = "Test Company",
            Description = "Test Description",
            OwnerId = userId,
            Members =
            [
                new CompanyMember
                {
                    UserId = userId,
                    RoleId = await db.CompanyRoles
                        .Where(r => r.Name == roleName)
                        .Select(r => r.Id)
                        .FirstAsync()
                }
            ]
        };

        if (projects != null) company.Projects = projects;

        var createdCompany = db.Companies.Add(company);
        await db.SaveChangesAsync();

        return createdCompany.Entity;
    }
}