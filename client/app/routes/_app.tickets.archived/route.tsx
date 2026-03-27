import { redirect, type LoaderFunctionArgs } from "@remix-run/node";
import { Link, useLoaderData, useLocation } from "@remix-run/react";
import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import type { BasicTicketInfo } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { JsonResponse, type JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import TicketTable from "~/components/TicketTable";
import { getArchivedTickets } from "./server.get-archived-tickets";

function ArchivedTicketsBreadcrumb() {
  const location = useLocation();

  return <Link to={{ pathname: "/tickets/archived", search: location.search }}>Archived</Link>;
}

export async function loader({ request }: LoaderFunctionArgs) {
  const session = await getSession(request);
  const {
    data: tokenResponse,
    error: tokenError
  } = await tryCatch(apiClient.auth.getValidToken(session));

  if (tokenError) {
    return redirect("/logout");
  }

  try {
    const response = await getArchivedTickets(tokenResponse.token);

    return JsonResponse({
      data: response,
      error: null,
      headers: tokenResponse.headers
    });
  } catch (error: any) {
    if (error instanceof AuthenticationError) {
      return redirect("/logout");
    }

    return JsonResponse({
      data: null,
      error: error.message,
      headers: tokenResponse.headers
    });
  }
}

export const handle = {
  breadcrumb: () => <ArchivedTicketsBreadcrumb />,
  breadcrumbLabel: "Archived",
};

export default function ArchivedTicketsRoute() {
  const { data: tickets, error } = useLoaderData<JsonResponseResult<BasicTicketInfo[]>>();
  const totalTickets = tickets?.length ?? 0;

  if (error) {
    return (
      <RouteLayout>
        <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
          {error}
        </div>
      </RouteLayout>
    );
  }

  return (
    <RouteLayout className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div className="space-y-2">
          <div>
            <h1 className="text-3xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">Archived Tickets</h1>
            <p className="mt-1 max-w-2xl text-sm text-[var(--app-on-surface-variant)] sm:text-base">
              Archived tickets currently visible to you.
            </p>
          </div>
        </div>
        <div className="px-4 py-2">
          <span className="app-shell-mono text-xs uppercase tracking-[0.22em] text-[var(--app-outline)]">{totalTickets} archived</span>
        </div>
      </div>

      <TicketTable tickets={tickets} />
    </RouteLayout>
  );
}
