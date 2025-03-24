import { redirect } from "@remix-run/node";
import apiService, {
  AuthenticationError,
  TokenRefreshError,
  TokenResponse,
} from "~/services/api.server/apiClient";
import {
  getSession,
  destroySession,
  commitSession,
} from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";

// After login, sets the session cookie
export default async function setSession(
  request: Request,
  tokens: TokenResponse,
  redirectUrl: string
) {
  const session = await getSession(request);
  // we pass in the raw token here because we don't have the session yet
  const { data, error } = await tryCatch(
    apiService.getUserInfo(tokens.accessToken)
  );

  if (error instanceof AuthenticationError) {
    redirect("/login", {
      headers: {
        "Set-Cookie": await destroySession(session),
      },
    });
  }

  if (error) {
    return Response.json({ message: error.message });
  }

  session.set("tokens", {
    accessToken: tokens.accessToken,
    expiresIn: apiService.expiresInToIso(tokens.expiresIn),
    refreshToken: tokens.refreshToken,
  });

  session.set("user", data);

  return redirect(redirectUrl, {
    headers: {
      "Set-Cookie": await commitSession(session),
    },
  });
}
