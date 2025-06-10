import { AttachmentFile } from "./AttachmentSection";

interface AttachmentListProps {
    attachments: AttachmentFile[];
    onRemove: (attachmentId: string) => void;
    onView: (attachment: AttachmentFile) => void;
    onDownload: (attachment: AttachmentFile) => void;
    canRemove: (attachment: AttachmentFile) => boolean;
}

export default function AttachmentList({ 
    attachments, 
    onRemove, 
    onView, 
    onDownload, 
    canRemove 
}: AttachmentListProps) {
    
    const formatFileSize = (bytes: number): string => {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    const getFileIcon = (type: string, name: string): string => {
        const extension = name.split('.').pop()?.toLowerCase();
        
        if (type.startsWith('image/')) return 'image';
        if (type === 'application/pdf' || extension === 'pdf') return 'picture_as_pdf';
        if (type.includes('word') || extension === 'doc' || extension === 'docx') return 'description';
        if (type === 'text/plain' || extension === 'txt') return 'text_snippet';
        if (type.includes('zip') || extension === 'zip') return 'folder_zip';
        
        return 'attach_file';
    };

    const getFileTypeColor = (type: string, name: string): string => {
        const extension = name.split('.').pop()?.toLowerCase();
        
        if (type.startsWith('image/')) return 'text-success';
        if (type === 'application/pdf' || extension === 'pdf') return 'text-error';
        if (type.includes('word') || extension === 'doc' || extension === 'docx') return 'text-info';
        if (type === 'text/plain' || extension === 'txt') return 'text-warning';
        if (type.includes('zip') || extension === 'zip') return 'text-secondary';
        
        return 'text-base-content';
    };

    const formatDate = (date: Date): string => {
        return new Intl.DateTimeFormat('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }).format(date);
    };

    const canPreview = (type: string): boolean => {
        return type.startsWith('image/') || type === 'application/pdf' || type === 'text/plain';
    };

    if (attachments.length === 0) {
        return (
            <div className="text-center py-8 text-base-content/60">
                <svg className="mx-auto h-12 w-12 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} 
                        d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
                </svg>
                <p>No attachments yet.</p>
            </div>
        );
    }

    return (
        <div className="space-y-3">
            {attachments.map((attachment) => (
                <div 
                    key={attachment.id} 
                    className="flex items-center gap-4 p-4 bg-base-200 rounded-lg hover:bg-base-300/50 transition-colors"
                >
                    {/* File Icon */}
                    <div className={`text-2xl ${getFileTypeColor(attachment.type, attachment.name)}`}>
                        <span className="material-symbols-outlined">
                            {getFileIcon(attachment.type, attachment.name)}
                        </span>
                    </div>

                    {/* File Info */}
                    <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                            <h4 className="font-medium truncate">{attachment.name}</h4>
                            <span className="text-sm text-base-content/60 shrink-0">
                                {formatFileSize(attachment.size)}
                            </span>
                        </div>
                        <div className="flex items-center gap-2 text-sm text-base-content/60">
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

                    {/* Actions */}
                    <div className="flex items-center gap-1">
                        {canPreview(attachment.type) && (
                            <button
                                onClick={() => onView(attachment)}
                                className="btn btn-sm btn-ghost btn-circle"
                                title="Preview"
                            >
                                <span className="material-symbols-outlined text-lg">visibility</span>
                            </button>
                        )}
                        
                        <button
                            onClick={() => onDownload(attachment)}
                            className="btn btn-sm btn-ghost btn-circle"
                            title="Download"
                        >
                            <span className="material-symbols-outlined text-lg">download</span>
                        </button>

                        {canRemove(attachment) && (
                            <button
                                onClick={() => onRemove(attachment.id)}
                                className="btn btn-sm btn-ghost btn-circle text-error hover:bg-error/10"
                                title="Remove"
                            >
                                <span className="material-symbols-outlined text-lg">delete</span>
                            </button>
                        )}
                    </div>
                </div>
            ))}

            {/* Summary */}
            <div className="flex justify-between items-center pt-4 border-t border-base-300 text-sm text-base-content/60">
                <span>{attachments.length} attachment{attachments.length !== 1 ? 's' : ''}</span>
                <span>
                    Total size: {formatFileSize(attachments.reduce((sum, att) => sum + att.size, 0))}
                </span>
            </div>
        </div>
    );
}
