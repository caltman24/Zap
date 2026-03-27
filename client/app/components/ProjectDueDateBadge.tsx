import { getDeadlineStatus } from "~/utils/deadline";

type ProjectDueDateBadgeProps = {
  dueDate: string;
};

function getDeadlineTone(dueDate: string) {
  const deadlineStatus = getDeadlineStatus(dueDate);

  switch (deadlineStatus) {
    case "overdue":
      return {
        containerClass: "bg-[var(--app-error-container)]/20 border-[var(--app-error)]/10",
        textClass: "text-[var(--app-error)]",
      };
    case "due-soon":
      return {
        containerClass: "bg-[var(--app-tertiary-container)]/20 border-[var(--app-tertiary)]/10",
        textClass: "text-[var(--app-tertiary)]",
      };
    default:
      return {
        containerClass: "bg-[var(--app-surface-container-highest)]/30 border-white/5",
        textClass: "text-[var(--app-on-surface)]",
      };
  }
}

function formatDateParts(dateValue: string) {
  const date = new Date(dateValue);

  return {
    month: date.toLocaleDateString(undefined, { month: "short" }).toUpperCase(),
    day: date.toLocaleDateString(undefined, { day: "2-digit" }),
  };
}

export default function ProjectDueDateBadge({ dueDate }: ProjectDueDateBadgeProps) {
  const deadlineTone = getDeadlineTone(dueDate);
  const dateParts = formatDateParts(dueDate);

  return (
    <div className={`flex h-12 w-12 shrink-0 flex-col items-center justify-center rounded-xl border ${deadlineTone.containerClass}`}>
      <span className="app-shell-mono text-[10px] uppercase text-[var(--app-outline)]">{dateParts.month}</span>
      <span className={`text-base font-bold ${deadlineTone.textClass}`}>{dateParts.day}</span>
    </div>
  );
}
