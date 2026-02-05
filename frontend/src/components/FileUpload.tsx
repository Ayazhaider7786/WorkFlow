import { useRef, useState } from 'react';
import { PaperClipIcon, XMarkIcon, PhotoIcon, VideoCameraIcon, DocumentIcon } from '@heroicons/react/24/outline';
import toast from 'react-hot-toast';

interface FileUploadProps {
  files: File[];
  onChange: (files: File[]) => void;
  maxImageSize?: number; // MB
  maxVideoSize?: number; // MB
  maxDocSize?: number; // MB
}

export default function FileUpload({ 
  files, 
  onChange, 
  maxImageSize = 10, 
  maxVideoSize = 70,
  maxDocSize = 25
}: FileUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = Array.from(e.target.files || []);
    
    const validFiles: File[] = [];
    
    for (const file of selectedFiles) {
      const isImage = file.type.startsWith('image/');
      const isVideo = file.type.startsWith('video/');
      const isDoc = file.type.includes('word') || file.type.includes('excel') || file.type.includes('spreadsheet') || 
                    file.type === 'application/pdf' || file.type.includes('document');
      const sizeMB = file.size / (1024 * 1024);
      
      if (isImage && sizeMB > maxImageSize) {
        toast.error(`Image "${file.name}" exceeds ${maxImageSize}MB limit`);
        continue;
      }
      
      if (isVideo && sizeMB > maxVideoSize) {
        toast.error(`Video "${file.name}" exceeds ${maxVideoSize}MB limit`);
        continue;
      }

      if (isDoc && sizeMB > maxDocSize) {
        toast.error(`Document "${file.name}" exceeds ${maxDocSize}MB limit`);
        continue;
      }
      
      validFiles.push(file);
    }
    
    onChange([...files, ...validFiles]);
    
    // Reset input
    if (inputRef.current) {
      inputRef.current.value = '';
    }
  };

  const handleRemove = (index: number) => {
    onChange(files.filter((_, i) => i !== index));
  };

  const getFileIcon = (file: File) => {
    if (file.type.startsWith('image/')) {
      return <PhotoIcon className="h-5 w-5 text-blue-500" />;
    }
    if (file.type.startsWith('video/')) {
      return <VideoCameraIcon className="h-5 w-5 text-purple-500" />;
    }
    return <DocumentIcon className="h-5 w-5 text-gray-500" />;
  };

  const formatSize = (bytes: number) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  return (
    <div className="space-y-3">
      {/* Upload button */}
      <div>
        <input
          ref={inputRef}
          type="file"
          multiple
          accept="image/*,video/*,.pdf,.doc,.docx,.xls,.xlsx,.txt"
          onChange={handleFileSelect}
          className="hidden"
          id="file-upload"
        />
        <label
          htmlFor="file-upload"
          className="inline-flex items-center gap-2 px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 cursor-pointer"
        >
          <PaperClipIcon className="h-5 w-5 text-gray-400" />
          Attach files
        </label>
        <p className="mt-1 text-xs text-gray-500">
          Images: {maxImageSize}MB, Videos: {maxVideoSize}MB, Docs (PDF/Word/Excel): {maxDocSize}MB
        </p>
      </div>

      {/* File list */}
      {files.length > 0 && (
        <ul className="divide-y divide-gray-200 border border-gray-200 rounded-md">
          {files.map((file, index) => (
            <li key={index} className="flex items-center justify-between py-2 px-3">
              <div className="flex items-center gap-2 min-w-0">
                {getFileIcon(file)}
                <span className="text-sm text-gray-700 truncate">{file.name}</span>
                <span className="text-xs text-gray-400">({formatSize(file.size)})</span>
              </div>
              <button
                type="button"
                onClick={() => handleRemove(index)}
                className="text-gray-400 hover:text-red-500"
              >
                <XMarkIcon className="h-5 w-5" />
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
