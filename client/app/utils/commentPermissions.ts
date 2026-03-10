import permissions from "~/data/permissions";
import { type RoleName } from "~/data/roles";

function normalizeRole(role: string | null | undefined): string {
  return (role ?? "").trim().toLowerCase();
}

function hasRolePermission(role: string, allowedRoles: RoleName[]): boolean {
  const normalizedRole = normalizeRole(role);
  return allowedRoles.some(
    (allowedRole) => normalizeRole(allowedRole) === normalizedRole,
  );
}

export function canEditComment(
  role: RoleName,
  isCommentAuthor: boolean,
  isArchived: boolean,
): boolean {
  if (isArchived || !isCommentAuthor) return false;
  return hasRolePermission(role, permissions.comment.editOwn);
}

export function canDeleteComment(
  role: RoleName,
  isCommentAuthor: boolean,
  isArchived: boolean,
): boolean {
  if (isArchived || !isCommentAuthor) return false;
  return hasRolePermission(role, permissions.comment.deleteOwn);
}
