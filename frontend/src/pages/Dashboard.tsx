import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { dashboardApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import type { Dashboard } from '../types';
import toast from 'react-hot-toast';

export default function DashboardPage() {
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const { currentProject } = useProject();
  const navigate = useNavigate();

  useEffect(() => {
    if (!currentProject) {
      navigate('/projects');
      return;
    }
    loadDashboard();
  }, [currentProject]);

  const loadDashboard = async () => {
    if (!currentProject) return;
    try {
      const response = await dashboardApi.get(currentProject.id);
      setDashboard(response.data);
    } catch (error) {
      toast.error('Failed to load dashboard');
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading || !dashboard) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  const priorityColors: Record<string, string> = {
    Low: '#10B981',
    Medium: '#3B82F6',
    High: '#F59E0B',
    Critical: '#EF4444',
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="mt-1 text-sm text-gray-500">{currentProject?.name}</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="h-12 w-12 rounded-md bg-indigo-500 flex items-center justify-center">
                  <span className="text-white text-lg font-bold">{dashboard.totalWorkItems}</span>
                </div>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">Total Work Items</dt>
                  <dd className="text-lg font-medium text-gray-900">{dashboard.completedWorkItems} completed</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="h-12 w-12 rounded-md bg-blue-500 flex items-center justify-center">
                  <span className="text-white text-lg font-bold">{dashboard.inProgressWorkItems}</span>
                </div>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">In Progress</dt>
                  <dd className="text-lg font-medium text-gray-900">{dashboard.blockedWorkItems} blocked</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="h-12 w-12 rounded-md bg-green-500 flex items-center justify-center">
                  <span className="text-white text-lg font-bold">{dashboard.activeSprints}</span>
                </div>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">Active Sprints</dt>
                  <dd className="text-lg font-medium text-gray-900">{dashboard.totalSprints} total</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="p-5">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <div className="h-12 w-12 rounded-md bg-yellow-500 flex items-center justify-center">
                  <span className="text-white text-lg font-bold">{dashboard.pendingFileTickets}</span>
                </div>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">Pending File Tickets</dt>
                  <dd className="text-lg font-medium text-gray-900">{dashboard.totalFileTickets} total</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Work Items by Status */}
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Work Items by Status</h3>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={dashboard.workItemsByStatus}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="status" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="count" fill="#6366f1" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Work Items by Priority */}
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Work Items by Priority</h3>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={dashboard.workItemsByPriority}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ priority, count }) => `${priority}: ${count}`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="count"
                >
                  {dashboard.workItemsByPriority.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={priorityColors[entry.priority] || '#8884d8'} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      {/* Team Workload */}
      <div className="bg-white shadow rounded-lg p-6">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Team Workload</h3>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Team Member</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Assigned</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Completed</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Progress</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {dashboard.userWorkload.map((user) => (
                <tr key={user.userId}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{user.userName}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{user.assignedItems}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{user.completedItems}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="w-full bg-gray-200 rounded-full h-2.5">
                      <div
                        className="bg-indigo-600 h-2.5 rounded-full"
                        style={{ width: `${user.assignedItems > 0 ? (user.completedItems / user.assignedItems) * 100 : 0}%` }}
                      ></div>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
