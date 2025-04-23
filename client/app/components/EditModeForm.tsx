import { Form } from "@remix-run/react";
import { ReactNode } from "react";

interface EditModeFormProps {
  method?: "post" | "get" | "put" | "delete";
  error?: string | null;
  isSubmitting?: boolean;
  onCancel: () => void;
  children: ReactNode;
  encType?: "application/x-www-form-urlencoded" | "multipart/form-data" | "text/plain" | undefined;
  action?: string;
}

export function EditModeForm({
  method = "post",
  error,
  isSubmitting = false,
  onCancel,
  children,
  encType,
  action
}: EditModeFormProps) {
  return (
    <Form method={method} encType={encType} action={action}>
      {error && <p className="text-error mb-4">{error}</p>}

      {children}

      <div className="flex justify-end gap-2 mt-4">
        <button
          type="button"
          className="btn btn-ghost"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancel
        </button>
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Saving..." : "Save Changes"}
        </button>
      </div>
    </Form>
  );
}

export function PrioritySelect({
  value,
  onChange,
  className = "",
  required = true
}: {
  value: string;
  onChange: (value: string) => void;
  className?: string;
  required?: boolean;
}) {
  const getPriorityTextClass = (value: string): string => {
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
  };

  return (
    <div className="form-control">
      <label className="label">
        <span className="label-text">Priority</span>
      </label>
      <div className="relative w-full">
        <select
          name="priority"
          className={`select select-bordered w-full ${getPriorityTextClass(value)} ${className}`}
          required={required}
          value={value}
          onChange={(e) => onChange(e.target.value)}
        >
          <option value="">Select priority</option>
          <option value="Low" className="text-info">Low</option>
          <option value="Medium" className="text-warning">Medium</option>
          <option value="High" className="text-error">High</option>
          <option value="Urgent" className="text-error font-bold">Urgent</option>
        </select>
      </div>
    </div>
  );
}
