import { Form, Link, useLoaderData, useSearchParams } from "@remix-run/react";
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
    const selectedProjectId = searchParams.get("projectId");
    const validProjectId = projectList.data?.some(
        (p) => p.id === selectedProjectId
    )
        ? selectedProjectId
        : "";
    return (
        <RouteLayout className="w-full bg-base-300 min-h-full p-6">
            <div className="card w-full bg-base-100 rounded-lg shadow-lg p-6">
                <div className="flex justify-between items-center mb-6">
                    <h1 className="text-3xl font-bold">Create New Ticket</h1>
                    <BackButton />
                </div>

                <Form method="post">
                    <fieldset disabled={false} className="fieldset">
                        <div className="mb-4">
                            <label className="label mb-1">
                                Project
                            </label>
                            <div className="relative w-full">
                                <select
                                    name="projectId"
                                    className={`select select-bordered w-full font-medium`}
                                    required
                                    defaultValue={validProjectId ?? ""}
                                >
                                    <option value="" disabled >-- select project --</option>
                                    {projectList.data?.map(p => (
                                        <option key={p.id} value={p.id}>{p.name}</option>
                                    ))}
                                </select>
                            </div>
                        </div>
                        <div className=" mb-4">
                            <label className="label mb-1">
                                Ticket Name
                            </label>
                            <input
                                type="text"
                                name="name"
                                className="input input-bordered w-full"
                                placeholder="Enter ticket name"
                                required
                                maxLength={50}
                            />
                        </div>

                        <div className=" mb-4">
                            <label className="label mb-1">
                                Description
                            </label>
                            <textarea
                                name="description"
                                className="textarea textarea-bordered w-full"
                                placeholder="Ticket description"
                                rows={4}
                                required
                                maxLength={1000}
                            ></textarea>
                        </div>

                        <div className="flex gap-4">
                            <div className=" mb-4">
                                <label className="label mb-1">
                                    Priority
                                </label>
                                <div className="relative w-[10rem]">
                                    <select
                                        name="priority"
                                        className={`select select-bordered w-full`}
                                        required
                                        defaultValue=""
                                    >
                                        <option value="" disabled >-- select priority --</option>
                                        <option value="Low" className="text-info">Low</option>
                                        <option value="Medium" className="text-warning">Medium</option>
                                        <option value="High" className="text-error">High</option>
                                        <option value="Urgent" className="text-error font-bold">Urgent</option>
                                    </select>
                                </div>
                            </div>
                            <div className=" mb-4">
                                <label className="label mb-1">
                                    Status
                                </label>
                                <div className="relative w-[13rem]">
                                    <select
                                        name="status"
                                        className={`select select-bordered w-full`}
                                        required
                                        defaultValue=""
                                    >
                                        <option value="" disabled >-- select status --</option>
                                        <option value="New" className="text-cyan-500">New</option>
                                        <option value="In Development" className="text-blue-500">In Development</option>
                                        <option value="Testing" className="text-warning">Testing</option>
                                        <option value="Resolved" className="text-green-500 ">Resolved</option>
                                    </select>
                                </div>
                            </div>
                            <div className=" mb-4">
                                <label className="label mb-1">
                                    Type
                                </label>
                                <div className="relative w-[13rem]">
                                    <select
                                        name="type"
                                        className={`select select-bordered w-full`}
                                        required
                                        defaultValue=""
                                    >
                                        <option value="" disabled >-- select type --</option>
                                        <option value="Defect" className="text-error">Defect</option>
                                        <option value="Feature" className="text-green-500">Feature</option>
                                        <option value="General Task" className="text-cyan-500">General Task</option>
                                        <option value="Work Task" className="text-indigo-500 ">Work Task</option>
                                        <option value="Change Request" className="text-amber-500 ">Change Request</option>
                                        <option value="Enhancement" className="text-blue-500 ">Enhancment</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div className="flex justify-end">
                            <button
                                type="submit"
                                className="btn btn-primary"
                            >Create Ticket</button>
                        </div>
                    </fieldset>
                </Form>
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


    const projectData: CreateTicketRequest = {
        name,
        description,
        priority,
        status,
        type,
        projectId
    };


    const { data, error } = await tryCatch(
        createNewTicket(
            tokenResponse.token,
            projectData));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }

    if (error) {
        return Response.json({
            error: "Failed to create project. Please try again later."
        });
    }

    return redirect(`/projects/${projectId}/tickets/${data.id}`);
}
