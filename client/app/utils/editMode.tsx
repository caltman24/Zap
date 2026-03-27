import { useState, useEffect } from "react";
import { ActionResponseResult } from "./response";

const statusChipBaseClass = "inline-flex rounded-md px-2.5 py-1 text-[10px] font-semibold uppercase tracking-[0.2em]";

interface EditModeOptions {
  initialState?: boolean;
  onSuccess?: () => void;
  resetOnSuccess?: boolean;
  actionData?: ActionResponseResult
}

export function useEditMode({
  initialState = false,
  onSuccess,
  resetOnSuccess = true,
  actionData
}: EditModeOptions = {}) {
  const [isEditing, setIsEditing] = useState(initialState);
  const [formError, setFormError] = useState<string | null>(null);

  const enableEditMode = () => {
    setIsEditing(true);
    setFormError(null);
  };

  const disableEditMode = () => {
    setIsEditing(false);
    setFormError(null);
  };

  const toggleEditMode = () => {
    setIsEditing(!isEditing);
    setFormError(null);
  };

  const setError = (error: string) => {
    setFormError(error);
  };

  const handleActionSuccess = () => {
    if (resetOnSuccess) {
      setIsEditing(false);
    }
    if (onSuccess) {
      onSuccess();
    }
  };

  useEffect(() => {
    if (actionData?.success) {
      handleActionSuccess()
    }
    if (actionData?.error) {
      setFormError(actionData.error);
    }
  }, [actionData]);

  return {
    isEditing,
    formError,
    enableEditMode,
    disableEditMode,
    toggleEditMode,
    setError,
    handleActionSuccess
  };
}

export function getPriorityTextClass(value: string): string {
  switch (value?.toLowerCase()) {
    case 'low':
      return 'text-[var(--app-success)]';
    case 'medium':
      return 'text-[var(--app-tertiary)]';
    case 'high':
      return 'text-[var(--app-error)]';
    case 'urgent':
      return 'text-[var(--app-error)] font-bold';
    default:
      return '';
  }
}

export function getPriorityClass(priority: string): string {
  switch (priority?.toLowerCase()) {
    case "high":
      return "text-[var(--app-error)]";
    case "medium":
      return "text-[var(--app-tertiary)]";
    case "low":
      return "text-[var(--app-success)]";
    case "urgent":
      return "text-[var(--app-error)] font-bold";
    default:
      return "text-[var(--app-on-surface-variant)]";
  }
}

export function getStatusClass(status: string): string {
  switch (status?.toLowerCase()) {
    case "open":
      return `${statusChipBaseClass} bg-[var(--app-secondary-container)]/40 text-[var(--app-secondary)]`;
    case "in progress":
      return `${statusChipBaseClass} bg-[var(--app-tertiary-container)]/25 text-[var(--app-tertiary)]`;
    case "resolved":
    case "completed":
      return `${statusChipBaseClass} bg-[var(--app-success)]/15 text-[var(--app-success)]`;
    case "closed":
      return `${statusChipBaseClass} bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]`;
    default:
      return `${statusChipBaseClass} bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]`;
  }
}
