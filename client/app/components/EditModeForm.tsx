import {Form} from "@remix-run/react";
import {ReactNode} from "react";
import {projectPriorityOptions} from "~/data/selectOptions";
import {FormFieldHeader} from "./FormShell";
import SelectControl from "./SelectControl";

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
            {error ? (
                <div
                    className="mb-4 rounded-2xl bg-[var(--app-error-container)]/20 px-4 py-3 text-sm text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10">
                    {error}
                </div>
            ) : null}

            {children}

            <div className="flex justify-end gap-2 mt-4">
                <button
                    type="button"
                    className="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] outline outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)] disabled:cursor-not-allowed disabled:opacity-60"
                    onClick={onCancel}
                    disabled={isSubmitting}
                >
                    Cancel
                </button>
                <button
                    type="submit"
                    className="inline-flex items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2.5 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
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
    return (
        <div>
            <FormFieldHeader label="Priority" required={required}/>
            <SelectControl
                className={className}
                controlSize="md"
                name="priority"
                onChange={(e) => onChange(e.target.value)}
                required={required}
                value={value}
            >
                <option value="">Select priority</option>
                {projectPriorityOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                        {option.label}
                    </option>
                ))}
            </SelectControl>
        </div>
    );
}
