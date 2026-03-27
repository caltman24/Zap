import { useRef, useEffect } from "react";

interface ArchiveWarningModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  message: string;
}

export default function ArchiveWarningModal({ 
  isOpen, 
  onClose, 
  title, 
  message 
}: ArchiveWarningModalProps) {
  const modalRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    if (isOpen) {
      modalRef.current?.showModal();
    } else {
      modalRef.current?.close();
    }
  }, [isOpen]);

  const handleClose = () => {
    onClose();
  };

  return (
    <dialog
      className="m-auto w-full max-w-lg overflow-visible border-0 bg-transparent p-0 text-left text-[var(--app-on-surface)] shadow-none backdrop:bg-black/70 backdrop:backdrop-blur-sm"
      onClick={(event) => {
        if (event.target === event.currentTarget) {
          handleClose();
        }
      }}
      onClose={handleClose}
      ref={modalRef}
    >
      <div className="w-full rounded-[2rem] bg-[var(--app-surface-container-low)] p-0 outline outline-1 outline-[var(--app-outline-variant-soft)]">
        <div className="border-b border-[var(--app-outline-variant)]/10 px-6 py-5 sm:px-8">
          <div className="flex items-start gap-4">
            <div className="rounded-2xl bg-[var(--app-tertiary-container)]/20 p-3 text-[var(--app-tertiary)] outline outline-1 outline-[var(--app-tertiary)]/10">
              <span className="material-symbols-outlined text-2xl">warning</span>
            </div>
            <div>
              <h3 className="text-xl font-bold tracking-tight text-[var(--app-on-surface)]">{title}</h3>
              <p className="mt-2 text-sm leading-6 text-[var(--app-on-surface-variant)]">{message}</p>
            </div>
          </div>
        </div>

        <div className="flex justify-end border-t border-[var(--app-outline-variant)]/10 px-6 py-5 sm:px-8">
          <button
            className="inline-flex min-w-24 items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2.5 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95"
            onClick={handleClose}
            type="button"
          >
            OK
          </button>
        </div>
      </div>
    </dialog>
  );
}
