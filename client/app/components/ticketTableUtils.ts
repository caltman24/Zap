export function compareTicketStrings(left: string, right: string): number {
  return left.localeCompare(right, undefined, { sensitivity: "base" });
}

export function truncateTicketText(value: string | null | undefined, maxLength = 72): string {
  if (!value?.trim()) {
    return "No additional description provided for this ticket yet.";
  }

  const normalized = value.trim().replace(/\s+/g, " ");

  if (normalized.length <= maxLength) {
    return normalized;
  }

  return `${normalized.slice(0, maxLength - 3)}...`;
}

export function getTicketInitials(name: string): string {
  const parts = name
    .split(" ")
    .map((part) => part.trim())
    .filter(Boolean)
    .slice(0, 2);

  if (parts.length === 0) {
    return "NA";
  }

  return parts.map((part) => part[0]?.toUpperCase() ?? "").join("");
}

export function getTicketStatusChipClass(status: string): string {
  switch (status.toLowerCase()) {
    case "resolved":
      return "bg-emerald-500/15 text-emerald-300";
    case "testing":
      return "bg-sky-500/20 text-sky-300";
    case "in development":
      return "bg-[var(--app-secondary-container)]/40 text-[var(--app-secondary)]";
    case "new":
      return "bg-[var(--app-surface-container-highest)] text-[var(--app-on-surface)]";
    default:
      return "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]";
  }
}

export function getTicketPriorityDotClass(priority: string): string {
  switch (priority.toLowerCase()) {
    case "urgent":
    case "high":
      return "bg-[var(--app-error)]";
    case "medium":
      return "bg-[var(--app-tertiary)]";
    case "low":
      return "bg-[var(--app-success)]";
    default:
      return "bg-[var(--app-outline)]";
  }
}

export function getTicketTypeChipClass(type: string): string {
  switch (type.toLowerCase()) {
    case "defect":
      return "bg-[var(--app-error-container)]/25 text-[var(--app-error)]";
    case "feature":
    case "enhancement":
      return "bg-[var(--app-secondary-container)]/30 text-[var(--app-secondary)]";
    case "change request":
      return "bg-[var(--app-tertiary-container)]/25 text-[var(--app-tertiary)]";
    case "general task":
    case "work task":
      return "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]";
    default:
      return "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]";
  }
}
