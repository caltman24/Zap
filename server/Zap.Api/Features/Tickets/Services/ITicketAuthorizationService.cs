using Zap.Api.Common.Authorization;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketAuthorizationService
{
    Task<bool> CanReadTicketAsync(string ticketId, CurrentUser currentUser);
    Task<bool> CanCreateTicketInProjectAsync(string projectId, CurrentUser currentUser);
    Task<bool> CanEditTicketDetailsAsync(string ticketId, CurrentUser currentUser);
    Task<bool> CanUpdateStatusAsync(string ticketId, CurrentUser currentUser);
    Task<bool> CanUpdatePriorityAsync(string ticketId, CurrentUser currentUser);
    Task<bool> CanUpdateTypeAsync(string ticketId, CurrentUser currentUser);
    Task<bool> CanAssignDeveloperAsync(string ticketId, CurrentUser currentUser);
    Task<bool> CanArchiveTicketAsync(string ticketId, CurrentUser currentUser);
    Task<bool> CanDeleteTicketAsync(string ticketId, CurrentUser currentUser);
    TicketCapabilitiesDto GetCapabilities(BasicTicketDto ticket, CurrentUser currentUser);
}

public record TicketCapabilitiesDto(
    bool CanEditDetails,
    bool CanEditNameDescription,
    bool CanUpdatePriority,
    bool CanUpdateStatus,
    bool CanUpdateType,
    bool CanAssignDeveloper,
    bool CanArchive,
    bool CanUnarchive,
    bool CanDelete,
    bool CanComment);
