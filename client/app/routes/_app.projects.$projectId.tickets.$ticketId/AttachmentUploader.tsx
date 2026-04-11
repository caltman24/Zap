import {useCallback, useRef, useState} from "react";

interface AttachmentUploaderProps {
    onFileUpload: (files: File[]) => void;
    maxFileSize: number;
    maxTotalSize: number;
    currentTotalSize: number;
}

const ALLOWED_FILE_TYPES = {
    "application/pdf": [".pdf"],
    "application/msword": [".doc"],
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [".docx"],
    "text/plain": [".txt"],
    "application/zip": [".zip"],
    "application/x-zip-compressed": [".zip"],
    "image/jpeg": [".jpg", ".jpeg"],
    "image/png": [".png"],
    "image/gif": [".gif"],
    "image/webp": [".webp"],
};

const ALLOWED_EXTENSIONS = Object.values(ALLOWED_FILE_TYPES).flat();

function formatFileSize(bytes: number): string {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

export default function AttachmentUploader({
                                               onFileUpload,
                                               maxFileSize,
                                               maxTotalSize,
                                               currentTotalSize
                                           }: AttachmentUploaderProps) {
    const inputRef = useRef<HTMLInputElement>(null);
    const [isDragOver, setIsDragOver] = useState(false);
    const [uploadProgress, setUploadProgress] = useState<Record<string, number>>({});
    const [errors, setErrors] = useState<string[]>([]);

    const validateFiles = (files: File[]) => {
        const validFiles: File[] = [];
        const newErrors: string[] = [];

        let totalSize = currentTotalSize;

        for (const file of files) {
            const fileExtension = `.${file.name.split(".").pop()?.toLowerCase()}`;
            const isValidType = ALLOWED_EXTENSIONS.includes(fileExtension) || Object.keys(ALLOWED_FILE_TYPES).includes(file.type);

            if (!isValidType) {
                newErrors.push(`${file.name}: File type not allowed. Allowed types: PDF, DOC, DOCX, TXT, ZIP, Images`);
                continue;
            }

            if (file.size > maxFileSize) {
                newErrors.push(`${file.name}: File too large. Maximum size is ${formatFileSize(maxFileSize)}`);
                continue;
            }

            if (totalSize + file.size > maxTotalSize) {
                newErrors.push(`${file.name}: Would exceed total size limit of ${formatFileSize(maxTotalSize)}`);
                continue;
            }

            validFiles.push(file);
            totalSize += file.size;
        }

        return {validFiles, errors: newErrors};
    };

    const handleFiles = useCallback(
        (files: FileList | File[]) => {
            const fileArray = Array.from(files);
            const {validFiles, errors: nextErrors} = validateFiles(fileArray);
            setErrors(nextErrors);

            if (validFiles.length > 0) {
                validFiles.forEach((file) => {
                    const fileId = file.name + file.size;
                    setUploadProgress((prev) => ({...prev, [fileId]: 0}));

                    const interval = setInterval(() => {
                        setUploadProgress((prev) => {
                            const currentProgress = prev[fileId] || 0;

                            if (currentProgress >= 90) {
                                clearInterval(interval);

                                setTimeout(() => {
                                    setUploadProgress((prev) => ({...prev, [fileId]: 100}));

                                    setTimeout(() => {
                                        setUploadProgress((prev) => {
                                            const next = {...prev};
                                            delete next[fileId];
                                            return next;
                                        });
                                    }, 500);
                                }, 200);

                                return prev;
                            }

                            return {...prev, [fileId]: currentProgress + 10};
                        });
                    }, 100);
                });

                onFileUpload(validFiles);
            }
        },
        [currentTotalSize, maxFileSize, maxTotalSize, onFileUpload],
    );

    const handleDragOver = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        setIsDragOver(true);
    }, []);

    const handleDragLeave = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        setIsDragOver(false);
    }, []);

    const handleDrop = useCallback(
        (event: React.DragEvent) => {
            event.preventDefault();
            setIsDragOver(false);

            const files = event.dataTransfer.files;
            if (files.length > 0) {
                handleFiles(files);
            }
        },
        [handleFiles],
    );

    const handleFileInput = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>) => {
            const files = event.target.files;
            if (files && files.length > 0) {
                handleFiles(files);
            }
            event.target.value = "";
        },
        [handleFiles],
    );

    const remainingSize = maxTotalSize - currentTotalSize;

    return (
        <div className="space-y-4">
            <button
                className={`w-full rounded-[1.6rem] border border-dashed px-6 py-10 text-center transition-all duration-200 ${
                    isDragOver
                        ? "border-[var(--app-primary-fixed)] bg-[var(--app-primary-fixed)]/10 shadow-[0_18px_36px_rgba(141,144,255,0.08)]"
                        : "border-[var(--app-outline-variant-soft)] bg-[var(--app-surface-container-lowest)]/60 hover:border-[var(--app-primary-fixed)]/40 hover:bg-[var(--app-surface-container-high)]/20"
                }`}
                onClick={() => inputRef.current?.click()}
                onDragLeave={handleDragLeave}
                onDragOver={handleDragOver}
                onDrop={handleDrop}
                type="button"
            >
                <input
                    accept={ALLOWED_EXTENSIONS.join(",")}
                    className="hidden"
                    multiple
                    onChange={handleFileInput}
                    ref={inputRef}
                    type="file"
                />

                <div className="space-y-4">
                    <div
                        className={`mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-[var(--app-surface-container-high)] text-[var(--app-outline)] transition-all duration-200 ${isDragOver ? "scale-105 text-[var(--app-primary)]" : ""}`}>
                        <span className="material-symbols-outlined text-4xl">upload_file</span>
                    </div>

                    <div>
                        <p className="text-lg font-semibold text-[var(--app-on-surface)]">Drop files here or click to
                            browse</p>
                        <p className="mt-2 text-sm text-[var(--app-on-surface-variant)]">
                            Supported: PDF, DOC, DOCX, TXT, ZIP, Images (JPG, PNG, GIF, WebP)
                        </p>
                        <p className="mt-1 text-xs text-[var(--app-outline)]">
                            Max file size: {formatFileSize(maxFileSize)} · Remaining
                            space: {formatFileSize(remainingSize)}
                        </p>
                    </div>
                </div>
            </button>

            {Object.keys(uploadProgress).length > 0 ? (
                <div
                    className="space-y-3 rounded-2xl bg-[var(--app-surface-container-lowest)]/80 px-4 py-4 outline outline-1 outline-[var(--app-outline-variant)]/10">
                    <h4 className="text-sm font-semibold text-[var(--app-on-surface)]">Uploading</h4>
                    {Object.entries(uploadProgress).map(([fileId, progress]) => (
                        <div className="space-y-2" key={fileId}>
                            <div
                                className="flex items-center justify-between gap-3 text-xs text-[var(--app-on-surface-variant)]">
                                <span className="truncate">{fileId.split(/\d+$/)[0]}</span>
                                <span className="app-shell-mono">{progress}%</span>
                            </div>
                            <div className="h-2 overflow-hidden rounded-full bg-[var(--app-surface-container-high)]">
                                <div
                                    className="h-full rounded-full bg-[linear-gradient(90deg,var(--app-primary),var(--app-primary-fixed))] transition-all duration-200"
                                    style={{width: `${progress}%`}}/>
                            </div>
                        </div>
                    ))}
                </div>
            ) : null}

            {errors.length > 0 ? (
                <div
                    className="rounded-2xl bg-[var(--app-error-container)]/20 px-4 py-4 text-sm text-[var(--app-error)] outline outline-1 outline-[var(--app-error)]/10">
                    <div className="flex items-start justify-between gap-4">
                        <div>
                            <h4 className="font-semibold">Upload Errors</h4>
                            <ul className="mt-2 list-disc space-y-1 pl-5">
                                {errors.map((error, index) => (
                                    <li key={`${error}-${index}`}>{error}</li>
                                ))}
                            </ul>
                        </div>
                        <button
                            className="inline-flex items-center justify-center rounded-xl px-3 py-2 text-xs font-medium text-[var(--app-error)] transition-colors hover:bg-[var(--app-error)]/10"
                            onClick={() => setErrors([])}
                            type="button"
                        >
                            Dismiss
                        </button>
                    </div>
                </div>
            ) : null}
        </div>
    );
}
