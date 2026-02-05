import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { activityLogsApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import type { ActivityLog } from '../types';
import toast from 'react-hot-toast';

export default function Activity() {
  const [logs, setLogs] = useState<ActivityLog[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const { currentProject } = useProject();
  const navigate = useNavigate();

  useEffect(() => {
    if (!currentProject) {
      navigate('/projects');
      return;
    }
    loadLogs();
  }, [currentProject]);

  const loadLogs = async () => {
    if (!currentProject) return;
    try {
      const response = await activityLogsApi.getAll(currentProject.id, { pageSize: 100 });
      setLogs(response.data);
    } catch (error) {
      toast.error('Failed to load activity logs');
    } finally {
      setIsLoading(false);
    }
  };

  const getActionIcon = (action: string) => {
    switch (action.toLowerCase()) {
      case 'created':
        return <span className="h-8 w-8 rounded-full bg-green-100 flex items-center justify-center">
          <span className="text-green-600 text-xs font-bold">+</span>
        </span>;
      case 'deleted':
        return <span className="h-8 w-8 rounded-full bg-red-100 flex items-center justify-center">
          <span className="text-red-600 text-xs font-bold">×</span>
        </span>;
      case 'updated':
      case 'statuschanged':
        return <span className="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center">
          <span className="text-blue-600 text-xs font-bold">↻</span>
        </span>;
      case 'assigned':
        return <span className="h-8 w-8 rounded-full bg-purple-100 flex items-center justify-center">
          <span className="text-purple-600 text-xs font-bold">→</span>
        </span>;
      case 'transferred':
        return <span className="h-8 w-8 rounded-full bg-yellow-100 flex items-center justify-center">
          <span className="text-yellow-600 text-xs font-bold">⇄</span>
        </span>;
      default:
        return <span className="h-8 w-8 rounded-full bg-gray-100 flex items-center justify-center">
          <span className="text-gray-600 text-xs font-bold">•</span>
        </span>;
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
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Activity Log</h1>
        <p className="mt-1 text-sm text-gray-500">Full audit trail of all project activities</p>
      </div>

      <div className="bg-white shadow rounded-lg overflow-hidden">
        {logs.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-sm text-gray-500">No activity logged yet.</p>
          </div>
        ) : (
          <div className="flow-root">
            <ul className="-mb-8 p-6">
              {logs.map((log, logIdx) => (
                <li key={log.id}>
                  <div className="relative pb-8">
                    {logIdx !== logs.length - 1 ? (
                      <span className="absolute top-4 left-4 -ml-px h-full w-0.5 bg-gray-200" aria-hidden="true" />
                    ) : null}
                    <div className="relative flex space-x-3">
                      <div>{getActionIcon(log.action)}</div>
                      <div className="min-w-0 flex-1 pt-1.5 flex justify-between space-x-4">
                        <div>
                          <p className="text-sm text-gray-500">
                            <span className="font-medium text-gray-900">{log.userName}</span>{' '}
                            {log.description || `${log.action} ${log.entityType}`}
                          </p>
                          {log.oldValue && log.newValue && (
                            <p className="mt-1 text-xs text-gray-400">
                              Changed from "{log.oldValue}" to "{log.newValue}"
                            </p>
                          )}
                        </div>
                        <div className="text-right text-sm whitespace-nowrap text-gray-500">
                          <time dateTime={log.timestamp}>{format(new Date(log.timestamp), 'MMM d, yyyy h:mm a')}</time>
                        </div>
                      </div>
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}
