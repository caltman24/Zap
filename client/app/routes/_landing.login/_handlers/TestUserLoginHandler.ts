import apiService, { TokenResponse } from "~/services/apiClient.server";
import tryCatch from "~/utils/tryCatch";
import setSession from "../setSession";

export default async function TestUserLoginHandler(request: Request) {
  const { data: res, error } = await tryCatch(apiService.signInTestUser());

  if (error) {
    return Response.json({ message: "Server Error: Please try again later." });
  }

  if (res.ok) {
    const tokenResponse: TokenResponse = await res.json();

    // Get user info and set session
    return await setSession(request, tokenResponse, "/dashboard");
  }

  return Response.json({ message: "Failed to login as test user" });
}
