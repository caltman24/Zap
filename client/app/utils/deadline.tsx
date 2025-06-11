import React from 'react';

export type DeadlineStatus = 'overdue' | 'due-soon' | 'normal';

// Helper function to get deadline status
export function getDeadlineStatus(dueDate: string): DeadlineStatus {
  const today = new Date();
  const deadline = new Date(dueDate);
  const diffTime = deadline.getTime() - today.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  if (diffDays < 0) {
    return 'overdue';
  } else if (diffDays <= 7) {
    return 'due-soon';
  } else {
    return 'normal';
  }
}

// Helper function to get deadline icon
export function getDeadlineIcon(dueDate: string, className: string = ""): JSX.Element {
  const status = getDeadlineStatus(dueDate);

  switch (status) {
    case 'overdue':
      return <span className={`material-symbols-outlined text-error ${className}`}>error</span>;
    case 'due-soon':
      return <span className={`material-symbols-outlined text-warning ${className}`}>schedule</span>;
    default:
      return <span className={`material-symbols-outlined text-base-content/60 ${className}`}>event</span>;
  }
}

// Helper function to get deadline text class
export function getDeadlineTextClass(dueDate: string): string {
  const status = getDeadlineStatus(dueDate);

  switch (status) {
    case 'overdue':
      return 'text-error font-semibold';
    case 'due-soon':
      return 'text-warning font-medium';
    default:
      return 'text-base-content/80 font-medium';
  }
}

// Helper function to get deadline status badge
export function getDeadlineStatusBadge(dueDate: string, variant: 'compact' | 'detailed' = 'detailed'): JSX.Element | null {
  const status = getDeadlineStatus(dueDate);
  const today = new Date();
  const deadline = new Date(dueDate);
  const diffTime = deadline.getTime() - today.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  switch (status) {
    case 'overdue':
      const overdueDays = Math.abs(diffDays);
      const overdueText = variant === 'compact'
        ? `${overdueDays}d overdue`
        : overdueDays === 1 ? '1 day overdue' : `${overdueDays} days overdue`;
      return (
        <span className="badge badge-error badge-sm font-medium shadow-sm">
          {overdueText}
        </span>
      );
    case 'due-soon':
      let dueSoonText;
      if (variant === 'compact') {
        dueSoonText = diffDays === 0 ? 'Today' : diffDays === 1 ? 'Tomorrow' : `${diffDays}d left`;
      } else {
        dueSoonText = diffDays === 0 ? 'Due today' : diffDays === 1 ? 'Due tomorrow' : `Due in ${diffDays} days`;
      }
      return (
        <span className="badge badge-warning badge-sm font-medium shadow-sm">
          {dueSoonText}
        </span>
      );
    default:
      return null;
  }
}

// Comprehensive deadline display component
interface DeadlineDisplayProps {
  dueDate: string;
  variant?: 'card' | 'detailed';
  showLabel?: boolean;
  className?: string;
}

export function DeadlineDisplay({
  dueDate,
  variant = 'detailed',
  showLabel = true,
  className = ""
}: DeadlineDisplayProps) {
  const status = getDeadlineStatus(dueDate);
  const formattedDate = new Date(dueDate).toLocaleDateString();

  if (variant === 'card') {
    return (
      <div className={`flex items-center gap-2 flex-wrap ${className}`}>
        {getDeadlineIcon(dueDate, "text-sm")}
        <span className={`text-sm ${getDeadlineTextClass(dueDate)}`}>
          {showLabel ? 'Due: ' : ''}{formattedDate}
        </span>
        {getDeadlineStatusBadge(dueDate, 'compact')}
      </div>
    );
  }

  return (
    <div className={`flex flex-col gap-2 ${className}`}>
      <div className="flex items-center gap-2">
        {getDeadlineIcon(dueDate, "text-lg")}
        <span className={`text-base ${getDeadlineTextClass(dueDate)}`}>
          {formattedDate}
        </span>
      </div>
      {getDeadlineStatusBadge(dueDate, 'detailed')}
    </div>
  );
}
