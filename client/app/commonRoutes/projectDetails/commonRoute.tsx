import { Link, useLoaderData, useOutletContext, useParams, useActionData, useNavigation, useFetcher, Form } from "@remix-run/react";
import { useRef, useState, } from "react";
import { EditModeForm, PrioritySelect } from "~/components/EditModeForm";
import { CompanyMemberPerRole, ProjectManagerInfo, ProjectResponse, UserInfoResponse } from "~/services/api.server/types";
import { useEditMode } from "~/utils/editMode";
import { ActionResponseParams, JsonResponseResult } from "~/utils/response";
import RouteLayout from "~/layouts/RouteLayout";
import roleNames from "~/data/roles";
import MembersListTable from "~/components/MembersListTable";
import TicketTable from "~/components/TicketTable";
import { DeadlineDisplay } from "~/utils/deadline";
import ProjectManagerListModal from "./components/ProjectManagerListModal";
import MemberListModal from "./components/MemberListModal";
import RemoveMemberListModal from "./components/RemoveMemberListModal";
import BackButton from "~/components/BackButton";

export type ProjectRouteParams = {
    loaderData: JsonResponseResult<ProjectResponse>,
    userInfo: UserInfoResponse
    collection?: "myprojects" | "archived"
}

export default function Route({ loaderData, userInfo, collection }: ProjectRouteParams) {
    const { projectId } = useParams()
    const { data: project, error } = loaderData;
    const actionData = useActionData() as ActionResponseParams;
    const userRole = userInfo?.role?.toLowerCase()
    const { isEditing, formError, toggleEditMode } = useEditMode({ actionData });

    const navigation = useNavigation();
    const isSubmitting = navigation.state === "submitting";

    const archiveFetcher = useFetcher({ key: "archive-project" });
    const getMembersFetcher = useFetcher({ key: "get-members" })
    const addMembersFetcher = useFetcher({ key: "add-members" })
    const removeMemberFetcher = useFetcher({ key: "remove-member" })
    const getProjectManagersFetcher = useFetcher({ key: "get-pms" })
    const assignProjectManagerFetcher = useFetcher({ key: "assign-pm" })

    const getMembersModalRef = useRef<HTMLDialogElement>(null)
    const removeMemberModalRef = useRef<HTMLDialogElement>(null)
    const assignProjectManagerModalRef = useRef<HTMLDialogElement>(null)

    // State for form fields
    const [priority, setPriority] = useState<string>(project?.priority || "");

    let isAssignedPM = false
    project?.members.forEach(m => {
        if (m.id === userInfo.memberId &&
            m.role.toLowerCase() == roleNames.projectManager &&
            m.role.toLowerCase() == userInfo.role.toLowerCase()) {
            isAssignedPM = true
            return
        }
    })
    const isAdmin = userRole === roleNames.admin
    const canEdit = isAdmin || isAssignedPM

    // Reset priority when toggling edit mode
    const handleEditToggle = () => {
        if (project) {
            setPriority(project.priority);
        }
        toggleEditMode();
    };

    const handleOnGetMembersList = () => {
        if (getMembersModalRef && projectId) {
            getMembersModalRef.current?.showModal()
            getMembersFetcher.load(`/projects/${projectId}/unassigned-members`)
        }
    }

    const handleOnRemoveMembersList = () => {
        if (removeMemberModalRef && projectId) {
            removeMemberModalRef.current?.showModal()
        }
    }

    const handleOnGetPMs = () => {
        if (assignProjectManagerModalRef && projectId) {
            assignProjectManagerModalRef.current?.showModal()
            getProjectManagersFetcher.load(`/projects/${projectId}/get-pms`)
        }
    }

    const handleOnRemovePM = () => {
        if (projectId && project?.projectManager) {
            const formData = new FormData();
            // Send empty form data. When no memberId present, it removes the pm
            assignProjectManagerFetcher.submit(formData, {
                method: "post",
                action: `/projects/${projectId}/assign-pm`
            })
        }
    }

    return (
        <RouteLayout >
            {error || !project ?
                <p className="text-error mt-4">{error}</p> :
                <>
                    <BackButton to={collection ? `/projects/${collection}` : "/projects"} />
                    <div className="bg-base-100 rounded-xl shadow-sm border border-base-300/50 mb-6 mt-3 overflow-hidden">
                        {isEditing ? (
                            <div className="p-6">
                                <EditModeForm
                                    error={formError}
                                    isSubmitting={isSubmitting}
                                    onCancel={handleEditToggle}
                                >
                                    <div className="form-control mb-4">
                                        <label className="label">
                                            <span className="label-text">Project Name</span>
                                        </label>
                                        <input
                                            type="text"
                                            name="name"
                                            className="input input-bordered w-full"
                                            defaultValue={project?.name}
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
                                            defaultValue={project?.description}
                                            rows={4}
                                            required
                                            maxLength={1000}
                                        ></textarea>
                                    </div>

                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                                        <PrioritySelect
                                            value={priority}
                                            onChange={setPriority}
                                        />

                                        <div className="form-control">
                                            <label className="label">
                                                <span className="label-text">Due Date</span>
                                            </label>
                                            <input
                                                type="date"
                                                name="dueDate"
                                                className="input input-bordered w-full"
                                                defaultValue={new Date(project!.dueDate).toISOString().split('T')[0]}
                                                required
                                            />
                                        </div>
                                    </div>
                                </EditModeForm>
                            </div>
                        ) : (
                            // Display project details
                            <>
                                {/* Header Section */}
                                <div className="border-b border-base-300/30 p-6">
                                    <div className="flex justify-between items-start">
                                        <div className="flex flex-col space-y-3">
                                            {project.isArchived && (
                                                <div className="font-semibold text-lg text-warning">
                                                    📁 Archived
                                                </div>
                                            )}
                                            <div className="space-y-2">
                                                <h1 className="text-3xl font-bold text-base-content leading-tight">
                                                    {project?.name}
                                                </h1>
                                                <p className="text-base-content/70 text-lg leading-relaxed max-w-3xl">
                                                    {project?.description}
                                                </p>
                                            </div>
                                        </div>
                                        <div className="flex gap-3 items-center flex-shrink-0 ml-6">
                                            {canEdit && (
                                                <button
                                                    onClick={handleEditToggle}
                                                    className="btn btn-soft btn-sm gap-2 shadow-sm"
                                                >
                                                    <span className="material-symbols-outlined text-sm">edit</span>
                                                    Edit Details
                                                </button>
                                            )}
                                            {isAdmin && (
                                                <Form method="post" action={`/projects/${project.id}/archive`}>
                                                    <button
                                                        type="submit"
                                                        name="intent"
                                                        value={project?.isArchived ? "unarchive" : "archive"}
                                                        className={`btn btn-sm gap-2 shadow-sm ${project?.isArchived ? 'btn-success' : 'btn-warning'}`}
                                                    >
                                                        <span className="material-symbols-outlined text-sm">folder</span>
                                                        {project?.isArchived ? "Unarchive" : "Archive"}
                                                    </button>
                                                </Form>
                                            )}
                                        </div>
                                    </div>
                                </div>

                                {/* Project Manager Modal */}
                                {isAdmin && (
                                    <ProjectManagerListModal
                                        modalRef={assignProjectManagerModalRef}
                                        loading={getProjectManagersFetcher.state === "loading"}
                                        members={(getProjectManagersFetcher.data as JsonResponseResult<ProjectManagerInfo[]>)?.data}
                                        actionFetcher={assignProjectManagerFetcher}
                                        currentPM={project.projectManager}
                                        actionFetcherSubmit={(formData) => {
                                            assignProjectManagerFetcher.submit(formData, {
                                                method: "post",
                                                action: `/projects/${projectId}/assign-pm`
                                            })
                                        }}
                                        modalTitle="Select Project Manager to Assign"
                                        buttonText="Assign"
                                    />
                                )}

                                {/* Project Details Stats */}
                                <div className="p-6">
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                        <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                                            <div className="flex justify-between items-center mb-3">
                                                <div className="text-sm font-medium text-base-content/70 uppercase tracking-wide">
                                                    Project Manager
                                                </div>
                                                {isAdmin && !project.isArchived && (
                                                    <div className="flex gap-1">
                                                        <button
                                                            onClick={() => handleOnGetPMs()}
                                                            className="btn btn-xs btn-ghost btn-circle"
                                                            title="Assign PM"
                                                        >
                                                            <span className="material-symbols-outlined text-xs">person_add</span>
                                                        </button>
                                                        {project.projectManager && (
                                                            <button
                                                                onClick={() => handleOnRemovePM()}
                                                                className="btn btn-xs btn-ghost btn-circle"
                                                                title="Remove PM"
                                                            >
                                                                <span className="material-symbols-outlined text-xs">person_remove</span>
                                                            </button>
                                                        )}
                                                    </div>
                                                )}
                                            </div>
                                            {project.projectManager ? (
                                                <div className="flex gap-3 items-center">
                                                    <div className="avatar">
                                                        <div className="w-10 rounded-full ring-2 ring-base-300/50">
                                                            <img src={project.projectManager.avatarUrl} alt="Project Manager" />
                                                        </div>
                                                    </div>
                                                    <div className="text-base font-semibold text-base-content">
                                                        {project.projectManager.name}
                                                    </div>
                                                </div>
                                            ) : (
                                                <div className="text-base font-medium text-base-content/50">
                                                    Not assigned
                                                </div>
                                            )}
                                        </div>

                                        <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                                            <div className="text-sm font-medium text-base-content/70 uppercase tracking-wide mb-3">
                                                Priority
                                            </div>
                                            <div className="text-base font-semibold">
                                                {getPriorityDisplay(project.priority)}
                                            </div>
                                        </div>

                                        <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                                            <div className="text-sm font-medium text-base-content/70 uppercase tracking-wide mb-3">
                                                Due Date
                                            </div>
                                            <div className="text-base font-semibold">
                                                <DeadlineDisplay
                                                    dueDate={project.dueDate}
                                                    variant="detailed"
                                                    showLabel={false}
                                                />
                                            </div>
                                        </div>

                                        <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                                            <div className="text-sm font-medium text-base-content/70 uppercase tracking-wide mb-3">
                                                Team Size
                                            </div>
                                            <div className="text-base font-semibold">
                                                {project.projectManager ?
                                                    project.members.length + 1 :
                                                    project.members.length} members
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </>
                        )}
                    </div>
                    {/* Team Members Section */}
                    <div className="bg-base-100 rounded-xl shadow-sm border border-base-300/50 p-6 mb-6">
                        <div className="flex justify-between items-center mb-4">
                            <h2 className="text-2xl font-bold">Assigned Members</h2>
                            {/* Add members */}
                            {(canEdit && !project.isArchived) && (
                                <>
                                    <div className="flex gap-2">
                                        <button className="btn btn-soft btn-sm gap-2 shadow-sm" onClick={() => handleOnGetMembersList()}>
                                            <span className="material-symbols-outlined text-success text-sm">person_add</span>
                                            Add
                                        </button>
                                        <button className="btn btn-soft btn-sm gap-2 shadow-sm" onClick={() => handleOnRemoveMembersList()}>
                                            <span className="material-symbols-outlined text-error text-sm">person_remove</span>
                                            Remove
                                        </button>
                                    </div>
                                    <MemberListModal
                                        modalRef={getMembersModalRef}
                                        loading={getMembersFetcher.state === "loading"}
                                        members={(getMembersFetcher.data as JsonResponseResult<CompanyMemberPerRole>)?.data}
                                        actionFetcher={addMembersFetcher}
                                        actionFetcherSubmit={(formData) => {
                                            addMembersFetcher.submit(formData, {
                                                method: "post",
                                                action: `/projects/${projectId}/add-members`
                                            })
                                        }}
                                        projectId={projectId}
                                    />
                                    {/* INFO: This modal removes a single member. There may be conditions to removing a member */}
                                    <RemoveMemberListModal
                                        modalRef={removeMemberModalRef}
                                        projectId={projectId}
                                        members={project.members.filter(m => m.id !== userInfo.memberId)}
                                        actionFetcher={removeMemberFetcher}
                                        actionFetcherSubmit={(formData) => {
                                            removeMemberFetcher.submit(formData, {
                                                method: "post",
                                                action: `/projects/${projectId}/remove-member`
                                            })
                                        }}
                                    />
                                </>
                            )}
                        </div>

                        {/* Members by Role */}
                        <MembersListTable members={project.members} />
                    </div>

                    {/* Tickets Section */}
                    <div className="bg-base-100 rounded-xl shadow-sm border border-base-300/50 p-6" >
                        <div className="flex justify-between items-center mb-4">
                            <h2 className="text-2xl font-bold">Tickets</h2>
                            {
                                !project.isArchived && (
                                    <Link to={`/tickets/new?projectId=${project.id}`} className="btn btn-soft btn-sm gap-2 shadow-sm">
                                        <span className="material-symbols-outlined text-success text-sm">add_circle</span>
                                        New Ticket
                                    </Link>
                                )
                            }
                        </div>

                        {/* Tickets Table */}
                        <TicketTable tickets={project?.tickets} />
                    </div>
                </>
            }
        </RouteLayout >
    );
}

// Helper function to get priority display with emoji
function getPriorityDisplay(priority: string): string {
    switch (priority?.toLowerCase()) {
        case 'urgent':
            return '🔴 Urgent';
        case 'high':
            return '🟠 High';
        case 'medium':
            return '🟡 Medium';
        case 'low':
            return '🟢 Low';
        default:
            return priority;
    }
}



