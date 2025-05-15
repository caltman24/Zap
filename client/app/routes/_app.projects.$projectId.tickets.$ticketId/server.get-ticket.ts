import {
  BasicProjectResponse,
  BasicTicketInfo,
} from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getTicketById(ticketId: string, accessToken: string) {
  return await requestJson<BasicTicketInfo>(
    `/tickets/${ticketId}`,
    { method: "GET" },
    accessToken,
  );
}
