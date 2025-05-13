import { BasicProjectResponse } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getNewTicketProjectList(accessToken: string) {
  return await requestJson<BasicProjectResponse[]>(
    `/tickets/project-list`,
    { method: "GET" },
    accessToken
  );
}
