import type { SelectHTMLAttributes } from "react";

type SelectControlProps = SelectHTMLAttributes<HTMLSelectElement> & {
  className?: string;
  controlSize?: "sm" | "md";
};

const baseClassName =
  "w-full appearance-none rounded-xl border border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-lowest)] text-[var(--app-on-surface)] outline-none transition-colors focus:border-[var(--app-primary-fixed)] [color-scheme:dark]";

const sizeClassNames = {
  sm: {
    select: "h-11 px-3 pr-11 text-sm",
    icon: "right-3 text-lg",
  },
  md: {
    select: "h-12 px-4 pr-12 text-sm",
    icon: "right-4 text-lg",
  },
} as const;

export default function SelectControl({
  children,
  className = "",
  controlSize = "md",
  ...props
}: SelectControlProps) {
  const sizeClasses = sizeClassNames[controlSize];

  return (
    <div className="relative">
      <select className={`${baseClassName} ${sizeClasses.select} ${className}`.trim()} {...props}>
        {children}
      </select>
      <span
        className={`material-symbols-outlined pointer-events-none absolute top-1/2 -translate-y-1/2 text-[var(--app-outline)] ${sizeClasses.icon}`}
      >
        expand_more
      </span>
    </div>
  );
}
