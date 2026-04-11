import type {BasicUserInfo} from "~/services/api.server/types";
import {getTicketInitials} from "./ticketTableUtils";

type TicketAssigneeProps = {
    person: BasicUserInfo | null;
    fallbackLabel?: string;
    size?: "sm" | "md";
    showLabel?: boolean;
};

const sizeStyles = {
    sm: {
        wrapper: "gap-2.5",
        avatar: "h-6 w-6 text-[10px]",
        text: "text-sm",
        label: "text-[10px]",
    },
    md: {
        wrapper: "gap-2.5",
        avatar: "h-7 w-7 text-[10px]",
        text: "text-xs",
        label: "text-[10px]",
    },
} as const;

export default function TicketAssignee({
                                           person,
                                           fallbackLabel = "Unassigned",
                                           size = "sm",
                                           showLabel = false,
                                       }: TicketAssigneeProps) {
    const displayName = person?.name ?? fallbackLabel;
    const styles = sizeStyles[size];

    return (
        <div className={`flex items-center ${styles.wrapper}`}>
            {person?.avatarUrl ? (
                <img
                    alt={displayName}
                    className={`${styles.avatar} rounded-full border border-[var(--app-outline-variant)]/20 object-cover`}
                    src={person.avatarUrl}
                />
            ) : (
                <span
                    className={`inline-flex ${styles.avatar} items-center justify-center rounded-full bg-[var(--app-surface-container-high)] font-bold text-[var(--app-outline)]`}
                >
          {getTicketInitials(displayName)}
        </span>
            )}

            <div className="min-w-0">
                {showLabel ? (
                    <p className={`app-shell-mono ${styles.label} uppercase tracking-[0.2em] text-[var(--app-outline)]`}>Assignee</p>
                ) : null}
                <p className={`truncate ${styles.text} text-[var(--app-on-surface)]`}>{displayName}</p>
            </div>
        </div>
    );
}
