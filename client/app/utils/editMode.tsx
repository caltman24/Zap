import { useState, useEffect } from "react";
import { ActionResponseResult } from "./response";

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
      return 'text-info';
    case 'medium':
      return 'text-warning';
    case 'high':
      return 'text-error';
    case 'urgent':
      return 'text-error font-bold';
    default:
      return '';
  }
}

export function getPriorityClass(priority: string): string {
  switch (priority?.toLowerCase()) {
    case "high":
      return "text-error";
    case "medium":
      return "text-warning";
    case "low":
      return "text-info";
    case "urgent":
      return "text-error font-bold";
    default:
      return "text-neutral";
  }
}

export function getStatusClass(status: string): string {
  switch (status?.toLowerCase()) {
    case "open":
      return "badge-primary";
    case "in progress":
      return "badge-warning";
    case "resolved":
    case "completed":
      return "badge-success";
    case "closed":
      return "badge-neutral";
    default:
      return "badge-neutral";
  }
}
