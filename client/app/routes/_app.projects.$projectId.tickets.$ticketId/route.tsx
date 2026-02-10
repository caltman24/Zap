
import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useFetcher, useLoaderData, useNavigation, useOutletContext, useParams } from "@remix-run/react"; import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient"; import { AuthenticationError } from "~/services/api.server/errors";
import { getSession } from "~/services/sessions.server"; import { ActionResponse, ActionResponseParams, ForbiddenResponse, JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { getTicketById } from "./server.get-ticket";
import BackButton from "~/components/BackButton";
import { useEffect, useRef, useState } from "react";
import DeveloperListModal from "./DeveloperListModal";
import { BasicUserInfo } from "~/services/api.server/types";
import { useEditMode } from "~/utils/editMode";
import { EditModeForm } from "~/components/EditModeForm";
import { validateRole } from "~/utils/validate";
import permissions from "~/data/permissions";
import updateTicket from "./server.update-ticket";
import ChatBox from "./ChatBox";
import TicketTimeline from "./TicketTimeline";
import ArchiveWarningModal from "~/components/ArchiveWarningModal";
import {
    canEditTicketFields,
    canUpdateStatus,
    canUpdatePriority,
    canUpdateType,
    canAssignDeveloper,
    canDeleteTicket,
    canArchiveTicket,
    canUnarchiveTicket,
    canCreateComment,
    type TicketPermissionContext,
} from "~/utils/ticketPermissions";

import AttachmentSection from "./AttachmentSection";
export const handle = {
    breadcrumb: (match: any) => {
        const ticketId = match.params.ticketId; const ticketName = match.data?.data?.name || "Ticket Details";
        return <Link to={`/tickets/mytickets/${ticketId}`}>{ticketName}</Link>;
    },
};
export async function loader({ request, params }: LoaderFunctionArgs) {
    const session = await getSession(request);
    const { ticketId } = params;
    const { data: tokenResponse,
        error: tokenError } = await tryCatch(apiClient.auth.getValidToken(session));
    if (tokenError) {
        return redirect("/logout");
    }
    try {
        const response = await getTicketById(ticketId!, tokenResponse.token);
        return JsonResponse({
            data: response,
            error: null, headers: tokenResponse.headers
        });
    } catch (error: any) {
        if (error instanceof AuthenticationError) {
            return redirect("/logout");
        }
        return JsonResponse({
            data: null,
            error: error.message, headers: tokenResponse.headers
        });
    }
}

export default function TicketDetailsRoute() {
    const { data: ticket, error } = useLoaderData<JsonResponseResult<any>>();
    const { ticketId } = useParams();
    const actionData = useActionData() as ActionResponseParams
    const { isEditing, formError, toggleEditMode } = useEditMode({ actionData });
    const { userInfo } = useOutletContext<any>();

    const navigation = useNavigation()

    // State for archive warning modal
    const [showArchiveWarning, setShowArchiveWarning] = useState(false);

    const updatePriorityFetcher = useFetcher({ key: "update-priority" })
    const updateStatusFetcher = useFetcher({ key: "update-status" })
    const updateTypeFetcher = useFetcher({ key: "update-type" })

    const getDevelopersFetcher = useFetcher({ key: "get-devs" })
    const assignDeveloperFetcher = useFetcher({ key: "update-dev" })

    const commentsFormRef = useRef<HTMLFormElement>(null);

    const deleteCommentFetcher = useFetcher({ key: "delete-comment" })
    const updateCommentFetcher = useFetcher({ key: "update-comment" })
    const getCommentsFetcher = useFetcher({ key: "get-comments" })
    const getHistoryFetcher = useFetcher({ key: "get-history" })

    useEffect(() => {
        getCommentsFetcher.load(`/tickets/${ticketId}/get-comments`)
        getHistoryFetcher.load(`/tickets/${ticketId}/get-history`)
    }, [])
    useEffect(() => {
        if (getCommentsFetcher.state === "idle" && getCommentsFetcher.data) {
            commentsFormRef.current?.reset();
        }
    }, [getCommentsFetcher.state, getCommentsFetcher.data])

    // Show success feedback for status/priority/type updates
    useEffect(() => {
        if (updateStatusFetcher.state === "idle" && updateStatusFetcher.data) {
            alert("Status updated successfully!");
        }
    }, [updateStatusFetcher.state, updateStatusFetcher.data]);

    useEffect(() => {
        if (updatePriorityFetcher.state === "idle" && updatePriorityFetcher.data) {
            alert("Priority updated successfully!");
        }
    }, [updatePriorityFetcher.state, updatePriorityFetcher.data]);

    useEffect(() => {
        if (updateTypeFetcher.state === "idle" && updateTypeFetcher.data) {
            alert("Type updated successfully!");
        }
    }, [updateTypeFetcher.state, updateTypeFetcher.data]);

    const developersModalRef = useRef<HTMLDialogElement>(null)

    const handleOnGetDevelopers = () => {
        developersModalRef?.current?.showModal();
        getDevelopersFetcher.load(`/tickets/${ticketId}/get-dev-list`)
    }

    const handleOnUnassignDeveloper = () => {
        const formData = new FormData()
        assignDeveloperFetcher.submit(formData, {
            method: "post",
            action: `/tickets/${ticketId}/update-dev`
        })
    }

    const handleOnDeleteComment = (commentId: string) => {
        const formData = new FormData()
        formData.set("commentId", commentId)
        deleteCommentFetcher.submit(formData, {
            method: "post",
            action: `/tickets/${ticketId}/delete-comment`
        })
    }

    const handleOnEditComment = (commentId: string, message: string) => {
        const formData = new FormData()
        formData.set("commentId", commentId)
        formData.set("message", message)
        updateCommentFetcher.submit(formData, {
            method: "post",
            action: `/tickets/${ticketId}/update-comment`
        })
    }

    const handleEditToggle = () => {
        toggleEditMode();
    };

    const handleArchiveClick = (e: React.FormEvent) => {
        // If trying to unarchive a ticket and the project is archived, show warning
        if (ticket.isArchived && ticket.projectIsArchived) {
            e.preventDefault();
            setShowArchiveWarning(true);
            return;
        }
        // Otherwise, allow the form to submit normally
    };

    // Calculate permission context
    const permissionContext: TicketPermissionContext = {
        role: userInfo.role,
        userId: userInfo.memberId,
        ticketSubmitterId: ticket.submitter.id,
        ticketAssignedDeveloperId: ticket.assignee?.id || null,
        isProjectManager: ticket.projectManagerId === userInfo.memberId,
        isArchived: ticket.isArchived,
    };

    // Permission checks
    const canEdit = canEditTicketFields(permissionContext);
    const canUpdateStatusField = canUpdateStatus(permissionContext);
    const canUpdatePriorityField = canUpdatePriority(permissionContext);
    const canUpdateTypeField = canUpdateType(permissionContext);
    const canAssign = canAssignDeveloper(userInfo.role, ticket.isArchived);
    const canDelete = canDeleteTicket(userInfo.role, ticket.isArchived);
    const canArchive = canArchiveTicket(userInfo.role);
    const canUnarchive = canUnarchiveTicket(userInfo.role, permissionContext.isProjectManager);
    const canComment = canCreateComment(userInfo.role, ticket.isArchived);

    // Handler for disabled field clicks
    const handleDisabledFieldClick = (fieldName: string) => {
        if (ticket.isArchived) {
            alert(`Cannot edit ${fieldName} - this ticket is archived. Only admins and project managers can unarchive it.`);
        } else {
            alert(`You don't have permission to edit ${fieldName}`);
        }
    };

    // TODO: Add confirm modal on delete

    if (error) {
        return <p className="text-error">{error}</p>;
    }

    const TicketDetails = (
        <>
            {/* Header Section */}
            <div className="border-b border-base-300/30 p-6">
                <div className="flex justify-between items-start">
                    <div className="flex flex-col space-y-3">
                        {ticket.isArchived && (
                            <div className="font-semibold text-lg text-warning">
                                📁 Archived
                            </div>
                        )}
                        <div className="space-y-2">
                            <h1 className="text-3xl font-bold text-base-content leading-tight">
                                {ticket.name}
                            </h1>
                            <p className="text-base-content/70 text-lg leading-relaxed max-w-3xl">
                                {ticket.description}
                            </p>
                        </div>
                    </div>
                    <div className="flex gap-3 items-center flex-shrink-0 ml-6">
                        {/* Only show edit button if user has edit permissions and ticket is not archived */}
                        {canEdit && !ticket.isArchived && (
                            <button
                                onClick={handleEditToggle}
                                className="btn btn-soft btn-sm gap-2 shadow-sm"
                            >
                                <span className="material-symbols-outlined text-sm">edit</span>
                                Edit Details
                            </button>
                        )}
                        {/* Show archive/unarchive button based on permissions */}
                        {(canArchive && !ticket.isArchived) || (canUnarchive && ticket.isArchived) ? (
                            <Form method="post" action={`/tickets/${ticketId}/archive`} onSubmit={handleArchiveClick}>
                                <input type="text" name="projectId" defaultValue={ticket.projectId} className="hidden" hidden aria-hidden />
                                <button
                                    type="submit"
                                    name="intent"
                                    value={ticket.isArchived ? "unarchive" : "archive"}
                                    className={`btn btn-sm gap-2 shadow-sm ${ticket.isArchived ? 'btn-success' : 'btn-warning'}`}
                                >
                                    <span className="material-symbols-outlined text-sm">folder</span>
                                    {ticket.isArchived ? "Unarchive" : "Archive"}
                                </button>
                            </Form>
                        ) : null}
                        {/* Only show delete button if user has delete permissions */}
                        {canDelete && (
                            <Form method="post" action={`/tickets/${ticketId}/delete`}>
                                <input type="text" name="projectId" defaultValue={ticket.projectId} className="hidden" hidden aria-hidden />
                                <button
                                    type="submit"
                                    className="btn btn-sm btn-error gap-2 shadow-sm"
                                >
                                    <span className="material-symbols-outlined text-sm">delete</span>
                                    Delete
                                </button>
                            </Form>
                        )}
                    </div>
                </div>
            </div>

            {/* Developer Modal */}
            <DeveloperListModal
                modalRef={developersModalRef}
                members={(getDevelopersFetcher.data as JsonResponseResult<BasicUserInfo[]>)?.data}
                currentMember={ticket.assignee}
                actionFetcher={assignDeveloperFetcher}
                actionFetcherSubmit={(formData) => {
                    assignDeveloperFetcher.submit(formData, {
                        method: "post",
                        action: `/tickets/${ticketId}/update-dev`
                    })
                }}
            />


            {/* Ticket Stats Section */}
            <div className="p-6">
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                    <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                        <div className="text-sm font-medium text-base-content/70 uppercase tracking-wide mb-3">
                            Submitter
                        </div>
                        {ticket.submitter && (
                            <div className="flex gap-3 items-center">
                                <div className="avatar">
                                    <div className="w-10 rounded-full ring-2 ring-base-300/50">
                                        <img src={ticket.submitter.avatarUrl} alt="Submitter" />
                                    </div>
                                </div>
                                <div className="text-base font-semibold text-base-content">
                                    {ticket.submitter.name}
                                </div>
                            </div>
                        )}
                    </div>

                    <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                        <div className="text-sm font-medium text-base-content/70 uppercase tracking-wide mb-3">
                            Priority
                        </div>
                        <div className="text-base font-semibold">
                            {updatePriorityFetcher.state === "submitting" ? (
                                <span className="loading loading-spinner loading-sm"></span>
                            ) : (
                                <select
                                    onChange={(e) => {
                                        const formData = new FormData();
                                        formData.append("priority", e.currentTarget.value)
                                        updatePriorityFetcher.submit(formData, {
                                            method: "post",
                                            action: `/tickets/${ticketId}/update-priority`,
                                        })
                                    }}
                                    onClick={(e) => {
                                        if (!canUpdatePriorityField) {
                                            e.preventDefault();
                                            handleDisabledFieldClick("priority");
                                        }
                                    }}
                                    className="select shadow-none border-none p-0 text-base font-semibold w-full focus:outline-none"
                                    value={ticket.priority}
                                    disabled={!canUpdatePriorityField}
                                    title={!canUpdatePriorityField ? "You don't have permission to update priority" : ""}
                                >
                                    <option value="Low">🟢 Low</option>
                                    <option value="Medium">🟡 Medium</option>
                                    <option value="High">🟠 High</option>
                                    <option value="Urgent">🔴 Urgent</option>
                                </select>
                            )}
                        </div>
                    </div>

                    <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                        <div className="text-sm font-medium text-base-content/70 uppercase tracking-wide mb-3">
                            Status
                        </div>
                        <div className="text-base font-semibold">
                            {updateStatusFetcher.state === "submitting" ? (
                                <span className="loading loading-spinner loading-sm"></span>
                            ) : (
                                <select
                                    onChange={(e) => {
                                        const formData = new FormData();
                                        formData.append("status", e.target.value)
                                        updateStatusFetcher.submit(formData, {
                                            method: "post",
                                            action: `/tickets/${ticketId}/update-status`,
                                        })
                                    }}
                                    onClick={(e) => {
                                        if (!canUpdateStatusField) {
                                            e.preventDefault();
                                            handleDisabledFieldClick("status");
                                        }
                                    }}
                                    name="status"
                                    className="select shadow-none border-none p-0 text-base font-semibold w-full focus:outline-none"
                                    value={ticket.status}
                                    disabled={!canUpdateStatusField}
                                    title={!canUpdateStatusField ? "You don't have permission to update status" : ""}
                                >
                                    <option value="New">🆕 New</option>
                                    <option value="In Development">⚙️ In Development</option>
                                    <option value="Testing">🧪 Testing</option>
                                    <option value="Resolved">✅ Resolved</option>
                                </select>
                            )}
                        </div>
                    </div>

                    <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                        <div className="text-sm font-medium text-base-content/70 uppercase tracking-wide mb-3">
                            Type
                        </div>
                        <div className="text-base font-semibold">
                            {updateTypeFetcher.state === "submitting" ? (
                                <span className="loading loading-spinner loading-sm"></span>
                            ) : (
                                <select
                                    onChange={(e) => {
                                        const formData = new FormData();
                                        formData.append("type", e.target.value)
                                        updateTypeFetcher.submit(formData, {
                                            method: "post",
                                            action: `/tickets/${ticketId}/update-type`,
                                        })
                                    }}
                                    onClick={(e) => {
                                        if (!canUpdateTypeField) {
                                            e.preventDefault();
                                            handleDisabledFieldClick("type");
                                        }
                                    }}
                                    name="type"
                                    className="select shadow-none border-none p-0 text-base font-semibold w-full focus:outline-none"
                                    value={ticket.type}
                                    disabled={!canUpdateTypeField}
                                    title={!canUpdateTypeField ? "You don't have permission to update type" : ""}
                                >
                                    <option value="Defect">🐛 Defect</option>
                                    <option value="Feature">✨ Feature</option>
                                    <option value="General Task">📋 General Task</option>
                                    <option value="Change Request">🔄 Change Request</option>
                                    <option value="Work Task">💼 Work Task</option>
                                    <option value="Enhancement">⚡ Enhancement</option>
                                </select>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Developer Assignment Section */}
            <div className="p-6 border-b border-base-300/30">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-semibold text-base-content">Assigned Developer</h3>
                    {canAssign && (
                        <div className="flex gap-2">
                            <button
                                onClick={() => handleOnGetDevelopers()}
                                className="btn btn-soft btn-sm gap-2 shadow-sm"
                                title="Assign Developer"
                            >
                                <span className="material-symbols-outlined text-sm">person_add</span>
                                Assign
                            </button>
                            {ticket.assignee && (
                                <button
                                    onClick={() => handleOnUnassignDeveloper()}
                                    className="btn btn-soft btn-sm gap-2 shadow-sm"
                                    title="Unassign Developer"
                                >
                                    <span className="material-symbols-outlined text-sm">person_remove</span>
                                    Unassign
                                </button>
                            )}
                        </div>
                    )}
                </div>
                {ticket.assignee ? (
                    <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                        <div className="flex gap-3 items-center">
                            <div className="avatar">
                                <div className="w-10 rounded-full ring-2 ring-base-300/50">
                                    <img src={ticket.assignee.avatarUrl} alt="Assigned Developer" />
                                </div>
                            </div>
                            <div className="text-base font-semibold text-base-content">
                                {ticket.assignee.name}
                            </div>
                        </div>
                    </div>
                ) : (
                    <div className="bg-base-50 rounded-lg p-4 border border-base-300/30">
                        <div className="text-base font-medium text-base-content/50">
                            Not assigned
                        </div>
                    </div>
                )}
            </div>
        </>
    )


    return (
        <RouteLayout>
            {ticket ? (
                <div className="flex flex-col gap-4">
                    <div className="flex justify-between items-center mb-2">
                        <BackButton />
                    </div>
                    <div className="bg-base-100 rounded-xl shadow-sm border border-base-300/50 overflow-hidden">
                        {isEditing ? (
                            <div className="p-6">
                                <EditModeForm
                                    error={formError}
                                    isSubmitting={navigation.state === "submitting"}
                                    onCancel={handleEditToggle}
                                >
                                    <div className="form-control mb-4">
                                        <label className="label">
                                            <span className="label-text">Ticket Name</span>
                                        </label>
                                        <input
                                            type="text"
                                            name="name"
                                            className="input input-bordered w-full"
                                            defaultValue={ticket.name}
                                            required
                                            maxLength={50}
                                            disabled={!canEdit}
                                        />
                                    </div>

                                    <div className="form-control mb-4">
                                        <label className="label">
                                            <span className="label-text">Description</span>
                                        </label>
                                        <textarea
                                            name="description"
                                            className="textarea textarea-bordered w-full"
                                            defaultValue={ticket.description}
                                            rows={4}
                                            required
                                            maxLength={1000}
                                            disabled={!canEdit}
                                        ></textarea>
                                    </div>

                                    <div className="flex gap-3">
                                        <div className="flex flex-col gap-1">
                                            <label className="label" htmlFor="priority">Priority</label>
                                            <select
                                                name="priority"
                                                className="select w-max"
                                                defaultValue={ticket.priority}
                                                disabled={!canUpdatePriorityField}
                                            >
                                                <option value="Low">🟢 Low</option>
                                                <option value="Medium">🟡 Medium</option>
                                                <option value="High">🟠 High</option>
                                                <option value="Urgent">🔴 Urgent</option>
                                            </select>
                                        </div>
                                        <div className="flex flex-col gap-1">
                                            <label className="label" htmlFor="status">Status</label>
                                            <select
                                                name="status"
                                                className="select w-max"
                                                defaultValue={ticket.status}
                                                disabled={!canUpdateStatusField}
                                            >
                                                <option value="New">🆕 New</option>
                                                <option value="In Development">⚙️ In Development</option>
                                                <option value="Testing">🧪 Testing</option>
                                                <option value="Resolved">✅ Resolved</option>
                                            </select>
                                        </div>
                                        <div className="flex flex-col gap-1">
                                            <label className="label" htmlFor="type">Type</label>
                                            <select
                                                name="type"
                                                className="select w-max"
                                                defaultValue={ticket.type}
                                                disabled={!canUpdateTypeField}
                                            >
                                                <option value="Defect">🐛 Defect</option>
                                                <option value="Feature">✨ Feature</option>
                                                <option value="General Task">📋 General Task</option>
                                                <option value="Change Request">🔄 Change Request</option>
                                                <option value="Work Task">💼 Work Task</option>
                                                <option value="Enhancement">⚡ Enhancement</option>
                                            </select>
                                        </div>
                                    </div>
                                </EditModeForm>
                            </div>
                        ) : (<>{TicketDetails}</>)}
                    </div>

                    {/* Comments and Attachments Side by Side */}
                    <div className="grid grid-cols-1 2xl:grid-cols-2 gap-4">
                        <div className="bg-base-100 rounded-xl shadow-sm border border-base-300/50 p-6">
                            <h2 className="text-xl font-bold mb-4">Comments</h2>
                            <div className={`max-w-full flex flex-col justify-end h-[450px]`}>
                                <ChatBox
                                    className={`p-4 flex flex-col w-full ${(getCommentsFetcher.state !== "loading" && (!(getCommentsFetcher.data as any)?.data || (getCommentsFetcher.data as any)?.data?.length === 0)) ? 'flex-1 justify-center' : 'col-reverse max-h-[450px] overflow-y-auto'}`}
                                    onDeleteComment={handleOnDeleteComment}
                                    onEditComment={handleOnEditComment}
                                    comments={(getCommentsFetcher.data as any)?.data}
                                    loading={getCommentsFetcher.state === "loading"}
                                    userId={userInfo.memberId}
                                    userRole={userInfo.role}
                                    isArchived={ticket.isArchived}
                                />

                                <Form
                                    className="mt-4"
                                    method="post"
                                    navigate={false}
                                    fetcherKey="create-comment"
                                    ref={commentsFormRef}
                                    action={`/tickets/${ticketId}/create-comment`}>
                                    <div className="flex gap-2">
                                        <button 
                                            type="submit" 
                                            className="btn btn-primary"
                                            disabled={!canComment}
                                        >
                                            Send
                                        </button>
                                        <textarea
                                            placeholder={canComment ? "Message" : "Cannot comment on archived tickets"}
                                            name="message"
                                            className="textarea w-full resize-none field-sizing-content min-h-auto"
                                            disabled={!canComment}
                                        />
                                    </div>
                                </Form>
                            </div>
                        </div>

                        <div className="bg-base-100 rounded-xl shadow-sm border border-base-300/50 p-6">
                            <h2 className="text-xl font-bold mb-4">Attachments</h2>
                            <AttachmentSection
                                ticketId={ticketId!}
                                userInfo={userInfo}
                                ticket={ticket}
                            />
                        </div>
                    </div>

                    <div className="bg-base-100 rounded-xl shadow-sm border border-base-300/50 p-6">
                        <div className="flex items-center justify-between mb-4">
                            <h2 className="text-xl font-bold">History</h2>
                            <button
                                onClick={() => getHistoryFetcher.load(`/tickets/${ticketId}/get-history`)}
                                disabled={getHistoryFetcher.state === "loading"}
                                className="btn btn-sm btn-ghost btn-circle shadow-sm"
                                title="Refresh history"
                            >
                                {getHistoryFetcher.state === "loading" ? (
                                    <span className="loading loading-spinner loading-sm"></span>
                                ) : (
                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                                    </svg>
                                )}
                            </button>
                        </div>
                        <div className="w-2xl">
                            <TicketTimeline
                                history={(getHistoryFetcher.data as any)?.data || []}
                                loading={getHistoryFetcher.state === "loading"}
                            />
                        </div>
                    </div>
                </div>
            ) : (
                <div className="flex justify-center items-center h-full">
                    <p>Loading ticket details...</p>
                </div>
            )}

            {/* Archive Warning Modal */}
            <ArchiveWarningModal
                isOpen={showArchiveWarning}
                onClose={() => setShowArchiveWarning(false)}
                title="Cannot Unarchive Ticket"
                message="Cannot unarchive this ticket because its project is archived. Please unarchive the project first before unarchiving individual tickets."
            />
        </RouteLayout>
    );
}

export async function action({ request, params }: ActionFunctionArgs) {
    const session = await getSession(request);
    const userRole = session.get("user").role;

    if (!validateRole(userRole, permissions.ticket.create)) {
        return ForbiddenResponse();
    }
    const { data: tokenResponse, error: tokenError } = await tryCatch(
        apiClient.auth.getValidToken(session),
    );

    if (tokenError) {
        return redirect("/logout");
    }

    const ticketId = params.ticketId!;
    const formData = await request.formData();
    const name = formData.get("name") as string;
    const description = formData.get("description") as string;
    const priority = formData.get("priority") as string;
    const status = formData.get("status") as string;
    const type = formData.get("type") as string;

    const ticketData = {
        name,
        description,
        priority,
        status,
        type
    };

    const { error } = await tryCatch(
        updateTicket(ticketId, ticketData, tokenResponse.token),
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


