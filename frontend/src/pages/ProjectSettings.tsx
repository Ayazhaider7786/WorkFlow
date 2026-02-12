import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon, TrashIcon, PencilIcon, CheckIcon, XMarkIcon } from '@heroicons/react/24/outline';
import { workflowStatusesApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useNotification } from '../context/NotificationContext';
import type { WorkflowStatus } from '../types';

export default function ProjectSettings() {
    const [statuses, setStatuses] = useState<WorkflowStatus[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [editingStatus, setEditingStatus] = useState<WorkflowStatus | null>(null);
    const [isCreating, setIsCreating] = useState(false);
    const [formData, setFormData] = useState({ name: '', color: '#3b82f6', order: 0 });
    const { currentProject } = useProject();
    const { notifySuccess, notifyError } = useNotification();
    const navigate = useNavigate();

    useEffect(() => {
        if (!currentProject) {
            navigate('/projects');
            return;
        }
        loadStatuses();
    }, [currentProject]);

    const loadStatuses = async () => {
        if (!currentProject) return;
        try {
            const res = await workflowStatusesApi.getAll(currentProject.id);
            setStatuses(res.data.sort((a, b) => a.order - b.order));
        } catch (error) {
            notifyError('Error', 'Failed to load statuses');
        } finally {
            setIsLoading(false);
        }
    };

    const handleEdit = (status: WorkflowStatus) => {
        setEditingStatus(status);
        setFormData({ name: status.name, color: status.color, order: status.order });
        setIsCreating(false);
    };

    const handleCreate = () => {
        setEditingStatus(null);
        setFormData({ name: '', color: '#3b82f6', order: statuses.length });
        setIsCreating(true);
    };

    const handleSave = async () => {
        if (!currentProject || !formData.name.trim()) return;

        try {
            if (isCreating) {
                const res = await workflowStatusesApi.create(currentProject.id, {
                    name: formData.name,
                    description: '',
                    color: formData.color,
                    order: formData.order,
                });
                setStatuses([...statuses, res.data].sort((a, b) => a.order - b.order));
                notifySuccess('Success', 'Status created');
            } else if (editingStatus) {
                const res = await workflowStatusesApi.update(currentProject.id, editingStatus.id, {
                    name: formData.name,
                    description: editingStatus.description || '',
                    color: formData.color,
                    order: formData.order,
                });
                setStatuses(statuses.map(s => s.id === editingStatus.id ? res.data : s).sort((a, b) => a.order - b.order));
                notifySuccess('Success', 'Status updated');
            }
            setIsCreating(false);
            setEditingStatus(null);
            setFormData({ name: '', color: '#3b82f6', order: 0 });
        } catch (error) {
            notifyError('Error', 'Failed to save status');
        }
    };

    const handleDelete = async (id: number) => {
        if (!currentProject || !confirm('Are you sure you want to delete this status? Items in this status will be moved to the default status or must be moved manually.')) return;
        try {
            await workflowStatusesApi.delete(currentProject.id, id);
            setStatuses(statuses.filter(s => s.id !== id));
            notifySuccess('Success', 'Status deleted');
        } catch (error) {
            notifyError('Error', 'Failed to delete status. Ensure it has no items.');
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
        <div className="max-w-4xl mx-auto space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Project Settings</h1>
                    <p className="text-sm text-gray-500">{currentProject?.name}</p>
                </div>
                <button
                    onClick={handleCreate}
                    disabled={isCreating}
                    className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50"
                >
                    <PlusIcon className="-ml-1 mr-2 h-5 w-5" />
                    Add Status
                </button>
            </div>

            <div className="bg-white shadow rounded-lg overflow-hidden">
                <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                    <h3 className="text-lg leading-6 font-medium text-gray-900">Workflow Statuses</h3>
                    <p className="mt-1 max-w-2xl text-sm text-gray-500">Manage the columns in your board.</p>
                </div>

                {isCreating && (
                    <div className="p-4 bg-gray-50 border-b border-gray-200">
                        <h4 className="text-sm font-medium text-gray-900 mb-3">New Status</h4>
                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-4 items-end">
                            <div className="col-span-2">
                                <label className="block text-xs font-medium text-gray-700">Name</label>
                                <input
                                    type="text"
                                    value={formData.name}
                                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                                    placeholder="e.g. In QA"
                                />
                            </div>
                            <div>
                                <label className="block text-xs font-medium text-gray-700">Color</label>
                                <div className="mt-1 flex items-center gap-2">
                                    <input
                                        type="color"
                                        value={formData.color}
                                        onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                                        className="h-9 w-9 p-1 rounded-md border border-gray-300"
                                    />
                                    <input
                                        type="text"
                                        value={formData.color}
                                        onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                                    />
                                </div>
                            </div>
                            <div className="flex gap-2">
                                <button onClick={handleSave} className="inline-flex items-center p-2 border border-transparent rounded-full shadow-sm text-white bg-green-600 hover:bg-green-700">
                                    <CheckIcon className="h-5 w-5" />
                                </button>
                                <button onClick={() => setIsCreating(false)} className="inline-flex items-center p-2 border border-gray-300 rounded-full shadow-sm text-gray-700 bg-white hover:bg-gray-50">
                                    <XMarkIcon className="h-5 w-5" />
                                </button>
                            </div>
                        </div>
                    </div>
                )}

                <ul className="divide-y divide-gray-200">
                    {statuses
                        .filter(status => status.name !== 'Blocked')
                        .map((status) => (
                            <li key={status.id} className="px-4 py-4 sm:px-6 hover:bg-gray-50 transition-colors">
                                {editingStatus?.id === status.id ? (
                                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-4 items-end">
                                        <div className="col-span-2">
                                            <input
                                                type="text"
                                                value={formData.name}
                                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                                className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                                            />
                                        </div>
                                        <div>
                                            <div className="flex items-center gap-2">
                                                <input
                                                    type="color"
                                                    value={formData.color}
                                                    onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                                                    className="h-9 w-9 p-1 rounded-md border border-gray-300"
                                                />
                                                <input
                                                    type="text"
                                                    value={formData.color}
                                                    onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                                                    className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                                                />
                                            </div>
                                        </div>
                                        <div className="flex gap-2 justify-end">
                                            <button onClick={handleSave} className="p-2 text-green-600 hover:bg-green-50 rounded-full">
                                                <CheckIcon className="h-5 w-5" />
                                            </button>
                                            <button onClick={() => setEditingStatus(null)} className="p-2 text-gray-400 hover:bg-gray-100 rounded-full">
                                                <XMarkIcon className="h-5 w-5" />
                                            </button>
                                        </div>
                                    </div>
                                ) : (
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-4">
                                            <div className="w-4 h-4 rounded-full ring-2 ring-gray-100 shadow-sm" style={{ backgroundColor: status.color }} />
                                            <span className="text-sm font-medium text-gray-900">{status.name}</span>
                                            {status.isCore && <span className="px-2 py-0.5 rounded text-[10px] bg-gray-100 text-gray-500 uppercase tracking-wider font-bold">Core</span>}
                                        </div>
                                        <div className="flex items-center gap-2">
                                            <span className="text-xs text-gray-400 mr-4">Order: {status.order}</span>
                                            <button
                                                onClick={() => handleEdit(status)}
                                                className="p-1 text-gray-400 hover:text-indigo-600 transition-colors"
                                                title="Edit"
                                            >
                                                <PencilIcon className="h-4 w-4" />
                                            </button>
                                            {(!status.isCore || status.name === 'Blocked') && (
                                                <button
                                                    onClick={() => handleDelete(status.id)}
                                                    className="p-1 text-gray-400 hover:text-red-600 transition-colors"
                                                    title="Delete"
                                                >
                                                    <TrashIcon className="h-4 w-4" />
                                                </button>
                                            )}
                                        </div>
                                    </div>
                                )}
                            </li>
                        ))}
                </ul>
            </div>
        </div>
    );
}
