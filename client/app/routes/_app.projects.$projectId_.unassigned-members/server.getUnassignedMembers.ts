import { CompanyMemberPerRole } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

async function getUnassignedProjectMembers(
  projectId: string,
  accessToken: string
): Promise<CompanyMemberPerRole> {
  return requestJson<CompanyMemberPerRole>(
    `/projects/${projectId}/members/unassigned`,
    { method: "GET" },
    accessToken
  );
}

export default getUnassignedProjectMembers;
