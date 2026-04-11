import {useState} from "react";
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

const fallbackAttachmentAvatarUrl =
    "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjQiIGhlaWdodD0iNjQiIHZpZXdCb3g9IjAgMCA2NCA2NCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iNjQiIGhlaWdodD0iNjQiIHJ4PSIzMiIgZmlsbD0iIzI1MjUzMiIvPjxjaXJjbGUgY3g9IjMyIiBjeT0iMjYiIHI9IjEyIiBmaWxsPSIjQkZDMkQ5Ii8+PHBhdGggZD0iTTE2IDUyQzE2IDQyLjA1ODkgMjMuMTYzNCAzNCAzMiAzNEM0MC44MzY2IDM0IDQ4IDQyLjA1ODkgNDggNTIiIGZpbGw9IiNCRkMyRDkiLz48L3N2Zz4=";

export default function AttachmentSection({ticketId, userInfo, ticket}: AttachmentSectionProps) {
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
                avatarUrl: fallbackAttachmentAvatarUrl
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
                avatarUrl: fallbackAttachmentAvatarUrl
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
                    avatarUrl: userInfo.avatarUrl || fallbackAttachmentAvatarUrl
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

    const allowUpload = canUpload();

    return (
        <div className="space-y-6">
            {allowUpload ? (
                <AttachmentUploader
                    currentTotalSize={attachments.reduce((sum, attachment) => sum + attachment.size, 0)}
                    maxFileSize={10 * 1024 * 1024}
                    maxTotalSize={50 * 1024 * 1024}
                    onFileUpload={handleFileUpload}
                />
            ) : null}

            {!allowUpload && attachments.length === 0 ? (
                <div
                    className="rounded-2xl border border-dashed border-[var(--app-outline-variant-soft)] px-6 py-10 text-center text-[var(--app-on-surface-variant)]">
                    <span
                        className="material-symbols-outlined mb-3 text-5xl text-[var(--app-outline)]">attachment_off</span>
                    <p className="text-base font-medium text-[var(--app-on-surface)]">No attachments available</p>
                    <p className="mt-1 text-sm">Only the submitter, assigned developer, project manager, or admin can
                        upload attachments.</p>
                </div>
            ) : null}

            <AttachmentList
                attachments={attachments}
                canRemove={(attachment) => {
                    const userRole = userInfo.role?.toLowerCase();
                    const isOwner = attachment.uploadedBy.id === userInfo.memberId;
                    const isAdmin = userRole === "admin";
                    const isProjectManager = ticket.projectManagerId === userInfo.memberId;
                    return isOwner || isAdmin || isProjectManager;
                }}
                onDownload={handleDownloadAttachment}
                onRemove={handleRemoveAttachment}
                onView={handleViewAttachment}
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
