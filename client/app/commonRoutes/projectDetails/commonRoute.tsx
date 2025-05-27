import { Link, useLoaderData, useOutletContext, useParams, useActionData, useNavigation, useFetcher, Form } from "@remix-run/react";
import { useRef, useState, } from "react";
import { EditModeForm, PrioritySelect } from "~/components/EditModeForm";
import { CompanyMemberPerRole, ProjectManagerInfo, ProjectResponse, UserInfoResponse } from "~/services/api.server/types";
import { useEditMode, getPriorityClass } from "~/utils/editMode";
import { ActionResponseParams, JsonResponseResult } from "~/utils/response";
import RouteLayout from "~/layouts/RouteLayout";
import roleNames from "~/data/roles";
import MembersListTable from "~/components/MembersListTable";
import TicketTable from "~/components/TicketTable";
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
                    <div className="bg-base-100 rounded-lg shadow-lg p-6 mb-6 mt-3">
                        {isEditing ? (
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
                        ) : (
                            // Display project details
                            <>
                                <div className="flex justify-between items-start">
                                    <div className="flex flex-col">
                                        {project.isArchived &&
                                            <div className="badge badge-warning font-medium mb-2">Archived</div>
                                        }
                                        <h1 className="flex items-center gap-3">
                                            <span className="flex items-center gap-3">
                                                <p className="font-bold text-3xl">{project?.name}</p>
                                                {canEdit && (
                                                    <>
                                                        <div className="dropdown dropdown-center">
                                                            <div tabIndex={0} role="button" className="btn btn-sm shadow-sm btn-soft flex gap-1 p-0 items-center border-0">
                                                                <div className="bg-base-200 grid place-items-center w-full h-full py-0.5 px-2 rounded-tl-sm rounded-bl-sm">
                                                                    <span className="!text-lg material-symbols-outlined w-full">edit</span>
                                                                </div>
                                                                <svg className="w-5 pr-1" viewBox="0 0 25 25" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                                    <path className="fill-base-content" d="M11.1808 15.8297L6.54199 9.20285C5.89247 8.27496 6.55629 7 7.68892 7L16.3111 7C17.4437 7 18.1075 8.27496 17.458 9.20285L12.8192 15.8297C12.4211 16.3984 11.5789 16.3984 11.1808 15.8297Z" fill="#33363F" />
                                                                </svg>
                                                            </div>
                                                            <ul tabIndex={0} className="menu dropdown-content bg-base-300 rounded-box z-1 w-52 p-2 shadow-sm mt-1">
                                                                <li>
                                                                    <a onClick={handleEditToggle} className="flex items-center gap-2">
                                                                        <span className="material-symbols-outlined">edit</span>
                                                                        Project Details
                                                                    </a>
                                                                </li>
                                                                {isAdmin && (
                                                                    <>
                                                                        {!project.isArchived && (
                                                                            <>
                                                                                <li>
                                                                                    <a onClick={() => handleOnGetPMs()}>
                                                                                        <span className="material-symbols-outlined">person_add</span>
                                                                                        Assign PM
                                                                                    </a>
                                                                                </li>
                                                                                {project.projectManager && (
                                                                                    <li>
                                                                                        <a onClick={() => handleOnRemovePM()}>
                                                                                            <span className="material-symbols-outlined">person_remove</span>
                                                                                            Remove PM
                                                                                        </a>
                                                                                    </li>
                                                                                )}
                                                                            </>
                                                                        )}
                                                                        <li>
                                                                            <Form method="post" action={`/projects/${project.id}/archive`} className="block hover:bg-warning/10 hover:text-warning">
                                                                                <button type="submit" name="intent" value={project?.isArchived ? "unarchive" : "archive"} className="flex items-center text-left gap-2 cursor-pointer w-full">
                                                                                    <span className={`material-symbols-outlined`}>folder</span>
                                                                                    <p className="w-full">
                                                                                        {project?.isArchived ? "Unarchive" : "Archive"}
                                                                                    </p>
                                                                                </button>
                                                                            </Form>
                                                                        </li>
                                                                    </>
                                                                )}
                                                            </ul>
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
                                                        </div>
                                                        {/* Archive/Unarchive Project button */}
                                                    </>
                                                )}
                                            </span>
                                        </h1>
                                        <p className="text-base-content/70 mt-2">{project?.description}</p>
                                    </div>
                                    <div className="flex gap-2 items-center">
                                    </div>
                                </div>

                                {/* Project Details */}
                                <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
                                    <div className="stat bg-base-200 rounded-lg">
                                        <div className="stat-title mb-2">Project Manager</div>
                                        {project.projectManager ? (
                                            <div className="flex gap-2 items-center">
                                                <div className="avatar">
                                                    <div className="w-9 rounded-full">
                                                        <img src={project.projectManager.avatarUrl} />
                                                    </div>
                                                </div>
                                                <div className={`stat-value text-lg font-bold`}>
                                                    {project.projectManager.name}
                                                </div>
                                            </div>
                                        ) : <p className="font-black">N/A</p>}
                                    </div>
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
                                        <div className="stat-value text-lg font-bold">
                                            {project.projectManager ?
                                                project.members.length + 1 :
                                                project.members.length}
                                        </div>
                                    </div>
                                </div>
                            </>
                        )}
                    </div>
                    {/* Team Members Section */}
                    <div className="bg-base-100 rounded-lg shadow-lg p-6 mb-6">
                        <div className="flex justify-between items-center mb-4">
                            <h2 className="text-2xl font-bold">Assigned Members</h2>
                            {/* Add members */}
                            {(canEdit && !project.isArchived) && (
                                <>
                                    <div className="flex gap-2">
                                        <button className="btn btn-soft btn-sm" onClick={() => handleOnGetMembersList()}>
                                            <span className="material-symbols-outlined text-success">person_add</span>
                                            Add
                                        </button>
                                        <button className="btn btn-soft btn-sm" onClick={() => handleOnRemoveMembersList()}>
                                            <span className="material-symbols-outlined text-error">person_remove</span>
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
                    <div className="bg-base-100 rounded-lg shadow-lg p-6" >
                        <div className="flex justify-between items-center mb-4">
                            <h2 className="text-2xl font-bold">Tickets</h2>
                            {
                                !project.isArchived && (
                                    <Link to={`/tickets/new?projectId=${project.id}`} className="btn btn-soft btn-sm">
                                        <span className="material-symbols-outlined text-success">add_circle</span>
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

