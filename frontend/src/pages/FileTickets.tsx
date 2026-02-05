import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon, FunnelIcon } from '@heroicons/react/24/outline';
import { fileTicketsApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useNotification } from '../context/NotificationContext';
import FileTicketModal from '../components/FileTicketModal';
import type { FileTicket } from '../types';

const FileTicketType = { Physical: 0, Digital: 1 };
const FileTicketStatus = { Created: 0, InTransit: 1, Received: 2, Processing: 3, Approved: 4, Rejected: 5, Completed: 6, Lost: 7 };
const FileTicketStatusLabels: Record<number, string> = { 0: 'Created', 1: 'In Transit', 2: 'Received', 3: 'Processing', 4: 'Approved', 5: 'Rejected', 6: 'Completed', 7: 'Lost' };
const FileTicketTypeLabels: Record<number, string> = { 0: 'Physical', 1: 'Digital' };

const statusColors: Record<number, string> = {
  0: 'bg-gray-100 text-gray-800',
  1: 'bg-yellow-100 text-yellow-800',
  2: 'bg-blue-100 text-blue-800',
  3: 'bg-purple-100 text-purple-800',
  4: 'bg-green-100 text-green-800',
  5: 'bg-red-100 text-red-800',
  6: 'bg-green-100 text-green-800',
  7: 'bg-red-100 text-red-800',
};

export default function FileTickets() {
  const [fileTickets, setFileTickets] = useState<FileTicket[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [selectedTicket, setSelectedTicket] = useState<FileTicket | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const [filterStatus, setFilterStatus] = useState<string>('');
  const { currentProject } = useProject();
  const { notifySuccess, notifyError } = useNotification();
  const navigate = useNavigate();

  useEffect(() => {
    if (!currentProject) {
      navigate('/projects');
      return;
    }
    loadFileTickets();
  }, [currentProject, filterStatus]);

  const loadFileTickets = async () => {
    if (!currentProject) return;
    try {
      const params: any = {};
      if (filterStatus) params.status = parseInt(filterStatus);
      const response = await fileTicketsApi.getAll(currentProject.id, params);
      setFileTickets(response.data);
    } catch (error) {
      notifyError('Error', 'Failed to load file tickets');
    } finally {
      setIsLoading(false);
    }
  };

  const openCreateModal = () => {
    setSelectedTicket(null);
    setIsCreating(true);
    setShowModal(true);
  };

  const openEditModal = (ticket: FileTicket) => {
    setSelectedTicket(ticket);
    setIsCreating(false);
    setShowModal(true);
  };

  const handleCreate = (newTicket: FileTicket) => {
    setFileTickets([newTicket, ...fileTickets]);
  };

  const handleUpdate = (updatedTicket: FileTicket) => {
    setFileTickets(fileTickets.map(t => t.id === updatedTicket.id ? updatedTicket : t));
  };

  const handleDelete = async (id: number) => {
    if (!currentProject || !confirm('Are you sure you want to delete this file ticket?')) return;
    try {
      await fileTicketsApi.delete(currentProject.id, id);
      setFileTickets(fileTickets.filter(t => t.id !== id));
      notifySuccess('File Ticket Deleted', 'The file ticket has been removed');
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to delete file ticket');
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
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">File Tickets</h1>
          <p className="text-sm text-gray-500">Track physical and digital documents</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2">
            <FunnelIcon className="h-5 w-5 text-gray-400" />
            <select
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              className="block w-36 rounded-md border-gray-300 text-sm focus:border-indigo-500 focus:ring-indigo-500"
            >
              <option value="">All Status</option>
              {Object.entries(FileTicketStatusLabels).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </div>
          <button
            onClick={openCreateModal}
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
          >
            <PlusIcon className="-ml-1 mr-2 h-5 w-5" />
            New File Ticket
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white shadow-sm rounded-lg overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Ticket #</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Title</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Current Holder</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created By</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {fileTickets.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                  No file tickets found. Create one to get started.
                </td>
              </tr>
            ) : (
              fileTickets.map((ticket) => (
                <tr 
                  key={ticket.id} 
                  className="hover:bg-gray-50 cursor-pointer"
                  onClick={() => openEditModal(ticket)}
                >
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="text-sm font-mono text-indigo-600">{ticket.ticketNumber}</span>
                  </td>
                  <td className="px-6 py-4">
                    <span className="text-sm text-gray-900">{ticket.title}</span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      ticket.type === FileTicketType.Physical ? 'bg-amber-100 text-amber-800' : 'bg-cyan-100 text-cyan-800'
                    }`}>
                      {FileTicketTypeLabels[ticket.type]}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusColors[ticket.status]}`}>
                      {FileTicketStatusLabels[ticket.status]}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {ticket.currentHolderName || '-'}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {ticket.createdByName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        handleDelete(ticket.id);
                      }}
                      className="text-red-600 hover:text-red-900"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* File Ticket Modal */}
      <FileTicketModal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        fileTicket={selectedTicket}
        onUpdate={handleUpdate}
        onCreate={handleCreate}
        isCreating={isCreating}
      />
    </div>
  );
}
