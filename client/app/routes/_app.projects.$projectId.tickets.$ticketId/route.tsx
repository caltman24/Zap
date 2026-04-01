import { type ActionFunctionArgs, type LoaderFunctionArgs, redirect } from "@remix-run/node";
import { Form, Link, useActionData, useFetcher, useLoaderData, useNavigation, useOutletContext, useParams } from "@remix-run/react";
import { useEffect, useRef, useState, type FormEvent } from "react";
import RouteLayout from "~/layouts/RouteLayout";
import SelectControl from "~/components/SelectControl";
import { FormFieldHeader, formInputClassName, formTextareaClassName } from "~/components/FormShell";
import BackButton from "~/components/BackButton";
import ArchiveWarningModal from "~/components/ArchiveWarningModal";
import DropdownMenu from "~/components/DropdownMenu";
import { ticketPriorityOptions, ticketStatusOptions, ticketTypeOptions } from "~/data/selectOptions";
import apiClient from "~/services/api.server/apiClient";
import { ApiError, AuthenticationError } from "~/services/api.server/errors";
import type { ActionResponseParams, JsonResponseResult } from "~/utils/response";
import { ActionResponse, ForbiddenResponse, JsonResponse } from "~/utils/response";
import { getSession } from "~/services/sessions.server";
import type { BasicTicketInfo, BasicUserInfo, UserInfoResponse } from "~/services/api.server/types";
import { useEditMode } from "~/utils/editMode";
import tryCatch from "~/utils/tryCatch";
import { getTicketById } from "./server.get-ticket";
import DeveloperListModal from "./DeveloperListModal";
import updateTicket from "./server.update-ticket";
import ChatBox from "./ChatBox";
import TicketTimeline from "./TicketTimeline";
import AttachmentSection from "./AttachmentSection";
import {
  getTicketPriorityDotClass,
  getTicketStatusChipClass,
  getTicketTypeChipClass,
} from "~/components/ticketTableUtils";

type InlineToast = {
    message: string;
    tone: "success" | "error" | "warning";
};

const panelClass = "rounded-[1.75rem] bg-[var(--app-surface-container-low)] outline outline-1 outline-[var(--app-outline-variant-soft)]";
const secondaryButtonClass =
  "inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] outline outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]";
const primaryButtonClass =
  "inline-flex items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2.5 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95";
const inlineSelectClass = "h-auto border-transparent bg-transparent px-0 pr-8 font-medium text-[var(--app-on-surface)] focus:border-transparent";

function PersonIdentity({
  label,
  person,
  fallback,
}: {
  label: string;
  person?: BasicUserInfo | null;
  fallback: string;
}) {
  const displayName = person?.name ?? fallback;

  return (
    <div className="space-y-3 border-l-2 border-[var(--app-primary-fixed-strong)] pl-4">
      <dt className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">{label}</dt>
      <dd className="flex items-center gap-3">
        {person?.avatarUrl ? (
          <img
            alt={displayName}
            className="h-10 w-10 rounded-full border border-[var(--app-outline-variant)]/20 object-cover"
            src={person.avatarUrl}
          />
        ) : (
          <span className="inline-flex h-10 w-10 items-center justify-center rounded-full bg-[var(--app-surface-container-high)] text-sm font-semibold text-[var(--app-outline)]">
            {displayName.slice(0, 1).toUpperCase()}
          </span>
        )}
        <span className="text-sm font-medium text-[var(--app-on-surface)]">{displayName}</span>
      </dd>
    </div>
  );
}

export const handle = {
    breadcrumb: (match: any) => {
        const ticketId = match.params.ticketId; const ticketName = match.data?.data?.name || "Ticket Details";
        return <Link to={`/tickets/mytickets/${ticketId}`}>{ticketName}</Link>;
    },
    breadcrumbLabel: (match: any) => match.data?.data?.name || "Ticket Details",
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
        if (error instanceof ApiError) {
            const message = error.status === 403
                ? "You do not have permission to view this ticket."
                : error.status === 404
                    ? "Ticket not found."
                    : error.message;

            return JsonResponse({
                data: null,
                error: message,
                headers: tokenResponse.headers
            });
        }
        return JsonResponse({
            data: null,
            error: error.message, headers: tokenResponse.headers
        });
    }
}

export default function TicketDetailsRoute() {
    const { data: ticket, error } = useLoaderData<JsonResponseResult<BasicTicketInfo>>();
    const { ticketId } = useParams();
    const actionData = useActionData() as ActionResponseParams
    const { isEditing, formError, toggleEditMode } = useEditMode({ actionData });
    const { userInfo } = useOutletContext<{ loaderData: any; userInfo: UserInfoResponse }>();

    const navigation = useNavigation()

    // State for archive warning modal
    const [showArchiveWarning, setShowArchiveWarning] = useState(false);
    const [toast, setToast] = useState<InlineToast | null>(null);
    const toastTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

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

    const showToast = (message: string, tone: InlineToast["tone"] = "success") => {
        setToast({ message, tone });

        if (toastTimeoutRef.current) {
            clearTimeout(toastTimeoutRef.current);
        }

        toastTimeoutRef.current = setTimeout(() => {
            setToast(null);
            toastTimeoutRef.current = null;
        }, 1800);
    };

    useEffect(() => {
        getCommentsFetcher.load(`/tickets/${ticketId}/get-comments`)
        getHistoryFetcher.load(`/tickets/${ticketId}/get-history`)
    }, [])

    useEffect(() => {
        return () => {
            if (toastTimeoutRef.current) {
                clearTimeout(toastTimeoutRef.current);
            }
        };
    }, []);

    useEffect(() => {
        if (getCommentsFetcher.state === "idle" && getCommentsFetcher.data) {
            commentsFormRef.current?.reset();
        }
    }, [getCommentsFetcher.state, getCommentsFetcher.data])

    // Show success feedback for status/priority/type updates
    useEffect(() => {
        const response = updateStatusFetcher.data as ActionResponseParams | undefined;
        if (updateStatusFetcher.state === "idle" && response) {
            if (response.success) {
                showToast("Status updated", "success");
            } else if (response.error) {
                showToast(response.error, "error");
            }
        }
    }, [updateStatusFetcher.state, updateStatusFetcher.data]);

    useEffect(() => {
        const response = updatePriorityFetcher.data as ActionResponseParams | undefined;
        if (updatePriorityFetcher.state === "idle" && response) {
            if (response.success) {
                showToast("Priority updated", "success");
            } else if (response.error) {
                showToast(response.error, "error");
            }
        }
    }, [updatePriorityFetcher.state, updatePriorityFetcher.data]);

    useEffect(() => {
        const response = updateTypeFetcher.data as ActionResponseParams | undefined;
        if (updateTypeFetcher.state === "idle" && response) {
            if (response.success) {
                showToast("Type updated", "success");
            } else if (response.error) {
                showToast(response.error, "error");
            }
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

    if (error) {
        return (
            <RouteLayout>
                <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
                    {error}
                </div>
            </RouteLayout>
        );
    }

    if (!ticket) {
        return (
            <RouteLayout>
                <div className="rounded-[1.5rem] bg-[var(--app-surface-container-low)] p-6 text-[var(--app-error)] outline outline-1 outline-[var(--app-outline-variant-soft)]">
                    Ticket details could not be loaded.
                </div>
            </RouteLayout>
        );
    }

    const handleArchiveClick = (e: FormEvent) => {
        if (ticket.isArchived && ticket.projectIsArchived) {
            e.preventDefault();
            setShowArchiveWarning(true);
        }
    };

    const canEdit = ticket.capabilities.canEditDetails;
    const canUpdateStatusField = ticket.capabilities.canUpdateStatus;
    const canUpdatePriorityField = ticket.capabilities.canUpdatePriority;
    const canUpdateTypeField = ticket.capabilities.canUpdateType;
    const canAssign = ticket.capabilities.canAssignDeveloper;
    const canDelete = ticket.capabilities.canDelete;
    const canArchive = ticket.capabilities.canArchive;
    const canUnarchive = ticket.capabilities.canUnarchive;
    const canComment = ticket.capabilities.canComment;
    const canEditNameDescriptionField = ticket.capabilities.canEditNameDescription;
    const hasToolbarActions = canEdit || canDelete || canArchive || canUnarchive;

    // Handler for disabled field clicks
    const handleDisabledFieldClick = (fieldName: string) => {
        if (ticket.isArchived) {
            showToast(`Cannot edit ${fieldName} while ticket is archived`, "warning");
        } else {
            showToast(`You do not have permission to edit ${fieldName}`, "warning");
        }
    };

    const priorityDisplay = (
        <div className="flex items-center gap-2 text-sm font-medium text-[var(--app-on-surface)]">
            <span className={`h-2.5 w-2.5 rounded-full ${getTicketPriorityDotClass(ticket.priority)}`} />
            {ticket.priority}
        </div>
    );

    const statusDisplay = (
        <span className={`app-shell-mono inline-flex rounded-md px-2.5 py-1 text-[10px] uppercase tracking-[0.2em] ${getTicketStatusChipClass(ticket.status)}`}>
            {ticket.status}
        </span>
    );

    const typeDisplay = (
        <span className={`inline-flex rounded-md px-2 py-1 text-[10px] font-medium ${getTicketTypeChipClass(ticket.type)}`}>
            {ticket.type}
        </span>
    );

    return (
        <RouteLayout className="space-y-8">
            {toast ? (
                <div className="fixed left-1/2 top-20 z-50 -translate-x-1/2">
                    <div
                        className={`rounded-2xl px-4 py-3 text-sm font-medium shadow-lg backdrop-blur-md ${
                            toast.tone === "success"
                                ? "bg-emerald-500/15 text-emerald-200 outline outline-1 outline-emerald-500/15"
                                : toast.tone === "error"
                                  ? "bg-[var(--app-error-container)]/35 text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10"
                                  : "bg-[var(--app-tertiary-container)]/25 text-[var(--app-tertiary)] outline outline-1 outline-[var(--app-tertiary)]/10"
                        }`}
                    >
                        {toast.message}
                    </div>
                </div>
            ) : null}

            <div className="flex flex-wrap items-start justify-between gap-3">
                <BackButton />

                {!isEditing && hasToolbarActions ? (
                    <>
                        <DropdownMenu
                            className="sm:hidden"
                            menuClassName="min-w-56"
                            triggerAriaLabel="Open actions menu"
                            triggerClassName={secondaryButtonClass}
                            trigger={
                                <>
                                    <span className="material-symbols-outlined text-lg">more_horiz</span>
                                    Actions
                                </>
                            }
                        >
                            {({ close }) => (
                                <>
                                {canEdit ? (
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

                                {(canArchive && !ticket.isArchived) || (canUnarchive && ticket.isArchived) ? (
                                    <Form action={`/tickets/${ticketId}/archive`} method="post" onSubmit={(event) => {
                                        close();
                                        handleArchiveClick(event);
                                    }}>
                                        <input aria-hidden className="hidden" defaultValue={ticket.projectId} hidden name="projectId" type="text" />
                                        <button
                                            className={`flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm font-medium transition-colors ${
                                                ticket.isArchived
                                                    ? "text-[var(--app-success)] hover:bg-emerald-500/10"
                                                    : "text-[var(--app-tertiary)] hover:bg-[var(--app-tertiary-container)]/15"
                                            }`}
                                            name="intent"
                                            type="submit"
                                            value={ticket.isArchived ? "unarchive" : "archive"}
                                        >
                                            <span className="material-symbols-outlined text-lg">folder</span>
                                            <span>{ticket.isArchived ? "Unarchive" : "Archive"}</span>
                                        </button>
                                    </Form>
                                ) : null}

                                {canDelete ? (
                                    <Form action={`/tickets/${ticketId}/delete`} method="post" onSubmit={close}>
                                        <input aria-hidden className="hidden" defaultValue={ticket.projectId} hidden name="projectId" type="text" />
                                        <button
                                            className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm font-medium text-[var(--app-error)] transition-colors hover:bg-[var(--app-error-container)]/15"
                                            type="submit"
                                        >
                                            <span className="material-symbols-outlined text-lg">delete</span>
                                            <span>Delete</span>
                                        </button>
                                    </Form>
                                ) : null}
                                </>
                            )}
                        </DropdownMenu>

                        <div className="hidden flex-wrap items-center justify-end gap-3 sm:flex">
                            {canEdit ? (
                                <button className={`${secondaryButtonClass} cursor-pointer`} onClick={handleEditToggle} type="button">
                                    <span className="material-symbols-outlined text-lg">edit</span>
                                    Edit Details
                                </button>
                            ) : null}

                            {(canArchive && !ticket.isArchived) || (canUnarchive && ticket.isArchived) ? (
                                <Form action={`/tickets/${ticketId}/archive`} method="post" onSubmit={handleArchiveClick}>
                                    <input aria-hidden className="hidden" defaultValue={ticket.projectId} hidden name="projectId" type="text" />
                                    <button
                                        className={`inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium transition-colors ${
                                            ticket.isArchived
                                                ? "text-[var(--app-success)] outline outline-1 outline-[var(--app-success)]/15 hover:bg-emerald-500/10"
                                                : "text-[var(--app-tertiary)] outline outline-1 outline-[var(--app-tertiary)]/15 hover:bg-[var(--app-tertiary-container)]/15"
                                        }`}
                                        name="intent"
                                        type="submit"
                                        value={ticket.isArchived ? "unarchive" : "archive"}
                                    >
                                        <span className="material-symbols-outlined text-lg">folder</span>
                                        {ticket.isArchived ? "Unarchive" : "Archive"}
                                    </button>
                                </Form>
                            ) : null}

                            {canDelete ? (
                                <Form action={`/tickets/${ticketId}/delete`} method="post">
                                    <input aria-hidden className="hidden" defaultValue={ticket.projectId} hidden name="projectId" type="text" />
                                    <button
                                        className="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/15 transition-colors hover:bg-[var(--app-error-container)]/15"
                                        type="submit"
                                    >
                                        <span className="material-symbols-outlined text-lg">delete</span>
                                        Delete
                                    </button>
                                </Form>
                            ) : null}
                        </div>
                    </>
                ) : null}
            </div>

            <DeveloperListModal
                actionFetcher={assignDeveloperFetcher}
                actionFetcherSubmit={(formData) => {
                    assignDeveloperFetcher.submit(formData, {
                        method: "post",
                        action: `/tickets/${ticketId}/update-dev`,
                    });
                }}
                currentMember={ticket.assignee ?? undefined}
                members={(getDevelopersFetcher.data as JsonResponseResult<BasicUserInfo[]>)?.data}
                modalRef={developersModalRef}
            />

            <section className="overflow-hidden border-b border-[var(--app-outline-variant)]/10 pb-8">
                {isEditing ? (
                    <div className="space-y-6 p-6 sm:p-8">
                        <div className="border-b border-[var(--app-outline-variant)]/10 pb-6">
                            <h1 className="text-3xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">Edit Ticket</h1>
                            <p className="mt-1 max-w-2xl text-sm text-[var(--app-on-surface-variant)] sm:text-base">
                                Update the core ticket details shown to the team.
                            </p>
                        </div>

                        <Form className="space-y-8" method="post">
                            {formError ? (
                                <div className="rounded-2xl bg-[var(--app-error-container)]/20 px-4 py-3 text-sm text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10">
                                    {formError}
                                </div>
                            ) : null}

                            <div className="grid gap-6">
                                <div>
                                    <FormFieldHeader label="Ticket Name" required />
                                    <input
                                        className={formInputClassName}
                                        defaultValue={ticket.name}
                                        disabled={!canEditNameDescriptionField}
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
                                        defaultValue={ticket.description}
                                        disabled={!canEditNameDescriptionField}
                                        maxLength={1000}
                                        name="description"
                                        required
                                        rows={5}
                                    />
                                </div>

                                <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
                                    <div>
                                        <FormFieldHeader label="Priority" required />
                                        <SelectControl controlSize="md" defaultValue={ticket.priority} disabled={!canUpdatePriorityField} name="priority">
                                            {ticketPriorityOptions.map((option) => (
                                                <option key={option.value} value={option.value}>
                                                    {option.label}
                                                </option>
                                            ))}
                                        </SelectControl>
                                    </div>

                                    <div>
                                        <FormFieldHeader label="Status" required />
                                        <SelectControl controlSize="md" defaultValue={ticket.status} disabled={!canUpdateStatusField} name="status">
                                            {ticketStatusOptions.map((option) => (
                                                <option key={option.value} value={option.value}>
                                                    {option.label}
                                                </option>
                                            ))}
                                        </SelectControl>
                                    </div>

                                    <div>
                                        <FormFieldHeader label="Type" required />
                                        <SelectControl controlSize="md" defaultValue={ticket.type} disabled={!canUpdateTypeField} name="type">
                                            {ticketTypeOptions.map((option) => (
                                                <option key={option.value} value={option.value}>
                                                    {option.label}
                                                </option>
                                            ))}
                                        </SelectControl>
                                    </div>
                                </div>
                            </div>

                            <div className="flex justify-end gap-3 border-t border-[var(--app-outline-variant)]/10 pt-5">
                                <button className={`${secondaryButtonClass} cursor-pointer`} disabled={navigation.state === "submitting"} onClick={handleEditToggle} type="button">
                                    Cancel
                                </button>
                                <button
                                    className="inline-flex min-w-36 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-5 py-3 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
                                    disabled={navigation.state === "submitting"}
                                    type="submit"
                                >
                                    {navigation.state === "submitting" ? "Saving..." : "Save Changes"}
                                </button>
                            </div>
                        </Form>
                    </div>
                ) : (
                    <>
                        <div className="space-y-6 px-6 pb-0 pt-6 sm:px-8 sm:pt-8">
                            <div className="min-w-0 max-w-4xl space-y-4">
                                {ticket.isArchived ? (
                                    <p className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-archive)]">Archived</p>
                                ) : null}

                                <div className="space-y-3">
                                    <h1 className="text-3xl font-bold tracking-[-0.04em] text-[var(--app-on-surface)] sm:text-[2.2rem]">
                                        {ticket.name}
                                    </h1>
                                    <p className="max-w-4xl text-sm leading-6 text-[var(--app-on-surface-variant)] sm:text-base sm:leading-7">
                                        {ticket.description}
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className="mt-8 border-t border-[var(--app-outline-variant)]/10 bg-[var(--app-surface-container-lowest)]/30 px-6 py-6 sm:px-8">
                            <dl className="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-4 xl:gap-5">
                                <PersonIdentity fallback="Unknown submitter" label="Submitter" person={ticket.submitter} />

                                <div className="space-y-3 border-l-2 border-[var(--app-tertiary)] pl-4">
                                    <dt className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Priority</dt>
                                    <dd>
                                        {updatePriorityFetcher.state === "submitting" ? (
                                            <span className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-[var(--app-outline)] border-r-transparent align-middle" />
                                        ) : canUpdatePriorityField ? (
                                            <SelectControl
                                                className={inlineSelectClass}
                                                controlSize="sm"
                                                onChange={(event) => {
                                                    const formData = new FormData();
                                                    formData.append("priority", event.currentTarget.value);
                                                    updatePriorityFetcher.submit(formData, {
                                                        method: "post",
                                                        action: `/tickets/${ticketId}/update-priority`,
                                                    });
                                                }}
                                                value={ticket.priority}
                                            >
                                                {ticketPriorityOptions.map((option) => (
                                                    <option key={option.value} value={option.value}>
                                                        {option.label}
                                                    </option>
                                                ))}
                                            </SelectControl>
                                        ) : (
                                            priorityDisplay
                                        )}
                                    </dd>
                                </div>

                                <div className="space-y-3 border-l-2 border-[var(--app-primary-fixed)] pl-4">
                                    <dt className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Status</dt>
                                    <dd>
                                        {updateStatusFetcher.state === "submitting" ? (
                                            <span className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-[var(--app-outline)] border-r-transparent align-middle" />
                                        ) : canUpdateStatusField ? (
                                            <SelectControl
                                                className={inlineSelectClass}
                                                controlSize="sm"
                                                onChange={(event) => {
                                                    const formData = new FormData();
                                                    formData.append("status", event.currentTarget.value);
                                                    updateStatusFetcher.submit(formData, {
                                                        method: "post",
                                                        action: `/tickets/${ticketId}/update-status`,
                                                    });
                                                }}
                                                value={ticket.status}
                                            >
                                                {ticketStatusOptions.map((option) => (
                                                    <option key={option.value} value={option.value}>
                                                        {option.label}
                                                    </option>
                                                ))}
                                            </SelectControl>
                                        ) : (
                                            statusDisplay
                                        )}
                                    </dd>
                                </div>

                                <div className="space-y-3 border-l-2 border-[var(--app-secondary)] pl-4">
                                    <dt className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Type</dt>
                                    <dd>
                                        {updateTypeFetcher.state === "submitting" ? (
                                            <span className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-[var(--app-outline)] border-r-transparent align-middle" />
                                        ) : canUpdateTypeField ? (
                                            <SelectControl
                                                className={inlineSelectClass}
                                                controlSize="sm"
                                                onChange={(event) => {
                                                    const formData = new FormData();
                                                    formData.append("type", event.currentTarget.value);
                                                    updateTypeFetcher.submit(formData, {
                                                        method: "post",
                                                        action: `/tickets/${ticketId}/update-type`,
                                                    });
                                                }}
                                                value={ticket.type}
                                            >
                                                {ticketTypeOptions.map((option) => (
                                                    <option key={option.value} value={option.value}>
                                                        {option.label}
                                                    </option>
                                                ))}
                                            </SelectControl>
                                        ) : (
                                            typeDisplay
                                        )}
                                    </dd>
                                </div>
                            </dl>
                        </div>

                        <div className="border-t border-[var(--app-outline-variant)]/10 px-6 py-6 sm:px-8">
                            <div className="flex flex-wrap items-center justify-between gap-4">
                                <div>
                                    <h2 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Assigned Developer</h2>
                                    <p className="mt-1 text-sm text-[var(--app-on-surface-variant)] sm:text-base">
                                        Ownership and active implementation responsibility for this ticket.
                                    </p>
                                </div>

                                {canAssign ? (
                                    <div className="flex flex-wrap items-center gap-3">
                                        <button className={`${secondaryButtonClass} cursor-pointer`} onClick={() => handleOnGetDevelopers()} type="button">
                                            <span className="material-symbols-outlined text-lg">person_add</span>
                                            Assign
                                        </button>
                                        {ticket.assignee ? (
                                            <button className={`${secondaryButtonClass} cursor-pointer`} onClick={() => handleOnUnassignDeveloper()} type="button">
                                                <span className="material-symbols-outlined text-lg">person_remove</span>
                                                Unassign
                                            </button>
                                        ) : null}
                                    </div>
                                ) : null}
                            </div>

                            <div className="mt-5 rounded-2xl bg-[var(--app-surface-container-lowest)]/70 px-5 py-4 outline outline-1 outline-[var(--app-outline-variant)]/10">
                                {ticket.assignee ? (
                                    <div className="flex items-center gap-3">
                                        <img
                                            alt="Assigned Developer"
                                            className="h-11 w-11 rounded-full border border-[var(--app-outline-variant)]/20 object-cover"
                                            src={ticket.assignee.avatarUrl}
                                        />
                                        <div>
                                            <p className="app-shell-mono text-[10px] uppercase tracking-[0.22em] text-[var(--app-outline)]">Assigned Developer</p>
                                            <p className="text-base font-semibold text-[var(--app-on-surface)]">{ticket.assignee.name}</p>
                                        </div>
                                    </div>
                                ) : (
                                    <div className="text-sm text-[var(--app-on-surface-variant)]">Not assigned</div>
                                )}
                            </div>
                        </div>
                    </>
                )}
            </section>

            <div className="grid grid-cols-1 gap-6 2xl:grid-cols-2">
                <section className={`${panelClass} p-6`}>
                    <div className="mb-4 border-b border-[var(--app-outline-variant)]/10 pb-4">
                        <h2 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Comments</h2>
                    </div>
                    <div className="flex h-[450px] max-w-full flex-col justify-end">
                        <ChatBox
                            className={`w-full p-4 ${(getCommentsFetcher.state !== "loading" && (!(getCommentsFetcher.data as any)?.data || (getCommentsFetcher.data as any)?.data?.length === 0)) ? "flex-1 justify-center" : "col-reverse max-h-[450px] overflow-y-auto"}`}
                            comments={(getCommentsFetcher.data as any)?.data}
                            loading={getCommentsFetcher.state === "loading"}
                            onDeleteComment={handleOnDeleteComment}
                            onEditComment={handleOnEditComment}
                            userId={userInfo.memberId ?? ""}
                        />

                        <Form action={`/tickets/${ticketId}/create-comment`} className="mt-4 border-t border-[var(--app-outline-variant)]/10 pt-4" fetcherKey="create-comment" method="post" navigate={false} ref={commentsFormRef}>
                            <div className="flex items-start gap-3">
                                <button
                                    className="inline-flex min-w-24 items-center justify-center rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-3 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
                                    disabled={!canComment}
                                    type="submit"
                                >
                                    Send
                                </button>
                                <textarea
                                    className="min-h-[3.25rem] w-full resize-none rounded-xl border border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-lowest)] px-4 py-3 text-sm text-[var(--app-on-surface)] outline-none transition-colors placeholder:text-[var(--app-outline)] focus:border-[var(--app-primary-fixed)]"
                                    disabled={!canComment}
                                    name="message"
                                    placeholder={canComment ? "Message" : "You cannot comment on this ticket"}
                                />
                            </div>
                        </Form>
                    </div>
                </section>

                <section className={`${panelClass} p-6`}>
                    <div className="mb-4 border-b border-[var(--app-outline-variant)]/10 pb-4">
                        <h2 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">Attachments</h2>
                    </div>
                    <AttachmentSection ticket={ticket} ticketId={ticketId!} userInfo={userInfo} />
                </section>
            </div>

            <section className={`${panelClass} p-6`}>
                <div className="mb-4 flex items-center justify-between border-b border-[var(--app-outline-variant)]/10 pb-4">
                    <div>
                        <h2 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">History</h2>
                        <p className="mt-1 text-sm text-[var(--app-on-surface-variant)]">A running record of edits and assignment changes.</p>
                    </div>
                    <button
                        className={`${secondaryButtonClass} cursor-pointer px-3 py-2`}
                        disabled={getHistoryFetcher.state === "loading"}
                        onClick={() => getHistoryFetcher.load(`/tickets/${ticketId}/get-history`)}
                        title="Refresh history"
                        type="button"
                    >
                        {getHistoryFetcher.state === "loading" ? (
                            <span className="inline-flex h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent" />
                        ) : (
                            <span className="material-symbols-outlined text-lg">refresh</span>
                        )}
                    </button>
                </div>

                <TicketTimeline history={(getHistoryFetcher.data as any)?.data || []} loading={getHistoryFetcher.state === "loading"} />
            </section>

            <ArchiveWarningModal
                isOpen={showArchiveWarning}
                message="Cannot unarchive this ticket because its project is archived. Please unarchive the project first before unarchiving individual tickets."
                onClose={() => setShowArchiveWarning(false)}
                title="Cannot Unarchive Ticket"
            />
        </RouteLayout>
    );
}

export async function action({ request, params }: ActionFunctionArgs) {
    const session = await getSession(request);
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
