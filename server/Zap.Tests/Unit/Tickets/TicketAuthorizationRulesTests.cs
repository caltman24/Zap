using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Tickets;

public sealed class TicketAuthorizationRulesTests
{
    [Fact]
    public void CanCommentOnTicket_When_Admin_Returns_True()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Admin, memberId: "admin-1");

        var canComment = TicketAuthorizationRules.CanCommentOnTicket("pm-1", "submitter-1", "dev-1", currentUser);

        Assert.True(canComment);
    }

    [Fact]
    public void CanCommentOnTicket_When_ProjectManager_Owns_Ticket_Returns_True()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.ProjectManager, memberId: "pm-1");

        var canComment = TicketAuthorizationRules.CanCommentOnTicket("pm-1", "submitter-1", "dev-1", currentUser);

        Assert.True(canComment);
    }

    [Fact]
    public void CanCommentOnTicket_When_ProjectManager_DoesNot_Own_Ticket_Returns_False()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.ProjectManager, memberId: "pm-2");

        var canComment = TicketAuthorizationRules.CanCommentOnTicket("pm-1", "submitter-1", "dev-1", currentUser);

        Assert.False(canComment);
    }

    [Fact]
    public void CanCommentOnTicket_When_Assigned_Developer_Returns_True()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, memberId: "dev-1");

        var canComment = TicketAuthorizationRules.CanCommentOnTicket("pm-1", "submitter-1", "dev-1", currentUser);

        Assert.True(canComment);
    }

    [Fact]
    public void CanCommentOnTicket_When_Unassigned_Developer_Returns_False()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, memberId: "dev-2");

        var canComment = TicketAuthorizationRules.CanCommentOnTicket("pm-1", "submitter-1", "dev-1", currentUser);

        Assert.False(canComment);
    }

    [Fact]
    public void CanCommentOnTicket_When_Ticket_Submitter_Returns_True()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, memberId: "submitter-1");

        var canComment = TicketAuthorizationRules.CanCommentOnTicket("pm-1", "submitter-1", "dev-1", currentUser);

        Assert.True(canComment);
    }

    [Fact]
    public void CanCommentOnTicket_When_CurrentUser_Has_No_Member_Returns_False()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(null);

        var canComment = TicketAuthorizationRules.CanCommentOnTicket("pm-1", "submitter-1", "dev-1", currentUser);

        Assert.False(canComment);
    }
}
