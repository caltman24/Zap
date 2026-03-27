import type { ChangeEvent } from "react";

type TicketSearchInputProps = {
  value: string;
  onChange: (value: string) => void;
  placeholder: string;
  className?: string;
  inputClassName?: string;
};

export default function TicketSearchInput({
  value,
  onChange,
  placeholder,
  className = "",
  inputClassName = "",
}: TicketSearchInputProps) {
  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    onChange(event.target.value);
  }

  return (
    <div className={`relative ${className}`.trim()}>
      <span className="material-symbols-outlined pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-lg text-[var(--app-outline)]">
        search
      </span>
      <input
        className={`h-11 w-full rounded-xl border border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-lowest)] pl-11 pr-4 text-sm text-[var(--app-on-surface)] outline-none transition-colors placeholder:text-[var(--app-outline)] focus:border-[var(--app-primary-fixed)] ${inputClassName}`.trim()}
        onChange={handleChange}
        placeholder={placeholder}
        type="text"
        value={value}
      />
    </div>
  );
}
