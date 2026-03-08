import apiClient from "~/services/api.server/apiClient";
import { TokenResponse } from "~/services/api.server/types";
import tryCatch from "~/utils/tryCatch";
import setSession from "../setSession";

export type DemoRole = "admin" | "projectManager" | "developer" | "submitter";

export default async function DemoUserLoginHandler(
  request: Request,
  role: DemoRole
) {
  const { data: res, error } = await tryCatch(apiClient.auth.signInDemoUser(role));

  if (error) {
    return Response.json({ message: "Failed to login as demo user." });
  }

  if (res.ok) {
    const tokenResponse: TokenResponse = await res.json();
    return await setSession(request, tokenResponse, "/dashboard");
  }

  return Response.json({ message: "Failed to login as demo user." });
}
