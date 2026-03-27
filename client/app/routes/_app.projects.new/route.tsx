import { type ActionFunctionArgs, type LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useNavigation } from "@remix-run/react";
import { useEffect, useRef, useState } from "react";
import FormShell, {
  FormFieldHeader,
  formInputClassName,
  FormSelectControl,
  formTextareaClassName,
} from "~/components/FormShell";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import type { CreateProjectRequest, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import { hasPermission } from "~/utils/permissions";
import tryCatch from "~/utils/tryCatch";
import RouteLayout from "~/layouts/RouteLayout";

export const handle = {
    breadcrumb: () => <Link to="/projects/new">New</Link>,
    breadcrumbLabel: "New",
};

export async function loader({ request }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const user = session.get("user") as UserInfoResponse;

    if (!user) {
        return redirect("/logout");
    }

    if (!hasPermission(user.permissions, "project.create")) {
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
      <FormShell
        description="Fill out the form below to create a new project for your organization."
        error={actionData?.error}
        title="Create New Project"
      >
        <Form className="space-y-8" method="post" ref={formRef}>
          <fieldset className="space-y-8" disabled={isSubmitting}>
            <div className="grid gap-6">
              <div>
                <FormFieldHeader detail={`${nameLength}/50`} label="Project Name" required />
                <input
                  className={formInputClassName}
                  maxLength={50}
                  name="name"
                  onChange={(e) => setNameLength(e.target.value.length)}
                  placeholder="Enter a descriptive project name"
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
                  placeholder="Provide a detailed description of the project..."
                  required
                />
              </div>

              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <div>
                  <FormFieldHeader label="Priority" required />
                  <FormSelectControl
                    name="priority"
                    onChange={(e) => setPriority(e.target.value)}
                    required
                    value={priority}
                  >
                    <option value="">Select priority</option>
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Urgent">Urgent</option>
                  </FormSelectControl>
                </div>

                <div>
                  <FormFieldHeader label="Due Date" required />
                  <input
                    className={formInputClassName}
                    min={new Date().toISOString().split("T")[0]}
                    name="dueDate"
                    required
                    type="date"
                  />
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
                    Create Project
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
