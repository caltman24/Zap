import {formatDateTimeShort} from "~/utils/dateTime";
import type {AttachmentFile} from "./AttachmentSection";

interface AttachmentListProps {
    attachments: AttachmentFile[];
    onRemove: (attachmentId: string) => void;
    onView: (attachment: AttachmentFile) => void;
    onDownload: (attachment: AttachmentFile) => void;
    canRemove: (attachment: AttachmentFile) => boolean;
}

function formatFileSize(bytes: number): string {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

function getFileIcon(type: string, name: string): string {
    const extension = name.split(".").pop()?.toLowerCase();

    if (type.startsWith("image/")) return "image";
    if (type === "application/pdf" || extension === "pdf") return "picture_as_pdf";
    if (type.includes("word") || extension === "doc" || extension === "docx") return "description";
    if (type === "text/plain" || extension === "txt") return "text_snippet";
    if (type.includes("zip") || extension === "zip") return "folder_zip";

    return "attach_file";
}

function getFileTone(type: string, name: string) {
    const extension = name.split(".").pop()?.toLowerCase();

    if (type.startsWith("image/")) return "bg-emerald-500/15 text-emerald-300";
    if (type === "application/pdf" || extension === "pdf") return "bg-[var(--app-error-container)]/25 text-[var(--app-error)]";
    if (type.includes("word") || extension === "doc" || extension === "docx") return "bg-sky-500/15 text-sky-300";
    if (type === "text/plain" || extension === "txt") return "bg-[var(--app-tertiary-container)]/25 text-[var(--app-tertiary)]";
    if (type.includes("zip") || extension === "zip") return "bg-[var(--app-secondary-container)]/30 text-[var(--app-secondary)]";

    return "bg-[var(--app-surface-container-high)] text-[var(--app-on-surface-variant)]";
}

function canPreview(type: string): boolean {
    return type.startsWith("image/") || type === "application/pdf" || type === "text/plain";
}

export default function AttachmentList({attachments, onRemove, onView, onDownload, canRemove}: AttachmentListProps) {
    if (attachments.length === 0) {
        return (
            <div
                className="rounded-2xl border border-dashed border-[var(--app-outline-variant-soft)] px-6 py-10 text-center text-[var(--app-on-surface-variant)]">
                <span className="material-symbols-outlined mb-3 text-5xl text-[var(--app-outline)]">attachment</span>
                <p className="text-base font-medium text-[var(--app-on-surface)]">No attachments yet</p>
                <p className="mt-1 text-sm">Files shared on this ticket will appear here.</p>
            </div>
        );
    }

    return (
        <div className="space-y-3">
            {attachments.map((attachment) => {
                const tone = getFileTone(attachment.type, attachment.name);

                return (
                    <div
                        className="flex flex-col gap-4 rounded-2xl bg-[var(--app-surface-container-lowest)]/80 px-4 py-4 outline outline-1 outline-[var(--app-outline-variant)]/10 transition-colors hover:bg-[var(--app-surface-container-high)]/25 sm:flex-row sm:items-center"
                        key={attachment.id}
                    >
                        <div
                            className={`inline-flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl ${tone}`}>
                            <span
                                className="material-symbols-outlined text-2xl">{getFileIcon(attachment.type, attachment.name)}</span>
                        </div>

                        <div className="min-w-0 flex-1 space-y-2">
                            <div className="flex flex-wrap items-center gap-2">
                                <h4 className="truncate text-sm font-semibold text-[var(--app-on-surface)]">{attachment.name}</h4>
                                <span
                                    className="app-shell-mono text-[10px] uppercase tracking-[0.2em] text-[var(--app-outline)]">
                  {formatFileSize(attachment.size)}
                </span>
                            </div>

                            <div
                                className="flex flex-wrap items-center gap-2 text-xs text-[var(--app-on-surface-variant)]">
                                <div className="flex items-center gap-2">
                                    <img
                                        alt={attachment.uploadedBy.name}
                                        className="h-5 w-5 rounded-full border border-[var(--app-outline-variant)]/20 object-cover"
                                        src={attachment.uploadedBy.avatarUrl}
                                    />
                                    <span>{attachment.uploadedBy.name}</span>
                                </div>
                                <span className="h-1 w-1 rounded-full bg-[var(--app-outline)]/50"/>
                                <span>{formatDateTimeShort(attachment.uploadedAt)}</span>
                            </div>
                        </div>

                        <div className="flex items-center gap-2 self-end sm:self-center">
                            {canPreview(attachment.type) ? (
                                <button
                                    className="inline-flex h-9 w-9 items-center justify-center rounded-xl text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                                    onClick={() => onView(attachment)}
                                    title="Preview"
                                    type="button"
                                >
                                    <span className="material-symbols-outlined text-lg">visibility</span>
                                </button>
                            ) : null}

                            <button
                                className="inline-flex h-9 w-9 items-center justify-center rounded-xl text-[var(--app-on-surface-variant)] transition-colors hover:bg-[var(--app-hover-overlay)] hover:text-[var(--app-on-surface)]"
                                onClick={() => onDownload(attachment)}
                                title="Download"
                                type="button"
                            >
                                <span className="material-symbols-outlined text-lg">download</span>
                            </button>

                            {canRemove(attachment) ? (
                                <button
                                    className="inline-flex h-9 w-9 items-center justify-center rounded-xl text-[var(--app-outline)] transition-colors hover:bg-[var(--app-error-container)]/15 hover:text-[var(--app-error)]"
                                    onClick={() => onRemove(attachment.id)}
                                    title="Remove"
                                    type="button"
                                >
                                    <span className="material-symbols-outlined text-lg">delete</span>
                                </button>
                            ) : null}
                        </div>
                    </div>
                );
            })}

            <div
                className="flex flex-wrap items-center justify-between gap-2 border-t border-[var(--app-outline-variant)]/10 pt-4 text-xs text-[var(--app-on-surface-variant)]">
        <span className="app-shell-mono uppercase tracking-[0.2em]">
          {attachments.length} attachment{attachments.length !== 1 ? "s" : ""}
        </span>
                <span>Total size: {formatFileSize(attachments.reduce((sum, attachment) => sum + attachment.size, 0))}</span>
            </div>
        </div>
    );
}
