namespace Zap.Tests.Unit.Tickets;

public sealed class CreateTicketRequestValidatorTests
{
    private readonly CreateTicket.RequestValidator _validator = new();

    [Fact]
    public void Validate_When_Request_Is_Valid_Passes()
    {
        var result = _validator.Validate(new CreateTicket.Request(
            "Ticket",
            "Description",
            Priorities.High,
            TicketStatuses.New,
            TicketTypes.Feature,
            "project-1"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_When_Name_Is_Too_Long_Fails()
    {
        var result = _validator.Validate(new CreateTicket.Request(
            new string('a', 51),
            "Description",
            Priorities.High,
            TicketStatuses.New,
            TicketTypes.Feature,
            "project-1"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_When_Description_Is_Too_Long_Fails()
    {
        var result = _validator.Validate(new CreateTicket.Request(
            "Ticket",
            new string('a', 1001),
            Priorities.High,
            TicketStatuses.New,
            TicketTypes.Feature,
            "project-1"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Description");
    }

    [Fact]
    public void Validate_When_Priority_Is_Invalid_Fails()
    {
        var result = _validator.Validate(new CreateTicket.Request(
            "Ticket",
            "Description",
            "Critical",
            TicketStatuses.New,
            TicketTypes.Feature,
            "project-1"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Priority");
    }

    [Fact]
    public void Validate_When_Status_Is_Invalid_Fails()
    {
        var result = _validator.Validate(new CreateTicket.Request(
            "Ticket",
            "Description",
            Priorities.High,
            "Waiting",
            TicketTypes.Feature,
            "project-1"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Status");
    }

    [Fact]
    public void Validate_When_Type_Is_Invalid_Fails()
    {
        var result = _validator.Validate(new CreateTicket.Request(
            "Ticket",
            "Description",
            Priorities.High,
            TicketStatuses.New,
            "Epic",
            "project-1"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Type");
    }
}
