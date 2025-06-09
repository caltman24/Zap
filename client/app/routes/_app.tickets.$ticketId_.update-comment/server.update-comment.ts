import { DEV_URL, handleResponse } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

export default async function updateComment(
  ticketId: string,
  commentId: string,
  message: string,
  accessToken: string
) {
  const method = "PUT";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/tickets/${ticketId}/comments/${commentId}`, {
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
