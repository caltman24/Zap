namespace Zap.Tests.Features.Companies;

public sealed class GetCompanySearchTests : IntegrationTestBase
{
    public GetCompanySearchTests() : base(false)
    {
    }

    [Fact]
    public async Task Search_ProjectName_SecondWordPrefix_Returns_Project()
    {
        var token = CreateAlphaToken();
        var query = $"report{token[..3]}";
        var projectName = $"Ops{token} Report{token}";

        var (client, company, adminMember) = await CreateAdminClientWithCompanyAsync();

        _db.Projects.Add(new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = projectName,
            Description = "Ignored by search",
            Priority = "High",
            CompanyId = company.Id,
            DueDate = DateTime.UtcNow.AddDays(7),
            ProjectManagerId = adminMember.Id,
            IsArchived = false
        });

        await _db.SaveChangesAsync();

        var response = await SearchAsync(client, query);

        Assert.Contains(response, result => result.Type == "project" && result.Name == projectName);
    }

    [Fact]
    public async Task Search_TicketName_SecondWordPrefix_Returns_Ticket()
    {
        var token = CreateAlphaToken();
        var query = $"review{token[..2]}";
        var ticketName = $"Sprint{token} Review{token}";

        var (client, company, adminMember) = await CreateAdminClientWithCompanyAsync();
        var project = await CreateProjectAsync(company.Id, adminMember.Id, $"Project{token}");
        var ticket = await CreateTicketAsync(project.Id, adminMember.Id, ticketName);

        var response = await SearchAsync(client, query);

        Assert.Contains(response,
            result => result.Type == "ticket" && result.Id == ticket.Id && result.Name == ticketName);
    }

    [Fact]
    public async Task Search_DisplayId_Exact_Prefix_And_Partial_Return_Ticket_And_Partial_Ranks_Above_Name()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];

        var (client, company, adminMember) = await CreateAdminClientWithCompanyAsync();
        var ticketProject = await CreateProjectAsync(company.Id, adminMember.Id, $"TicketProject{unique}");
        var ticket = await CreateTicketAsync(ticketProject.Id, adminMember.Id, $"Ticket{unique}");
        await _db.Entry(ticket).ReloadAsync();

        var normalizedDisplayId = ticket.DisplayId.Replace("#", string.Empty).Replace("-", string.Empty);
        var partialQuery = normalizedDisplayId[^2..];

        _db.Projects.Add(new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{partialQuery} Roadmap {unique}",
            Description = "Should rank below the ticket display id",
            Priority = "Medium",
            CompanyId = company.Id,
            DueDate = DateTime.UtcNow.AddDays(14),
            ProjectManagerId = adminMember.Id,
            IsArchived = false
        });

        await _db.SaveChangesAsync();

        var exactResponse = await SearchAsync(client, ticket.DisplayId);
        var prefixResponse = await SearchAsync(client, ticket.DisplayId[..^2]);
        var partialResponse = await SearchAsync(client, partialQuery);

        Assert.Equal(ticket.Id, exactResponse[0].Id);
        Assert.Equal(ticket.Id, prefixResponse[0].Id);
        Assert.Equal(ticket.Id, partialResponse[0].Id);
        Assert.Contains(partialResponse,
            result => result.Type == "project" && result.Name == $"{partialQuery} Roadmap {unique}");
    }

    [Fact]
    public async Task Search_AsDeveloper_Returns_Only_Assigned_Active_Project_And_Ticket()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var query = $"scope{unique[..3]}";

        var adminUserId = Guid.NewGuid().ToString();
        var developerUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(adminUserId);
        await _app.CreateUserAsync(developerUserId);

        var adminUser = await _db.Users.FindAsync(adminUserId);
        Assert.NotNull(adminUser);

        var company = await CompanyTestData.CreateTestCompanyAsync(_db, adminUserId, adminUser, role: RoleNames.Admin);
        var adminMember =
            await _db.CompanyMembers.FirstAsync(member =>
                member.UserId == adminUserId && member.CompanyId == company.Id);

        var developerMember = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = developerUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(role => role.Name == RoleNames.Developer).Select(role => role.Id)
                .FirstAsync()
        };

        var visibleProject = await CreateProjectAsync(company.Id, adminMember.Id, $"Scope{unique} Visible");
        var hiddenProject = await CreateProjectAsync(company.Id, adminMember.Id, $"Scope{unique} Hidden");
        var archivedProject = await CreateProjectAsync(company.Id, adminMember.Id, $"Scope{unique} Archived", true);

        developerMember.AssignedProjects.Add(visibleProject);
        developerMember.AssignedProjects.Add(archivedProject);
        _db.CompanyMembers.Add(developerMember);

        var visibleTicket = await CreateTicketAsync(visibleProject.Id, adminMember.Id, $"Scope{unique} Visible Ticket");
        var hiddenTicket = await CreateTicketAsync(hiddenProject.Id, adminMember.Id, $"Scope{unique} Hidden Ticket");
        var archivedTicket =
            await CreateTicketAsync(archivedProject.Id, adminMember.Id, $"Scope{unique} Archived Ticket", true);

        await _db.SaveChangesAsync();

        var client = _app.CreateClient(developerUserId, RoleNames.Developer);
        var response = await SearchAsync(client, query);

        Assert.Contains(response, result => result.Type == "project" && result.Id == visibleProject.Id);
        Assert.Contains(response, result => result.Type == "ticket" && result.Id == visibleTicket.Id);
        Assert.DoesNotContain(response, result => result.Id == hiddenProject.Id || result.Id == hiddenTicket.Id);
        Assert.DoesNotContain(response, result => result.Id == archivedProject.Id || result.Id == archivedTicket.Id);
    }

    private async Task<(HttpClient Client, Company Company, CompanyMember AdminMember)>
        CreateAdminClientWithCompanyAsync()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);

        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CompanyTestData.CreateTestCompanyAsync(_db, userId, user, role: RoleNames.Admin);
        var adminMember =
            await _db.CompanyMembers.FirstAsync(member => member.UserId == userId && member.CompanyId == company.Id);

        return (_app.CreateClient(userId), company, adminMember);
    }

    private async Task<Project> CreateProjectAsync(string companyId, string projectManagerId, string name,
        bool isArchived = false)
    {
        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"{name} description",
            Priority = "High",
            CompanyId = companyId,
            ProjectManagerId = projectManagerId,
            DueDate = DateTime.UtcNow.AddDays(14),
            IsArchived = isArchived
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return project;
    }

    private async Task<Ticket> CreateTicketAsync(string projectId, string submitterId, string name,
        bool isArchived = false)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"{name} description",
            ProjectId = projectId,
            SubmitterId = submitterId,
            PriorityId = await _db.TicketPriorities.Where(priority => priority.Name == Priorities.Low)
                .Select(priority => priority.Id).FirstAsync(),
            StatusId = await _db.TicketStatuses.Where(status => status.Name == TicketStatuses.New)
                .Select(status => status.Id).FirstAsync(),
            TypeId = await _db.TicketTypes.Where(type => type.Name == TicketTypes.Defect).Select(type => type.Id)
                .FirstAsync(),
            IsArchived = isArchived
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        return ticket;
    }

    private static async Task<List<SearchResultResponse>> SearchAsync(HttpClient client, string query)
    {
        var response = await client.GetAsync($"/company/search?query={Uri.EscapeDataString(query)}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<SearchResultResponse>>();
        Assert.NotNull(payload);

        return payload;
    }

    private static string CreateAlphaToken()
    {
        var letters = new string(Guid.NewGuid().ToString("N")
            .Where(char.IsLetter)
            .ToArray());

        return letters.Length >= 6
            ? letters[..6]
            : (letters + "searchabc")[..6];
    }

    private sealed record SearchResultResponse(
        string Type,
        string Id,
        string? ProjectId,
        string Name,
        string? DisplayId);
}