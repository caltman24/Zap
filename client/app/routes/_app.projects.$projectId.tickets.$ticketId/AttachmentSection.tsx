import { useState } from "react";
import AttachmentUploader from "./AttachmentUploader";
import AttachmentList from "./AttachmentList";
import AttachmentModal from "./AttachmentModal";

interface AttachmentSectionProps {
    ticketId: string;
    userInfo: any;
    ticket: any;
}

export interface AttachmentFile {
    id: string;
    name: string;
    size: number;
    type: string;
    url?: string;
    uploadedAt: Date;
    uploadedBy: {
        id: string;
        name: string;
        avatarUrl: string;
    };
}

export default function AttachmentSection({ ticketId, userInfo, ticket }: AttachmentSectionProps) {
    // Initialize with some sample attachments for demo purposes
    const [attachments, setAttachments] = useState<AttachmentFile[]>([
        {
            id: 'demo-1',
            name: 'bug-screenshot.png',
            size: 245760, // 240KB
            type: 'image/png',
            url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZjMmNjY2Ii8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCwgc2Fucy1zZXJpZiIgZm9udC1zaXplPSIxNCIgZmlsbD0id2hpdGUiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj5CdWcgU2NyZWVuc2hvdDwvdGV4dD48L3N2Zz4=',
            uploadedAt: new Date(Date.now() - 2 * 60 * 60 * 1000), // 2 hours ago
            uploadedBy: {
                id: 'demo-user-1',
                name: 'John Doe',
                avatarUrl: 'https://img.daisyui.com/images/stock/photo-1534528741775-53994a69daeb.webp'
            }
        },
        {
            id: 'demo-2',
            name: 'requirements.pdf',
            size: 1048576, // 1MB
            type: 'application/pdf',
            uploadedAt: new Date(Date.now() - 24 * 60 * 60 * 1000), // 1 day ago
            uploadedBy: {
                id: 'demo-user-2',
                name: 'Jane Smith',
                avatarUrl: 'https://img.daisyui.com/images/stock/photo-1534528741775-53994a69daeb.webp'
            }
        }
    ]);
    const [selectedAttachment, setSelectedAttachment] = useState<AttachmentFile | null>(null);
    const [isModalOpen, setIsModalOpen] = useState(false);

    // Check if user can upload attachments
    const canUpload = () => {
        const userRole = userInfo.role?.toLowerCase();
        const isSubmitter = ticket.submitter?.id === userInfo.memberId;
        const isAssignedDev = ticket.assignee?.id === userInfo.memberId;
        const isProjectManager = ticket.projectManagerId === userInfo.memberId;
        const isAdmin = userRole === 'admin';

        return isSubmitter || isAssignedDev || isProjectManager || isAdmin;
    };

    const handleFileUpload = (files: File[]) => {
        // For now, just simulate adding files to the list
        // In real implementation, this would upload to server
        const newAttachments: AttachmentFile[] = files.map(file => {
            // Create a blob URL for image preview
            const url = file.type.startsWith('image/') ? URL.createObjectURL(file) : undefined;

            return {
                id: Math.random().toString(36).substring(2, 11),
                name: file.name,
                size: file.size,
                type: file.type,
                url,
                uploadedAt: new Date(),
                uploadedBy: {
                    id: userInfo.memberId,
                    name: userInfo.name,
                    avatarUrl: userInfo.avatarUrl || 'https://img.daisyui.com/images/stock/photo-1534528741775-53994a69daeb.webp'
                }
            };
        });

        setAttachments(prev => [...prev, ...newAttachments]);
    };

    const handleRemoveAttachment = (attachmentId: string) => {
        setAttachments(prev => prev.filter(att => att.id !== attachmentId));
    };

    const handleViewAttachment = (attachment: AttachmentFile) => {
        setSelectedAttachment(attachment);
        setIsModalOpen(true);
    };

    const handleDownloadAttachment = (attachment: AttachmentFile) => {
        // In real implementation, this would download from server
        console.log('Download attachment:', attachment.name);
    };

    return (
        <div className="space-y-6">
            {canUpload() && (
                <AttachmentUploader
                    onFileUpload={handleFileUpload}
                    maxFileSize={10 * 1024 * 1024} // 10MB
                    maxTotalSize={50 * 1024 * 1024} // 50MB
                    currentTotalSize={attachments.reduce((sum, att) => sum + att.size, 0)}
                />
            )}

            {!canUpload() && attachments.length === 0 && (
                <div className="text-center py-8 text-base-content/60">
                    <svg className="mx-auto h-12 w-12 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
                            d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
                    </svg>
                    <p>No attachments available.</p>
                    <p className="text-sm mt-1">Only the submitter, assigned developer, project manager, or admin can upload attachments.</p>
                </div>
            )}

            <AttachmentList
                attachments={attachments}
                onRemove={handleRemoveAttachment}
                onView={handleViewAttachment}
                onDownload={handleDownloadAttachment}
                canRemove={(attachment) => {
                    // Can remove if user uploaded it, or if user is admin/PM
                    const userRole = userInfo.role?.toLowerCase();
                    const isOwner = attachment.uploadedBy.id === userInfo.memberId;
                    const isAdmin = userRole === 'admin';
                    const isProjectManager = ticket.projectManagerId === userInfo.memberId;
                    return isOwner || isAdmin || isProjectManager;
                }}
            />

            <AttachmentModal
                attachment={selectedAttachment}
                isOpen={isModalOpen}
                onClose={() => {
                    setIsModalOpen(false);
                    setSelectedAttachment(null);
                }}
                onDownload={handleDownloadAttachment}
            />
        </div>
    );
}
