import { BasicUserInfo } from "~/services/api.server/types";
import { DEV_URL, requestJson } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

export async function getAssignablePMs(projectId: string, accessToken: string) {
  return await requestJson<BasicUserInfo[]>(
    `/projects/${projectId}/assignable-pms`,
    { method: "GET" },
    accessToken,
  );
}
