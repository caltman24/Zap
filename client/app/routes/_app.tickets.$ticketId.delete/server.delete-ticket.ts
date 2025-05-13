import { DEV_URL, handleResponse } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

export default async function removeTicket(
  ticketId: string,
  accessToken: string
) {
  const method = "DELETE";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/tickets/${ticketId}`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    })
  );

  return handleResponse(response, error, method);
}
