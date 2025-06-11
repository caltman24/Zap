import { useCallback, useState } from "react";

interface AttachmentUploaderProps {
    onFileUpload: (files: File[]) => void;
    maxFileSize: number;
    maxTotalSize: number;
    currentTotalSize: number;
}

const ALLOWED_FILE_TYPES = {
    'application/pdf': ['.pdf'],
    'application/msword': ['.doc'],
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document': ['.docx'],
    'text/plain': ['.txt'],
    'application/zip': ['.zip'],
    'application/x-zip-compressed': ['.zip'],
    'image/jpeg': ['.jpg', '.jpeg'],
    'image/png': ['.png'],
    'image/gif': ['.gif'],
    'image/webp': ['.webp']
};

const ALLOWED_EXTENSIONS = Object.values(ALLOWED_FILE_TYPES).flat();

export default function AttachmentUploader({
    onFileUpload,
    maxFileSize,
    maxTotalSize,
    currentTotalSize
}: AttachmentUploaderProps) {
    const [isDragOver, setIsDragOver] = useState(false);
    const [uploadProgress, setUploadProgress] = useState<{ [key: string]: number }>({});
    const [errors, setErrors] = useState<string[]>([]);

    const validateFiles = (files: File[]): { validFiles: File[], errors: string[] } => {
        const validFiles: File[] = [];
        const newErrors: string[] = [];

        let totalSize = currentTotalSize;

        for (const file of files) {
            // Check file type
            const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
            const isValidType = ALLOWED_EXTENSIONS.includes(fileExtension) ||
                Object.keys(ALLOWED_FILE_TYPES).includes(file.type);

            if (!isValidType) {
                newErrors.push(`${file.name}: File type not allowed. Allowed types: PDF, DOC, DOCX, TXT, ZIP, Images`);
                continue;
            }

            // Check file size
            if (file.size > maxFileSize) {
                newErrors.push(`${file.name}: File too large. Maximum size is ${formatFileSize(maxFileSize)}`);
                continue;
            }

            // Check total size
            if (totalSize + file.size > maxTotalSize) {
                newErrors.push(`${file.name}: Would exceed total size limit of ${formatFileSize(maxTotalSize)}`);
                continue;
            }

            validFiles.push(file);
            totalSize += file.size;
        }

        return { validFiles, errors: newErrors };
    };

    const handleFiles = useCallback((files: FileList | File[]) => {
        const fileArray = Array.from(files);
        const { validFiles, errors } = validateFiles(fileArray);

        setErrors(errors);

        if (validFiles.length > 0) {
            // Simulate upload progress
            validFiles.forEach(file => {
                const fileId = file.name + file.size;
                setUploadProgress(prev => ({ ...prev, [fileId]: 0 }));

                // ===================================================================
                // ACTUAL UPLOAD IMPLEMENTATION:
                // Replace the simulated upload below with your actual API upload logic
                // Example using fetch:
                // 
                // const formData = new FormData();
                // formData.append('file', file);
                // formData.append('ticketId', ticketId);
                // 
                // fetch('/api/attachments/upload', {
                //     method: 'POST',
                //     body: formData,
                //     // Include progress tracking:
                //     onUploadProgress: (progressEvent) => {
                //         const percentCompleted = Math.round(
                //             (progressEvent.loaded * 100) / progressEvent.total
                //         );
                //         setUploadProgress(prev => ({ ...prev, [fileId]: percentCompleted }));
                //     }
                // })
                // .then(response => response.json())
                // .then(data => {
                //     // Show 100% complete
                //     setUploadProgress(prev => ({ ...prev, [fileId]: 100 }));
                //     
                //     // Remove progress bar after a moment
                //     setTimeout(() => {
                //         setUploadProgress(prev => {
                //             const newProgress = { ...prev };
                //             delete newProgress[fileId];
                //             return newProgress;
                //         });
                //     }, 500);
                //     
                //     // Handle successful upload (e.g., add attachment to list)
                // })
                // .catch(error => {
                //     // Handle upload failure
                //     setErrors(prev => [...prev, `Failed to upload ${file.name}: ${error.message}`]);
                //     setUploadProgress(prev => {
                //         const newProgress = { ...prev };
                //         delete newProgress[fileId];
                //         return newProgress;
                //     });
                // });
                // ===================================================================

                // Simulate progress (REPLACE THIS WITH ACTUAL UPLOAD CODE)
                const interval = setInterval(() => {
                    setUploadProgress(prev => {
                        const currentProgress = prev[fileId] || 0;
                        // Stop at 90% to allow for completion step
                        if (currentProgress >= 90) {
                            clearInterval(interval);

                            // Set to 100% and then remove after a short delay
                            setTimeout(() => {
                                setUploadProgress(prev => ({ ...prev, [fileId]: 100 }));

                                // Remove the progress bar after showing 100%
                                setTimeout(() => {
                                    setUploadProgress(prev => {
                                        const newProgress = { ...prev };
                                        delete newProgress[fileId];
                                        return newProgress;
                                    });
                                }, 500);
                            }, 200);

                            return prev;
                        }
                        return { ...prev, [fileId]: currentProgress + 10 };
                    });
                }, 100);
            });

            onFileUpload(validFiles);
        }
    }, [onFileUpload, maxFileSize, maxTotalSize, currentTotalSize]);

    const handleDragOver = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        setIsDragOver(true);
    }, []);

    const handleDragLeave = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        setIsDragOver(false);
    }, []);

    const handleDrop = useCallback((e: React.DragEvent) => {
        e.preventDefault();
        setIsDragOver(false);

        const files = e.dataTransfer.files;
        if (files.length > 0) {
            handleFiles(files);
        }
    }, [handleFiles]);

    const handleFileInput = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;
        if (files && files.length > 0) {
            handleFiles(files);
        }
        // Reset input value to allow selecting the same file again
        e.target.value = '';
    }, [handleFiles]);

    const formatFileSize = (bytes: number): string => {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    const remainingSize = maxTotalSize - currentTotalSize;

    return (
        <div className="space-y-4">
            {/* Upload Area */}
            <div
                className={`
                    border-2 border-dashed rounded-lg p-8 text-center transition-all duration-200 cursor-pointer
                    ${isDragOver
                        ? 'border-primary bg-primary/10 scale-[1.02] shadow-lg'
                        : 'border-base-300 hover:border-primary/50 hover:bg-base-200/50 hover:shadow-md'
                    }
                `}
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                onClick={() => document.getElementById('file-input')?.click()}
            >
                <input
                    id="file-input"
                    type="file"
                    multiple
                    className="hidden"
                    onChange={handleFileInput}
                    accept={ALLOWED_EXTENSIONS.join(',')}
                />

                <div className="space-y-4">
                    <div className={`mx-auto w-16 h-16 text-base-content/40 transition-transform duration-200 ${isDragOver ? 'scale-110 text-primary' : ''}`}>
                        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                                d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                        </svg>
                    </div>

                    <div>
                        <p className="text-lg font-medium">Drop files here or click to browse</p>
                        <p className="text-sm text-base-content/60 mt-2">
                            Supported: PDF, DOC, DOCX, TXT, ZIP, Images (JPG, PNG, GIF, WebP)
                        </p>
                        <p className="text-sm text-base-content/60">
                            Max file size: {formatFileSize(maxFileSize)} |
                            Remaining space: {formatFileSize(remainingSize)}
                        </p>
                    </div>
                </div>
            </div>

            {/* Upload Progress */}
            {Object.keys(uploadProgress).length > 0 && (
                <div className="space-y-2">
                    <h4 className="font-medium">Uploading...</h4>
                    {Object.entries(uploadProgress).map(([fileId, progress]) => (
                        <div key={fileId} className="space-y-1">
                            <div className="flex justify-between text-sm">
                                <span className="truncate">{fileId.split(/\d+$/)[0]}</span>
                                <span>{progress}%</span>
                            </div>
                            <progress className="progress progress-primary w-full" value={progress} max="100"></progress>
                        </div>
                    ))}
                </div>
            )}

            {/* Errors */}
            {errors.length > 0 && (
                <div className="alert alert-error">
                    <svg className="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2"
                            d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <div>
                        <h3 className="font-bold">Upload Errors:</h3>
                        <ul className="list-disc list-inside">
                            {errors.map((error, index) => (
                                <li key={index} className="text-sm">{error}</li>
                            ))}
                        </ul>
                    </div>
                    <button
                        className="btn btn-sm btn-ghost"
                        onClick={() => setErrors([])}
                    >
                        Dismiss
                    </button>
                </div>
            )}
        </div>
    );
}
