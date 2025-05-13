import {
  CreateTicketRequest,
  CreateTicketResult,
} from "~/services/api.server/types";
import { DEV_URL, requestJson } from "~/utils/api";

export async function createNewTicket(
  accessToken: string,
  data: CreateTicketRequest
) {
  return await requestJson<CreateTicketResult>(
    `/tickets`,
    { method: "POST", body: JSON.stringify(data) },
    accessToken
  );
}
