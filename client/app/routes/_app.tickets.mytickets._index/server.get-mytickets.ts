import { BasicTicketInfo } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getMyTickets(accessToken: string) {
  return await requestJson<BasicTicketInfo[]>(
    "/tickets/mytickets",
    { method: "GET" },
    accessToken
  );
}
