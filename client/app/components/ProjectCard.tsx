import { Link } from "@remix-run/react";
import { CompanyProjectsResponse } from "~/services/api.server/types";

interface ProjectCardProps {
  project: CompanyProjectsResponse;
}

export default function ProjectCard({ project }: ProjectCardProps) {
  return (
    <Link
      to={`/projects/${project.id}`}
      className="card bg-base-100 shadow-lg hover:shadow-xl transition-all duration-300"
    >
      <div className="card-body p-5">
        {/* Priority indicator */}
        <div className="absolute top-3 right-3">
          <div className={`badge ${getPriorityClass(project.priority)}`}>
            {project.priority}
          </div>
        </div>

        {/* Project title */}
        <h2 className="card-title text-xl mb-1">{project.name}</h2>

        {/* Due date */}
        <div className="flex items-center text-sm text-base-content/70 mb-4">
          <span className="material-symbols-outlined mr-1">assignment_late</span>
          Due: {new Date(project.dueDate).toLocaleDateString()}
        </div>

        {/* Divider */}
        <div className="divider my-1"></div>

        {/* Team members */}
        <div className="flex items-center justify-between mt-2">
          <div className="avatar-group -space-x-4 rtl:space-x-reverse">
            {project.avatarUrls.slice(0, 3).map((avatarUrl, index) => (
              <div key={index} className="avatar border-2 border-base-100">
                <div className="w-10 rounded-full">
                  <img src={avatarUrl} alt="Team member" />
                </div>
              </div>
            ))}
            {project.memberCount > 3 && (
              <div className="avatar placeholder border-2 border-base-100">
                <div className="w-10 h-10 rounded-full bg-primary text-primary-content relative">
                  <span className="absolute inset-0 flex items-center justify-center text-xs">
                    +{project.memberCount - 3}
                  </span>
                </div>
              </div>
            )}
          </div>
          <span className="badge badge-outline">
            {project.memberCount} {project.memberCount === 1 ? 'Member' : 'Members'}
          </span>
        </div>
      </div>
    </Link>
  );
}

// Helper function to get badge color based on priority
function getPriorityClass(priority: string): string {
  switch (priority.toLowerCase()) {
    case 'high':
      return 'badge-error';
    case 'medium':
      return 'badge-warning';
    case 'low':
      return 'badge-info';
    default:
      return 'badge-ghost';
  }
}