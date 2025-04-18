import apiClient from "~/services/api.server/apiClient";
import tryCatch from "~/utils/tryCatch";
import setSession from "../setSession";
import { TokenResponse } from "~/services/api.server/types";

export default async function PwdLoginHandler(
  request: Request,
  formData: FormData
) {
  const email = formData.get("email") as string;
  const password = formData.get("password") as string;

  const { data: res, error } = await tryCatch(
    apiClient.auth.signInUser(email, password)
  );

  if (error) {
    return Response.json({
      message: "Failed to login. Please try again later.",
    });
  }

  if (res.status === 400) {
    return Response.json({ message: await res.json() });
  }

  if (res.ok) {
    const tokenResonse: TokenResponse = await res.json();

    return await setSession(request, tokenResonse, "/dashboard");
  }

  return Response.json({ message: "Failed to login. Please try again later." });
}
