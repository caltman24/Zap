import { useEffect, useRef, useState } from "react";
import type { AttachmentFile } from "./AttachmentSection";

interface AttachmentModalProps {
  attachment: AttachmentFile | null;
  isOpen: boolean;
  onClose: () => void;
  onDownload: (attachment: AttachmentFile) => void;
}

function formatFileSize(bytes: number): string {
  if (bytes === 0) return "0 Bytes";
  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

function formatDate(date: Date): string {
  return new Intl.DateTimeFormat("en-US", {
    month: "long",
    day: "numeric",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

export default function AttachmentModal({ attachment, isOpen, onClose, onDownload }: AttachmentModalProps) {
  const modalRef = useRef<HTMLDialogElement>(null);
  const [textContent, setTextContent] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (isOpen && attachment) {
      modalRef.current?.showModal();

      if (attachment.type === "text/plain") {
        setIsLoading(true);
        setTimeout(() => {
          setTextContent(`This is a preview of ${attachment.name}\n\nIn a real implementation, this would show the actual file content loaded from the server.`);
          setIsLoading(false);
        }, 500);
      }
    } else {
      modalRef.current?.close();
      setTextContent("");
    }
  }, [attachment, isOpen]);

  function handleClose() {
    onClose();
  }

  function renderPreview() {
    if (!attachment) return null;

    if (attachment.type.startsWith("image/")) {
      return (
        <div className="flex justify-center">
          <img
            alt={attachment.name}
            className="max-h-[60vh] max-w-full rounded-2xl object-contain"
            onError={(event) => {
              event.currentTarget.src =
                "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjMjdjYWZmIi8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCwgc2Fucy1zZXJpZiIgZm9udC1zaXplPSIxNCIgZmlsbD0iIzEwMTAxMCIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkltYWdlIFByZXZpZXc8L3RleHQ+PC9zdmc+";
            }}
            src={attachment.url || `data:${attachment.type};base64,`}
          />
        </div>
      );
    }

    if (attachment.type === "application/pdf") {
      return (
        <div className="rounded-2xl bg-[var(--app-surface-container-lowest)] px-6 py-10 text-center outline outline-1 outline-[var(--app-outline-variant)]/10">
          <span className="material-symbols-outlined text-6xl text-[var(--app-error)]">picture_as_pdf</span>
          <p className="mt-4 text-lg font-semibold text-[var(--app-on-surface)]">PDF Preview</p>
          <p className="mt-2 text-sm text-[var(--app-on-surface-variant)]">
            PDF preview would be displayed here in a real implementation.
          </p>
        </div>
      );
    }

    if (attachment.type === "text/plain") {
      return (
        <div className="rounded-2xl bg-[var(--app-surface-container-lowest)] p-4 outline outline-1 outline-[var(--app-outline-variant)]/10">
          {isLoading ? (
            <div className="grid min-h-[220px] place-items-center">
              <span className="inline-flex h-6 w-6 animate-spin rounded-full border-2 border-[var(--app-outline)] border-r-transparent" />
            </div>
          ) : (
            <pre className="max-h-[60vh] overflow-auto whitespace-pre-wrap text-sm leading-6 text-[var(--app-on-surface)]">{textContent}</pre>
          )}
        </div>
      );
    }

    return (
      <div className="rounded-2xl bg-[var(--app-surface-container-lowest)] px-6 py-10 text-center outline outline-1 outline-[var(--app-outline-variant)]/10">
        <span className="material-symbols-outlined text-6xl text-[var(--app-outline)]">attach_file</span>
        <p className="mt-4 text-lg font-semibold text-[var(--app-on-surface)]">Preview not available</p>
        <p className="mt-2 text-sm text-[var(--app-on-surface-variant)]">This file type cannot be previewed in the browser.</p>
      </div>
    );
  }

  if (!attachment) return null;

  return (
    <dialog
      className="m-auto w-full max-w-4xl overflow-visible border-0 bg-transparent p-0 text-left text-[var(--app-on-surface)] shadow-none backdrop:bg-black/70 backdrop:backdrop-blur-sm"
      onClick={(event) => {
        if (event.target === event.currentTarget) {
          handleClose();
        }
      }}
      onClose={handleClose}
      ref={modalRef}
    >
      <div className="w-full rounded-[2rem] bg-[var(--app-surface-container-low)] p-0 outline outline-1 outline-[var(--app-outline-variant-soft)]">
        <div className="flex items-start justify-between gap-4 border-b border-[var(--app-outline-variant)]/10 px-6 py-5 sm:px-8">
          <div className="min-w-0 flex-1">
            <h3 className="truncate text-xl font-bold text-[var(--app-on-surface)]">{attachment.name}</h3>
            <div className="mt-2 flex flex-wrap items-center gap-2 text-sm text-[var(--app-on-surface-variant)]">
              <span>{formatFileSize(attachment.size)}</span>
              <span className="h-1 w-1 rounded-full bg-[var(--app-outline)]/50" />
              <div className="flex items-center gap-2">
                <img alt={attachment.uploadedBy.name} className="h-5 w-5 rounded-full border border-[var(--app-outline-variant)]/20 object-cover" src={attachment.uploadedBy.avatarUrl} />
                <span>{attachment.uploadedBy.name}</span>
              </div>
              <span className="h-1 w-1 rounded-full bg-[var(--app-outline)]/50" />
              <span>{formatDate(attachment.uploadedAt)}</span>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <button
              className="inline-flex h-10 w-10 items-center justify-center rounded-xl text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
              onClick={() => onDownload(attachment)}
              title="Download"
              type="button"
            >
              <span className="material-symbols-outlined text-lg">download</span>
            </button>
            <button
              className="inline-flex h-10 w-10 items-center justify-center rounded-xl text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
              onClick={handleClose}
              type="button"
            >
              <span className="material-symbols-outlined text-lg">close</span>
            </button>
          </div>
        </div>

        <div className="px-6 py-6 sm:px-8">{renderPreview()}</div>

        <div className="flex justify-end gap-3 border-t border-[var(--app-outline-variant)]/10 px-6 py-5 sm:px-8">
          <button
            className="inline-flex items-center justify-center rounded-xl px-4 py-2.5 text-sm font-medium text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
            onClick={handleClose}
            type="button"
          >
            Close
          </button>
          <button
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-[linear-gradient(135deg,var(--app-primary)_0%,var(--app-primary-fixed)_100%)] px-4 py-2.5 text-sm font-bold text-[#1000a9] transition-all duration-200 hover:opacity-95 active:scale-95"
            onClick={() => onDownload(attachment)}
            type="button"
          >
            <span className="material-symbols-outlined text-lg">download</span>
            Download
          </button>
        </div>
      </div>
    </dialog>
  );
}
