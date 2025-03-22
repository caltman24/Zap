import { redirect } from "@remix-run/node";
import apiService, {
  TokenRefreshError,
  TokenResponse,
} from "~/services/ApiService";
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
  const { data, error } = await tryCatch(apiService.GetUserInfo(tokens));

  if (error instanceof TokenRefreshError) {
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
    refreshToken: tokens.refreshToken,
  });

  session.set("user", data);

  return redirect(redirectUrl, {
    headers: {
      "Set-Cookie": await commitSession(session),
    },
  });
}
