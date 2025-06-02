import { TicketComment } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getTicketComments(ticketId: string, accessToken: string) {
  return await requestJson<TicketComment[]>(
    `/tickets/${ticketId}/comments`,
    { method: "GET" },
    accessToken
  );
}
