import { Form, Link, useLoaderData, useSearchParams, useNavigation, useActionData } from "@remix-run/react";
import RouteLayout from "~/layouts/RouteLayout";
import { ActionFunctionArgs, data, LoaderFunctionArgs, redirect } from "@remix-run/node";
import permissions from "~/data/permissions";
import apiClient from "~/services/api.server/apiClient";
import { getSession } from "~/services/sessions.server";
import { ForbiddenResponse, JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { validateRole } from "~/utils/validate";
import { getNewTicketProjectList } from "./server.get-projects-list";
import { BasicProjectResponse, CreateTicketRequest } from "~/services/api.server/types";
import { createNewTicket } from "./server.create-ticket";
import { AuthenticationError } from "~/services/api.server/errors";
import BackButton from "~/components/BackButton";
import { useState, useEffect, useRef } from "react";

export const handle = {
    breadcrumb: () => <Link to="/tickets/new">New</Link>,
};


export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const userRole = session.get("user").role

    if (!validateRole(userRole, permissions.ticket.create)) {
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
    const projectList = useLoaderData<typeof loader>() as JsonResponseResult<BasicProjectResponse[]>
    const [searchParams, setSearchParams] = useSearchParams();
    const navigation = useNavigation();
    const actionData = useActionData<typeof action>();
    const formRef = useRef<HTMLFormElement>(null);

    const selectedProjectId = searchParams.get("projectId");
    const validProjectId = projectList.data?.some(
        (p) => p.id === selectedProjectId
    )
        ? selectedProjectId
        : "";

    const isSubmitting = navigation.state === "submitting";
    const [nameLength, setNameLength] = useState(0);
    const [descriptionLength, setDescriptionLength] = useState(0);

    // Reset form on successful submission
    useEffect(() => {
        if (actionData?.success) {
            formRef.current?.reset();
            setNameLength(0);
            setDescriptionLength(0);
        }
    }, [actionData]);

    return (
        <RouteLayout>
            {validProjectId && <div className="mb-3"><BackButton /></div>}

            <div className="max-w-4xl mx-auto">
                <div className="mb-6">
                    <h1 className="text-3xl font-bold text-base-content mb-2">Create New Ticket</h1>
                    <p className="text-base-content/70">Fill out the form below to create a new ticket for your project.</p>
                </div>

                <div className="bg-base-100 rounded-lg shadow-lg p-6">
                    {/* Error Display */}
                    {actionData?.error && (
                        <div className="alert alert-error mb-6">
                            <svg className="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                            <span>{actionData.error}</span>
                        </div>
                    )}

                    <Form method="post" ref={formRef} className="space-y-6">
                        <fieldset disabled={isSubmitting} className="space-y-6">
                            {/* Project Selection */}
                            <div className="form-control">
                                <label className="label">
                                    <span className="label-text font-medium">
                                        Project <span className="text-error">*</span>
                                    </span>
                                </label>
                                <select
                                    name="projectId"
                                    className="select select-bordered w-full focus:select-primary"
                                    required
                                    defaultValue={validProjectId ?? ""}
                                    aria-describedby="project-help"
                                >
                                    <option value="" disabled>-- Select a project --</option>
                                    {projectList.data?.map(p => (
                                        <option key={p.id} value={p.id}>{p.name}</option>
                                    ))}
                                </select>
                                <div className="label">
                                    <span id="project-help" className="label-text-alt text-base-content/60">
                                        Choose the project this ticket belongs to
                                    </span>
                                </div>
                            </div>

                            {/* Ticket Name */}
                            <div className="form-control">
                                <label className="label">
                                    <span className="label-text font-medium">
                                        Ticket Name <span className="text-error">*</span>
                                    </span>
                                    <span className="label-text-alt text-base-content/60">
                                        {nameLength}/50
                                    </span>
                                </label>
                                <input
                                    type="text"
                                    name="name"
                                    className="input input-bordered w-full focus:input-primary"
                                    placeholder="Enter a descriptive ticket name"
                                    required
                                    maxLength={50}
                                    onChange={(e) => setNameLength(e.target.value.length)}
                                    aria-describedby="name-help"
                                />
                                <div className="label">
                                    <span id="name-help" className="label-text-alt text-base-content/60">
                                        A clear, concise title that describes the issue or request
                                    </span>
                                </div>
                            </div>

                            {/* Description */}
                            <div className="form-control">
                                <label className="label">
                                    <span className="label-text font-medium">
                                        Description <span className="text-error">*</span>
                                    </span>
                                    <span className="label-text-alt text-base-content/60">
                                        {descriptionLength}/1000
                                    </span>
                                </label>
                                <textarea
                                    name="description"
                                    className="textarea textarea-bordered w-full h-32 focus:textarea-primary"
                                    placeholder="Provide a detailed description of the ticket..."
                                    required
                                    maxLength={1000}
                                    onChange={(e) => setDescriptionLength(e.target.value.length)}
                                    aria-describedby="description-help"
                                />
                                <div className="label">
                                    <span id="description-help" className="label-text-alt text-base-content/60">
                                        Include steps to reproduce, expected behavior, and any relevant details
                                    </span>
                                </div>
                            </div>

                            {/* Priority, Status, and Type Row */}
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                {/* Priority */}
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text font-medium">
                                            Priority <span className="text-error">*</span>
                                        </span>
                                    </label>
                                    <select
                                        name="priority"
                                        className="select select-bordered w-full focus:select-primary"
                                        required
                                        defaultValue=""
                                        aria-describedby="priority-help"
                                    >
                                        <option value="" disabled>-- Select priority --</option>
                                        <option value="Low">🟢 Low</option>
                                        <option value="Medium">🟡 Medium</option>
                                        <option value="High">🟠 High</option>
                                        <option value="Urgent">🔴 Urgent</option>
                                    </select>
                                    <div className="label">
                                        <span id="priority-help" className="label-text-alt text-base-content/60">
                                            How urgent is this ticket?
                                        </span>
                                    </div>
                                </div>

                                {/* Status */}
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text font-medium">
                                            Status <span className="text-error">*</span>
                                        </span>
                                    </label>
                                    <select
                                        name="status"
                                        className="select select-bordered w-full focus:select-primary"
                                        required
                                        defaultValue="New"
                                        aria-describedby="status-help"
                                    >
                                        <option value="New">🆕 New</option>
                                        <option value="In Development">⚙️ In Development</option>
                                        <option value="Testing">🧪 Testing</option>
                                        <option value="Resolved">✅ Resolved</option>
                                    </select>
                                    <div className="label">
                                        <span id="status-help" className="label-text-alt text-base-content/60">
                                            Current state of the ticket
                                        </span>
                                    </div>
                                </div>

                                {/* Type */}
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text font-medium">
                                            Type <span className="text-error">*</span>
                                        </span>
                                    </label>
                                    <select
                                        name="type"
                                        className="select select-bordered w-full focus:select-primary"
                                        required
                                        defaultValue=""
                                        aria-describedby="type-help"
                                    >
                                        <option value="" disabled>-- Select type --</option>
                                        <option value="Defect">🐛 Defect</option>
                                        <option value="Feature">✨ Feature</option>
                                        <option value="General Task">📋 General Task</option>
                                        <option value="Work Task">💼 Work Task</option>
                                        <option value="Change Request">🔄 Change Request</option>
                                        <option value="Enhancement">⚡ Enhancement</option>
                                    </select>
                                    <div className="label">
                                        <span id="type-help" className="label-text-alt text-base-content/60">
                                            What kind of ticket is this?
                                        </span>
                                    </div>
                                </div>
                            </div>

                            {/* Submit Button */}
                            <div className="flex justify-end pt-4 border-t border-base-300">
                                <button
                                    type="submit"
                                    className="btn btn-primary btn-lg min-w-32"
                                    disabled={isSubmitting}
                                >
                                    {isSubmitting ? (
                                        <>
                                            <span className="loading loading-spinner loading-sm"></span>
                                            Creating...
                                        </>
                                    ) : (
                                        <>
                                            <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                                            </svg>
                                            Create Ticket
                                        </>
                                    )}
                                </button>
                            </div>
                        </fieldset>
                    </Form>
                </div>
            </div>
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
