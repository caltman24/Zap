import { DEV_URL, handleResponse } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

export default async function removeMember(
  projectId: string,
  memberId: string,
  accessToken: string,
) {
  const method = "DELETE";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/projects/${projectId}/members/${memberId}`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    }),
  );

  return handleResponse(response, error, method);
}
