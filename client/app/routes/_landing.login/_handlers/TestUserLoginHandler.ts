import apiClient from "~/services/api.server/apiClient";
import tryCatch from "~/utils/tryCatch";
import setSession from "../setSession";
import { TokenResponse } from "~/services/api.server/types";

export default async function TestUserLoginHandler(request: Request) {
  const { data: res, error } = await tryCatch(apiClient.auth.signInTestUser());

  if (error) {
    return Response.json({ message: "Failed to login as test user." });
  }

  if (res.ok) {
    const tokenResponse: TokenResponse = await res.json();

    // Get user info and set session
    return await setSession(request, tokenResponse, "/dashboard");
  }

  return Response.json({ message: "Failed to login as test user" });
}
