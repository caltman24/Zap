import { BasicUserInfo, ProjectManagerInfo } from "~/services/api.server/types";
import { DEV_URL, handleResponse, requestJson } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

async function updateTicketDev(
  ticketId: string,
  memberId: string,
  accessToken: string,
): Promise<Response> {
  const method = "PUT";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/tickets/${ticketId}/developer`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ memberId: memberId }),
    }),
  );

  return handleResponse(response, error, method);
}

export default updateTicketDev;
