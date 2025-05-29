import { DEV_URL, handleResponse } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

export default async function toggleArchiveTicket(
  ticketId: string,
  accessToken: string
) {
  const method = "PUT";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/tickets/${ticketId}/archive`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    })
  );

  return handleResponse(response, error, method);
}
