import { ActionFunctionArgs, redirect } from "@remix-run/node";
import permissions from "~/data/permissions";
import apiClient from "~/services/api.server/apiClient";
import { AuthenticationError } from "~/services/api.server/errors";
import { getSession } from "~/services/sessions.server";
import { ActionResponse, ForbiddenResponse } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { validateRole } from "~/utils/validate";

export default async function action({ request, params }: ActionFunctionArgs) {
  const session = await getSession(request);
  const userRole = session.get("user").role;

  if (!validateRole(userRole, permissions.project.edit)) {
    return ForbiddenResponse();
  }
  const { data: tokenResponse, error: tokenError } = await tryCatch(
    apiClient.auth.getValidToken(session),
  );

  if (tokenError) {
    return redirect("/logout");
  }

  const projectId = params.projectId!;
  const formData = await request.formData();
  const name = formData.get("name") as string;
  const description = formData.get("description") as string;
  const priority = formData.get("priority") as string;
  const dueDateValue = formData.get("dueDate") as string;
  const dueDate = new Date(dueDateValue).toISOString();

  const projectData = {
    name,
    description,
    priority,
    dueDate,
  };

  const { error } = await tryCatch(
    apiClient.updateProject(projectId, projectData, tokenResponse.token),
  );

  if (error instanceof AuthenticationError) {
    return redirect("/logout");
  }

  if (error) {
    return ActionResponse({
      success: false,
      error: error.message,
    });
  }

  return ActionResponse({
    success: true,
    error: null,
  });
}
