import { useEffect, useRef, useState } from "react";
import { Form } from "@remix-run/react";
import { TicketComment } from "~/services/api.server/types";
import { convertTo12HourTime, formatDateHeader, isSameDay, isToday } from "~/utils/dateTime";
import { canEditComment, canDeleteComment } from "~/utils/ticketPermissions";
import roleNames, { type RoleName } from "~/data/roles";

type ChatBoxProps = {
    className?: string,
    comments?: TicketComment[]
    userId: string
    userRole: RoleName
    isArchived: boolean
    loading: boolean
    onDeleteComment: (commentId: string) => void
    onEditComment: (commentId: string, message: string) => void
}

export default function ChatBox({ className, comments, userId, userRole, isArchived, loading, onDeleteComment, onEditComment }: ChatBoxProps) {
    const scrollableChatContainerRef = useRef<HTMLDivElement>(null)
    const [editingCommentId, setEditingCommentId] = useState<string | null>(null)
    const [editMessage, setEditMessage] = useState("")

    useEffect(() => {
        const container = scrollableChatContainerRef.current;
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }, [comments]);

    const handleEditClick = (commentId: string, currentMessage: string) => {
        setEditingCommentId(commentId)
        setEditMessage(currentMessage)
    }

    const handleCancelEdit = () => {
        setEditingCommentId(null)
        setEditMessage("")
    }

    const handleSaveEdit = (commentId: string) => {
        if (editMessage.trim()) {
            onEditComment(commentId, editMessage.trim())
            setEditingCommentId(null)
            setEditMessage("")
        }
    }

    const commentsList = (
        <>
            {comments && comments.length > 0 ? (
                comments.map((c, index) => {
                    const css = c.sender.id === userId ? "chat-end" : "chat-start";
                    const createdAtDate = new Date(c.createdAt);
                    const convertedCreatedAt = convertTo12HourTime(createdAtDate)
                    const formatedCreatedAt = `${convertedCreatedAt.hours}:${convertedCreatedAt.minutes} ${convertedCreatedAt.meridiem}`

                    let formatedEditedTime = ""
                    if (c.updatedAt) {
                        const editedAtDate = new Date(c.updatedAt)
                        const convertedEditedTime = convertTo12HourTime(editedAtDate)
                        formatedEditedTime = `${convertedEditedTime.hours}:${convertedEditedTime.minutes} ${convertedEditedTime.meridiem}`
                    }

                    // Check if we need to show a date header
                    let showDateHeader = false;
                    if (index === 0) {
                        // Always show date header for the first message if it's not today
                        showDateHeader = !isToday(createdAtDate);
                    } else {
                        // Show date header if this message is on a different day than the previous message
                        const previousMessageDate = new Date(comments[index - 1].createdAt);
                        showDateHeader = !isSameDay(createdAtDate, previousMessageDate);
                    }

                    return (
                        <div key={c.id}>
                            {showDateHeader && (
                                <div className="flex justify-center my-4">
                                    <div className="bg-base-200 px-3 py-1 rounded-full text-sm text-base-content/70">
                                        {isToday(createdAtDate) ? "Today" : formatDateHeader(createdAtDate)}
                                    </div>
                                </div>
                            )}
                            <div className={`chat ${css} group`}>
                                <div className="chat-image avatar">
                                    <div className="w-10 rounded-full">
                                        <img
                                            alt={`${c.sender.name} picture`}
                                            src={c.sender.avatarUrl}
                                        />
                                    </div>
                                </div>
                                <div className="chat-header">
                                    {c.sender.name}
                                    <time className="text-xs opacity-50">{formatedCreatedAt}</time>
                                </div>
                                <div className="flex gap-2">
                                    {/* Check permissions before showing edit/delete icons */}
                                    {(canEditComment(userRole, c.sender.id === userId, isArchived) || 
                                      canDeleteComment(userRole, c.sender.id === userId, isArchived)) && (
                                        <div className="flex flex-col gap-1 opacity-0 group-hover:opacity-100 transform duration-100 ease-in">
                                            {canEditComment(userRole, c.sender.id === userId, isArchived) && (
                                                <span
                                                    onClick={() => handleEditClick(c.id, c.message)}
                                                    className="material-symbols-outlined text-gray-600 hover:text-primary hover:cursor-pointer"
                                                    title="Edit comment"
                                                >
                                                    edit
                                                </span>
                                            )}
                                            {canDeleteComment(userRole, c.sender.id === userId, isArchived) && (
                                                <span
                                                    onClick={() => onDeleteComment(c.id)}
                                                    className="material-symbols-outlined text-gray-600 hover:text-error hover:cursor-pointer"
                                                    title="Delete comment"
                                                >
                                                    delete
                                                </span>
                                            )}
                                        </div>
                                    )}
                                    {editingCommentId === c.id ? (
                                        <div className="chat-bubble p-2 w-full">
                                            <textarea
                                                value={editMessage}
                                                onChange={(e) => setEditMessage(e.target.value)}
                                                className="textarea w-full resize-none field-sizing-content min-h-auto"
                                                maxLength={150}
                                                placeholder="Edit your message..."
                                                autoFocus
                                            />
                                            <div className="flex gap-2 mt-2 justify-end">
                                                <button
                                                    type="button"
                                                    onClick={handleCancelEdit}
                                                    className="btn btn-ghost btn-sm"
                                                >
                                                    Cancel
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => handleSaveEdit(c.id)}
                                                    className="btn btn-primary btn-sm"
                                                    disabled={!editMessage.trim() || editMessage.length > 150}
                                                >
                                                    Save
                                                </button>
                                            </div>
                                        </div>
                                    ) : (
                                        <div className="chat-bubble whitespace-pre-line">{c.message}</div>
                                    )}
                                </div>
                                {c.updatedAt && (
                                    <div className="chat-footer opacity-50">Edited at: {formatedEditedTime}</div>
                                )}
                            </div>
                        </div>
                    )
                })
            ) : (
                <div className="flex flex-col items-center justify-center text-center py-8">
                    <div className="text-base-content/50 mb-2">
                        <span className="material-symbols-outlined text-4xl">mail</span>
                    </div>
                    <p className="text-base-content/70 text-lg font-medium">No comments yet</p>
                    <p className="text-base-content/50 text-sm">Start the conversation by adding a comment below</p>
                </div>
            )}
        </>
    )

    return (
        <div ref={scrollableChatContainerRef} className={className}>
            {loading ? (
                <div className="w-full h-auto grid place-items-center">
                    <span className="loading loading-spinner loading-lg">Loading</span>
                </div>
            ) : commentsList}
        </div>
    )
}
