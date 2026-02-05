import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon, FolderIcon, UsersIcon, MagnifyingGlassIcon, XMarkIcon, ArrowRightOnRectangleIcon, ClockIcon, ChevronLeftIcon, ChevronRightIcon } from '@heroicons/react/24/outline';
import { projectsApi, usersApi, activityLogsApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';
import type { Project, User, ActivityLog, PaginatedResponse } from '../types';

const SystemRoleLabels: Record<number, string> = { 0: 'Member', 1: 'QA', 2: 'Manager', 3: 'Admin', 4: 'Super Admin' };
const SystemRoleColors: Record<number, string> = { 
  0: 'bg-gray-100 text-gray-800', 
  1: 'bg-yellow-100 text-yellow-800', 
  2: 'bg-blue-100 text-blue-800', 
  3: 'bg-purple-100 text-purple-800', 
  4: 'bg-red-100 text-red-800' 
};

export default function Projects() {
  const [activeTab, setActiveTab] = useState<'projects' | 'users' | 'activity'>('projects');
  const [projects, setProjects] = useState<Project[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [managers, setManagers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showProjectModal, setShowProjectModal] = useState(false);
  const [showUserModal, setShowUserModal] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [userFilter, setUserFilter] = useState<'all' | 'unassigned'>('all');
  
  // Activity Log State
  const [activityLogs, setActivityLogs] = useState<ActivityLog[]>([]);
  const [activityPage, setActivityPage] = useState(1);
  const [activityTotalPages, setActivityTotalPages] = useState(1);
  const [activityTotalCount, setActivityTotalCount] = useState(0);
  const [activityStartDate, setActivityStartDate] = useState('');
  const [activityEndDate, setActivityEndDate] = useState('');
  const [activityUserId, setActivityUserId] = useState<string>('');
  const [activityProjectId, setActivityProjectId] = useState<string>('');
  const [isLoadingActivity, setIsLoadingActivity] = useState(false);
  
  const [projectFormData, setProjectFormData] = useState({
    name: '',
    description: '',
    key: '',
    startDate: '',
    endDate: '',
    managerId: '',
  });
  const [userFormData, setUserFormData] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    phone: '',
    role: 0,
    managerId: '',
  });
  const { setCurrentProject } = useProject();
  const { user: currentUser, logout } = useAuth();
  const { notifySuccess, notifyError } = useNotification();
  const navigate = useNavigate();

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (activeTab === 'activity') {
      loadActivityLogs();
    }
  }, [activeTab, activityPage, activityStartDate, activityEndDate, activityUserId, activityProjectId]);

  const loadData = async () => {
    try {
      const [projectsRes, usersRes] = await Promise.all([
        projectsApi.getAll(),
        usersApi.getAll(),
      ]);
      setProjects(projectsRes.data);
      setUsers(usersRes.data);
      setManagers(usersRes.data.filter(u => u.systemRole >= 2));
    } catch (error) {
      notifyError('Error', 'Failed to load data');
    } finally {
      setIsLoading(false);
    }
  };

  const loadActivityLogs = async () => {
    setIsLoadingActivity(true);
    try {
      const params: any = {
        page: activityPage,
        pageSize: 20,
      };
      if (activityStartDate) params.startDate = activityStartDate;
      if (activityEndDate) params.endDate = activityEndDate;
      if (activityUserId) params.userId = parseInt(activityUserId);
      if (activityProjectId) params.projectId = parseInt(activityProjectId);

      const response = await activityLogsApi.getAll(params);
      const data = response.data as PaginatedResponse<ActivityLog>;
      setActivityLogs(data.data);
      setActivityTotalPages(data.totalPages);
      setActivityTotalCount(data.totalCount);
    } catch (error) {
      notifyError('Error', 'Failed to load activity logs');
    } finally {
      setIsLoadingActivity(false);
    }
  };

  const handleSelectProject = (project: Project) => {
    setCurrentProject(project);
    navigate('/dashboard');
  };

  const handleCreateProject = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!projectFormData.managerId) {
      notifyError('Validation Error', 'Please assign a Manager or Admin to the project');
      return;
    }
    try {
      const response = await projectsApi.create({
        name: projectFormData.name,
        description: projectFormData.description || undefined,
        key: projectFormData.key,
        startDate: projectFormData.startDate || undefined,
        endDate: projectFormData.endDate || undefined,
        managerId: parseInt(projectFormData.managerId),
      });
      setProjects([...projects, response.data]);
      setShowProjectModal(false);
      setProjectFormData({ name: '', description: '', key: '', startDate: '', endDate: '', managerId: '' });
      notifySuccess('Project Created', `${response.data.name} has been created`);
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to create project');
    }
  };

  const handleDeleteProject = async (projectId: number, projectName: string) => {
    if (!confirm(`Are you sure you want to delete "${projectName}"? This action can be undone by an administrator.`)) {
      return;
    }
    try {
      await projectsApi.delete(projectId);
      setProjects(projects.filter(p => p.id !== projectId));
      notifySuccess('Project Deleted', `${projectName} has been deleted`);
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to delete project');
    }
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await usersApi.create({
        email: userFormData.email,
        password: userFormData.password,
        firstName: userFormData.firstName,
        lastName: userFormData.lastName,
        phone: userFormData.phone || undefined,
        role: userFormData.role,
        managerId: userFormData.managerId ? parseInt(userFormData.managerId) : undefined,
      });
      setUsers([...users, response.data]);
      if (response.data.systemRole >= 2) {
        setManagers([...managers, response.data]);
      }
      setShowUserModal(false);
      setUserFormData({ email: '', password: '', firstName: '', lastName: '', phone: '', role: 0, managerId: '' });
      notifySuccess('User Created', `${response.data.firstName} ${response.data.lastName} has been added`);
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to create user');
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleSearchUsers = async () => {
    try {
      const params: any = {};
      if (searchQuery) params.search = searchQuery;
      if (userFilter === 'unassigned') params.unassigned = true;
      const response = await usersApi.getAll(params);
      setUsers(response.data);
    } catch (error) {
      notifyError('Error', 'Failed to search users');
    }
  };

  useEffect(() => {
    if (activeTab === 'users') {
      handleSearchUsers();
    }
  }, [searchQuery, userFilter, activeTab]);

  const filteredUsers = users.filter(u => 
    u.firstName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    u.lastName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    u.email.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const resetActivityFilters = () => {
    setActivityStartDate('');
    setActivityEndDate('');
    setActivityUserId('');
    setActivityProjectId('');
    setActivityPage(1);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center gap-4">
              <h1 className="text-2xl font-bold text-indigo-600">WorkFlow</h1>
            </div>
            <div className="flex items-center gap-4">
              <span className="text-sm text-gray-600">
                {currentUser?.firstName} {currentUser?.lastName}
                <span className={`ml-2 px-2 py-0.5 rounded text-xs ${SystemRoleColors[currentUser?.systemRole || 0]}`}>
                  {SystemRoleLabels[currentUser?.systemRole || 0]}
                </span>
              </span>
              <button
                onClick={handleLogout}
                className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700"
              >
                <ArrowRightOnRectangleIcon className="h-5 w-5 mr-1" />
                Logout
              </button>
            </div>
          </div>

          {/* Tabs */}
          <div className="flex gap-6 -mb-px">
            <button
              onClick={() => setActiveTab('projects')}
              className={`py-3 text-sm font-medium border-b-2 ${
                activeTab === 'projects'
                  ? 'border-indigo-500 text-indigo-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              <FolderIcon className="h-5 w-5 inline mr-2" />
              Projects
            </button>
            <button
              onClick={() => setActiveTab('users')}
              className={`py-3 text-sm font-medium border-b-2 ${
                activeTab === 'users'
                  ? 'border-indigo-500 text-indigo-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              <UsersIcon className="h-5 w-5 inline mr-2" />
              Users
            </button>
            <button
              onClick={() => setActiveTab('activity')}
              className={`py-3 text-sm font-medium border-b-2 ${
                activeTab === 'activity'
                  ? 'border-indigo-500 text-indigo-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              <ClockIcon className="h-5 w-5 inline mr-2" />
              Activity Log
            </button>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {activeTab === 'projects' && (
          <>
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-lg font-semibold text-gray-900">{projects.length} Projects</h2>
              {(currentUser?.systemRole === 3 || currentUser?.systemRole === 4) && (
                <button
                  onClick={() => setShowProjectModal(true)}
                  className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
                >
                  <PlusIcon className="h-5 w-5 mr-2" />
                  New Project
                </button>
              )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {projects.map((project) => (
                <div
                  key={project.id}
                  className="bg-white rounded-lg shadow-sm border border-gray-200 hover:shadow-md transition-shadow"
                >
                  <div className="p-6">
                    <div className="flex items-start justify-between">
                      <div className="flex items-center">
                        <div className="flex-shrink-0 h-10 w-10 rounded-lg bg-indigo-100 flex items-center justify-center">
                          <FolderIcon className="h-6 w-6 text-indigo-600" />
                        </div>
                        <div className="ml-4">
                          <h3 className="text-lg font-medium text-gray-900">{project.name}</h3>
                          <p className="text-sm text-gray-500">{project.key}</p>
                        </div>
                      </div>
                    </div>
                    {project.description && (
                      <p className="mt-4 text-sm text-gray-600 line-clamp-2">{project.description}</p>
                    )}
                    <div className="mt-4 flex items-center justify-between">
                      <span className={`px-2 py-1 text-xs rounded-full ${project.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
                        {project.isActive ? 'Active' : 'Inactive'}
                      </span>
                      <div className="flex gap-2">
                        {(currentUser?.systemRole === 3 || currentUser?.systemRole === 4) && (
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDeleteProject(project.id, project.name);
                            }}
                            className="text-red-600 hover:text-red-800 text-sm"
                          >
                            Delete
                          </button>
                        )}
                        <button
                          onClick={() => handleSelectProject(project)}
                          className="text-indigo-600 hover:text-indigo-800 text-sm font-medium"
                        >
                          Open →
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}

        {activeTab === 'users' && (
          <>
            <div className="flex justify-between items-center mb-6">
              <div className="flex items-center gap-4">
                <h2 className="text-lg font-semibold text-gray-900">{users.length} users in your company</h2>
              </div>
              {(currentUser?.systemRole === 3 || currentUser?.systemRole === 4) && (
                <button
                  onClick={() => setShowUserModal(true)}
                  className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
                >
                  <PlusIcon className="h-5 w-5 mr-2" />
                  Add User
                </button>
              )}
            </div>

            {/* Search and Filter */}
            <div className="mb-4 flex gap-4">
              <div className="relative flex-1 max-w-md">
                <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
                <input
                  type="text"
                  placeholder="Search by name or email..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>
              <select
                value={userFilter}
                onChange={(e) => setUserFilter(e.target.value as 'all' | 'unassigned')}
                className="border border-gray-300 rounded-md px-3 py-2 focus:ring-indigo-500 focus:border-indigo-500"
              >
                <option value="all">All Users</option>
                <option value="unassigned">Not on any project</option>
              </select>
            </div>

            <div className="bg-white shadow-sm rounded-lg overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Manager</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {filteredUsers.map((user) => (
                    <tr key={user.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div className="h-10 w-10 flex-shrink-0">
                            <div className="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center">
                              <span className="text-indigo-600 font-medium">
                                {user.firstName[0]}{user.lastName[0]}
                              </span>
                            </div>
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">
                              {user.firstName} {user.lastName}
                            </div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{user.email}</td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`px-2 py-1 text-xs rounded-full ${SystemRoleColors[user.systemRole]}`}>
                          {SystemRoleLabels[user.systemRole]}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {user.managerName || '-'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}

        {activeTab === 'activity' && (
          <>
            <div className="mb-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Activity Log</h2>
              
              {/* Filters */}
              <div className="bg-white p-4 rounded-lg shadow-sm mb-4">
                <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                    <input
                      type="date"
                      value={activityStartDate}
                      onChange={(e) => { setActivityStartDate(e.target.value); setActivityPage(1); }}
                      className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:ring-indigo-500 focus:border-indigo-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                    <input
                      type="date"
                      value={activityEndDate}
                      onChange={(e) => { setActivityEndDate(e.target.value); setActivityPage(1); }}
                      className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:ring-indigo-500 focus:border-indigo-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">User</label>
                    <select
                      value={activityUserId}
                      onChange={(e) => { setActivityUserId(e.target.value); setActivityPage(1); }}
                      className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:ring-indigo-500 focus:border-indigo-500"
                    >
                      <option value="">All Users</option>
                      {users.map((u) => (
                        <option key={u.id} value={u.id}>{u.firstName} {u.lastName}</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Project</label>
                    <select
                      value={activityProjectId}
                      onChange={(e) => { setActivityProjectId(e.target.value); setActivityPage(1); }}
                      className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:ring-indigo-500 focus:border-indigo-500"
                    >
                      <option value="">All Projects</option>
                      {projects.map((p) => (
                        <option key={p.id} value={p.id}>{p.name}</option>
                      ))}
                    </select>
                  </div>
                  <div className="flex items-end">
                    <button
                      onClick={resetActivityFilters}
                      className="w-full px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                    >
                      Reset Filters
                    </button>
                  </div>
                </div>
              </div>

              {/* Activity Count */}
              <div className="text-sm text-gray-500 mb-2">
                Showing {activityLogs.length} of {activityTotalCount} activities
              </div>

              {/* Activity List */}
              {isLoadingActivity ? (
                <div className="flex justify-center py-8">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                </div>
              ) : (
                <div className="bg-white shadow-sm rounded-lg overflow-hidden">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date & Time</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Project</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {activityLogs.map((log) => (
                        <tr key={log.id} className="hover:bg-gray-50">
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {new Date(log.timestamp).toLocaleString()}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                            {log.userName}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className="px-2 py-1 text-xs rounded-full bg-blue-100 text-blue-800">
                              {log.action}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {log.projectName || '-'}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-500 max-w-xs truncate">
                            {log.description || `${log.action} ${log.entityType}`}
                          </td>
                        </tr>
                      ))}
                      {activityLogs.length === 0 && (
                        <tr>
                          <td colSpan={5} className="px-6 py-8 text-center text-gray-500">
                            No activity logs found for the selected filters.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>

                  {/* Pagination */}
                  {activityTotalPages > 1 && (
                    <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
                      <div className="flex-1 flex justify-between sm:hidden">
                        <button
                          onClick={() => setActivityPage(p => Math.max(1, p - 1))}
                          disabled={activityPage === 1}
                          className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                        >
                          Previous
                        </button>
                        <button
                          onClick={() => setActivityPage(p => Math.min(activityTotalPages, p + 1))}
                          disabled={activityPage === activityTotalPages}
                          className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                        >
                          Next
                        </button>
                      </div>
                      <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                        <div>
                          <p className="text-sm text-gray-700">
                            Page <span className="font-medium">{activityPage}</span> of{' '}
                            <span className="font-medium">{activityTotalPages}</span>
                          </p>
                        </div>
                        <div>
                          <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                            <button
                              onClick={() => setActivityPage(p => Math.max(1, p - 1))}
                              disabled={activityPage === 1}
                              className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                            >
                              <ChevronLeftIcon className="h-5 w-5" />
                            </button>
                            <button
                              onClick={() => setActivityPage(p => Math.min(activityTotalPages, p + 1))}
                              disabled={activityPage === activityTotalPages}
                              className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                            >
                              <ChevronRightIcon className="h-5 w-5" />
                            </button>
                          </nav>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              )}
            </div>
          </>
        )}
      </div>

      {/* Create Project Modal */}
      {showProjectModal && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex items-center justify-center min-h-screen p-4">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75" onClick={() => setShowProjectModal(false)} />
            <div className="relative bg-white rounded-lg shadow-xl w-full max-w-md">
              <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
                <h3 className="text-lg font-semibold text-gray-900">Create New Project</h3>
                <button onClick={() => setShowProjectModal(false)} className="text-gray-400 hover:text-gray-500">
                  <XMarkIcon className="h-6 w-6" />
                </button>
              </div>
              <form onSubmit={handleCreateProject} className="p-6 space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Project Name <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    value={projectFormData.name}
                    onChange={(e) => setProjectFormData({ ...projectFormData, name: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    placeholder="My Awesome Project"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Project Key <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    maxLength={5}
                    value={projectFormData.key}
                    onChange={(e) => setProjectFormData({ ...projectFormData, key: e.target.value.toUpperCase() })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    placeholder="PRJ"
                  />
                  <p className="mt-1 text-xs text-gray-500">Used for ticket IDs (e.g., PRJ-1, PRJ-2)</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Description</label>
                  <textarea
                    rows={3}
                    value={projectFormData.description}
                    onChange={(e) => setProjectFormData({ ...projectFormData, description: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    placeholder="Brief description of the project..."
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Project Manager <span className="text-red-500">*</span>
                  </label>
                  <select
                    required
                    value={projectFormData.managerId}
                    onChange={(e) => setProjectFormData({ ...projectFormData, managerId: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  >
                    <option value="">Select a Manager or Admin</option>
                    {managers.map((m) => (
                      <option key={m.id} value={m.id}>
                        {m.firstName} {m.lastName} ({SystemRoleLabels[m.systemRole]})
                      </option>
                    ))}
                  </select>
                  <p className="mt-1 text-xs text-gray-500">A Manager or Admin is required to manage the project</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Start Date</label>
                    <input
                      type="date"
                      value={projectFormData.startDate}
                      onChange={(e) => setProjectFormData({ ...projectFormData, startDate: e.target.value })}
                      className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">End Date</label>
                    <input
                      type="date"
                      value={projectFormData.endDate}
                      onChange={(e) => setProjectFormData({ ...projectFormData, endDate: e.target.value })}
                      className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    />
                  </div>
                </div>
                <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
                  <button
                    type="button"
                    onClick={() => setShowProjectModal(false)}
                    className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
                  >
                    Create Project
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Create User Modal */}
      {showUserModal && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex items-center justify-center min-h-screen p-4">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75" onClick={() => setShowUserModal(false)} />
            <div className="relative bg-white rounded-lg shadow-xl w-full max-w-md">
              <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
                <h3 className="text-lg font-semibold text-gray-900">Add New User</h3>
                <button onClick={() => setShowUserModal(false)} className="text-gray-400 hover:text-gray-500">
                  <XMarkIcon className="h-6 w-6" />
                </button>
              </div>
              <form onSubmit={handleCreateUser} className="p-6 space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      First Name <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      required
                      value={userFormData.firstName}
                      onChange={(e) => setUserFormData({ ...userFormData, firstName: e.target.value })}
                      className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Last Name <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      required
                      value={userFormData.lastName}
                      onChange={(e) => setUserFormData({ ...userFormData, lastName: e.target.value })}
                      className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Email <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="email"
                    required
                    value={userFormData.email}
                    onChange={(e) => setUserFormData({ ...userFormData, email: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Password <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="password"
                    required
                    value={userFormData.password}
                    onChange={(e) => setUserFormData({ ...userFormData, password: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Phone</label>
                  <input
                    type="tel"
                    value={userFormData.phone}
                    onChange={(e) => setUserFormData({ ...userFormData, phone: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Role <span className="text-red-500">*</span>
                  </label>
                  <select
                    required
                    value={userFormData.role}
                    onChange={(e) => setUserFormData({ ...userFormData, role: parseInt(e.target.value) })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  >
                    <option value={0}>Member</option>
                    <option value={1}>QA</option>
                    <option value={2}>Manager</option>
                    <option value={3}>Admin</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Reports To</label>
                  <select
                    value={userFormData.managerId}
                    onChange={(e) => setUserFormData({ ...userFormData, managerId: e.target.value })}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  >
                    <option value="">No Manager</option>
                    {managers.map((m) => (
                      <option key={m.id} value={m.id}>{m.firstName} {m.lastName}</option>
                    ))}
                  </select>
                </div>
                <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
                  <button
                    type="button"
                    onClick={() => setShowUserModal(false)}
                    className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
                  >
                    Add User
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
