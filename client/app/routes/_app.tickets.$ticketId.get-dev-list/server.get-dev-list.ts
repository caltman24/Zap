import { BasicUserInfo, ProjectManagerInfo } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getProjectDevList(ticketId: string, accessToken: string) {
  return await requestJson<BasicUserInfo[]>(
    `/tickets/${ticketId}/developer-list`,
    {
      method: "GET",
    },
    accessToken,
  );
}
