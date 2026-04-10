import { redirect, type LoaderFunctionArgs } from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import type { CompanySearchResult } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { JsonResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";

export async function loader({ request }: LoaderFunctionArgs) {
  const session = await getSession(request);
  const url = new URL(request.url);
  const query = url.searchParams.get("query")?.trim() ?? "";
  const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));

  if (tokenError) {
    return redirect("/logout");
  }

  if (query.length < 2) {
    return JsonResponse({
      data: [] as CompanySearchResult[],
      error: null,
      headers: tokenResponse.headers,
    });
  }

  try {
    const results = await apiClient.getCompanySearch(query, tokenResponse.token);

    return JsonResponse({
      data: results,
      error: null,
      headers: tokenResponse.headers,
    });
  } catch (error: unknown) {
    if (error instanceof AuthenticationError) {
      return redirect("/logout");
    }

    return JsonResponse({
      data: [] as CompanySearchResult[],
      error: error instanceof Error ? error.message : "Failed to search.",
      headers: tokenResponse.headers,
    });
  }
}
