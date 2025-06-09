import { useEffect, useRef, useState } from "react";
import { Form } from "@remix-run/react";
import { TicketComment } from "~/services/api.server/types";
import { convertTo12HourTime } from "~/utils/dateTime";

type ChatBoxProps = {
    className?: string,
    comments?: TicketComment[]
    userId: string
    loading: boolean
    onDeleteComment: (commentId: string) => void
    onEditComment: (commentId: string, message: string) => void
}

export default function ChatBox({ className, comments, userId, loading, onDeleteComment, onEditComment }: ChatBoxProps) {
    const scrollableChatContainerRef = useRef<HTMLDivElement>(null)
    const [editingCommentId, setEditingCommentId] = useState<string | null>(null)
    const [editMessage, setEditMessage] = useState("")

    useEffect(() => {
        const container = scrollableChatContainerRef.current;
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }, []);

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
            {comments ? (comments.map(c => {
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

                return (
                    <div className={`chat ${css} group`} key={c.id}>
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
                            {c.sender.id === userId && (
                                <div className="flex flex-col gap-1 opacity-0 group-hover:opacity-100 transform duration-100 ease-in">
                                    <span
                                        onClick={() => handleEditClick(c.id, c.message)}
                                        className="material-symbols-outlined text-gray-600 hover:text-primary hover:cursor-pointer"
                                        title="Edit comment"
                                    >
                                        edit
                                    </span>
                                    <span
                                        onClick={() => onDeleteComment(c.id)}
                                        className="material-symbols-outlined text-gray-600 hover:text-error hover:cursor-pointer"
                                        title="Delete comment"
                                    >
                                        delete
                                    </span>
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
                )
            })) :
                <div>No Comments</div>
            }
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
