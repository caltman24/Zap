import {Link} from "@remix-run/react";
import type {CompanyProjectsResponse} from "~/services/api.server/types";
import ProjectDueDateBadge from "~/components/ProjectDueDateBadge";
import {getDeadlineStatus} from "~/utils/deadline";

type ProjectCollection = "archived" | "myprojects" | "projects";

type ProjectCardProps = {
    project: CompanyProjectsResponse;
    showArchived?: boolean;
    collection: ProjectCollection;
};

function getProjectHref(project: CompanyProjectsResponse, collection: ProjectCollection) {
    return collection === "projects" ? `/projects/${project.id}` : `/projects/${collection}/${project.id}`;
}

function getPriorityTone(priority: string) {
    switch (priority.toLowerCase()) {
        case "urgent":
            return {
                accentClass: "border-l-[var(--app-error)]",
                chipClass: "bg-[var(--app-error-container)]/25 text-[var(--app-error)]",
                dotClass: "bg-[var(--app-error)]",
            };
        case "high":
            return {
                accentClass: "border-l-[var(--app-tertiary)]",
                chipClass: "bg-[var(--app-tertiary-container)]/25 text-[var(--app-tertiary)]",
                dotClass: "bg-[var(--app-tertiary)]",
            };
        case "medium":
            return {
                accentClass: "border-l-[var(--app-primary-fixed)]",
                chipClass: "bg-[var(--app-secondary-container)]/35 text-[var(--app-secondary)]",
                dotClass: "bg-[var(--app-secondary)]",
            };
        case "low":
            return {
                accentClass: "border-l-[var(--app-success)]",
                chipClass: "bg-emerald-500/15 text-[var(--app-success)]",
                dotClass: "bg-[var(--app-success)]",
            };
        default:
            return {
                accentClass: "border-l-[var(--app-outline)]",
                chipClass: "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]",
                dotClass: "bg-[var(--app-outline)]",
            };
    }
}

function getDeadlineLabel(dueDate: string) {
    const deadlineStatus = getDeadlineStatus(dueDate);
    const now = new Date();
    const deadline = new Date(dueDate);
    const diffMs = deadline.getTime() - now.getTime();
    const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));

    if (deadlineStatus === "overdue") {
        const overdueDays = Math.abs(diffDays);
        return overdueDays <= 1 ? "Overdue" : `${overdueDays} days overdue`;
    }

    if (deadlineStatus === "due-soon") {
        if (diffDays <= 0) return "Due today";
        if (diffDays === 1) return "Due tomorrow";
        return `${diffDays} days left`;
    }

    return "On track";
}

export default function ProjectCard({project, showArchived, collection}: ProjectCardProps) {
    const priorityTone = getPriorityTone(project.priority);
    const visibleAvatars = project.avatarUrls.slice(0, 3);
    const extraMemberCount = Math.max(project.memberCount - visibleAvatars.length, 0);
    const isArchivedView = Boolean(showArchived);
    const deadlineStatus = getDeadlineStatus(project.dueDate);
    const deadlineIcon = deadlineStatus === "overdue" ? "error" : deadlineStatus === "due-soon" ? "schedule" : "event";
    const deadlineTextClass =
        deadlineStatus === "overdue"
            ? "text-[var(--app-error)]"
            : deadlineStatus === "due-soon"
                ? "text-[var(--app-tertiary)]"
                : "text-[var(--app-on-surface)]";

    return (
        <Link
            className={`group flex h-full flex-col rounded-[1.6rem] border-l-2 bg-[var(--app-surface-container-low)] p-5 outline outline-1 outline-[var(--app-outline-variant-soft)] transition-all duration-200 hover:-translate-y-1 hover:bg-[var(--app-surface-container-high)]/35 hover:shadow-[0_24px_48px_rgba(0,0,0,0.2)] ${priorityTone.accentClass}`}
            to={getProjectHref(project, collection)}
        >
            <div className="flex items-start justify-between gap-4">
                <div className="space-y-2">
                    {isArchivedView ? (
                        <div className="flex flex-wrap items-center gap-2">
              <span className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-archive)]">
                Archived
              </span>
                        </div>
                    ) : null}

                    <h2 className="text-[1.35rem] font-bold tracking-[-0.03em] text-[var(--app-on-surface)] transition-colors group-hover:text-[var(--app-primary)]">
                        {project.name}
                    </h2>
                </div>

                <span
                    className={`inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-medium ${priorityTone.chipClass}`}>
          <span className={`h-2 w-2 rounded-full ${priorityTone.dotClass}`}/>
                    {project.priority}
        </span>
            </div>

            <div className="mt-4 flex items-center gap-3">
                <ProjectDueDateBadge dueDate={project.dueDate}/>

                <div className="min-w-0 flex-1">
                    <p className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Due
                        Date</p>
                    <div className="mt-1 flex items-center gap-2">
                        <span className={`material-symbols-outlined text-sm ${deadlineTextClass}`}>{deadlineIcon}</span>
                        <span className={`text-xs ${deadlineTextClass}`}>{getDeadlineLabel(project.dueDate)}</span>
                    </div>
                </div>
            </div>

            <div className="mt-4 flex items-center justify-between gap-4">
                <div className="flex items-center -space-x-3">
                    {visibleAvatars.map((avatarUrl, index) => (
                        <span
                            className="inline-flex h-10 w-10 items-center justify-center overflow-hidden rounded-full border-2 border-[var(--app-surface-container-low)] bg-[var(--app-surface-container-high)]"
                            key={`${project.id}-${index}`}
                        >
              <img alt="Project member" className="h-full w-full object-cover" src={avatarUrl}/>
            </span>
                    ))}
                    {extraMemberCount > 0 ? (
                        <span
                            className="inline-flex h-10 w-10 items-center justify-center rounded-full border-2 border-[var(--app-surface-container-low)] bg-[var(--app-surface-container-high)] text-[10px] font-bold text-[var(--app-on-surface)]">
              +{extraMemberCount}
            </span>
                    ) : null}
                </div>

                <div className="flex items-center gap-3">
          <span className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">
            {project.memberCount} {project.memberCount === 1 ? "Member" : "Members"}
          </span>
                    <span
                        className="material-symbols-outlined text-lg text-[var(--app-outline)] transition-transform group-hover:translate-x-1 group-hover:text-[var(--app-primary)]">
            arrow_outward
          </span>
                </div>
            </div>
        </Link>
    );
}
