import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon, ExclamationTriangleIcon } from '@heroicons/react/24/outline';
import { workItemsApi, workflowStatusesApi, sprintsApi, boardsApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useNotification } from '../context/NotificationContext';
import type { WorkItem, WorkflowStatus, Sprint, Board as BoardType } from '../types';
import TicketCard from '../components/TicketCard';
import TicketModal from '../components/TicketModal';

export default function Board() {
  const [workItems, setWorkItems] = useState<WorkItem[]>([]);
  const [allStatuses, setAllStatuses] = useState<WorkflowStatus[]>([]); // All available statuses in project
  const [displayedStatuses, setDisplayedStatuses] = useState<WorkflowStatus[]>([]); // Statuses to show on board
  const [boards, setBoards] = useState<BoardType[]>([]);
  const [activeBoard, setActiveBoard] = useState<BoardType | null>(null);

  const [sprints, setSprints] = useState<Sprint[]>([]);
  const [selectedSprint, setSelectedSprint] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Ticket Modal
  const [showTicketModal, setShowTicketModal] = useState(false);
  const [selectedItem, setSelectedItem] = useState<WorkItem | null>(null);
  const [isCreatingTicket, setIsCreatingTicket] = useState(false);



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
      const [statusesRes, sprintsRes, boardsRes] = await Promise.all([
        workflowStatusesApi.getAll(currentProject.id),
        sprintsApi.getAll(currentProject.id),
        boardsApi.getProjectBoards(currentProject.id).catch(() => ({ data: [] as BoardType[] })) // Handle legacy projects smoothly
      ]);

      setAllStatuses(statusesRes.data);
      setSprints(sprintsRes.data);

      const projectBoards = boardsRes.data || [];
      setBoards(projectBoards);

      // Determine active board
      let currentActive = activeBoard;

      // If we don't have an active board selected, or the selected one is no longer valid
      if (!currentActive || !projectBoards.find(b => b.id === currentActive?.id)) {
        // Prefer personal board, then default
        // Since getProjectBoards returns Default then Personal (sorted by isDefault desc), 
        // we actually want the LAST one if it's a personal board belonging to user?
        // The API filters for getting Default + User's boards.
        // Usually we want the Default board first unless user switched.

        if (projectBoards.length > 0) {
          currentActive = projectBoards[0]; // Default behavior: pick first available
        }
      } else {
        // Refresh active board data
        currentActive = projectBoards.find(b => b.id === currentActive!.id) || null;
      }

      setActiveBoard(currentActive);
      updateDisplayedStatuses(currentActive, statusesRes.data);

      const itemsRes = await workItemsApi.getAll(currentProject.id, { sprintId: selectedSprint || undefined });
      setWorkItems(itemsRes.data);
    } catch (error) {
      console.error(error);
      notifyError('Error', 'Failed to load board data');
    } finally {
      setIsLoading(false);
    }
  };

  const updateDisplayedStatuses = (board: BoardType | null, allStats: WorkflowStatus[]) => {
    if (board && board.columns && board.columns.length > 0) {
      const ordered = board.columns
        .sort((a, b) => a.order - b.order)
        .map(col => allStats.find(s => s.id === col.statusId))
        .filter((s): s is WorkflowStatus => !!s);
      setDisplayedStatuses(ordered);
    } else {
      // Fallback for legacy (all statuses sorted by order)
      setDisplayedStatuses([...allStats].sort((a, b) => a.order - b.order));
    }
  };

  // Switch board
  const handleBoardChange = (boardId: string) => {
    const board = boards.find(b => b.id === parseInt(boardId)) || null;
    setActiveBoard(board);
    updateDisplayedStatuses(board, allStatuses);
  };



  // Drag & Drop
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

    const newStatus = allStatuses.find(s => s.id === statusId);
    try {
      await workItemsApi.update(currentProject.id, workItemId, { statusId });
      setWorkItems(workItems.map(w => w.id === workItemId ? { ...w, statusId, statusName: newStatus?.name || '' } : w));
      notifySuccess('Status Updated', `${item.itemKey} moved to ${newStatus?.name}`);
    } catch (error) {
      notifyError('Error', 'Failed to update status');
    }
  };

  const isBlocked = (item: WorkItem) => item.title.startsWith('[BLOCKED] ');

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
          <div className="flex items-center gap-2">
            <p className="text-sm text-gray-500">{currentProject?.name}</p>
            {/* Board Selector */}
            {boards.length > 0 && (
              <select
                value={activeBoard?.id || ''}
                onChange={(e) => handleBoardChange(e.target.value)}
                className="ml-2 text-xs border-gray-200 rounded-md py-1 pr-8 pl-2 focus:ring-indigo-500 focus:border-indigo-500"
              >
                {boards.map(b => (
                  <option key={b.id} value={b.id}>
                    {b.name} {b.isDefault ? '(Default)' : ''}
                  </option>
                ))}
              </select>
            )}
          </div>
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
            onClick={() => { setSelectedItem(null); setIsCreatingTicket(true); setShowTicketModal(true); }}
            className="inline-flex items-center px-3 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
          >
            <PlusIcon className="-ml-0.5 mr-1.5 h-5 w-5" />
            Create Ticket
          </button>
        </div>
      </div>

      {/* Kanban Board */}
      <div className="flex overflow-x-auto pb-4 gap-2 h-[calc(100vh-12rem)] min-h-[500px]">
        {displayedStatuses
          .filter(status => status.name !== 'Blocked')
          .map((status) => {
            const columnItems = workItems.filter(item => item.statusId === status.id);
            const blockedItems = columnItems.filter(isBlocked);
            const activeItems = columnItems.filter(item => !isBlocked(item));

            return (
              <div
                key={status.id}
                className="flex-shrink-0 w-[14rem] flex flex-col bg-gray-100/50 rounded-lg p-2 border border-gray-200/60"
                onDragOver={handleDragOver}
                onDrop={(e) => handleDrop(e, status.id)}
              >
                {/* Column Header */}
                <div className="flex items-center justify-between mb-2.5 px-1 pt-1">
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full ring-2 ring-white shadow-sm" style={{ backgroundColor: status.color }}></div>
                    <h3 className="font-semibold text-gray-900 text-sm">{status.name}</h3>
                  </div>
                  <span className="bg-white text-gray-500 text-[10px] font-bold px-2 py-0.5 rounded-full shadow-sm border border-gray-100">
                    {columnItems.length}
                  </span>
                </div>

                <div className="flex-1 overflow-y-auto pr-1 space-y-2 scrollbar-thin scrollbar-thumb-gray-200">
                  {/* Blocked Items Section */}
                  {blockedItems.length > 0 && (
                    <div className="space-y-2 mb-2 p-1.5 bg-red-50/50 rounded-lg border border-red-100">
                      <div className="flex items-center gap-1 text-[10px] font-bold text-red-600 uppercase tracking-wider px-1">
                        <ExclamationTriangleIcon className="h-3 w-3" />
                        Blocked ({blockedItems.length})
                      </div>
                      {blockedItems.map((item) => (
                        <TicketCard
                          key={item.id}
                          itemKey={item.itemKey}
                          title={item.title.replace('[BLOCKED] ', '')}
                          type={item.type}
                          priority={item.priority}
                          assignedToName={item.assignedToName}
                          estimatedHours={item.estimatedHours}
                          commentCount={item.commentCount}
                          attachmentCount={item.attachmentCount}
                          childCount={item.childCount}
                          onClick={() => { setSelectedItem(item); setIsCreatingTicket(false); setShowTicketModal(true); }}
                          onDragStart={(e) => handleDragStart(e, item)}
                          className="border-l-4 border-l-red-500 shadow-red-100"
                        />
                      ))}
                    </div>
                  )}

                  {/* Regular Items */}
                  {activeItems.map((item) => (
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
                      onClick={() => { setSelectedItem(item); setIsCreatingTicket(false); setShowTicketModal(true); }}
                      onDragStart={(e) => handleDragStart(e, item)}
                    />
                  ))}
                  {/* Drop area visual cue */}
                  <div className="h-10"></div>
                </div>
              </div>
            );
          })}
      </div>

      {/* Ticket Modal */}
      <TicketModal
        isOpen={showTicketModal}
        onClose={() => setShowTicketModal(false)}
        workItem={selectedItem}
        onUpdate={(updated) => setWorkItems(workItems.map(w => w.id === updated.id ? updated : w))}
        onCreate={(newOne) => setWorkItems([newOne, ...workItems])}
        isCreating={isCreatingTicket}
      />


    </div>
  );
}

