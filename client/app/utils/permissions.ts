export function hasPermission(
  permissions: string[] | null | undefined,
  requiredPermission: string,
) {
  return (permissions ?? []).includes(requiredPermission);
}
