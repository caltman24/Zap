import { BasicTicketInfo } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export async function getArchivedTickets(accessToken: string) {
  return await requestJson<BasicTicketInfo[]>(
    "/tickets/archived",
    { method: "GET" },
    accessToken
  );
}
