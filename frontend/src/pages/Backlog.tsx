import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon, ArrowRightIcon } from '@heroicons/react/24/outline';
import { workItemsApi, sprintsApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useNotification } from '../context/NotificationContext';
import TicketModal from '../components/TicketModal';
import type { WorkItem, Sprint } from '../types';

const Priority = { Low: 0, Medium: 1, High: 2, Critical: 3 };
const PriorityLabels: Record<number, string> = { 0: 'Low', 1: 'Medium', 2: 'High', 3: 'Critical' };
const WorkItemTypeLabels: Record<number, string> = { 0: 'Epic', 1: 'Feature', 2: 'Story', 3: 'Task', 4: 'Bug', 5: 'Subtask' };
const WorkItemTypeColors: Record<number, string> = { 
  0: 'bg-purple-100 text-purple-800', 
  1: 'bg-blue-100 text-blue-800', 
  2: 'bg-green-100 text-green-800', 
  3: 'bg-gray-100 text-gray-800', 
  4: 'bg-red-100 text-red-800', 
  5: 'bg-yellow-100 text-yellow-800' 
};

export default function Backlog() {
  const [backlogItems, setBacklogItems] = useState<WorkItem[]>([]);
  const [sprints, setSprints] = useState<Sprint[]>([]);
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
  }, [currentProject]);

  const loadData = async () => {
    if (!currentProject) return;
    try {
      const [itemsRes, sprintsRes] = await Promise.all([
        workItemsApi.getAll(currentProject.id, { backlogOnly: true }),
        sprintsApi.getAll(currentProject.id),
      ]);
      setBacklogItems(itemsRes.data);
      setSprints(sprintsRes.data.filter(s => s.status !== 2));
    } catch (error) {
      notifyError('Error', 'Failed to load backlog');
    } finally {
      setIsLoading(false);
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
    setBacklogItems([newItem, ...backlogItems]);
  };

  const handleUpdate = (updatedItem: WorkItem) => {
    if (updatedItem.isInBacklog) {
      setBacklogItems(backlogItems.map(i => i.id === updatedItem.id ? updatedItem : i));
    } else {
      setBacklogItems(backlogItems.filter(i => i.id !== updatedItem.id));
    }
  };

  const handleMoveToSprint = async (e: React.MouseEvent, itemId: number, sprintId: number) => {
    e.stopPropagation();
    if (!currentProject) return;
    try {
      await workItemsApi.update(currentProject.id, itemId, { sprintId, isInBacklog: false });
      setBacklogItems(backlogItems.filter(i => i.id !== itemId));
      const sprint = sprints.find(s => s.id === sprintId);
      notifySuccess('Moved to Sprint', `Item moved to ${sprint?.name}`);
    } catch (error) {
      notifyError('Error', 'Failed to move item');
    }
  };

  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case Priority.Critical: return 'bg-red-100 text-red-800 border-red-200';
      case Priority.High: return 'bg-orange-100 text-orange-800 border-orange-200';
      case Priority.Medium: return 'bg-blue-100 text-blue-800 border-blue-200';
      case Priority.Low: return 'bg-green-100 text-green-800 border-green-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Backlog</h1>
          <p className="mt-1 text-sm text-gray-500">{backlogItems.length} items in backlog</p>
        </div>
        <button
          onClick={openCreateModal}
          className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="-ml-1 mr-2 h-5 w-5" />
          Add Item
        </button>
      </div>

      {/* Backlog List */}
      <div className="bg-white shadow rounded-lg overflow-hidden">
        {backlogItems.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-sm text-gray-500">No items in backlog</p>
          </div>
        ) : (
          <ul className="divide-y divide-gray-200">
            {backlogItems.map((item) => (
              <li 
                key={item.id} 
                className="p-4 hover:bg-gray-50 cursor-pointer"
                onClick={() => openEditModal(item)}
              >
                <div className="flex items-center justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center space-x-2">
                      <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${WorkItemTypeColors[item.type]}`}>
                        {WorkItemTypeLabels[item.type]}
                      </span>
                      <span className="text-xs font-mono text-gray-500">{item.itemKey}</span>
                      <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium border ${getPriorityColor(item.priority)}`}>
                        {PriorityLabels[item.priority]}
                      </span>
                    </div>
                    <p className="mt-1 text-sm font-medium text-gray-900">{item.title}</p>
                    {item.description && (
                      <p className="mt-1 text-sm text-gray-500 line-clamp-1" dangerouslySetInnerHTML={{ __html: item.description }} />
                    )}
                    <div className="mt-2 flex items-center space-x-4 text-xs text-gray-500">
                      {item.assignedToName && <span>Assigned to: {item.assignedToName}</span>}
                      {item.estimatedHours && <span>{item.estimatedHours}h estimated</span>}
                      {item.parentKey && <span>Parent: {item.parentKey}</span>}
                      {item.childCount > 0 && <span>{item.childCount} child item(s)</span>}
                    </div>
                  </div>
                  <div className="ml-4 flex items-center space-x-2" onClick={e => e.stopPropagation()}>
                    {sprints.length > 0 && (
                      <div className="relative group">
                        <button className="inline-flex items-center px-3 py-1 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">
                          Move to Sprint
                          <ArrowRightIcon className="ml-1 h-4 w-4" />
                        </button>
                        <div className="absolute right-0 mt-1 w-48 bg-white rounded-md shadow-lg hidden group-hover:block z-10 border border-gray-200">
                          <div className="py-1">
                            {sprints.map((sprint) => (
                              <button
                                key={sprint.id}
                                onClick={(e) => handleMoveToSprint(e, item.id, sprint.id)}
                                className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                              >
                                {sprint.name}
                              </button>
                            ))}
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
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
