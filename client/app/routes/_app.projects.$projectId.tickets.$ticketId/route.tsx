
import { ActionFunctionArgs, LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useFetcher, useLoaderData, useNavigate, useNavigation, useOutletContext, useParams } from "@remix-run/react"; import RouteLayout from "~/layouts/RouteLayout";
import apiClient from "~/services/api.server/apiClient"; import { AuthenticationError } from "~/services/api.server/errors";
import { getSession } from "~/services/sessions.server"; import { ActionResponse, ActionResponseParams, ActionResponseResult, ForbiddenResponse, JsonResponse, JsonResponseResult } from "~/utils/response";
import tryCatch from "~/utils/tryCatch";
import { getTicketById } from "./server.get-ticket";
import BackButton from "~/components/BackButton";
import { useEffect, useRef } from "react";
import DeveloperListModal from "./DeveloperListModal";
import { BasicUserInfo, UserInfoResponse } from "~/services/api.server/types";
import { useEditMode } from "~/utils/editMode";
import { EditModeForm } from "~/components/EditModeForm";
import { validateRole } from "~/utils/validate";
import permissions from "~/data/permissions";
import updateTicket from "./server.update-ticket";
import ChatBox from "./ChatBox";
import TicketTimeline from "./TicketTimeline";
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
    const { _, userInfo } = useOutletContext<any>();

    const navigation = useNavigation()

    const updatePriorityFetcher = useFetcher({ key: "update-priority" })
    const updateStatusFetcher = useFetcher({ key: "update-status" })
    const updateTypeFetcher = useFetcher({ key: "update-type" })

    const getDevelopersFetcher = useFetcher({ key: "get-devs" })
    const assignDeveloperFetcher = useFetcher({ key: "update-dev" })

    const commentsFormRef = useRef<HTMLFormElement>(null);

    const deleteCommentFetcher = useFetcher({ key: "delete-comment" })
    const updateCommentFetcher = useFetcher({ key: "update-comment" })
    const getCommentsFetcher = useFetcher({ key: "get-comments" })
    useEffect(() => {
        getCommentsFetcher.load(`/tickets/${ticketId}/get-comments`)
    }, [])
    useEffect(() => {
        if (getCommentsFetcher.state === "idle" && getCommentsFetcher.data) {
            commentsFormRef.current?.reset();
        }
    }, [getCommentsFetcher.state, getCommentsFetcher.data])

    // Refresh comments after successful edit or delete
    useEffect(() => {
        if (updateCommentFetcher.state === "idle" && updateCommentFetcher.data) {
            getCommentsFetcher.load(`/tickets/${ticketId}/get-comments`)
        }
    }, [updateCommentFetcher.state, updateCommentFetcher.data])

    useEffect(() => {
        if (deleteCommentFetcher.state === "idle" && deleteCommentFetcher.data) {
            getCommentsFetcher.load(`/tickets/${ticketId}/get-comments`)
        }
    }, [deleteCommentFetcher.state, deleteCommentFetcher.data])


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

    // TODO: Update permissions

    // TODO: Add confirm modal on delete

    if (error) {
        return <p className="text-error">{error}</p>;
    }

    const TicketDetails = (
        <>
            <div className="flex justify-between items-center">
                <div>
                    {ticket.isArchived &&
                        <div className="badge badge-warning font-medium mb-2">Archived</div>
                    }
                    <div className="flex gap-2 items-center">
                        <h1 className="text-3xl font-bold mb-2">{ticket.name}</h1>
                        {true && (
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
                                                Ticket Details
                                            </a>
                                        </li>
                                        {true && (
                                            <>
                                                {true && (
                                                    <>
                                                        <li>
                                                            <a onClick={() => handleOnGetDevelopers()}>
                                                                <span className="material-symbols-outlined">person_add</span>
                                                                Assign Developer
                                                            </a>
                                                        </li>
                                                        {ticket.assignee && (
                                                            <li>
                                                                <a onClick={() => handleOnUnassignDeveloper()}>
                                                                    <span className="material-symbols-outlined">person_remove</span>
                                                                    Unassign Developer
                                                                </a>
                                                            </li>
                                                        )}
                                                        <li>
                                                            <Form method="post" className="block hover:bg-warning/10 hover:text-warning" action={`/tickets/${ticketId}/archive`}>
                                                                <input type="text" name="projectId" defaultValue={ticket.projectId} className="hidden" hidden aria-hidden />
                                                                <button type="submit" name="intent" defaultValue={ticket.isArchived ? "unarchive" : "archive"} className="flex items-center text-left gap-2 cursor-pointer w-full">
                                                                    <span className={`material-symbols-outlined `}>folder</span>
                                                                    <p className="w-full">
                                                                        {ticket.isArchived ? "Unarchive" : "Archive"}
                                                                    </p>
                                                                </button>
                                                            </Form>
                                                        </li>
                                                        <li>
                                                            <Form method="post" action={`/tickets/${ticketId}/delete`} className="block hover:text-error hover:bg-error/10">
                                                                <button type="submit" className="flex items-center text-left gap-2 cursor-pointer w-full">
                                                                    <input type="text" name="projectId" defaultValue={ticket.projectId} className="hidden" hidden aria-hidden />
                                                                    <span className={`material-symbols-outlined`}>delete</span>
                                                                    <p className="w-full">Delete</p>
                                                                </button>
                                                            </Form>
                                                        </li>
                                                    </>
                                                )}
                                            </>
                                        )}
                                    </ul>
                                    {true && (
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
                                    )}
                                </div>
                                {/* Archive/Unarchive Project button */}
                            </>
                        )}
                    </div>
                </div>
            </div>


            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-4">
                <div className="stat bg-base-200 rounded-lg">
                    <div className="stat-title mb-2">Submitter</div>
                    {ticket.submitter && (
                        <div className="flex gap-2 items-center">
                            <div className="avatar">
                                <div className="w-9 rounded-full">

                                    <img src={ticket.submitter.avatarUrl} />
                                </div>
                            </div>
                            <div className="stat-value text-lg font-bold">
                                {ticket.submitter.name}
                            </div>
                        </div>
                    )}
                </div>
                <div className="stat bg-base-200 rounded-lg">
                    <div className="stat-title">Priority</div>
                    <div
                        className={`stat-value text-lg ${getPriorityClass(
                            ticket.priority
                        )}`}
                    >
                        {updatePriorityFetcher.state === "submitting" ? (
                            <span className="loading loading-spinner"></span>
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
                                className="bg-base-200 w-full"
                                value={ticket.priority}>
                                <option value="Low">Low</option>
                                <option value="Medium">Medium</option>
                                <option value="High">High</option>
                                <option value="Urgent">Urgent</option>
                            </select>
                        )}
                    </div>
                </div>
                <div className="stat bg-base-200 rounded-lg">

                    <div className="stat-title">Status</div>
                    <div
                        className={`stat-value text-lg ${getStatusClass(
                            ticket.status
                        )}`}
                    >
                        {updateStatusFetcher.state === "submitting" ? (
                            <span className="loading loading-spinner"></span>
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
                                name="status"
                                className="bg-base-200 w-full"
                                value={ticket.status}>
                                <option value="New">New</option>
                                <option value="In Development">In Development</option>
                                <option value="Testing">Testing</option>
                                <option value="Resolved">Resolved</option>
                            </select>
                        )
                        }
                    </div>
                </div>
                <div className="stat bg-base-200 rounded-lg">
                    <div className="stat-title">Type</div>
                    <div className="stat-value text-lg font-bold">
                        {updateTypeFetcher.state === "submitting" ? (
                            <span className="loading loading-spinner"></span>
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
                                name="type"
                                className="bg-base-200 w-full"
                                value={ticket.type}>
                                <option value="Defect">Defect</option>
                                <option value="Feature">Feature</option>
                                <option value="General Task">General Task</option>
                                <option value="Change Request">Change Request</option>
                                <option value="Work Task">Work Task</option>
                                <option value="Enhancement">Enhancement</option>
                            </select>
                        )
                        }
                    </div>
                </div>
            </div>

            <div className="mt-6">
                <h2 className="text-lg font-bold mb-2">Developer</h2>
                {ticket.assignee ? (
                    <div className="flex gap-2 items-center">
                        <div className="avatar">
                            <div className="w-9 rounded-full">
                                <img src={ticket.assignee.avatarUrl} />
                            </div>
                        </div>
                        <div className="font-bold">{ticket.assignee.name} </div>
                    </div>
                ) : (
                    <p className="font-medium text-gray-400">Unassigned</p>
                )}
            </div>

            <div className="flex flex-col mt-4">
                <p className="text-lg font-medium">Description:</p>
                <p className="">{ticket.description}</p>
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
                    <div className="bg-base-100 rounded-lg shadow-lg p-6">
                        {isEditing ? (
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
                                    ></textarea>
                                </div>

                                <div className="flex gap-3">
                                    <div className="flex flex-col gap-1">
                                        <label className="label" htmlFor="priority">Description</label>
                                        <select
                                            name="priority"
                                            className="select w-max"
                                            defaultValue={ticket.priority}>
                                            <option value="Low">Low</option>
                                            <option value="Medium">Medium</option>
                                            <option value="High">High</option>
                                            <option value="Urgent">Urgent</option>
                                        </select>
                                    </div>
                                    <div className="flex flex-col gap-1">
                                        <label className="label" htmlFor="status">Status</label>
                                        <select
                                            name="status"
                                            className="select w-max"
                                            defaultValue={ticket.status}>
                                            <option value="New">New</option>
                                            <option value="In Development">In Development</option>
                                            <option value="Testing">Testing</option>
                                            <option value="Resolved">Resolved</option>
                                        </select>
                                    </div>
                                    <div className="flex flex-col gap-1">
                                        <label className="label" htmlFor="type">Type</label>
                                        <select
                                            name="type"
                                            className="select w-max"
                                            defaultValue={ticket.type}>
                                            <option value="Defect">Defect</option>
                                            <option value="Feature">Feature</option>
                                            <option value="General Task">General Task</option>
                                            <option value="Change Request">Change Request</option>
                                            <option value="Work Task">Work Task</option>
                                            <option value="Enhancement">Enhancement</option>
                                        </select>
                                    </div>
                                </div>
                            </EditModeForm>
                        ) : (<>{TicketDetails}</>)}
                    </div>

                    <div className="bg-base-100 rounded-lg shadow-lg p-6">
                        <h2 className="text-xl font-bold mb-4">Comments</h2>
                        <div className="max-w-6xl">
                            <ChatBox
                                className="p-4 flex flex-col w-full max-h-[600px] overflow-y-auto"
                                onDeleteComment={handleOnDeleteComment}
                                onEditComment={handleOnEditComment}
                                comments={(getCommentsFetcher.data as any)?.data}
                                loading={getCommentsFetcher.state === "loading"}
                                userId={userInfo.memberId} />

                            <Form
                                className="mt-4"
                                method="post"
                                navigate={false}
                                fetcherKey="create-comment"
                                ref={commentsFormRef}
                                action={`/tickets/${ticketId}/create-comment`}>
                                <div className="flex gap-2">
                                    <button type="submit" className="btn btn-primary">Send</button>
                                    <textarea
                                        placeholder="Message"
                                        name="message"
                                        className="textarea w-full resize-none field-sizing-content min-h-auto" />
                                </div>
                            </Form>
                        </div>
                    </div>

                    <div className="bg-base-100 rounded-lg shadow-lg p-6">
                        <h2 className="text-xl font-bold mb-4">Attachments</h2>
                    </div>

                    <div className="bg-base-100 rounded-lg shadow-lg p-6">
                        <h2 className="text-xl font-bold mb-4">History</h2>
                        <div className="max-w-lg">
                            <TicketTimeline />
                        </div>
                    </div>
                </div>
            ) : (
                <div className="flex justify-center items-center h-full">
                    <p>Loading ticket details...</p>
                </div>
            )}
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
// Helper function to get badge color based on priority
function getPriorityClass(priority: string): string {
    switch (priority?.toLowerCase()) {
        case 'high': return 'text-error';
        case 'medium': return 'text-warning';
        case 'low': return 'text-info';
        default: return '';
    }
}
// Helper function to get badge color based on status
function getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
        case 'open': return 'text-info';
        case 'in progress': return 'text-warning';
        case 'resolved': return 'text-success';
        case 'closed': return 'text-neutral';
        default: return '';
    }
}

