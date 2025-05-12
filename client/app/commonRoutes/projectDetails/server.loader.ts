import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { JsonResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

export default async function loader({ request, params }: LoaderFunctionArgs) {
  const projectId = params.projectId!;

  const session = await getSession(request);
  const { data: tokenResponse, error: tokenError } = await tryCatch(
    apiClient.auth.getValidToken(session),
  );

  if (tokenError) {
    return redirect("/logout");
  }

  const { data: project, error } = await tryCatch(
    apiClient.getProjectById(projectId, tokenResponse.token),
  );

  if (error) {
    return JsonResponse({
      data: null,
      error: error.message,
      headers: tokenResponse.headers,
    });
  }

  return JsonResponse({
    data: project,
    error: null,
    headers: tokenResponse.headers,
  });
}
