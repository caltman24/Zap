import { LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Link, useLoaderData, useOutletContext, useParams } from "@remix-run/react";
import { useMemo } from "react";
import apiClient from "~/services/api.server/apiClient";
import { ProjectResponse, UserInfoResponse } from "~/services/api.server/types";
import { getSession } from "~/services/sessions.server";
import tryCatch from "~/utils/tryCatch";

export const handle = {
  breadcrumb: (match: any) => {
    const projectId = match.params.projectId;
    const projectName = match.data?.project?.name || "Project Details";
    return <Link to={`/projects/${projectId}`}>{projectName}</Link>;
  },
};

export async function loader({ request, params }: LoaderFunctionArgs) {
  const session = await getSession(request);
  const user = session.get("user");

  if (!user) {
    return redirect("/login");
  }

  const projectId = params.projectId!;

  const { data: tokenResponse, error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));
  if (tokenError) {
    return redirect("/logout");
  }
  const { data: project, error } = await tryCatch(apiClient.getProjectById(projectId, tokenResponse.token));

  if (error) {
    return Response.json({ error: "Failed to get project details. Please try again later." });
  }

  return Response.json(project);
}

export default function ProjectDetailsRoute() {
  const project = useLoaderData<typeof loader>() as ProjectResponse;
  const userInfo = useOutletContext<UserInfoResponse>();
  const isAdmin = useMemo(() => userInfo?.role?.toLowerCase() === "admin", [userInfo]);
  const isProjectManager = useMemo(() => userInfo?.role?.toLowerCase() === "projectmanager", [userInfo]);
  const canEdit = isAdmin || isProjectManager;

  // Placeholder data for tickets
  const tickets = [
    { id: "1", title: "Fix login bug", status: "Open", priority: "High", assignee: "John Doe" },
    { id: "2", title: "Update dashboard UI", status: "In Progress", priority: "Medium", assignee: "Jane Smith" },
    { id: "3", title: "Optimize database queries", status: "Open", priority: "Low", assignee: "Unassigned" },
  ];



  return (
    <div className="w-full bg-base-300 min-h-full p-6">
      {/* Project Header */}
      <div className="bg-base-100 rounded-lg shadow-lg p-6 mb-6">
        <div className="flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold">{project.name}</h1>
            <p className="text-base-content/70 mt-2">{project.description}</p>
          </div>
          <div className="flex gap-2">
            {canEdit && (
              <Link to={`/projects/${project.id}/edit`} className="btn btn-soft">
                <span className="material-symbols-outlined">edit</span>
                Edit
              </Link>
            )}
            <Link to="/projects" className="btn btn-outline">
              <span className="material-symbols-outlined">arrow_back</span>
              Back
            </Link>
          </div>
        </div>

        {/* Project Details */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-6">
          <div className="stat bg-base-200 rounded-lg">
            <div className="stat-title">Priority</div>
            <div className={`stat-value text-lg ${getPriorityClass(project.priority)}`}>
              {project.priority}
            </div>
          </div>
          <div className="stat bg-base-200 rounded-lg">
            <div className="stat-title">Due Date</div>
            <div className="stat-value text-lg">
              {new Date(project.dueDate).toLocaleDateString()}
            </div>
          </div>
          <div className="stat bg-base-200 rounded-lg">
            <div className="stat-title">Team Size</div>
            <div className="stat-value text-lg">
              {project.members.length}
            </div>
          </div>
        </div>
      </div>

      {/* Team Members Section */}
      <div className="bg-base-100 rounded-lg shadow-lg p-6 mb-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-2xl font-bold">Assigned Members</h2>
          {canEdit && (
            <button className="btn btn-soft btn-sm">
              <span className="material-symbols-outlined">person_add</span>
              Add Member
            </button>
          )}
        </div>

        {/* Members by Role */}
        <div className="grid grid-cols-2 md:grid-cols-4  gap-4">
          {project.members.map((member, index) => (
            <div key={index} className="flex items-center gap-3 bg-base-200 p-3 rounded-lg">
              <div className="avatar">
                <div className="w-10 rounded-full">
                  <img src={member.avatarUrl} alt={member.name} />
                </div>
              </div>
              <span>{member.name}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Tickets Section */}
      <div className="bg-base-100 rounded-lg shadow-lg p-6" >
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-2xl font-bold">Tickets</h2>
          <div className="flex gap-2">
            <Link to={`/tickets/new?projectId=${project.id}`} className="btn btn-soft btn-sm">
              <span className="material-symbols-outlined">add_circle</span>
              New Ticket
            </Link>
            <Link to={`/projects/${project.id}/tickets`} className="btn btn-outline btn-sm">
              <span className="material-symbols-outlined">visibility</span>
              View All
            </Link>
          </div>
        </div>

        {/* Tickets Table */}
        <div className="overflow-x-auto">
          <table className="table table-zebra w-full">
            <thead>
              <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Assignee</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {tickets.map((ticket) => (
                <tr key={ticket.id}>
                  <td>{ticket.title}</td>
                  <td>
                    <div className={`badge ${getStatusClass(ticket.status)}`}>
                      {ticket.status}
                    </div>
                  </td>
                  <td>
                    <div className={`badge ${getPriorityClass(ticket.priority)}`}>
                      {ticket.priority}
                    </div>
                  </td>
                  <td>{ticket.assignee}</td>
                  <td>
                    <Link to={`/tickets/${ticket.id}`} className="btn btn-xs btn-ghost">
                      <span className="material-symbols-outlined">visibility</span>
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div >
  );
}

// Helper functions for styling
function getPriorityClass(priority: string): string {
  switch (priority.toLowerCase()) {
    case "high":
      return "text-error";
    case "medium":
      return "text-warning";
    case "low":
      return "text-info";
    default:
      return "text-neutral";
  }
}

function getStatusClass(status: string): string {
  switch (status.toLowerCase()) {
    case "open":
      return "badge-primary";
    case "in progress":
      return "badge-warning";
    case "resolved":
    case "completed":
      return "badge-success";
    case "closed":
      return "badge-neutral";
    default:
      return "badge-neutral";
  }
}
