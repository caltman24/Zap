import apiService, { TokenResponse } from "~/services/ApiService";
import tryCatch from "~/utils/tryCatch";
import setSession from "../setSession";

export default async function PwdLoginHandler(
  request: Request,
  formData: FormData
) {
  const email = formData.get("email") as string;
  const password = formData.get("password") as string;

  const { data: res, error } = await tryCatch(
    apiService.SignInUser(email, password)
  );

  if (error) {
    return Response.json({ message: "Server Error: Please try again later." });
  }

  if (res.status === 400) {
    return Response.json({ message: await res.json() });
  }

  if (res.ok) {
    const tokenResonse: TokenResponse = await res.json();

    return await setSession(request, tokenResonse, "/dashboard");
  }
}
