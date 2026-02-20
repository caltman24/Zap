import { TicketHistoryEntry } from "./server.get-ticket-history";

interface TicketTimelineProps {
    history: TicketHistoryEntry[];
    loading?: boolean;
}

// Enum mapping for history types
const HistoryTypes = {
    Created: 0,
    UpdateName: 1,
    UpdateDescription: 2,
    UpdateStatus: 3,
    UpdateType: 4,
    UpdatePriority: 5,
    Archived: 6,
    Unarchived: 7,
    Resolved: 8,
    DeveloperAssigned: 9,
    DeveloperRemoved: 10,
} as const;

function getHistoryIcon(type: number) {
    switch (type) {
        case HistoryTypes.Created:
            return (
                <svg className="h-5 w-5 text-success" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z" clipRule="evenodd" />
                </svg>
            );
        case HistoryTypes.UpdateName:
        case HistoryTypes.UpdateDescription:
            return (
                <svg className="h-5 w-5 text-info" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M2.695 14.763l-1.262 3.154a.5.5 0 00.65.65l3.155-1.262a4 4 0 001.343-.885L17.5 5.5a2.121 2.121 0 00-3-3L3.58 13.42a4 4 0 00-.885 1.343z" />
                </svg>
            );
        case HistoryTypes.UpdateStatus:
        case HistoryTypes.Resolved:
            return (
                <svg className="h-5 w-5 text-warning" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
            );
        case HistoryTypes.UpdateType:
        case HistoryTypes.UpdatePriority:
            return (
                <svg className="h-5 w-5 text-secondary" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clipRule="evenodd" />
                </svg>
            );
        case HistoryTypes.Archived:
        case HistoryTypes.Unarchived:
            return (
                <svg className="h-5 w-5 text-neutral" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M4 3a2 2 0 100 4h12a2 2 0 100-4H4z" />
                    <path fillRule="evenodd" d="M3 8h14v7a2 2 0 01-2 2H5a2 2 0 01-2-2V8zm5 3a1 1 0 011-1h2a1 1 0 110 2H9a1 1 0 01-1-1z" clipRule="evenodd" />
                </svg>
            );
        case HistoryTypes.DeveloperAssigned:
        case HistoryTypes.DeveloperRemoved:
            return (
                <svg className="h-5 w-5 text-primary" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clipRule="evenodd" />
                </svg>
            );
        default:
            return (
                <svg className="h-5 w-5 text-base-content" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                </svg>
            );
    }
}

function formatDate(dateString: string) {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

    if (diffInHours < 24) {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } else if (diffInHours < 24 * 7) {
        return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
    } else {
        return date.toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' });
    }
}

function removeCreatorNamePrefix(message: string, creatorName: string): string {
    const escapedName = creatorName.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const creatorNamePattern = new RegExp(escapedName, "gi");
    const withoutName = message.replace(creatorNamePattern, "");

    return withoutName
        .replace(/^\s*[-:,.]?\s*/, "")
        .replace(/\s+by\s*$/i, "")
        .replace(/\s{2,}/g, " ")
        .trim();
}

export default function TicketTimeline({ history, loading = false }: TicketTimelineProps) {
    if (loading) {
        return (
            <div className="flex justify-center items-center py-8">
                <span className="loading loading-spinner loading-md"></span>
            </div>
        );
    }

    if (!history || history.length === 0) {
        return (
            <div className="text-center py-8 text-base-content/60">
                <svg className="mx-auto h-12 w-12 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                <p>No history available for this ticket.</p>
            </div>
        );
    }

    return (
        <div className="max-h-96 overflow-y-auto scrollbar-thin scrollbar-thumb-base-300 scrollbar-track-transparent">
            <ul className="timeline timeline-vertical flex-col-reverse max-w-2xl">
                {history.map((entry, index) => (
                    <li key={entry.id} className="lg:!grid-cols-[auto_auto_auto_1fr] !grid-cols-[auto_auto_2fr_1fr]">
                        {index > 0 && <hr className="bg-base-300/50" />}
                        <div className="timeline-start text-sm text-base-content/70 font-medium min-w-20 pr-4">
                            {formatDate(entry.createdAt)}
                        </div>
                        <div className="timeline-middle bg-base-100 p-2 rounded-full border-2 border-base-300 shadow-sm">
                            {getHistoryIcon(entry.type)}
                        </div>
                        <div className="timeline-end timeline-box bg-base-100 border border-base-300 shadow-sm hover:shadow-md transition-shadow duration-200">
                            <div className="flex items-start gap-3">
                                <div className="avatar">
                                    <div className="w-8 rounded-full ring-2 ring-base-300 ring-offset-1 ring-offset-base-100">
                                        <img
                                            src={entry.creator.avatarUrl || '/default-avatar.png'}
                                            alt={entry.creator.name}
                                            className="w-8 h-8 rounded-full object-cover"
                                        />
                                    </div>
                                </div>
                                <div className="flex-1 min-w-0">
                                    <p className="text-sm font-medium text-base-content leading-relaxed">
                                        {removeCreatorNamePrefix(entry.formattedMessage, entry.creator.name)}
                                    </p>
                                    <div className="flex items-center gap-2 mt-2">
                                        <p className="text-xs text-base-content/60">
                                            by {entry.creator.name}
                                        </p>
                                        <div className="w-1 h-1 bg-base-content/30 rounded-full"></div>
                                        <span className="text-xs text-base-content/60 bg-base-200 px-2 py-0.5 rounded-full">
                                            {entry.creator.role}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        {index < history.length - 1 && <hr className="bg-base-300/50" />}
                    </li>
                ))}
            </ul>

            {/* Scroll to bottom indicator */}
            {history.length > 5 && (
                <div className="text-center mt-4 pb-2">
                    <div className="text-xs text-base-content/50 bg-base-200 inline-block px-3 py-1 rounded-full">
                        {history.length} total events
                    </div>
                </div>
            )}
        </div>
    );
}
