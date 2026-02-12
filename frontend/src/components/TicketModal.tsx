import { useState, useEffect, useCallback } from 'react';
import { XMarkIcon, TrashIcon, ChatBubbleLeftIcon, ClockIcon, DocumentTextIcon, ArrowDownTrayIcon, LinkIcon, ExclamationTriangleIcon } from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import { workItemsApi, commentsApi, attachmentsApi, activityLogsApi, projectsApi, workflowStatusesApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';
import RichTextEditor from './RichTextEditor';
import FileUpload from './FileUpload';
import type { WorkItem, Comment, Attachment, ActivityLog, ProjectMember, WorkflowStatus } from '../types';

interface TicketModalProps {
  isOpen: boolean;
  onClose: () => void;
  workItem: WorkItem | null;
  onUpdate: (workItem: WorkItem) => void;
  onCreate: (workItem: WorkItem) => void;
  isCreating?: boolean;
  defaultParentId?: number;
}

const Priority = { Low: 0, Medium: 1, High: 2, Critical: 3 };
const WorkItemTypes = { Epic: 0, Feature: 1, Story: 2, Task: 3, Bug: 4, Subtask: 5 };
const WorkItemTypeLabels: Record<number, string> = { 0: 'Epic', 1: 'Feature', 2: 'Story', 3: 'Task', 4: 'Bug', 5: 'Subtask' };
const WorkItemTypeColors: Record<number, string> = {
  0: 'bg-purple-100 text-purple-800',
  1: 'bg-blue-100 text-blue-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-gray-100 text-gray-800',
  4: 'bg-red-100 text-red-800',
  5: 'bg-yellow-100 text-yellow-800'
};

const tabs = ['Details', 'Comments', 'Activity'];

export default function TicketModal({ isOpen, onClose, workItem, onUpdate, onCreate, isCreating = false, defaultParentId }: TicketModalProps) {
  const [activeTab, setActiveTab] = useState(0);
  const [comments, setComments] = useState<Comment[]>([]);
  const [attachments, setAttachments] = useState<Attachment[]>([]);
  const [activities, setActivities] = useState<ActivityLog[]>([]);
  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [statuses, setStatuses] = useState<WorkflowStatus[]>([]);
  const [parentItems, setParentItems] = useState<WorkItem[]>([]);
  // const [isLoading, setIsLoading] = useState(false);
  const [newComment, setNewComment] = useState('');
  const [pendingFiles, setPendingFiles] = useState<File[]>([]);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    type: WorkItemTypes.Task,
    priority: Priority.Medium,
    estimatedHours: '',
    assignedToId: '',
    parentId: '',
    statusId: '',
    isBlocked: false,
  });
  const { currentProject } = useProject();
  const { user } = useAuth();
  const { notifySuccess, notifyError, notifyInfo } = useNotification();

  const loadTicketData = useCallback(async () => {
    if (!currentProject || !workItem) return;
    // setIsLoading(true);
    try {
      const [commentsRes, attachmentsRes, activitiesRes] = await Promise.all([
        commentsApi.getAll(currentProject.id, workItem.id),
        attachmentsApi.getAll(currentProject.id, workItem.id),
        activityLogsApi.getWorkItemLogs(currentProject.id, workItem.id),
      ]);
      setComments(commentsRes.data);
      setAttachments(attachmentsRes.data);
      setActivities(activitiesRes.data);
    } catch (error) {
      console.error('Failed to load ticket data');
    } finally {
      // setIsLoading(false);
    }
  }, [currentProject, workItem]);

  useEffect(() => {
    if (isOpen && currentProject) {
      loadMembers();
      loadStatuses();
      loadParentItems();
      if (workItem && !isCreating) {
        const isBlocked = workItem.title.startsWith('[BLOCKED] ');
        setFormData({
          title: isBlocked ? workItem.title.substring(10) : workItem.title,
          description: workItem.description || '',
          type: workItem.type,
          priority: workItem.priority,
          estimatedHours: workItem.estimatedHours?.toString() || '',
          assignedToId: workItem.assignedToId?.toString() || '',
          parentId: workItem.parentId?.toString() || '',
          statusId: workItem.statusId?.toString() || '',
          isBlocked: isBlocked,
        });
        loadTicketData();
      } else {
        resetForm();
      }
    }
  }, [isOpen, workItem, currentProject, isCreating, loadTicketData]);

  const loadMembers = async () => {
    if (!currentProject) return;
    try {
      const res = await projectsApi.getMembers(currentProject.id);
      setMembers(res.data);
    } catch (error) {
      console.error('Failed to load members');
    }
  };

  const loadStatuses = async () => {
    if (!currentProject) return;
    try {
      const res = await workflowStatusesApi.getAll(currentProject.id);
      setStatuses(res.data.sort((a, b) => a.order - b.order));

      // Set default status for new items if not set
      if (isCreating && !formData.statusId && res.data.length > 0) {
        setFormData(prev => ({ ...prev, statusId: res.data[0].id.toString() }));
      }
    } catch (error) {
      console.error('Failed to load statuses');
    }
  };

  const loadParentItems = async () => {
    if (!currentProject) return;
    try {
      const res = await workItemsApi.getAll(currentProject.id, {});
      // Filter to items that can be parents (Epic, Feature, Story, Task, Bug)
      setParentItems(res.data.filter(w => w.type <= WorkItemTypes.Bug));
    } catch (error) {
      console.error('Failed to load parent items');
    }
  };

  const resetForm = () => {
    setFormData({
      title: '',
      description: '',
      type: WorkItemTypes.Task,
      priority: Priority.Medium,
      estimatedHours: '',
      assignedToId: '',
      parentId: defaultParentId?.toString() || '',
      statusId: '',
      isBlocked: false,
    });
    setComments([]);
    setAttachments([]);
    setActivities([]);
    setPendingFiles([]);
    setNewComment('');
    setActiveTab(0);
  };

  const handleSave = async () => {
    if (!currentProject || !formData.title.trim()) {
      notifyError('Validation Error', 'Title is required');
      return;
    }

    const cleanTitle = formData.title.trim();
    const finalTitle = formData.isBlocked ? `[BLOCKED] ${cleanTitle}` : cleanTitle;

    try {
      if (isCreating) {
        const response = await workItemsApi.create(currentProject.id, {
          title: finalTitle,
          description: formData.description || undefined,
          type: formData.type,
          priority: formData.priority,
          estimatedHours: formData.estimatedHours ? parseFloat(formData.estimatedHours) : undefined,
          assignedToId: formData.assignedToId ? parseInt(formData.assignedToId) : undefined,
          parentId: formData.parentId ? parseInt(formData.parentId) : undefined,
        });

        for (const file of pendingFiles) {
          await attachmentsApi.upload(currentProject.id, response.data.id, file);
        }
        onCreate(response.data);
        notifySuccess('Ticket Created', `${response.data.itemKey} has been created`);
        onClose();
      } else if (workItem) {
        const response = await workItemsApi.update(currentProject.id, workItem.id, {
          title: finalTitle,
          description: formData.description || undefined,
          type: formData.type,
          priority: formData.priority,
          estimatedHours: formData.estimatedHours ? parseFloat(formData.estimatedHours) : undefined,
          assignedToId: formData.assignedToId ? parseInt(formData.assignedToId) : undefined,
          parentId: formData.parentId ? parseInt(formData.parentId) : 0,
          statusId: formData.statusId ? parseInt(formData.statusId) : undefined,
        });
        for (const file of pendingFiles) {
          await attachmentsApi.upload(currentProject.id, workItem.id, file);
        }
        setPendingFiles([]);
        onUpdate(response.data);
        notifySuccess('Ticket Updated', 'Changes have been saved');
        loadTicketData();
      }
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to save ticket');
    }
  };

  const handleAddComment = async () => {
    if (!currentProject || !workItem || !newComment.trim()) return;
    try {
      const response = await commentsApi.create(currentProject.id, workItem.id, { content: newComment });
      setComments([response.data, ...comments]);
      setNewComment('');
      notifySuccess('Comment Added', 'Your comment has been posted');
      const activitiesRes = await activityLogsApi.getWorkItemLogs(currentProject.id, workItem.id);
      setActivities(activitiesRes.data);
    } catch (error) {
      notifyError('Error', 'Failed to add comment');
    }
  };

  const handleDeleteComment = async (commentId: number) => {
    if (!currentProject || !workItem || !confirm('Delete this comment?')) return;
    try {
      await commentsApi.delete(currentProject.id, workItem.id, commentId);
      setComments(comments.filter(c => c.id !== commentId));
      notifySuccess('Comment Deleted', 'The comment has been removed');
    } catch (error) {
      notifyError('Error', 'Failed to delete comment');
    }
  };

  const handleUploadFile = async (file: File) => {
    if (!currentProject || !workItem) return;
    try {
      const response = await attachmentsApi.upload(currentProject.id, workItem.id, file);
      setAttachments([response.data, ...attachments]);
      notifySuccess('File Uploaded', `${file.name} has been attached`);
      const activitiesRes = await activityLogsApi.getWorkItemLogs(currentProject.id, workItem.id);
      setActivities(activitiesRes.data);
    } catch (error) {
      notifyError('Upload Failed', `Failed to upload ${file.name}`);
    }
  };

  const handleDeleteAttachment = async (attachmentId: number) => {
    if (!currentProject || !workItem || !confirm('Delete this attachment?')) return;
    try {
      await attachmentsApi.delete(currentProject.id, workItem.id, attachmentId);
      setAttachments(attachments.filter(a => a.id !== attachmentId));
      notifySuccess('Attachment Deleted', 'The file has been removed');
    } catch (error) {
      notifyError('Error', 'Failed to delete attachment');
    }
  };

  const handleDownload = (attachment: Attachment) => {
    if (!currentProject || !workItem) return;
    const url = attachmentsApi.getDownloadUrl(currentProject.id, workItem.id, attachment.id);
    window.open(url, '_blank');
    notifyInfo('Downloading', `Downloading ${attachment.fileName}...`);
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  const getFileIcon = (contentType: string) => {
    if (contentType.startsWith('image/')) return '🖼️';
    if (contentType.startsWith('video/')) return '🎬';
    if (contentType.includes('pdf')) return '📄';
    if (contentType.includes('word') || contentType.includes('document')) return '📝';
    if (contentType.includes('excel') || contentType.includes('spreadsheet')) return '📊';
    return '📎';
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75" onClick={onClose} />

        <div className="relative bg-white rounded-lg shadow-xl w-full max-w-4xl max-h-[90vh] flex flex-col">
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-3 border-b border-gray-200">
            <div className="flex items-center gap-3">
              {workItem && !isCreating && (
                <span className={`px-2 py-0.5 rounded text-xs font-medium ${WorkItemTypeColors[workItem.type]}`}>
                  {WorkItemTypeLabels[workItem.type]}
                </span>
              )}
              <h2 className="text-lg font-semibold text-gray-900">
                {isCreating ? 'Create Ticket' : workItem?.itemKey}
              </h2>
            </div>
            <button onClick={onClose} className="text-gray-400 hover:text-gray-500">
              <XMarkIcon className="h-6 w-6" />
            </button>
          </div>

          {/* Tabs */}
          <div className="border-b border-gray-200">
            <nav className="flex px-6 -mb-px">
              {tabs.map((tab, index) => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(index)}
                  disabled={isCreating && index > 0}
                  className={`py-2 px-3 text-sm font-medium border-b-2 ${activeTab === index
                    ? 'border-indigo-500 text-indigo-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    } ${isCreating && index > 0 ? 'opacity-50 cursor-not-allowed' : ''}`}
                >
                  <div className="flex items-center gap-1.5">
                    {index === 0 && <DocumentTextIcon className="h-4 w-4" />}
                    {index === 1 && <ChatBubbleLeftIcon className="h-4 w-4" />}
                    {index === 2 && <ClockIcon className="h-4 w-4" />}
                    {tab}
                    {index === 1 && comments.length > 0 && (
                      <span className="bg-gray-100 text-gray-600 text-xs px-1.5 py-0.5 rounded-full">{comments.length}</span>
                    )}
                  </div>
                </button>
              ))}
            </nav>
          </div>

          {/* Content */}
          <div className="flex-1 overflow-y-auto p-6">
            {/* Details Tab */}
            {activeTab === 0 && (
              <div className="grid grid-cols-3 gap-6">
                {/* Left column - Main content (2/3 width) */}
                <div className="col-span-2 space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Title *</label>
                    <input
                      type="text"
                      value={formData.title}
                      onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                      placeholder="Enter ticket title"
                      className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    />
                  </div>

                  <div className="flex-1">
                    <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                    <RichTextEditor
                      value={formData.description}
                      onChange={(val) => setFormData({ ...formData, description: val })}
                      placeholder="Describe the ticket..."
                      minHeight="200px"
                    />
                  </div>

                  {/* Attachments */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Attachments</label>
                    {isCreating ? (
                      <FileUpload files={pendingFiles} onChange={setPendingFiles} />
                    ) : (
                      <div className="space-y-2">
                        <input
                          type="file"
                          multiple
                          accept="image/*,video/*,.pdf,.doc,.docx,.xls,.xlsx"
                          onChange={(e) => {
                            const files = Array.from(e.target.files || []);
                            files.forEach(handleUploadFile);
                            e.target.value = '';
                          }}
                          className="block w-full text-sm text-gray-500 file:mr-4 file:py-1.5 file:px-3 file:rounded-md file:border-0 file:text-sm file:font-medium file:bg-indigo-50 file:text-indigo-700 hover:file:bg-indigo-100"
                        />
                      </div>
                    )}

                    {attachments.length > 0 && (
                      <ul className="mt-2 divide-y divide-gray-200 border border-gray-200 rounded-md">
                        {attachments.map((att) => (
                          <li key={att.id} className="flex items-center justify-between py-2 px-3 hover:bg-gray-50">
                            <div className="flex items-center gap-2 min-w-0 flex-1">
                              <span className="text-base">{getFileIcon(att.contentType)}</span>
                              <span className="text-sm text-gray-700 truncate">{att.fileName}</span>
                              <span className="text-xs text-gray-400">({formatFileSize(att.fileSize)})</span>
                            </div>
                            <div className="flex items-center gap-1">
                              <button onClick={() => handleDownload(att)} className="p-1 text-indigo-600 hover:text-indigo-800" title="Download">
                                <ArrowDownTrayIcon className="h-4 w-4" />
                              </button>
                              <button onClick={() => handleDeleteAttachment(att.id)} className="p-1 text-gray-400 hover:text-red-500" title="Delete">
                                <TrashIcon className="h-4 w-4" />
                              </button>
                            </div>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                </div>

                {/* Right column - Metadata (1/3 width) */}
                <div className="space-y-4">
                  {/* Status Dropdown */}
                  {!isCreating && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                      <select
                        value={formData.statusId}
                        onChange={(e) => setFormData({ ...formData, statusId: e.target.value })}
                        className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                      >
                        {statuses
                          .filter(status => status.name !== 'Blocked')
                          .map((status) => (
                            <option key={status.id} value={status.id}>
                              {status.name}
                            </option>
                          ))}
                      </select>
                    </div>
                  )}

                  {/* Blocked Toggle */}
                  <div className="flex items-center justify-between bg-red-50 p-3 rounded-md border border-red-100">
                    <div className="flex items-center gap-2">
                      <ExclamationTriangleIcon className="h-5 w-5 text-red-500" />
                      <span className="text-sm font-medium text-red-700">Blocked</span>
                    </div>
                    <label className="relative inline-flex items-center cursor-pointer">
                      <input
                        type="checkbox"
                        checked={formData.isBlocked}
                        onChange={(e) => setFormData({ ...formData, isBlocked: e.target.checked })}
                        className="sr-only peer"
                      />
                      <div className="w-9 h-5 bg-gray-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-red-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-red-600"></div>
                    </label>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                    <select
                      value={formData.type}
                      onChange={(e) => setFormData({ ...formData, type: parseInt(e.target.value) })}
                      className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    >
                      {Object.entries(WorkItemTypeLabels).map(([value, label]) => (
                        <option key={value} value={value}>{label}</option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Priority</label>
                    <select
                      value={formData.priority}
                      onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) })}
                      className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    >
                      <option value={0}>Low</option>
                      <option value={1}>Medium</option>
                      <option value={2}>High</option>
                      <option value={3}>Critical</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Estimated Hours</label>
                    <input
                      type="number"
                      step="0.5"
                      value={formData.estimatedHours}
                      onChange={(e) => setFormData({ ...formData, estimatedHours: e.target.value })}
                      className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Assignee</label>
                    <select
                      value={formData.assignedToId}
                      onChange={(e) => setFormData({ ...formData, assignedToId: e.target.value })}
                      className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    >
                      <option value="">Unassigned</option>
                      {members.map((m) => (
                        <option key={m.userId} value={m.userId}>{m.userName}</option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      <span className="flex items-center gap-1">
                        <LinkIcon className="h-4 w-4" />
                        Parent Item
                      </span>
                    </label>
                    <select
                      value={formData.parentId}
                      onChange={(e) => setFormData({ ...formData, parentId: e.target.value })}
                      className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    >
                      <option value="">No parent</option>
                      {parentItems
                        .filter(p => p.id !== workItem?.id)
                        .map((p) => (
                          <option key={p.id} value={p.id}>
                            {p.itemKey} - {p.title.substring(0, 30)}{p.title.length > 30 ? '...' : ''}
                          </option>
                        ))}
                    </select>
                  </div>

                  {workItem && !isCreating && workItem.childCount > 0 && (
                    <div className="pt-2 border-t border-gray-200">
                      <p className="text-sm text-gray-500">{workItem.childCount} child item(s)</p>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Comments Tab */}
            {activeTab === 1 && (
              <div className="space-y-4">
                <div>
                  <RichTextEditor value={newComment} onChange={setNewComment} placeholder="Add a comment..." minHeight="80px" />
                  <button
                    onClick={handleAddComment}
                    disabled={!newComment.trim()}
                    className="mt-2 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-md hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Add Comment
                  </button>
                </div>

                <div className="space-y-3">
                  {comments.length === 0 ? (
                    <p className="text-sm text-gray-500 text-center py-8">No comments yet</p>
                  ) : (
                    comments.map((comment) => (
                      <div key={comment.id} className="bg-gray-50 rounded-lg p-4">
                        <div className="flex items-start justify-between">
                          <div className="flex items-center gap-2">
                            <div className="h-8 w-8 rounded-full bg-indigo-100 flex items-center justify-center">
                              <span className="text-xs font-medium text-indigo-600">
                                {comment.authorName.split(' ').map(n => n[0]).join('')}
                              </span>
                            </div>
                            <div>
                              <p className="text-sm font-medium text-gray-900">{comment.authorName}</p>
                              <p className="text-xs text-gray-500">{format(new Date(comment.createdAt), 'MMM d, yyyy h:mm a')}</p>
                            </div>
                          </div>
                          {comment.authorId === user?.id && (
                            <button onClick={() => handleDeleteComment(comment.id)} className="text-gray-400 hover:text-red-500">
                              <TrashIcon className="h-4 w-4" />
                            </button>
                          )}
                        </div>
                        <div className="mt-2 text-sm text-gray-700 prose prose-sm max-w-none" dangerouslySetInnerHTML={{ __html: comment.content }} />
                      </div>
                    ))
                  )}
                </div>
              </div>
            )}

            {/* Activity Tab */}
            {activeTab === 2 && (
              <div className="space-y-3">
                {activities.length === 0 ? (
                  <p className="text-sm text-gray-500 text-center py-8">No activity yet</p>
                ) : (
                  <div className="flow-root">
                    <ul className="-mb-8">
                      {activities.map((activity, idx) => (
                        <li key={activity.id}>
                          <div className="relative pb-8">
                            {idx !== activities.length - 1 && (
                              <span className="absolute left-4 top-4 -ml-px h-full w-0.5 bg-gray-200" />
                            )}
                            <div className="relative flex space-x-3">
                              <div className="h-8 w-8 rounded-full bg-gray-100 flex items-center justify-center ring-8 ring-white">
                                <ClockIcon className="h-4 w-4 text-gray-500" />
                              </div>
                              <div className="flex-1 min-w-0">
                                <p className="text-sm text-gray-700">
                                  <span className="font-medium">{activity.userName}</span>{' '}
                                  {activity.description || `${activity.action} this ticket`}
                                </p>
                                <p className="text-xs text-gray-500">
                                  {format(new Date(activity.timestamp), 'MMM d, yyyy h:mm a')}
                                </p>
                              </div>
                            </div>
                          </div>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="flex justify-end gap-3 px-6 py-3 border-t border-gray-200 bg-gray-50">
            <button
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
            >
              {isCreating ? 'Cancel' : 'Close'}
            </button>
            <button
              onClick={handleSave}
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
            >
              {isCreating ? 'Create Ticket' : 'Save Changes'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
