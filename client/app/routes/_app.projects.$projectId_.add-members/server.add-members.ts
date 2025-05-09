import { BasicUserInfo, ProjectManagerInfo } from "~/services/api.server/types";
import { DEV_URL, handleResponse, requestJson } from "~/utils/api";
import tryCatch from "~/utils/tryCatch";

async function assignProjectMembers(
  projectId: string,
  memberIds: string[],
  accessToken: string
): Promise<Response> {
  const method = "POST";
  const { data: response, error } = await tryCatch(
    fetch(`${DEV_URL}/projects/${projectId}/members`, {
      method,
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ memberIds: [...memberIds] }),
    })
  );

  return handleResponse(response, error, method);
}

export default assignProjectMembers;
