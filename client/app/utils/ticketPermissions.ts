import permissions from "~/data/permissions";
import roleNames, { type RoleName } from "~/data/roles";

export interface TicketPermissionContext {
  role: RoleName;
  userId: string | undefined;
  ticketSubmitterId: string;
  ticketAssignedDeveloperId: string | null;
  isProjectManager: boolean;
  isArchived: boolean;
  status: string;
}

/**
 * Check if user has a specific permission based on their role
 */
function normalizeRole(role: string | null | undefined): string {
  return (role ?? "").trim().toLowerCase();
}

function hasPermission(role: string, allowedRoles: RoleName[]): boolean {
  const normalizedRole = normalizeRole(role);
  return allowedRoles.some(
    (allowedRole) => normalizeRole(allowedRole) === normalizedRole,
  );
}

/**
 * Check if user can edit all ticket fields (name, description, priority, status, type)
 * Admin and PM can always edit. Submitters can edit their own tickets.
 */
export function canEditTicketFields(context: TicketPermissionContext): boolean {
  return (
    canEditNameDescription(context) ||
    canUpdatePriority(context) ||
    canUpdateStatus(context) ||
    canUpdateType(context)
  );
}

/**
 * Check if user can update ticket status
 * Developers can update status if assigned.
 */
export function canUpdateStatus(context: TicketPermissionContext): boolean {
  const role = normalizeRole(context.role);

  if (context.isArchived) return false;

  // Admin and PM can always update status
  if (
    role === roleNames.admin ||
    role === roleNames.projectManager
  ) {
    return true;
  }

  // Developers can update status if assigned
  if (role === roleNames.developer) {
    if (!context.userId) return false;
    return context.ticketAssignedDeveloperId === context.userId;
  }

  return false;
}

/**
 * Check if user can update ticket priority
 * Only Admin and PM can update priority
 */
export function canUpdatePriority(context: TicketPermissionContext): boolean {
  const role = normalizeRole(context.role);

  if (context.isArchived) return false;

  if (hasPermission(role, permissions.ticket.editPriority)) {
    return true;
  }

  return false;
}

/**
 * Check if user can update ticket type
 * Only Admin and PM can update type
 */
export function canUpdateType(context: TicketPermissionContext): boolean {
  const role = normalizeRole(context.role);

  if (context.isArchived) return false;

  if (hasPermission(role, permissions.ticket.editType)) {
    return true;
  }

  return false;
}

/**
 * Check if user can assign/unassign developers
 * Only Admin and PM can assign developers
 */
export function canAssignDeveloper(
  role: RoleName,
  isArchived: boolean,
  isProjectManager: boolean,
): boolean {
  if (isArchived) return false;
  const normalizedRole = normalizeRole(role);
  if (normalizedRole === roleNames.admin) return true;
  if (normalizedRole === roleNames.projectManager) return isProjectManager;
  return false;
}

/**
 * Check if user can delete ticket
 * Only Admin and PM can delete tickets
 */
export function canDeleteTicket(
  role: RoleName,
  isArchived: boolean,
  isProjectManager: boolean,
): boolean {
  if (isArchived) return false;
  const normalizedRole = normalizeRole(role);
  if (normalizedRole === roleNames.admin) return true;
  if (normalizedRole === roleNames.projectManager) return isProjectManager;
  return false;
}

/**
 * Check if user can archive ticket
 * Only Admin and PM can archive tickets
 */
export function canArchiveTicket(
  role: RoleName,
  isProjectManager: boolean,
): boolean {
  const normalizedRole = normalizeRole(role);
  if (normalizedRole === roleNames.admin) return true;
  if (normalizedRole === roleNames.projectManager) return isProjectManager;
  return false;
}

/**
 * Check if user can unarchive ticket
 * Only Admin and PM can unarchive tickets (PM must be the project manager)
 */
export function canUnarchiveTicket(
  role: RoleName,
  isProjectManager: boolean,
): boolean {
  const normalizedRole = normalizeRole(role);
  if (normalizedRole === roleNames.admin) return true;
  if (normalizedRole === roleNames.projectManager && isProjectManager) return true;
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
 * Users can delete their own comments
 */
export function canDeleteComment(
  role: RoleName,
  isCommentAuthor: boolean,
  isArchived: boolean,
): boolean {
  if (isArchived) return false;
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
  const role = normalizeRole(context.role);

  if (role === roleNames.admin) {
    return true;
  }

  if (role === roleNames.projectManager) {
    return context.isProjectManager;
  }

  if (context.isArchived) return false;

  if (role === roleNames.submitter) {
    return (
      context.userId === context.ticketSubmitterId &&
      context.status === "New"
    );
  }

  return false;
}
