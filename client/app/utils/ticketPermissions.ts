import permissions from "~/data/permissions";
import roleNames, { type RoleName } from "~/data/roles";

export interface TicketPermissionContext {
  role: RoleName;
  userId: string;
  ticketSubmitterId: string;
  ticketAssignedDeveloperId: string | null;
  isProjectManager: boolean;
  isArchived: boolean;
}

/**
 * Check if user has a specific permission based on their role
 */
function hasPermission(role: RoleName, allowedRoles: RoleName[]): boolean {
  return allowedRoles.includes(role);
}

/**
 * Check if user can edit all ticket fields (name, description, priority, status, type)
 * Admin and PM can always edit. Submitters can edit their own tickets.
 */
export function canEditTicketFields(context: TicketPermissionContext): boolean {
  if (context.isArchived) return false;

  // Admin and PM can edit any ticket
  if (hasPermission(context.role, permissions.ticket.edit)) {
    return true;
  }

  // Submitters can edit their own tickets
  if (context.role === roleNames.submitter) {
    return context.userId === context.ticketSubmitterId;
  }

  return false;
}

/**
 * Check if user can update ticket status
 * Developers can update status if assigned. Submitters can update their own.
 */
export function canUpdateStatus(context: TicketPermissionContext): boolean {
  if (context.isArchived) return false;

  // Admin and PM can always update status
  if (
    context.role === roleNames.admin ||
    context.role === roleNames.projectManager
  ) {
    return true;
  }

  // Developers can update status if assigned
  if (context.role === roleNames.developer) {
    return context.ticketAssignedDeveloperId === context.userId;
  }

  // Submitters can update status of their own tickets
  if (context.role === roleNames.submitter) {
    return context.userId === context.ticketSubmitterId;
  }

  return false;
}

/**
 * Check if user can update ticket priority
 * Only Admin, PM, and submitters (their own tickets) can update priority
 */
export function canUpdatePriority(context: TicketPermissionContext): boolean {
  if (context.isArchived) return false;

  // Admin and PM can always update priority
  if (hasPermission(context.role, permissions.ticket.editPriority)) {
    // Submitters need to be the ticket owner
    if (context.role === roleNames.submitter) {
      return context.userId === context.ticketSubmitterId;
    }
    return true;
  }

  return false;
}

/**
 * Check if user can update ticket type
 * Only Admin, PM, and submitters (their own tickets) can update type
 */
export function canUpdateType(context: TicketPermissionContext): boolean {
  if (context.isArchived) return false;

  // Admin and PM can always update type
  if (hasPermission(context.role, permissions.ticket.editType)) {
    // Submitters need to be the ticket owner
    if (context.role === roleNames.submitter) {
      return context.userId === context.ticketSubmitterId;
    }
    return true;
  }

  return false;
}

/**
 * Check if user can assign/unassign developers
 * Only Admin and PM can assign developers
 */
export function canAssignDeveloper(role: RoleName, isArchived: boolean): boolean {
  if (isArchived) return false;
  return hasPermission(role, permissions.ticket.assign);
}

/**
 * Check if user can delete ticket
 * Only Admin and PM can delete tickets
 */
export function canDeleteTicket(role: RoleName, isArchived: boolean): boolean {
  if (isArchived) return false;
  return hasPermission(role, permissions.ticket.delete);
}

/**
 * Check if user can archive ticket
 * Only Admin and PM can archive tickets
 */
export function canArchiveTicket(role: RoleName): boolean {
  return hasPermission(role, permissions.ticket.archive);
}

/**
 * Check if user can unarchive ticket
 * Only Admin and PM can unarchive tickets (PM must be the project manager)
 */
export function canUnarchiveTicket(
  role: RoleName,
  isProjectManager: boolean,
): boolean {
  if (role === roleNames.admin) return true;
  if (role === roleNames.projectManager && isProjectManager) return true;
  return false;
}

/**
 * Check if user can create comments
 * All roles can create comments on non-archived tickets
 */
export function canCreateComment(
  role: RoleName,
  isArchived: boolean,
): boolean {
  if (isArchived) return false;
  return hasPermission(role, permissions.comment.create);
}

/**
 * Check if user can edit a comment
 * Users can edit their own comments, archived tickets prevent edits
 */
export function canEditComment(
  role: RoleName,
  isCommentAuthor: boolean,
  isArchived: boolean,
): boolean {
  if (isArchived) return false;
  if (!isCommentAuthor) return false;
  return hasPermission(role, permissions.comment.editOwn);
}

/**
 * Check if user can delete a comment
 * Users can delete their own comments, admins can delete any comment
 */
export function canDeleteComment(
  role: RoleName,
  isCommentAuthor: boolean,
  isArchived: boolean,
): boolean {
  if (isArchived) return false;
  
  // Admins can delete any comment
  if (role === roleNames.admin) return true;
  
  // Users can delete their own comments
  if (!isCommentAuthor) return false;
  return hasPermission(role, permissions.comment.deleteOwn);
}

/**
 * Check if user can edit ticket name/description
 * Submitters can edit name/description of their own tickets
 */
export function canEditNameDescription(
  context: TicketPermissionContext,
): boolean {
  if (context.isArchived) {
    // Only admin and PM can edit archived ticket name/description
    return (
      context.role === roleNames.admin ||
      context.role === roleNames.projectManager
    );
  }

  // Admin and PM can always edit
  if (hasPermission(context.role, permissions.ticket.edit)) {
    return true;
  }

  // Submitters can edit their own tickets
  if (context.role === roleNames.submitter) {
    return context.userId === context.ticketSubmitterId;
  }

  return false;
}
