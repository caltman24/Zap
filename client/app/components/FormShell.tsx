import type {ReactNode, SelectHTMLAttributes} from "react";
import SelectControl from "./SelectControl";

type FormShellProps = {
    eyebrow?: string;
    title: string;
    description: string;
    error?: string | null;
    leading?: ReactNode;
    children: ReactNode;
};

type FormFieldHeaderProps = {
    label: string;
    required?: boolean;
    detail?: string;
};

type FormSelectControlProps = SelectHTMLAttributes<HTMLSelectElement> & {
    className?: string;
};

export const formInputClassName =
    "h-12 w-full rounded-xl border border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-lowest)] px-4 text-sm text-[var(--app-on-surface)] outline-none transition-colors placeholder:text-[var(--app-outline)] focus:border-[var(--app-primary-fixed)]";

export const formTextareaClassName =
    "min-h-36 w-full rounded-xl border border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-lowest)] px-4 py-3 text-sm text-[var(--app-on-surface)] outline-none transition-colors placeholder:text-[var(--app-outline)] focus:border-[var(--app-primary-fixed)]";

export function FormFieldHeader({label, required = false, detail}: FormFieldHeaderProps) {
    return (
        <div className="mb-2 flex items-center justify-between gap-3">
            <label className="text-sm font-medium text-[var(--app-on-surface)]">
                {label}
                {required ? <span className="ml-1 text-[var(--app-error)]">*</span> : null}
            </label>
            {detail ? <span
                className="app-shell-mono text-[10px] uppercase tracking-[0.2em] text-[var(--app-outline)]">{detail}</span> : null}
        </div>
    );
}

export function FormSelectControl({children, className = "", ...props}: FormSelectControlProps) {
    return <SelectControl className={className} controlSize="md" {...props}>{children}</SelectControl>;
}

export default function FormShell({eyebrow, title, description, error, leading, children}: FormShellProps) {
    return (
        <div className="mx-auto w-full max-w-4xl space-y-6">
            {leading ? <div>{leading}</div> : null}

            <header className="space-y-2">
                {eyebrow ?
                    <p className="app-shell-mono text-xs uppercase tracking-[0.28em] text-[var(--app-outline)]">{eyebrow}</p> : null}
                <div>
                    <h1 className="text-3xl font-bold tracking-[-0.03em] text-[var(--app-on-surface)]">{title}</h1>
                    <p className="mt-1 max-w-2xl text-sm text-[var(--app-on-surface-variant)] sm:text-base">{description}</p>
                </div>
            </header>

            <section
                className="rounded-[1.75rem] bg-[var(--app-surface-container-low)] p-6 outline outline-1 outline-[var(--app-outline-variant-soft)] sm:p-8">
                {error ? (
                    <div
                        className="mb-6 rounded-2xl bg-[var(--app-error-container)]/20 px-4 py-3 text-sm text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10">
                        {error}
                    </div>
                ) : null}

                {children}
            </section>
        </div>
    );
}
