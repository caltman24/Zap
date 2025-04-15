import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useState } from "react";
import { getRolesByRouteName } from "~/data/routes";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { CreateProjectRequest } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";
import RouteLayout from "~/layouts/RouteLayout";

export const handle = {
    breadcrumb: () => <Link to="/projects/new">New</Link>,
};

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const user = session.get("user");
    const allowedRoles = getRolesByRouteName("Create Project");

    if (!user) {
        return redirect("/logout");
    }

    if (!allowedRoles.includes(user.role.toLowerCase())) {
        throw Response.json("Unauthorized", { status: 401, statusText: "Unauthorized" });
    }

    return Response.json({ user });
}

export async function action({ request }: ActionFunctionArgs) {
    const session = await getSession(request);
    const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));

    if (tokenError) {
        return redirect("/logout");
    }

    const formData = await request.formData();
    const name = formData.get("name") as string;
    const description = formData.get("description") as string;
    const priority = formData.get("priority") as string;

    const dueDateValue = formData.get("dueDate") as string;
    // Create a Date object and convert to ISO string
    const dueDate = new Date(dueDateValue).toISOString();

    const projectData: CreateProjectRequest = {
        name,
        description,
        priority,
        dueDate
    };

    const { error } = await tryCatch(apiClient.createProject(projectData, tokenResponse.token));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }

    if (error) {
        return Response.json({ error: "Failed to create project. Please try again later." });
    }

    return redirect("/projects");
}

export default function NewProjectRoute() {
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const actionData = useActionData<typeof action>();
    const [error, setError] = useState<string | null>(null);
    const [priority, setPriority] = useState<string>("");

    useEffect(() => {
        if (actionData?.error) {
            setError(actionData.error);
        }
    }, [actionData]);

    // Helper function to get the appropriate text color class based on priority
    const getPriorityClass = (value: string): string => {
        switch (value.toLowerCase()) {
            case 'low':
                return 'text-info';
            case 'medium':
                return 'text-warning';
            case 'high':
                return 'text-error';
            case 'urgent':
                return 'text-error font-bold';
            default:
                return '';
        }
    };

    return (
        <RouteLayout className="w-full bg-base-300 min-h-full p-6">
            <div className="card w-full bg-base-100 rounded-lg shadow-lg p-6">
                <div className="flex justify-between items-center mb-6">
                    <h1 className="text-3xl font-bold">Create New Project</h1>
                    <Link to="/projects" className="btn btn-outline btn-sm">
                        <span className="material-symbols-outlined">arrow_back</span>
                        Back
                    </Link>
                </div>

                {error && <div className="alert alert-error mb-4">{error}</div>}

                <Form method="post">
                    <fieldset disabled={isSubmitting}>
                        <div className="form-control mb-4">
                            <label className="label">
                                <span className="label-text">Project Name</span>
                            </label>
                            <input
                                type="text"
                                name="name"
                                className="input input-bordered w-full"
                                placeholder="Enter project name"
                                required
                                maxLength={50}
                            />
                        </div>

                        <div className="form-control mb-4">
                            <label className="label">
                                <span className="label-text">Description</span>
                            </label>
                            <textarea
                                name="description"
                                className="textarea textarea-bordered w-full"
                                placeholder="Project description"
                                rows={4}
                                required
                                maxLength={1000}
                            ></textarea>
                        </div>


                        <div className="flex gap-4">
                            <div className="form-control mb-4">
                                <label className="label">
                                    <span className="label-text">Priority</span>
                                </label>
                                <div className="relative w-[10rem]">
                                    <select
                                        name="priority"
                                        className={`select select-bordered w-full ${getPriorityClass(priority)}`}
                                        required
                                        value={priority}
                                        onChange={(e) => setPriority(e.target.value)}
                                    >
                                        <option value="">Select priority</option>
                                        <option value="Low" className="text-info">Low</option>
                                        <option value="Medium" className="text-warning">Medium</option>
                                        <option value="High" className="text-error">High</option>
                                        <option value="Urgent" className="text-error font-bold">Urgent</option>
                                    </select>
                                </div>
                            </div>
                            <div className="form-control mb-6">
                                <label className="label">
                                    <span className="label-text">Due Date</span>
                                </label>
                                <input
                                    type="date"
                                    name="dueDate"
                                    className="input input-bordered w-full"
                                    required
                                    min={new Date().toISOString().split('T')[0]}
                                />
                            </div>
                        </div>
                        <div className="flex justify-end">
                            <button
                                type="submit"
                                className="btn btn-primary"
                                disabled={isSubmitting}
                            >
                                {isSubmitting ? "Creating..." : "Create Project"}
                            </button>
                        </div>
                    </fieldset>
                </Form>
            </div>
        </RouteLayout>
    );
}
