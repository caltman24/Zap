import { DEV_URL, handleResponse } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

export default async function createComment(
  ticketId: string,
  accessToken: string,
  message: string
) {
  const method = "POST";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/tickets/${ticketId}/comments`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "Application/json",
      },
      body: JSON.stringify({ message }),
    })
  );

  return handleResponse(response, error, method);
}
