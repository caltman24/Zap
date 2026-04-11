import {useEffect, useRef, useState} from "react";
import type {TicketComment} from "~/services/api.server/types";
import {convertTo12HourTime, formatDateHeader, isSameDay, isToday} from "~/utils/dateTime";

type ChatBoxProps = {
    className?: string;
    comments?: TicketComment[];
    userId: string | undefined;
    loading: boolean;
    onDeleteComment: (commentId: string) => void;
    onEditComment: (commentId: string, message: string) => void;
};

function formatCommentTime(dateValue: string) {
    const date = new Date(dateValue);
    const converted = convertTo12HourTime(date);
    return `${converted.hours}:${converted.minutes} ${converted.meridiem}`;
}

export default function ChatBox({className, comments, userId, loading, onDeleteComment, onEditComment}: ChatBoxProps) {
    const scrollableChatContainerRef = useRef<HTMLDivElement>(null);
    const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
    const [editMessage, setEditMessage] = useState("");

    useEffect(() => {
        const container = scrollableChatContainerRef.current;
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }, [comments]);

    function handleEditClick(commentId: string, currentMessage: string) {
        setEditingCommentId(commentId);
        setEditMessage(currentMessage);
    }

    function handleCancelEdit() {
        setEditingCommentId(null);
        setEditMessage("");
    }

    function handleSaveEdit(commentId: string) {
        if (!editMessage.trim()) {
            return;
        }

        onEditComment(commentId, editMessage.trim());
        setEditingCommentId(null);
        setEditMessage("");
    }

    if (loading) {
        return (
            <div className={`${className ?? ""} grid min-h-[220px] place-items-center`.trim()}
                 ref={scrollableChatContainerRef}>
                <span
                    className="inline-flex h-7 w-7 animate-spin rounded-full border-2 border-[var(--app-outline)] border-r-transparent"/>
            </div>
        );
    }

    if (!comments || comments.length === 0) {
        return (
            <div
                className={`${className ?? ""} flex min-h-[220px] flex-col items-center justify-center text-center`.trim()}
                ref={scrollableChatContainerRef}>
                <span className="material-symbols-outlined mb-3 text-5xl text-[var(--app-outline)]">forum</span>
                <p className="text-lg font-medium text-[var(--app-on-surface)]">No comments yet</p>
                <p className="mt-1 text-sm text-[var(--app-on-surface-variant)]">Start the conversation by adding a
                    comment below.</p>
            </div>
        );
    }

    return (
        <div className={className} ref={scrollableChatContainerRef}>
            <div className="space-y-5">
                {comments.map((comment, index) => {
                    const isOwnComment = Boolean(userId && comment.sender.id === userId);
                    const createdAtDate = new Date(comment.createdAt);
                    const showDateHeader =
                        index === 0
                            ? !isToday(createdAtDate)
                            : !isSameDay(createdAtDate, new Date(comments[index - 1].createdAt));

                    return (
                        <div key={comment.id}>
                            {showDateHeader ? (
                                <div className="flex justify-center py-1">
                  <span
                      className="app-shell-mono rounded-full bg-[var(--app-surface-container-high)] px-3 py-1 text-[10px] uppercase tracking-[0.2em] text-[var(--app-outline)]">
                    {isToday(createdAtDate) ? "Today" : formatDateHeader(createdAtDate)}
                  </span>
                                </div>
                            ) : null}

                            <div className={`group flex gap-3 ${isOwnComment ? "justify-end" : "justify-start"}`}>
                                {!isOwnComment ? (
                                    <img
                                        alt={`${comment.sender.name} picture`}
                                        className="h-10 w-10 shrink-0 rounded-full border border-[var(--app-outline-variant)]/20 object-cover"
                                        src={comment.sender.avatarUrl}
                                    />
                                ) : null}

                                <div
                                    className={`max-w-[82%] space-y-2 ${isOwnComment ? "items-end text-right" : "items-start text-left"}`}>
                                    <div
                                        className={`flex items-center gap-2 text-xs ${isOwnComment ? "justify-end" : "justify-start"}`}>
                                        <span
                                            className="font-medium text-[var(--app-on-surface)]">{comment.sender.name}</span>
                                        <time
                                            className="app-shell-mono text-[var(--app-outline)]">{formatCommentTime(comment.createdAt)}</time>
                                    </div>

                                    <div className={`flex gap-2 ${isOwnComment ? "justify-end" : "justify-start"}`}>
                                        {!isOwnComment && (comment.capabilities.canEdit || comment.capabilities.canDelete) ? (
                                            <div
                                                className="flex flex-col gap-1 pt-2 opacity-0 transition-opacity duration-150 group-hover:opacity-100">
                                                {comment.capabilities.canEdit ? (
                                                    <button
                                                        className="inline-flex h-7 w-7 items-center justify-center rounded-lg text-[var(--app-outline)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                                                        onClick={() => handleEditClick(comment.id, comment.message)}
                                                        title="Edit comment"
                                                        type="button"
                                                    >
                                                        <span
                                                            className="material-symbols-outlined text-base">edit</span>
                                                    </button>
                                                ) : null}
                                                {comment.capabilities.canDelete ? (
                                                    <button
                                                        className="inline-flex h-7 w-7 items-center justify-center rounded-lg text-[var(--app-outline)] transition-colors hover:bg-[var(--app-error-container)]/15 hover:text-[var(--app-error)]"
                                                        onClick={() => onDeleteComment(comment.id)}
                                                        title="Delete comment"
                                                        type="button"
                                                    >
                                                        <span
                                                            className="material-symbols-outlined text-base">delete</span>
                                                    </button>
                                                ) : null}
                                            </div>
                                        ) : null}

                                        <div
                                            className={`rounded-2xl px-4 py-3 ${
                                                isOwnComment
                                                    ? "bg-[var(--app-primary-fixed)]/15 text-[var(--app-on-surface)] outline outline-1 outline-[var(--app-primary-fixed)]/10"
                                                    : "bg-[var(--app-surface-container-lowest)] text-[var(--app-on-surface)] outline outline-1 outline-[var(--app-outline-variant)]/10"
                                            }`}
                                        >
                                            {editingCommentId === comment.id ? (
                                                <div className="w-full min-w-[16rem] space-y-3 text-left">
                          <textarea
                              autoFocus
                              className="min-h-24 w-full resize-none rounded-xl border border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-low)] px-3 py-2 text-sm text-[var(--app-on-surface)] outline-none transition-colors focus:border-[var(--app-primary-fixed)]"
                              maxLength={150}
                              onChange={(event) => setEditMessage(event.target.value)}
                              placeholder="Edit your message..."
                              value={editMessage}
                          />
                                                    <div className="flex justify-end gap-2">
                                                        <button
                                                            className="inline-flex items-center justify-center rounded-xl px-3 py-2 text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                                                            onClick={handleCancelEdit}
                                                            type="button"
                                                        >
                                                            Cancel
                                                        </button>
                                                        <button
                                                            className="inline-flex items-center justify-center rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
                                                            disabled={!editMessage.trim() || editMessage.length > 150}
                                                            onClick={() => handleSaveEdit(comment.id)}
                                                            type="button"
                                                        >
                                                            Save
                                                        </button>
                                                    </div>
                                                </div>
                                            ) : (
                                                <p className="whitespace-pre-line text-sm leading-6">{comment.message}</p>
                                            )}
                                        </div>

                                        {isOwnComment && (comment.capabilities.canEdit || comment.capabilities.canDelete) ? (
                                            <div
                                                className="flex flex-col gap-1 pt-2 opacity-0 transition-opacity duration-150 group-hover:opacity-100">
                                                {comment.capabilities.canEdit ? (
                                                    <button
                                                        className="inline-flex h-7 w-7 items-center justify-center rounded-lg text-[var(--app-outline)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                                                        onClick={() => handleEditClick(comment.id, comment.message)}
                                                        title="Edit comment"
                                                        type="button"
                                                    >
                                                        <span
                                                            className="material-symbols-outlined text-base">edit</span>
                                                    </button>
                                                ) : null}
                                                {comment.capabilities.canDelete ? (
                                                    <button
                                                        className="inline-flex h-7 w-7 items-center justify-center rounded-lg text-[var(--app-outline)] transition-colors hover:bg-[var(--app-error-container)]/15 hover:text-[var(--app-error)]"
                                                        onClick={() => onDeleteComment(comment.id)}
                                                        title="Delete comment"
                                                        type="button"
                                                    >
                                                        <span
                                                            className="material-symbols-outlined text-base">delete</span>
                                                    </button>
                                                ) : null}
                                            </div>
                                        ) : null}
                                    </div>

                                    {comment.updatedAt ? (
                                        <div
                                            className={`app-shell-mono text-[10px] text-[var(--app-outline)] ${isOwnComment ? "text-right" : "text-left"}`}>
                                            Edited at {formatCommentTime(comment.updatedAt)}
                                        </div>
                                    ) : null}
                                </div>

                                {isOwnComment ? (
                                    <img
                                        alt={`${comment.sender.name} picture`}
                                        className="h-10 w-10 shrink-0 rounded-full border border-[var(--app-outline-variant)]/20 object-cover"
                                        src={comment.sender.avatarUrl}
                                    />
                                ) : null}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
