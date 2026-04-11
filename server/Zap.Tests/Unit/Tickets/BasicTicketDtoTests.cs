namespace Zap.Tests.Unit.Tickets;

public sealed class BasicTicketDtoTests
{
    [Fact]
    public void FormatDisplayId_When_Given_Guid_Returns_Uppercase_Suffix()
    {
        var displayId = BasicTicketDto.FormatDisplayId("12345678-abcd-ef90-1234-56789abcdeff");

        Assert.Equal("#ZAP-DEFF", displayId);
    }

    [Fact]
    public void FormatDisplayId_When_Given_Short_Value_Pads_Left()
    {
        var displayId = BasicTicketDto.FormatDisplayId("ab");

        Assert.Equal("#ZAP-00AB", displayId);
    }

    [Fact]
    public void DisplayId_When_StoredDisplayId_Is_Present_Uses_It()
    {
        var ticket = new BasicTicketDto(
            "ticket-1",
            "Ticket",
            "Description",
            Priorities.Low,
            TicketStatuses.New,
            TicketTypes.Defect,
            "project-1",
            null,
            false,
            false,
            DateTime.UtcNow,
            null,
            new MemberInfoDto("submitter-1", "Submitter", "avatar"),
            null)
        {
            StoredDisplayId = "#ZAP-CUSTOM"
        };

        Assert.Equal("#ZAP-CUSTOM", ticket.DisplayId);
    }

    [Fact]
    public void DisplayId_When_StoredDisplayId_Is_Missing_Formats_Id()
    {
        var ticket = new BasicTicketDto(
            "abcd",
            "Ticket",
            "Description",
            Priorities.Low,
            TicketStatuses.New,
            TicketTypes.Defect,
            "project-1",
            null,
            false,
            false,
            DateTime.UtcNow,
            null,
            new MemberInfoDto("submitter-1", "Submitter", "avatar"),
            null);

        Assert.Equal("#ZAP-ABCD", ticket.DisplayId);
    }
}
