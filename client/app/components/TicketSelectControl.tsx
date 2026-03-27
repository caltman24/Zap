import type { SelectHTMLAttributes } from "react";

type TicketSelectControlProps = SelectHTMLAttributes<HTMLSelectElement> & {
  className?: string;
};

const filterControlClass =
  "h-11 rounded-xl border border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-lowest)] px-3 text-sm text-[var(--app-on-surface)] outline-none transition-colors focus:border-[var(--app-primary-fixed)]";

export default function TicketSelectControl({
  children,
  className = "",
  ...props
}: TicketSelectControlProps) {
  return (
    <div className="relative">
      <select
        className={`${filterControlClass} appearance-none pr-11 ${className}`.trim()}
        {...props}
      >
        {children}
      </select>
      <span className="material-symbols-outlined pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-lg text-[var(--app-outline)]">
        expand_more
      </span>
    </div>
  );
}
