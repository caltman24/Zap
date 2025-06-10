import { useEffect, useRef, useState } from "react";
import { AttachmentFile } from "./AttachmentSection";

interface AttachmentModalProps {
    attachment: AttachmentFile | null;
    isOpen: boolean;
    onClose: () => void;
    onDownload: (attachment: AttachmentFile) => void;
}

export default function AttachmentModal({ 
    attachment, 
    isOpen, 
    onClose, 
    onDownload 
}: AttachmentModalProps) {
    const modalRef = useRef<HTMLDialogElement>(null);
    const [textContent, setTextContent] = useState<string>('');
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (isOpen && attachment) {
            modalRef.current?.showModal();
            
            // Load text content for text files
            if (attachment.type === 'text/plain') {
                setIsLoading(true);
                // In real implementation, fetch the file content from server
                // For now, simulate loading
                setTimeout(() => {
                    setTextContent(`This is a preview of ${attachment.name}\n\nIn a real implementation, this would show the actual file content loaded from the server.`);
                    setIsLoading(false);
                }, 500);
            }
        } else {
            modalRef.current?.close();
            setTextContent('');
        }
    }, [isOpen, attachment]);

    const handleClose = () => {
        onClose();
    };

    const formatFileSize = (bytes: number): string => {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    const formatDate = (date: Date): string => {
        return new Intl.DateTimeFormat('en-US', {
            month: 'long',
            day: 'numeric',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }).format(date);
    };

    const renderPreview = () => {
        if (!attachment) return null;

        if (attachment.type.startsWith('image/')) {
            return (
                <div className="flex justify-center">
                    <img 
                        src={attachment.url || `data:${attachment.type};base64,`} 
                        alt={attachment.name}
                        className="max-w-full max-h-[60vh] object-contain rounded-lg"
                        onError={(e) => {
                            // Fallback for demo - show placeholder
                            e.currentTarget.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZGRkIi8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCwgc2Fucy1zZXJpZiIgZm9udC1zaXplPSIxNCIgZmlsbD0iIzk5OSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkltYWdlIFByZXZpZXc8L3RleHQ+PC9zdmc+';
                        }}
                    />
                </div>
            );
        }

        if (attachment.type === 'application/pdf') {
            return (
                <div className="text-center py-8">
                    <div className="text-6xl text-error mb-4">
                        <span className="material-symbols-outlined text-6xl">picture_as_pdf</span>
                    </div>
                    <p className="text-lg font-medium mb-2">PDF Preview</p>
                    <p className="text-base-content/60 mb-4">
                        PDF preview would be displayed here in a real implementation
                    </p>
                    <button 
                        onClick={() => attachment && onDownload(attachment)}
                        className="btn btn-primary"
                    >
                        <span className="material-symbols-outlined">download</span>
                        Download to View
                    </button>
                </div>
            );
        }

        if (attachment.type === 'text/plain') {
            return (
                <div className="bg-base-200 rounded-lg p-4 max-h-[60vh] overflow-auto">
                    {isLoading ? (
                        <div className="flex justify-center items-center py-8">
                            <span className="loading loading-spinner loading-md"></span>
                        </div>
                    ) : (
                        <pre className="whitespace-pre-wrap text-sm font-mono">
                            {textContent}
                        </pre>
                    )}
                </div>
            );
        }

        // Fallback for other file types
        return (
            <div className="text-center py-8">
                <div className="text-6xl text-base-content/40 mb-4">
                    <span className="material-symbols-outlined text-6xl">attach_file</span>
                </div>
                <p className="text-lg font-medium mb-2">Preview not available</p>
                <p className="text-base-content/60 mb-4">
                    This file type cannot be previewed in the browser
                </p>
                <button 
                    onClick={() => attachment && onDownload(attachment)}
                    className="btn btn-primary"
                >
                    <span className="material-symbols-outlined">download</span>
                    Download File
                </button>
            </div>
        );
    };

    if (!attachment) return null;

    return (
        <dialog ref={modalRef} className="modal" onClose={handleClose}>
            <div className="modal-box max-w-4xl w-full">
                {/* Header */}
                <div className="flex justify-between items-start mb-4">
                    <div className="flex-1 min-w-0">
                        <h3 className="font-bold text-lg truncate">{attachment.name}</h3>
                        <div className="flex items-center gap-4 text-sm text-base-content/60 mt-1">
                            <span>{formatFileSize(attachment.size)}</span>
                            <span>•</span>
                            <div className="flex items-center gap-1">
                                <div className="avatar">
                                    <div className="w-4 rounded-full">
                                        <img src={attachment.uploadedBy.avatarUrl} alt={attachment.uploadedBy.name} />
                                    </div>
                                </div>
                                <span>{attachment.uploadedBy.name}</span>
                            </div>
                            <span>•</span>
                            <span>{formatDate(attachment.uploadedAt)}</span>
                        </div>
                    </div>
                    
                    <div className="flex items-center gap-2 ml-4">
                        <button
                            onClick={() => onDownload(attachment)}
                            className="btn btn-sm btn-ghost"
                            title="Download"
                        >
                            <span className="material-symbols-outlined">download</span>
                        </button>
                        <button
                            onClick={handleClose}
                            className="btn btn-sm btn-ghost btn-circle"
                        >
                            <span className="material-symbols-outlined">close</span>
                        </button>
                    </div>
                </div>

                {/* Preview Content */}
                <div className="mb-4">
                    {renderPreview()}
                </div>

                {/* Footer Actions */}
                <div className="modal-action">
                    <button 
                        onClick={() => onDownload(attachment)}
                        className="btn btn-primary"
                    >
                        <span className="material-symbols-outlined">download</span>
                        Download
                    </button>
                    <button className="btn btn-ghost" onClick={handleClose}>
                        Close
                    </button>
                </div>
            </div>
            
            <form method="dialog" className="modal-backdrop">
                <button onClick={handleClose}>close</button>
            </form>
        </dialog>
    );
}
