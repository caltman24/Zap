import { ProjectManagerInfo } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function assignPM(
  projectId: string,
  memberId: string,
  accessToken: string
) {
  return await requestJson<ProjectManagerInfo[]>(
    `/projects/${projectId}/pm`,
    {
      method: "PUT",
      body: JSON.stringify({ memberId }),
    },
    accessToken
  );
}
