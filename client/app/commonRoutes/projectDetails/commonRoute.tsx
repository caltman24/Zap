import { Form, Link, useActionData, useFetcher, useLocation, useNavigation, useParams } from "@remix-run/react";
import { useRef, useState } from "react";
import MembersListTable from "~/components/MembersListTable";
import {
  FormFieldHeader,
  FormSelectControl,
  formInputClassName,
  formTextareaClassName,
} from "~/components/FormShell";
import TicketTable from "~/components/TicketTable";
import ProjectDueDateBadge from "~/components/ProjectDueDateBadge";
import RouteLayout from "~/layouts/RouteLayout";
import { projectPriorityOptions } from "~/data/selectOptions";
import type {
  CompanyMemberPerRole,
  ProjectManagerInfo,
  ProjectResponse,
  UserInfoResponse,
} from "~/services/api.server/types";
import { useEditMode } from "~/utils/editMode";
import type { ActionResponseParams, JsonResponseResult } from "~/utils/response";
import { getDeadlineStatus } from "~/utils/deadline";
import BackButton from "~/components/BackButton";
import DropdownMenu from "~/components/DropdownMenu";
import MemberListModal from "./components/MemberListModal";
import ProjectManagerListModal from "./components/ProjectManagerListModal";
import RemoveMemberListModal from "./components/RemoveMemberListModal";

export type ProjectRouteParams = {
  loaderData: JsonResponseResult<ProjectResponse>;
  userInfo: UserInfoResponse;
  collection?: "myprojects" | "archived";
};

function getPriorityTone(priority: string) {
  switch (priority.toLowerCase()) {
    case "urgent":
      return {
        textClass: "text-[var(--app-error)]",
        dotClass: "bg-[var(--app-error)]",
      };
    case "high":
      return {
        textClass: "text-[var(--app-tertiary)]",
        dotClass: "bg-[var(--app-tertiary)]",
      };
    case "medium":
      return {
        textClass: "text-[var(--app-primary-fixed)]",
        dotClass: "bg-[var(--app-primary-fixed)]",
      };
    case "low":
      return {
        textClass: "text-[var(--app-success)]",
        dotClass: "bg-[var(--app-success)]",
      };
    default:
      return {
        textClass: "text-[var(--app-on-surface)]",
        dotClass: "bg-[var(--app-outline)]",
      };
  }
}

function getDeadlineMeta(dueDate: string) {
  const deadlineStatus = getDeadlineStatus(dueDate);
  const today = new Date();
  const deadline = new Date(dueDate);
  const diffDays = Math.ceil((deadline.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

  if (deadlineStatus === "overdue") {
    const overdueDays = Math.abs(diffDays);
    return {
      containerClass: "bg-[var(--app-error-container)]/20 border-[var(--app-error)]/10",
      textClass: "text-[var(--app-error)]",
      icon: "error",
      label: overdueDays <= 1 ? "Overdue" : `${overdueDays} days overdue`,
    };
  }

  if (deadlineStatus === "due-soon") {
    return {
      containerClass: "bg-[var(--app-tertiary-container)]/20 border-[var(--app-tertiary)]/10",
      textClass: "text-[var(--app-tertiary)]",
      icon: "schedule",
      label: diffDays <= 0 ? "Due today" : diffDays === 1 ? "Due tomorrow" : `${diffDays} days left`,
    };
  }

  return {
    containerClass: "bg-[var(--app-surface-container-highest)]/30 border-white/5",
    textClass: "text-[var(--app-on-surface-variant)]",
    icon: "event",
    label: "On track",
  };
}

function getBackLink(collection?: "myprojects" | "archived") {
  return collection ? `/projects/${collection}` : "/projects";
}

export default function ProjectCommonRoute({ loaderData, userInfo, collection }: ProjectRouteParams) {
  const { projectId } = useParams();
  const { data: project, error } = loaderData;
  const actionData = useActionData() as ActionResponseParams;
  const { isEditing, formError, toggleEditMode } = useEditMode({ actionData });
  const navigation = useNavigation();
  const location = useLocation();
  const isSubmitting = navigation.state === "submitting";

  const getMembersFetcher = useFetcher({ key: "get-members" });
  const addMembersFetcher = useFetcher({ key: "add-members" });
  const removeMemberFetcher = useFetcher({ key: "remove-member" });
  const getProjectManagersFetcher = useFetcher({ key: "get-pms" });
  const assignProjectManagerFetcher = useFetcher({ key: "assign-pm" });

  const getMembersModalRef = useRef<HTMLDialogElement>(null);
  const removeMemberModalRef = useRef<HTMLDialogElement>(null);
  const assignProjectManagerModalRef = useRef<HTMLDialogElement>(null);

  const [priority, setPriority] = useState<string>(project?.priority || "");

  function handleEditToggle() {
    if (project) {
      setPriority(project.priority);
    }
    toggleEditMode();
  }

  function handleOnGetMembersList() {
    if (getMembersModalRef && projectId) {
      getMembersModalRef.current?.showModal();
      getMembersFetcher.load(`/projects/${projectId}/unassigned-members`);
    }
  }

  function handleOnRemoveMembersList() {
    if (removeMemberModalRef && projectId) {
      removeMemberModalRef.current?.showModal();
    }
  }

  function handleOnGetPMs() {
    if (assignProjectManagerModalRef && projectId) {
      assignProjectManagerModalRef.current?.showModal();
      getProjectManagersFetcher.load(`/projects/${projectId}/get-pms`);
    }
  }

  function handleOnRemovePM() {
    if (projectId && project?.projectManager) {
      const formData = new FormData();
      assignProjectManagerFetcher.submit(formData, {
        method: "post",
        action: `/projects/${projectId}/assign-pm`,
      });
    }
  }

  if (error || !project) {
    return (
      <RouteLayout>
        <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline outline-[var(--app-outline-variant-soft)]">
          {error}
        </div>
      </RouteLayout>
    );
  }

  const priorityTone = getPriorityTone(project.priority);
  const deadlineMeta = getDeadlineMeta(project.dueDate);
  const dueDateDisplay = new Date(project.dueDate).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
  const teamSize = project.projectManager ? project.members.length + 1 : project.members.length;
  const hasToolbarActions = project.capabilities.canEdit || project.capabilities.canArchive;

  return (
    <RouteLayout className="space-y-8">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <BackButton to={location.state?.from || getBackLink(collection)} />

        {!isEditing && hasToolbarActions ? (
          <>
            <DropdownMenu
              className="sm:hidden"
              menuClassName="min-w-56"
              triggerAriaLabel="Open actions menu"
              triggerClassName="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] outline outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
              trigger={
                <>
                  <span className="material-symbols-outlined text-lg">more_horiz</span>
                  Actions
                </>
              }
            >
              {({ close }) => (
                <>
                {project.capabilities.canEdit ? (
                  <button
                    className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                    onClick={() => {
                      close();
                      handleEditToggle();
                    }}
                    type="button"
                  >
                    <span className="material-symbols-outlined text-lg">edit</span>
                    <span>Edit Details</span>
                  </button>
                ) : null}

                {project.capabilities.canArchive ? (
                  <Form action={`/projects/${project.id}/archive`} method="post" onSubmit={close}>
                    <button
                      className={`flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm font-medium transition-colors ${project.isArchived
                        ? "text-[var(--app-success)] hover:bg-emerald-500/10"
                        : "text-[var(--app-tertiary)] hover:bg-[var(--app-tertiary-container)]/15"
                        }`}
                      name="intent"
                      type="submit"
                      value={project.isArchived ? "unarchive" : "archive"}
                    >
                      <span className="material-symbols-outlined text-lg">folder</span>
                      <span>{project.isArchived ? "Unarchive" : "Archive"}</span>
                    </button>
                  </Form>
                ) : null}
                </>
              )}
            </DropdownMenu>

            <div className="hidden flex-wrap items-center justify-end gap-3 sm:flex">
              {project.capabilities.canEdit ? (
                <button
                  className="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                  onClick={handleEditToggle}
                  type="button"
                >
                  <span className="material-symbols-outlined text-lg">edit</span>
                  Edit Details
                </button>
              ) : null}

              {project.capabilities.canArchive ? (
                <Form action={`/projects/${project.id}/archive`} method="post">
                  <button
                    className={`inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium backdrop-blur-sm transition-colors ${project.isArchived
                      ? "text-[var(--app-success)] outline-1 outline-[var(--app-success)]/15 hover:bg-emerald-500/10"
                      : "text-[var(--app-tertiary)] outline-1 outline-[var(--app-tertiary)]/15 hover:bg-[var(--app-tertiary-container)]/15"
                      }`}
                    name="intent"
                    type="submit"
                    value={project.isArchived ? "unarchive" : "archive"}
                  >
                    <span className="material-symbols-outlined text-lg">folder</span>
                    {project.isArchived ? "Unarchive" : "Archive"}
                  </button>
                </Form>
              ) : null}
            </div>
          </>
        ) : null}
      </div>

      <section className="border-b border-[var(--app-outline-variant)]/10 pb-8">
        {isEditing ? (
          <div className="space-y-6 p-6 sm:p-8">
            <div className="flex flex-wrap items-start justify-between gap-4 border-b border-[var(--app-outline-variant)]/10 pb-6">
              <div>
                <h1 className="text-3xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">Edit Project</h1>
                <p className="mt-1 max-w-2xl text-sm text-[var(--app-on-surface-variant)] sm:text-base">
                  Update the project details shown to your team.
                </p>
              </div>
            </div>

            <Form className="space-y-8" method="post">
              {formError ? (
                <div className="rounded-2xl bg-[var(--app-error-container)]/20 px-4 py-3 text-sm text-[var(--app-error)] outline-1 outline-[var(--app-error)]/10">
                  {formError}
                </div>
              ) : null}

              <div className="grid gap-6">
                <div>
                  <FormFieldHeader label="Project Name" required />
                  <input
                    className={formInputClassName}
                    defaultValue={project.name}
                    maxLength={50}
                    name="name"
                    required
                    type="text"
                  />
                </div>

                <div>
                  <FormFieldHeader label="Description" required />
                  <textarea
                    className={formTextareaClassName}
                    defaultValue={project.description}
                    maxLength={1000}
                    name="description"
                    required
                    rows={5}
                  />
                </div>

                <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                  <div>
                    <FormFieldHeader label="Priority" required />
                    <FormSelectControl name="priority" onChange={(event) => setPriority(event.target.value)} required value={priority}>
                      <option value="">Select priority</option>
                      {projectPriorityOptions.map((option) => (
                        <option key={option.value} value={option.value}>
                          {option.label}
                        </option>
                      ))}
                    </FormSelectControl>
                  </div>

                  <div>
                    <FormFieldHeader label="Due Date" required />
                    <input
                      className={formInputClassName}
                      defaultValue={new Date(project.dueDate).toISOString().split("T")[0]}
                      name="dueDate"
                      required
                      type="date"
                    />
                  </div>
                </div>
              </div>

              <div className="flex justify-end gap-3 border-t border-[var(--app-outline-variant)]/10 pt-5">
                <button
                  className="inline-flex min-w-28 items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                  disabled={isSubmitting}
                  onClick={handleEditToggle}
                  type="button"
                >
                  Cancel
                </button>
                <button
                  className="inline-flex min-w-36 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-5 py-3 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={isSubmitting}
                  type="submit"
                >
                  {isSubmitting ? "Saving..." : "Save Changes"}
                </button>
              </div>
            </Form>
          </div>
        ) : (
          <>
            <div className="px-6 pb-0 pt-6 sm:px-8 sm:pt-8">
              <div className="min-w-0 max-w-3xl space-y-4">
                {project.isArchived ? (
                  <p className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-archive)]">Archived</p>
                ) : null}
                <div className="space-y-3">
                  <h1 className="text-3xl font-bold tracking-[-0.04em] text-[var(--app-on-surface)] sm:text-[2.2rem]">
                    {project.name}
                  </h1>
                  <p className="max-w-3xl text-sm leading-6 text-[var(--app-on-surface-variant)] sm:text-base sm:leading-7">
                    {project.description}
                  </p>
                </div>
              </div>
            </div>

            <div className="mt-8 border-t border-[var(--app-outline-variant)]/10 bg-[var(--app-surface-container-lowest)]/30 px-6 py-6 sm:px-8">
              <dl className="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-4 xl:gap-5">
                <div className="space-y-3 border-l-2 border-[var(--app-primary-fixed-strong)] pl-4">
                  <div className="flex items-center justify-between gap-3">
                    <dt className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Project Manager</dt>
                    {project.capabilities.canAssignProjectManager ? (
                      <div className="flex items-center gap-1">
                        <button
                          className="inline-flex h-8 w-8 items-center justify-center rounded-lg text-[var(--app-outline)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                          onClick={handleOnGetPMs}
                          title="Assign project manager"
                          type="button"
                        >
                          <span className="material-symbols-outlined text-base">person_add</span>
                        </button>
                        {project.projectManager ? (
                          <button
                            className="inline-flex h-8 w-8 items-center justify-center rounded-lg text-[var(--app-outline)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-error)]"
                            onClick={handleOnRemovePM}
                            title="Remove project manager"
                            type="button"
                          >
                            <span className="material-symbols-outlined text-base">person_remove</span>
                          </button>
                        ) : null}
                      </div>
                    ) : null}
                  </div>
                  {project.projectManager ? (
                    <dd className="flex items-center gap-3">
                      <img
                        alt={project.projectManager.name}
                        className="h-10 w-10 rounded-full border border-[var(--app-outline-variant)]/20 object-cover"
                        src={project.projectManager.avatarUrl}
                      />
                      <span className="text-sm font-medium text-[var(--app-on-surface)]">{project.projectManager.name}</span>
                    </dd>
                  ) : (
                    <dd className="text-sm text-[var(--app-on-surface-variant)]">Not assigned</dd>
                  )}
                </div>

                <div className="space-y-3 border-l-2 border-[var(--app-tertiary)] pl-4">
                  <dt className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Priority</dt>
                  <dd className={`flex items-center gap-2 text-sm font-medium ${priorityTone.textClass}`}>
                    <span className={`h-2.5 w-2.5 rounded-full ${priorityTone.dotClass}`} />
                    {project.priority}
                  </dd>
                </div>

                <div className="space-y-3 border-l-2 border-[var(--app-secondary)] pl-4">
                  <dt className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Due Date</dt>
                  <dd className="flex items-center gap-3">
                    <ProjectDueDateBadge dueDate={project.dueDate} />
                    <div className="space-y-1">
                      <p className="text-sm font-medium text-[var(--app-on-surface)]">{dueDateDisplay}</p>
                      <div className={`flex items-center gap-2 text-xs ${deadlineMeta.textClass}`}>
                        <span className="material-symbols-outlined text-sm">{deadlineMeta.icon}</span>
                        {deadlineMeta.label}
                      </div>
                    </div>
                  </dd>
                </div>

                <div className="space-y-3 border-l-2 border-[var(--app-success)] pl-4">
                  <dt className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Team Size</dt>
                  <dd className="text-sm font-medium text-[var(--app-on-surface)]">{teamSize} members</dd>
                </div>
              </dl>
            </div>
          </>
        )}
      </section>

      {project.capabilities.canAssignProjectManager ? (
        <ProjectManagerListModal
          actionFetcher={assignProjectManagerFetcher}
          actionFetcherSubmit={(formData) => {
            assignProjectManagerFetcher.submit(formData, {
              method: "post",
              action: `/projects/${projectId}/assign-pm`,
            });
          }}
          buttonText="Assign"
          currentPM={project.projectManager}
          loading={getProjectManagersFetcher.state === "loading"}
          members={(getProjectManagersFetcher.data as JsonResponseResult<ProjectManagerInfo[]>)?.data}
          modalRef={assignProjectManagerModalRef}
          modalTitle="Select Project Manager to Assign"
        />
      ) : null}

      <section className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h2 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Assigned Members</h2>
            <p className="mt-1 text-sm text-[var(--app-on-surface-variant)] sm:text-base">
              People currently assigned to this project.
            </p>
          </div>

          {project.capabilities.canManageMembers ? (
            <div className="flex flex-wrap items-center gap-3">
              <button
                className="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                onClick={handleOnGetMembersList}
                type="button"
              >
                <span className="material-symbols-outlined text-lg">person_add</span>
                Add
              </button>
              <button
                className="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                onClick={handleOnRemoveMembersList}
                type="button"
              >
                <span className="material-symbols-outlined text-lg">person_remove</span>
                Remove
              </button>
            </div>
          ) : null}
        </div>

        <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] px-6 py-5 outline-1 outline-[var(--app-outline-variant-soft)]">
          {project.capabilities.canManageMembers ? (
            <>
              <MemberListModal
                actionFetcher={addMembersFetcher}
                actionFetcherSubmit={(formData) => {
                  addMembersFetcher.submit(formData, {
                    method: "post",
                    action: `/projects/${projectId}/add-members`,
                  });
                }}
                loading={getMembersFetcher.state === "loading"}
                members={(getMembersFetcher.data as JsonResponseResult<CompanyMemberPerRole>)?.data}
                modalRef={getMembersModalRef}
                projectId={projectId}
              />

              <RemoveMemberListModal
                actionFetcher={removeMemberFetcher}
                actionFetcherSubmit={(formData) => {
                  removeMemberFetcher.submit(formData, {
                    method: "post",
                    action: `/projects/${projectId}/remove-member`,
                  });
                }}
                members={project.members.filter((member) => member.id !== userInfo.memberId)}
                modalRef={removeMemberModalRef}
                projectId={projectId}
              />
            </>
          ) : null}

          <MembersListTable members={project.members} />
        </div>
      </section>

      <section className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h2 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Tickets</h2>
            <p className="mt-1 text-sm text-[var(--app-on-surface-variant)] sm:text-base">
              Active and historical work tracked against this project.
            </p>
          </div>

          {project.capabilities.canCreateTicket ? (
            <Link
              className="inline-flex items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2 text-xs font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95"
              to={`/tickets/new?projectId=${project.id}`}
            >
              <span className="material-symbols-outlined text-sm">add</span>
              New Ticket
            </Link>
          ) : null}
        </div>

        <TicketTable enableFiltering={false} tickets={project.tickets} />
      </section>
    </RouteLayout>
  );
}
