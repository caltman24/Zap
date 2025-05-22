import { BasicUserInfo, ProjectManagerInfo } from "~/services/api.server/types";
import { DEV_URL, handleResponse, requestJson } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

async function updateTicketPriority(
  ticketId: string,
  priority: string,
  accessToken: string,
): Promise<Response> {
  const method = "PUT";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/tickets/${ticketId}/priority`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ priority: priority }),
    }),
  );

  return handleResponse(response, error, method);
}

export default updateTicketPriority;
