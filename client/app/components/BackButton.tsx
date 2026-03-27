import { useNavigate } from "@remix-run/react";

export default function BackButton({ to }: { to?: string }) {
    const navigate = useNavigate();
    return (
        <button
            className="inline-flex w-fit items-center gap-2 rounded-xl px-3 py-2 text-sm font-medium text-[var(--app-on-surface-variant)] outline outline-1 outline-[var(--app-outline-variant-soft)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
            onClick={() => to ? navigate({ pathname: to }) : navigate(-1)}
            type="button"
        >
            <span className="material-symbols-outlined text-lg text-[var(--app-primary)]">arrow_back</span>
            Back
        </button>
    )
}
