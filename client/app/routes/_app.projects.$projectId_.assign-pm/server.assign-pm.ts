import { BasicUserInfo, ProjectManagerInfo } from "~/services/api.server/types";
import { DEV_URL, requestJson } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

export async function assignPM(
  projectId: string,
  memberId: string,
  accessToken: string
) {
  return await requestJson<ProjectManagerInfo[]>(
    `/projects/${projectId}/assign-pm`,
    {
      method: "PUT",
      body: JSON.stringify({ memberId }),
    },
    accessToken
  );
}
