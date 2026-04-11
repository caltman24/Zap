using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Tickets;

public sealed class TicketHistoryFormattersTests
{
    [Fact]
    public void CreatedFormatter_Formats_Message_With_Creator_Name()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.Created, creatorName: "Jane Smith");

        var formatted = CreatedFormatter.FormatHistoryEntry(entry);

        Assert.Contains("Ticket created by: Jane Smith", formatted);
    }

    [Fact]
    public void UpdateNameFormatter_Formats_Old_And_New_Values()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.UpdateName, oldValue: "Old", newValue: "New");

        var formatted = UpdateNameFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Name updated from 'Old' to 'New' by John Doe", formatted);
    }

    [Fact]
    public void UpdateDescriptionFormatter_Returns_Fixed_Message()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.UpdateDescription);

        var formatted = UpdateDescriptionFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Description updated", formatted);
    }

    [Fact]
    public void UpdateStatusFormatter_Formats_Old_And_New_Values()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.UpdateStatus,
            oldValue: TicketStatuses.New,
            newValue: TicketStatuses.Testing);

        var formatted = UpdateStatusFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Status updated from 'New' to 'Testing' by John Doe", formatted);
    }

    [Fact]
    public void UpdateTypeFormatter_Formats_Old_And_New_Values()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.UpdateType,
            oldValue: TicketTypes.Defect,
            newValue: TicketTypes.Feature);

        var formatted = UpdateTypeFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Type updated from 'Defect' to 'Feature' by John Doe", formatted);
    }

    [Fact]
    public void UpdatePriorityFormatter_Formats_Old_And_New_Values()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.UpdatePriority,
            oldValue: Priorities.Low,
            newValue: Priorities.Urgent);

        var formatted = UpdatePriorityFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Priority updated from 'Low' to 'Urgent' by John Doe", formatted);
    }

    [Fact]
    public void ArchivedFormatter_Formats_Message()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.Archived);

        var formatted = ArchivedFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Moved to Archived by John Doe", formatted);
    }

    [Fact]
    public void UnarchivedFormatter_Formats_Message()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.Unarchived);

        var formatted = UnarchivedFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Moved from Archived by John Doe", formatted);
    }

    [Fact]
    public void ResolvedFormatter_Formats_Message()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.Resolved);

        var formatted = ResolvedFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Marked as resolved by John Doe", formatted);
    }

    [Fact]
    public void DeveloperAssignedFormatter_Formats_Related_Entity_Name()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.DeveloperAssigned,
            relatedEntityName: "Alex Dev");

        var formatted = DeveloperAssignedFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Assigned to Alex Dev by John Doe", formatted);
    }

    [Fact]
    public void DeveloperRemovedFormatter_Formats_Related_Entity_Name()
    {
        var entry = UnitTestFactory.CreateHistoryEntry(TicketHistoryTypes.DeveloperRemoved,
            relatedEntityName: "Alex Dev");

        var formatted = DeveloperRemovedFormatter.FormatHistoryEntry(entry);

        Assert.Equal("Assigned developer Alex Dev removed by John Doe", formatted);
    }
}
