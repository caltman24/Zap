import type { TicketHistoryEntry } from "./server.get-ticket-history";

interface TicketTimelineProps {
  history: TicketHistoryEntry[];
  loading?: boolean;
}

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

function getHistoryIconMeta(type: number) {
  switch (type) {
    case HistoryTypes.Created:
      return { icon: "check_circle", className: "bg-emerald-500/15 text-emerald-300" };
    case HistoryTypes.UpdateName:
    case HistoryTypes.UpdateDescription:
      return { icon: "edit_note", className: "bg-sky-500/15 text-sky-300" };
    case HistoryTypes.UpdateStatus:
    case HistoryTypes.Resolved:
      return { icon: "task_alt", className: "bg-[var(--app-tertiary-container)]/25 text-[var(--app-tertiary)]" };
    case HistoryTypes.UpdateType:
    case HistoryTypes.UpdatePriority:
      return { icon: "tune", className: "bg-[var(--app-secondary-container)]/30 text-[var(--app-secondary)]" };
    case HistoryTypes.Archived:
    case HistoryTypes.Unarchived:
      return { icon: "inventory_2", className: "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]" };
    case HistoryTypes.DeveloperAssigned:
    case HistoryTypes.DeveloperRemoved:
      return { icon: "person", className: "bg-[var(--app-primary-fixed)]/15 text-[var(--app-primary)]" };
    default:
      return { icon: "info", className: "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]" };
  }
}

function formatDate(dateString: string) {
  const date = new Date(dateString);
  const now = new Date();
  const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

  if (diffInHours < 24) {
    return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
  }

  if (diffInHours < 24 * 7) {
    return date.toLocaleDateString([], { month: "short", day: "numeric" });
  }

  return date.toLocaleDateString([], { month: "short", day: "numeric", year: "numeric" });
}

function removeCreatorNamePrefix(message: string, creatorName: string): string {
  const escapedName = creatorName.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  const creatorNamePattern = new RegExp(escapedName, "gi");
  const withoutName = message.replace(creatorNamePattern, "");

  return withoutName.replace(/^\s*[-:,.]?\s*/, "").replace(/\s+by\s*$/i, "").replace(/\s{2,}/g, " ").trim();
}

export default function TicketTimeline({ history, loading = false }: TicketTimelineProps) {
  if (loading) {
      return (
        <div className="grid min-h-[220px] place-items-center py-8">
        <span className="inline-flex h-5 w-5 animate-spin rounded-full border-2 border-[var(--app-outline)] border-r-transparent" />
        </div>
      );
  }

  if (!history || history.length === 0) {
    return (
      <div className="py-10 text-center text-[var(--app-on-surface-variant)]">
        <span className="material-symbols-outlined mb-3 text-5xl text-[var(--app-outline)]">history</span>
        <p className="text-base">No history available for this ticket.</p>
      </div>
    );
  }

  return (
    <div className="max-h-[30rem] overflow-y-auto pr-1">
      <div className="space-y-5">
        {history.map((entry, index) => {
          const iconMeta = getHistoryIconMeta(entry.type);
          const showConnector = index < history.length - 1;

          return (
            <div className="relative flex gap-4" key={entry.id}>
              {showConnector ? (
                <div className="absolute bottom-0 left-5 top-12 w-px bg-[color:var(--app-outline-variant)]/12" />
              ) : null}

              <div className={`z-10 flex h-10 w-10 shrink-0 items-center justify-center rounded-full ${iconMeta.className}`}>
                <span className="material-symbols-outlined text-lg">{iconMeta.icon}</span>
              </div>

              <div className="min-w-0 flex-1 space-y-2 rounded-2xl bg-[var(--app-surface-container-lowest)]/70 px-4 py-4 outline outline-1 outline-[var(--app-outline-variant)]/10">
                <div className="flex flex-wrap items-center gap-2 text-xs text-[var(--app-outline)]">
                  <span className="app-shell-mono uppercase tracking-[0.2em]">{formatDate(entry.createdAt)}</span>
                  <span className="h-1 w-1 rounded-full bg-[var(--app-outline)]/50" />
                  <span>{entry.creator.name}</span>
                  <span className="rounded-full bg-[var(--app-surface-container-high)] px-2 py-0.5 text-[10px] text-[var(--app-on-surface-variant)]">
                    {entry.creator.role}
                  </span>
                </div>

                <p className="text-sm leading-6 text-[var(--app-on-surface)]">
                  {removeCreatorNamePrefix(entry.formattedMessage, entry.creator.name)}
                </p>
              </div>
            </div>
          );
        })}
      </div>

      {history.length > 5 ? (
        <div className="pt-4 text-center">
          <span className="app-shell-mono text-[10px] uppercase tracking-[0.2em] text-[var(--app-outline)]">
            {history.length} total events
          </span>
        </div>
      ) : null}
    </div>
  );
}
