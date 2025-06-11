import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useState, useRef } from "react";
import { getRolesByRouteName } from "~/data/routes";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { CreateProjectRequest } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";
import RouteLayout from "~/layouts/RouteLayout";
import BackButton from "~/components/BackButton";

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
    const dueDateValue = formData.get("dueDate") as string;

    // Basic validation
    if (!name?.trim()) {
        return Response.json({
            error: "Project name is required."
        }, { status: 400 });
    }

    if (!description?.trim()) {
        return Response.json({
            error: "Description is required."
        }, { status: 400 });
    }

    if (!priority) {
        return Response.json({
            error: "Please select a priority."
        }, { status: 400 });
    }

    if (!dueDateValue) {
        return Response.json({
            error: "Due date is required."
        }, { status: 400 });
    }

    // Create a Date object and convert to ISO string
    const dueDate = new Date(dueDateValue).toISOString();

    const projectData: CreateProjectRequest = {
        name: name.trim(),
        description: description.trim(),
        priority,
        dueDate
    };

    const { error } = await tryCatch(
        apiClient.createProject(
            projectData,
            tokenResponse.token));

    if (error instanceof AuthenticationError) {
        return redirect("/logout");
    }

    if (error) {
        return Response.json({
            error: "Failed to create project. Please try again later."
        }, { status: 500 });
    }

    return redirect("/projects");
}

export default function NewProjectRoute() {
    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";
    const actionData = useActionData<typeof action>();
    const formRef = useRef<HTMLFormElement>(null);

    const [priority, setPriority] = useState<string>("");
    const [nameLength, setNameLength] = useState(0);
    const [descriptionLength, setDescriptionLength] = useState(0);

    // Reset form on successful submission
    useEffect(() => {
        if (actionData?.success) {
            formRef.current?.reset();
            setPriority("");
            setNameLength(0);
            setDescriptionLength(0);
        }
    }, [actionData]);

    return (
        <RouteLayout>
            <div className="max-w-4xl mx-auto">
                <div className="mb-6">
                    <h1 className="text-3xl font-bold text-base-content mb-2">Create New Project</h1>
                    <p className="text-base-content/70">Fill out the form below to create a new project for your organization.</p>
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
                            {/* Project Name */}
                            <div className="form-control">
                                <label className="label">
                                    <span className="label-text font-medium">
                                        Project Name <span className="text-error">*</span>
                                    </span>
                                    <span className="label-text-alt text-base-content/60">
                                        {nameLength}/50
                                    </span>
                                </label>
                                <input
                                    type="text"
                                    name="name"
                                    className="input input-bordered w-full focus:input-primary"
                                    placeholder="Enter a descriptive project name"
                                    required
                                    maxLength={50}
                                    onChange={(e) => setNameLength(e.target.value.length)}
                                    aria-describedby="name-help"
                                />
                                <div className="label">
                                    <span id="name-help" className="label-text-alt text-base-content/60">
                                        A clear, concise name that describes the project
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
                                    placeholder="Provide a detailed description of the project..."
                                    required
                                    maxLength={1000}
                                    onChange={(e) => setDescriptionLength(e.target.value.length)}
                                    aria-describedby="description-help"
                                />
                                <div className="label">
                                    <span id="description-help" className="label-text-alt text-base-content/60">
                                        Include project goals, scope, and any relevant details
                                    </span>
                                </div>
                            </div>

                            {/* Priority and Due Date Row */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                                        value={priority}
                                        onChange={(e) => setPriority(e.target.value)}
                                        aria-describedby="priority-help"
                                    >
                                        <option value="">-- Select priority --</option>
                                        <option value="Low">🟢 Low</option>
                                        <option value="Medium">🟡 Medium</option>
                                        <option value="High">🟠 High</option>
                                        <option value="Urgent">🔴 Urgent</option>
                                    </select>
                                    <div className="label">
                                        <span id="priority-help" className="label-text-alt text-base-content/60">
                                            How urgent is this project?
                                        </span>
                                    </div>
                                </div>

                                {/* Due Date */}
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text font-medium">
                                            Due Date <span className="text-error">*</span>
                                        </span>
                                    </label>
                                    <input
                                        type="date"
                                        name="dueDate"
                                        className="input input-bordered w-full focus:input-primary"
                                        required
                                        min={new Date().toISOString().split('T')[0]}
                                        aria-describedby="date-help"
                                    />
                                    <div className="label">
                                        <span id="date-help" className="label-text-alt text-base-content/60">
                                            Target completion date for the project
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
                                            Create Project
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
