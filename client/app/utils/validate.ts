import { RoleName } from "~/data/roles";

export function validateRole(role: string, allowed: RoleName[]) {
  return allowed.includes(role.toLowerCase() as RoleName);
}
