import { Form, Link, useActionData, useLoaderData, useNavigation, useSearchParams } from "@remix-run/react";
import RouteLayout from "~/layouts/RouteLayout";
import { type ActionFunctionArgs, type LoaderFunctionArgs, redirect } from "@remix-run/node";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { ForbiddenResponse, JsonResponse, type JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { getNewTicketProjectList } from "./server.get-projects-list";
import type { BasicProjectResponse, CreateTicketRequest, UserInfoResponse } from "~/services/api.server/types";
import { createNewTicket } from "./server.create-ticket";
import { AuthenticationError } from "~/services/api.server/errors";
import BackButton from "~/components/BackButton";
import { useEffect, useRef, useState } from "react";
import { hasPermission } from "~/utils/permissions";
import FormShell, {
  FormFieldHeader,
  formInputClassName,
  FormSelectControl,
  formTextareaClassName,
} from "~/components/FormShell";

export const handle = {
    breadcrumb: () => <Link to="/tickets/new">New</Link>,
    breadcrumbLabel: "New",
};


export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const user = session.get("user") as UserInfoResponse;

    if (!hasPermission(user.permissions, "ticket.create")) {
        return ForbiddenResponse()
    }

    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    // Return promise to show skeleton
    try {
        const projects = await getNewTicketProjectList(tokenResponse.token);

        return JsonResponse({
            data: projects,
            error: null,
            headers: tokenResponse.headers
        })
    } catch (e: any) {
        return JsonResponse({
            data: null,
            error: e,
            headers: tokenResponse.headers
        })
    }
}

export default function NewTicketRoute() {
  const projectList = useLoaderData<typeof loader>() as JsonResponseResult<BasicProjectResponse[]>;
  const [searchParams] = useSearchParams();
  const navigation = useNavigation();
  const actionData = useActionData<typeof action>();
  const formRef = useRef<HTMLFormElement>(null);

  const selectedProjectId = searchParams.get("projectId");
  const validProjectId = projectList.data?.some((p) => p.id === selectedProjectId) ? selectedProjectId : "";

  const isSubmitting = navigation.state === "submitting";
  const [nameLength, setNameLength] = useState(0);
  const [descriptionLength, setDescriptionLength] = useState(0);

  useEffect(() => {
    if (actionData?.success) {
      formRef.current?.reset();
      setNameLength(0);
      setDescriptionLength(0);
    }
  }, [actionData]);

  return (
    <RouteLayout>
      <FormShell
        description="Fill out the form below to create a new ticket for your project."
        error={actionData?.error}
        leading={validProjectId ? <BackButton /> : null}
        title="Create New Ticket"
      >
        <Form className="space-y-8" method="post" ref={formRef}>
          <fieldset className="space-y-8" disabled={isSubmitting}>
            <div className="grid gap-6">
              <div>
                <FormFieldHeader label="Project" required />
                <FormSelectControl
                  defaultValue={validProjectId ?? ""}
                  name="projectId"
                  required
                >
                  <option disabled value="">
                    Select a project
                  </option>
                  {projectList.data?.map((project) => (
                    <option key={project.id} value={project.id}>
                      {project.name}
                    </option>
                  ))}
                </FormSelectControl>
              </div>

              <div>
                <FormFieldHeader detail={`${nameLength}/50`} label="Ticket Name" required />
                <input
                  className={formInputClassName}
                  maxLength={50}
                  name="name"
                  onChange={(e) => setNameLength(e.target.value.length)}
                  placeholder="Enter a descriptive ticket name"
                  required
                  type="text"
                />
              </div>

              <div>
                <FormFieldHeader detail={`${descriptionLength}/1000`} label="Description" required />
                <textarea
                  className={formTextareaClassName}
                  maxLength={1000}
                  name="description"
                  onChange={(e) => setDescriptionLength(e.target.value.length)}
                  placeholder="Provide a detailed description of the ticket..."
                  required
                />
              </div>

              <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
                <div>
                  <FormFieldHeader label="Priority" required />
                  <FormSelectControl defaultValue="" name="priority" required>
                    <option disabled value="">
                      Select priority
                    </option>
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Urgent">Urgent</option>
                  </FormSelectControl>
                </div>

                <div>
                  <FormFieldHeader label="Status" required />
                  <FormSelectControl defaultValue="New" name="status" required>
                    <option value="New">New</option>
                    <option value="In Development">In Development</option>
                    <option value="Testing">Testing</option>
                    <option value="Resolved">Resolved</option>
                  </FormSelectControl>
                </div>

                <div>
                  <FormFieldHeader label="Type" required />
                  <FormSelectControl defaultValue="" name="type" required>
                    <option disabled value="">
                      Select type
                    </option>
                    <option value="Defect">Defect</option>
                    <option value="Feature">Feature</option>
                    <option value="General Task">General Task</option>
                    <option value="Work Task">Work Task</option>
                    <option value="Change Request">Change Request</option>
                    <option value="Enhancement">Enhancement</option>
                  </FormSelectControl>
                </div>
              </div>
            </div>

            <div className="flex justify-end border-t border-[var(--app-outline-variant)]/10 pt-5">
              <button
                className="inline-flex min-w-36 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-5 py-3 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
                disabled={isSubmitting}
                type="submit"
              >
                {isSubmitting ? (
                  <>
                    <span className="loading loading-spinner loading-sm" />
                    Creating...
                  </>
                ) : (
                  <>
                    <span className="material-symbols-outlined text-lg">add</span>
                    Create Ticket
                  </>
                )}
              </button>
            </div>
          </fieldset>
        </Form>
      </FormShell>
    </RouteLayout>
  );
}

export async function action({ request }: ActionFunctionArgs) {
    const session = await getSession(request);
    const {
        data: tokenResponse,
        error: tokenError
    } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const formData = await request.formData();
    const name = formData.get("name") as string;
    const description = formData.get("description") as string;
    const priority = formData.get("priority") as string;
    const status = formData.get("status") as string;
    const type = formData.get("type") as string;
    const projectId = formData.get("projectId") as string;

    // Basic validation
    if (!name?.trim()) {
        return Response.json({
            error: "Ticket name is required."
        }, { status: 400 });
    }

    if (!description?.trim()) {
        return Response.json({
            error: "Description is required."
        }, { status: 400 });
    }

    if (!projectId) {
        return Response.json({
            error: "Please select a project."
        }, { status: 400 });
    }

    const ticketData: CreateTicketRequest = {
        name: name.trim(),
        description: description.trim(),
        priority,
        status,
        type,
        projectId
    };

    const { data, error } = await tryCatch(
        createNewTicket(tokenResponse.token, ticketData)
    );

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }

    if (error) {
        return Response.json({
            error: "Failed to create ticket. Please try again later."
        }, { status: 500 });
    }

    return redirect(`/projects/${projectId}/tickets/${data.id}`);
}
