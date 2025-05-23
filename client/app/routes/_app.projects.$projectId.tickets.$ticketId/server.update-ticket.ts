import { BasicUserInfo, ProjectManagerInfo } from "~/services/api.server/types";
import { DEV_URL, handleResponse, requestJson } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

async function updateTicket(
  ticketId: string,
  ticket: {
    name: string;
    description: string;
    priority: string;
    status: string;
    type: string;
  },
  accessToken: string
): Promise<Response> {
  const method = "PUT";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/tickets/${ticketId}`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(ticket),
    })
  );

  return handleResponse(response, error, method);
}

export default updateTicket;
