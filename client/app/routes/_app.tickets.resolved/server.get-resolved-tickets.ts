import { BasicTicketInfo } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getResolvedTickets(accessToken: string) {
  return await requestJson<BasicTicketInfo[]>(
    "/tickets/resolved",
    { method: "GET" },
    accessToken
  );
}
