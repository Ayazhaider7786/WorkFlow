import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon, PlayIcon, CheckIcon } from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import { sprintsApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import type { Sprint, SprintStatus } from '../types';
import toast from 'react-hot-toast';

export default function Sprints() {
  const [sprints, setSprints] = useState<Sprint[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    goal: '',
    startDate: '',
    endDate: '',
  });
  const { currentProject } = useProject();
  const navigate = useNavigate();

  useEffect(() => {
    if (!currentProject) {
      navigate('/projects');
      return;
    }
    loadSprints();
  }, [currentProject]);

  const loadSprints = async () => {
    if (!currentProject) return;
    try {
      const response = await sprintsApi.getAll(currentProject.id);
      setSprints(response.data);
    } catch (error) {
      toast.error('Failed to load sprints');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentProject) return;

    try {
      const response = await sprintsApi.create(currentProject.id, {
        name: formData.name,
        goal: formData.goal || undefined,
        startDate: formData.startDate,
        endDate: formData.endDate,
      });
      setSprints([response.data, ...sprints]);
      setShowModal(false);
      setFormData({ name: '', goal: '', startDate: '', endDate: '' });
      toast.success('Sprint created');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to create sprint');
    }
  };

  const handleStartSprint = async (sprintId: number) => {
    if (!currentProject) return;
    try {
      const response = await sprintsApi.start(currentProject.id, sprintId);
      setSprints(sprints.map(s => s.id === sprintId ? response.data : s));
      toast.success('Sprint started');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to start sprint');
    }
  };

  const handleCompleteSprint = async (sprintId: number) => {
    if (!currentProject) return;
    try {
      const response = await sprintsApi.complete(currentProject.id, sprintId);
      setSprints(sprints.map(s => s.id === sprintId ? response.data : s));
      toast.success('Sprint completed');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to complete sprint');
    }
  };

  const getStatusBadge = (status: SprintStatus) => {
    switch (status) {
      case 0: return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">Planning</span>;
      case 1: return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">Active</span>;
      case 2: return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">Completed</span>;
      default: return null;
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
          <h1 className="text-2xl font-bold text-gray-900">Sprints</h1>
          <p className="mt-1 text-sm text-gray-500">{currentProject?.name}</p>
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="-ml-1 mr-2 h-5 w-5" />
          New Sprint
        </button>
      </div>

      <div className="bg-white shadow rounded-lg overflow-hidden">
        {sprints.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-sm text-gray-500">No sprints yet. Create your first sprint to get started.</p>
          </div>
        ) : (
          <ul className="divide-y divide-gray-200">
            {sprints.map((sprint) => (
              <li key={sprint.id} className="p-6">
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3">
                      <h3 className="text-lg font-medium text-gray-900">{sprint.name}</h3>
                      {getStatusBadge(sprint.status)}
                    </div>
                    {sprint.goal && (
                      <p className="mt-1 text-sm text-gray-600">Goal: {sprint.goal}</p>
                    )}
                    <div className="mt-2 flex items-center space-x-4 text-sm text-gray-500">
                      <span>
                        {format(new Date(sprint.startDate), 'MMM d, yyyy')} - {format(new Date(sprint.endDate), 'MMM d, yyyy')}
                      </span>
                      <span>{sprint.workItemCount} items</span>
                    </div>
                  </div>
                  <div className="ml-4 flex items-center space-x-2">
                    {sprint.status === 0 && (
                      <button
                        onClick={() => handleStartSprint(sprint.id)}
                        className="inline-flex items-center px-3 py-1.5 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                      >
                        <PlayIcon className="-ml-0.5 mr-1 h-4 w-4" />
                        Start Sprint
                      </button>
                    )}
                    {sprint.status === 1 && (
                      <button
                        onClick={() => handleCompleteSprint(sprint.id)}
                        className="inline-flex items-center px-3 py-1.5 border border-transparent text-sm font-medium rounded-md text-white bg-green-600 hover:bg-green-700"
                      >
                        <CheckIcon className="-ml-0.5 mr-1 h-4 w-4" />
                        Complete Sprint
                      </button>
                    )}
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      {showModal && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => setShowModal(false)}></div>
            <div className="inline-block align-bottom bg-white rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full sm:p-6">
              <form onSubmit={handleSubmit}>
                <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">Create New Sprint</h3>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Sprint Name</label>
                    <input
                      type="text"
                      required
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Sprint Goal</label>
                    <textarea
                      rows={2}
                      value={formData.goal}
                      onChange={(e) => setFormData({ ...formData, goal: e.target.value })}
                      className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Start Date</label>
                      <input
                        type="date"
                        required
                        value={formData.startDate}
                        onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">End Date</label>
                      <input
                        type="date"
                        required
                        value={formData.endDate}
                        onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                      />
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 sm:text-sm"
                  >
                    Create
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowModal(false)}
                    className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 sm:mt-0 sm:text-sm"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
