import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon } from '@heroicons/react/24/outline';
import { workItemsApi, workflowStatusesApi, sprintsApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useNotification } from '../context/NotificationContext';
import type { WorkItem, WorkflowStatus, Sprint } from '../types';
import TicketCard from '../components/TicketCard';
import TicketModal from '../components/TicketModal';

export default function Board() {
  const [workItems, setWorkItems] = useState<WorkItem[]>([]);
  const [statuses, setStatuses] = useState<WorkflowStatus[]>([]);
  const [sprints, setSprints] = useState<Sprint[]>([]);
  const [selectedSprint, setSelectedSprint] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [selectedItem, setSelectedItem] = useState<WorkItem | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const { currentProject } = useProject();
  const { notifySuccess, notifyError } = useNotification();
  const navigate = useNavigate();

  useEffect(() => {
    if (!currentProject) {
      navigate('/projects');
      return;
    }
    loadData();
  }, [currentProject, selectedSprint]);

  const loadData = async () => {
    if (!currentProject) return;
    try {
      const [statusesRes, sprintsRes] = await Promise.all([
        workflowStatusesApi.getAll(currentProject.id),
        sprintsApi.getAll(currentProject.id),
      ]);
      setStatuses(statusesRes.data);
      setSprints(sprintsRes.data);

      const itemsRes = await workItemsApi.getAll(currentProject.id, { sprintId: selectedSprint || undefined });
      setWorkItems(itemsRes.data);
    } catch (error) {
      notifyError('Error', 'Failed to load board data');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDragStart = (e: React.DragEvent, item: WorkItem) => {
    e.dataTransfer.setData('workItemId', item.id.toString());
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDrop = async (e: React.DragEvent, statusId: number) => {
    e.preventDefault();
    const workItemId = parseInt(e.dataTransfer.getData('workItemId'));
    const item = workItems.find(w => w.id === workItemId);
    if (!item || item.statusId === statusId || !currentProject) return;

    const newStatus = statuses.find(s => s.id === statusId);
    try {
      await workItemsApi.update(currentProject.id, workItemId, { statusId });
      setWorkItems(workItems.map(w => w.id === workItemId ? { ...w, statusId, statusName: newStatus?.name || '' } : w));
      notifySuccess('Status Updated', `${item.itemKey} moved to ${newStatus?.name}`);
    } catch (error) {
      notifyError('Error', 'Failed to update status');
    }
  };

  const openCreateModal = () => {
    setSelectedItem(null);
    setIsCreating(true);
    setShowModal(true);
  };

  const openEditModal = (item: WorkItem) => {
    setSelectedItem(item);
    setIsCreating(false);
    setShowModal(true);
  };

  const handleCreate = (newItem: WorkItem) => {
    setWorkItems([newItem, ...workItems]);
  };

  const handleUpdate = (updatedItem: WorkItem) => {
    setWorkItems(workItems.map(w => w.id === updatedItem.id ? updatedItem : w));
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Board</h1>
          <p className="text-sm text-gray-500">{currentProject?.name}</p>
        </div>
        <div className="flex items-center gap-3">
          <select
            value={selectedSprint || ''}
            onChange={(e) => setSelectedSprint(e.target.value ? parseInt(e.target.value) : null)}
            className="block w-40 rounded-md border-gray-300 text-sm focus:border-indigo-500 focus:ring-indigo-500"
          >
            <option value="">All Items</option>
            {sprints.filter(s => s.status !== 2).map((sprint) => (
              <option key={sprint.id} value={sprint.id}>{sprint.name}</option>
            ))}
          </select>
          <button
            onClick={openCreateModal}
            className="inline-flex items-center px-3 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
          >
            <PlusIcon className="-ml-0.5 mr-1.5 h-5 w-5" />
            Create Ticket
          </button>
        </div>
      </div>

      {/* Kanban Board */}
      <div className="flex overflow-x-auto pb-4 gap-3">
        {statuses.map((status) => (
          <div
            key={status.id}
            className="flex-shrink-0 w-64"
            onDragOver={handleDragOver}
            onDrop={(e) => handleDrop(e, status.id)}
          >
            <div className="bg-gray-100 rounded-lg p-3">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <div className="w-2.5 h-2.5 rounded-full" style={{ backgroundColor: status.color }}></div>
                  <h3 className="font-medium text-gray-900 text-sm">{status.name}</h3>
                </div>
                <span className="bg-gray-200 text-gray-600 text-xs font-medium px-1.5 py-0.5 rounded">
                  {workItems.filter(w => w.statusId === status.id).length}
                </span>
              </div>
              
              <div className="space-y-2 min-h-[150px]">
                {workItems
                  .filter((item) => item.statusId === status.id)
                  .map((item) => (
                    <TicketCard
                      key={item.id}
                      itemKey={item.itemKey}
                      title={item.title}
                      type={item.type}
                      priority={item.priority}
                      assignedToName={item.assignedToName}
                      estimatedHours={item.estimatedHours}
                      commentCount={item.commentCount}
                      attachmentCount={item.attachmentCount}
                      childCount={item.childCount}
                      onClick={() => openEditModal(item)}
                      onDragStart={(e) => handleDragStart(e, item)}
                    />
                  ))}
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Ticket Modal */}
      <TicketModal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        workItem={selectedItem}
        onUpdate={handleUpdate}
        onCreate={handleCreate}
        isCreating={isCreating}
      />
    </div>
  );
}
