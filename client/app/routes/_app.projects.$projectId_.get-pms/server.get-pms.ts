import { ProjectManagerInfo } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getAssignablePMs(projectId: string, accessToken: string) {
  return await requestJson<ProjectManagerInfo[]>(
    `/projects/${projectId}/assignable-pms`,
    { method: "GET" },
    accessToken
  );
}
