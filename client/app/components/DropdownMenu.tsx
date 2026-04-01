import { useEffect, useRef, useState, type ReactNode } from "react";

type DropdownRenderProps = {
  close: () => void;
  open: boolean;
};

type DropdownMenuProps = {
  trigger: ReactNode | ((props: Pick<DropdownRenderProps, "open">) => ReactNode);
  children: ReactNode | ((props: DropdownRenderProps) => ReactNode);
  className?: string;
  triggerClassName?: string;
  menuClassName?: string;
  triggerAriaLabel?: string;
  align?: "left" | "right";
};

const defaultMenuClassName =
  "mt-3 rounded-2xl bg-[var(--app-surface-container-menu)] p-2.5 outline outline-1 outline-[var(--app-outline-variant-strong)] shadow-[var(--app-menu-shadow)]";

export default function DropdownMenu({
  trigger,
  children,
  className,
  triggerClassName,
  menuClassName,
  triggerAriaLabel,
  align = "right",
}: DropdownMenuProps) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  function close() {
    setOpen(false);
  }

  useEffect(() => {
    if (!open) {
      return;
    }

    function handlePointerDown(event: MouseEvent | TouchEvent) {
      if (!rootRef.current?.contains(event.target as Node)) {
        close();
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        close();
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("touchstart", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);

    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("touchstart", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [open]);

  const triggerContent = typeof trigger === "function" ? trigger({ open }) : trigger;
  const menuContent = typeof children === "function" ? children({ close, open }) : children;
  const menuAlignmentClass = align === "left" ? "left-0" : "right-0";

  return (
    <div className={`relative ${className ?? ""}`.trim()} ref={rootRef}>
      <button
        aria-expanded={open}
        aria-haspopup="menu"
        aria-label={triggerAriaLabel}
        className={triggerClassName}
        onClick={() => setOpen((currentOpen) => !currentOpen)}
        type="button"
      >
        {triggerContent}
      </button>

      {open ? (
        <div className={`absolute ${menuAlignmentClass} z-20 ${defaultMenuClassName} ${menuClassName ?? ""}`.trim()} role="menu">
          {menuContent}
        </div>
      ) : null}
    </div>
  );
}
