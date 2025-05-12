import { CompanyProjectsResponse } from "~/services/api.server/types";
import { requestJson } from "~/utils/api";

export default async function getMyProjects(
  memberId: string,
  accessToken: string
): Promise<CompanyProjectsResponse> {
  return requestJson<CompanyProjectsResponse>(
    `/members/${memberId}/myprojects`,
    { method: "GET" },
    accessToken
  );
}
