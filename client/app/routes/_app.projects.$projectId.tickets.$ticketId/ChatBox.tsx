import { useEffect, useRef } from "react";
import { TicketComment } from "~/services/api.server/types";
import { convertTo12HourTime } from "~/utils/dateTime";

type ChatBoxProps = {
    className?: string,
    comments?: TicketComment[]
    userId: string
    loading: boolean
}

export default function ChatBox({ className, comments, userId, loading }: ChatBoxProps) {
    const scrollableChatContainerRef = useRef<HTMLDivElement>(null)

    useEffect(() => {
        const container = scrollableChatContainerRef.current;
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }, []);

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
                    <div className={`chat ${css}`} key={c.id}>
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
                        <div className="chat-bubble whitespace-pre-line">{c.message}</div>
                        {c.updatedAt && (
                            <div className="chat-footer opacity-50">Edited at: [value]</div>
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
