import { useState, useEffect } from 'react';
import { XMarkIcon, TrashIcon, ChatBubbleLeftIcon, ClockIcon, DocumentTextIcon, ArrowPathIcon } from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import { fileTicketsApi, activityLogsApi, projectsApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';
import RichTextEditor from './RichTextEditor';
import type { FileTicket, ActivityLog, ProjectMember } from '../types';

interface FileTicketModalProps {
  isOpen: boolean;
  onClose: () => void;
  fileTicket: FileTicket | null;
  onUpdate: (fileTicket: FileTicket) => void;
  onCreate: (fileTicket: FileTicket) => void;
  isCreating?: boolean;
}

const FileTicketType = { Physical: 0, Digital: 1 };
const FileTicketStatus = { Created: 0, InTransit: 1, Received: 2, Processing: 3, Approved: 4, Rejected: 5, Completed: 6, Lost: 7 };
const FileTicketStatusLabels: Record<number, string> = { 0: 'Created', 1: 'In Transit', 2: 'Received', 3: 'Processing', 4: 'Approved', 5: 'Rejected', 6: 'Completed', 7: 'Lost' };

const tabs = ['Details', 'Comments', 'Activity'];

export default function FileTicketModal({ isOpen, onClose, fileTicket, onUpdate, onCreate, isCreating = false }: FileTicketModalProps) {
  const [activeTab, setActiveTab] = useState(0);
  const [comments, setComments] = useState<{ id: number; content: string; authorName: string; createdAt: string }[]>([]);
  const [activities, setActivities] = useState<ActivityLog[]>([]);
  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [newComment, setNewComment] = useState('');
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    type: FileTicketType.Physical,
    currentHolderId: '',
  });
  const { currentProject } = useProject();
  const { user } = useAuth();
  const { notifySuccess, notifyError, notifyInfo } = useNotification();

  useEffect(() => {
    if (isOpen && currentProject) {
      loadMembers();
      if (fileTicket && !isCreating) {
        setFormData({
          title: fileTicket.title,
          description: fileTicket.description || '',
          type: fileTicket.type,
          currentHolderId: fileTicket.currentHolderId?.toString() || '',
        });
        loadTicketData();
      } else {
        resetForm();
      }
    }
  }, [isOpen, fileTicket, currentProject, isCreating]);

  const loadMembers = async () => {
    if (!currentProject) return;
    try {
      const res = await projectsApi.getMembers(currentProject.id);
      setMembers(res.data);
    } catch (error) {
      console.error('Failed to load members');
    }
  };

  const loadTicketData = async () => {
    if (!currentProject || !fileTicket) return;
    setIsLoading(true);
    try {
      const activitiesRes = await activityLogsApi.getFileTicketLogs(currentProject.id, fileTicket.id);
      setActivities(activitiesRes.data);
      // Comments would need backend endpoint - for now using activity as proxy
      const commentActivities = activitiesRes.data.filter(a => a.action === 'Commented');
      setComments(commentActivities.map(a => ({
        id: a.id,
        content: a.description || '',
        authorName: a.userName,
        createdAt: a.timestamp
      })));
    } catch (error) {
      console.error('Failed to load ticket data');
    } finally {
      setIsLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({ title: '', description: '', type: FileTicketType.Physical, currentHolderId: '' });
    setComments([]);
    setActivities([]);
    setNewComment('');
    setActiveTab(0);
  };

  const handleSave = async () => {
    if (!currentProject || !formData.title.trim()) {
      notifyError('Validation Error', 'Title is required');
      return;
    }

    try {
      if (isCreating) {
        const response = await fileTicketsApi.create(currentProject.id, {
          title: formData.title,
          description: formData.description || undefined,
          type: formData.type,
          currentHolderId: formData.currentHolderId ? parseInt(formData.currentHolderId) : undefined,
        });
        onCreate(response.data);
        notifySuccess('File Ticket Created', `${response.data.ticketNumber} has been created`);
      } else if (fileTicket) {
        const response = await fileTicketsApi.update(currentProject.id, fileTicket.id, {
          title: formData.title,
          description: formData.description || undefined,
        });
        onUpdate(response.data);
        notifySuccess('File Ticket Updated', 'Changes have been saved');
      }
      onClose();
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to save file ticket');
    }
  };

  const handleTransfer = async () => {
    if (!currentProject || !fileTicket || !formData.currentHolderId) {
      notifyError('Error', 'Please select a user to transfer to');
      return;
    }

    try {
      const response = await fileTicketsApi.transfer(currentProject.id, fileTicket.id, {
        toUserId: parseInt(formData.currentHolderId),
        notes: 'Transferred via ticket modal'
      });
      onUpdate(response.data);
      notifySuccess('File Transferred', `Transferred to ${members.find(m => m.userId === parseInt(formData.currentHolderId))?.userName}`);
      loadTicketData(); // Refresh activity
    } catch (error: any) {
      notifyError('Transfer Failed', error.response?.data?.message || 'Failed to transfer file');
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75" onClick={onClose} />
        
        <div className="relative bg-white rounded-lg shadow-xl w-full max-w-4xl max-h-[90vh] flex flex-col">
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
            <div>
              <h2 className="text-xl font-semibold text-gray-900">
                {isCreating ? 'Create File Ticket' : fileTicket?.ticketNumber}
              </h2>
              {fileTicket && !isCreating && (
                <p className="text-sm text-gray-500">
                  Status: <span className="font-medium">{FileTicketStatusLabels[fileTicket.status]}</span>
                  {fileTicket.currentHolderName && ` • Held by ${fileTicket.currentHolderName}`}
                </p>
              )}
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
                  className={`py-3 px-4 text-sm font-medium border-b-2 ${
                    activeTab === index
                      ? 'border-indigo-500 text-indigo-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  } ${isCreating && index > 0 ? 'opacity-50 cursor-not-allowed' : ''}`}
                >
                  <div className="flex items-center gap-2">
                    {index === 0 && <DocumentTextIcon className="h-4 w-4" />}
                    {index === 1 && <ChatBubbleLeftIcon className="h-4 w-4" />}
                    {index === 2 && <ClockIcon className="h-4 w-4" />}
                    {tab}
                  </div>
                </button>
              ))}
            </nav>
          </div>

          {/* Content */}
          <div className="flex-1 overflow-y-auto p-6">
            {/* Details Tab */}
            {activeTab === 0 && (
              <div className="space-y-5">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Title *</label>
                  <input
                    type="text"
                    value={formData.title}
                    onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                    placeholder="Enter file ticket title"
                    className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                  <RichTextEditor
                    value={formData.description}
                    onChange={(val) => setFormData({ ...formData, description: val })}
                    placeholder="Describe the file..."
                    minHeight="120px"
                  />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                    <select
                      value={formData.type}
                      onChange={(e) => setFormData({ ...formData, type: parseInt(e.target.value) })}
                      disabled={!isCreating}
                      className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-100"
                    >
                      <option value={0}>Physical</option>
                      <option value={1}>Digital</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {isCreating ? 'Assign To' : 'Transfer To'}
                    </label>
                    <div className="flex gap-2">
                      <select
                        value={formData.currentHolderId}
                        onChange={(e) => setFormData({ ...formData, currentHolderId: e.target.value })}
                        className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                      >
                        <option value="">Select user...</option>
                        {members.map((m) => (
                          <option key={m.userId} value={m.userId}>{m.userName}</option>
                        ))}
                      </select>
                      {!isCreating && fileTicket && (
                        <button
                          onClick={handleTransfer}
                          disabled={!formData.currentHolderId}
                          className="px-3 py-2 bg-indigo-100 text-indigo-700 rounded-md hover:bg-indigo-200 disabled:opacity-50 disabled:cursor-not-allowed"
                          title="Transfer file"
                        >
                          <ArrowPathIcon className="h-5 w-5" />
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Comments Tab */}
            {activeTab === 1 && (
              <div className="space-y-4">
                <div>
                  <RichTextEditor
                    value={newComment}
                    onChange={setNewComment}
                    placeholder="Add a comment..."
                    minHeight="80px"
                  />
                  <button
                    onClick={() => {
                      if (newComment.trim()) {
                        notifyInfo('Coming Soon', 'File ticket comments will be available in the next update');
                        setNewComment('');
                      }
                    }}
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
                        <div className="flex items-center gap-2 mb-2">
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
                        <div className="text-sm text-gray-700" dangerouslySetInnerHTML={{ __html: comment.content }} />
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
                                  {activity.description || `${activity.action} this file ticket`}
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
          <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-gray-50">
            <button
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={handleSave}
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
            >
              {isCreating ? 'Create File Ticket' : 'Save Changes'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
