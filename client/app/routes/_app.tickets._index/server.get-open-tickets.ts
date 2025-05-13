import { BasicTicketInfo } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getOpenTickets(accessToken: string) {
  return await requestJson<BasicTicketInfo[]>(
    "/tickets/open",
    { method: "GET" },
    accessToken
  );
}
