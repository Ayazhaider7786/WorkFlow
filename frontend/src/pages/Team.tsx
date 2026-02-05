import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon, TrashIcon } from '@heroicons/react/24/outline';
import { projectsApi, usersApi } from '../services/api';
import { useProject } from '../context/ProjectContext';
import { useNotification } from '../context/NotificationContext';
import type { ProjectMember, User } from '../types';

// Enum values inline to avoid import issues
const ProjectRole = { Viewer: 0, Member: 1, Manager: 2, Admin: 3 };

export default function Team() {
  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [availableUsers, setAvailableUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [selectedRole, setSelectedRole] = useState<number>(ProjectRole.Member);
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
      const [membersRes, usersRes] = await Promise.all([
        projectsApi.getMembers(currentProject.id),
        usersApi.getAll(),
      ]);
      setMembers(membersRes.data);
      setAvailableUsers(usersRes.data);
    } catch (error) {
      notifyError('Error', 'Failed to load team');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddMember = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentProject) return;

    try {
      const response = await projectsApi.addMember(currentProject.id, {
        userId: parseInt(selectedUserId),
        role: selectedRole,
      });
      setMembers([...members, response.data]);
      setShowModal(false);
      setSelectedUserId('');
      setSelectedRole(ProjectRole.Member);
      notifySuccess('Member Added', 'User has been added to the project');
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to add member');
    }
  };

  const handleUpdateRole = async (memberId: number, newRole: number) => {
    if (!currentProject) return;
    try {
      await projectsApi.updateMember(currentProject.id, memberId, { role: newRole });
      setMembers(members.map(m => m.id === memberId ? { ...m, role: newRole } : m));
      notifySuccess('Role Updated', 'Member role has been changed');
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to update role');
    }
  };

  const handleRemoveMember = async (memberId: number) => {
    if (!currentProject) return;
    if (!confirm('Are you sure you want to remove this member?')) return;
    
    try {
      await projectsApi.removeMember(currentProject.id, memberId);
      setMembers(members.filter(m => m.id !== memberId));
      notifySuccess('Member Removed', 'User has been removed from the project');
    } catch (error: any) {
      notifyError('Error', error.response?.data?.message || 'Failed to remove member');
    }
  };

  const nonMemberUsers = availableUsers.filter(u => !members.some(m => m.userId === u.id));

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
          <h1 className="text-2xl font-bold text-gray-900">Team</h1>
          <p className="mt-1 text-sm text-gray-500">{members.length} members in {currentProject?.name}</p>
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="-ml-1 mr-2 h-5 w-5" />
          Add Member
        </button>
      </div>

      <div className="bg-white shadow rounded-lg overflow-hidden">
        <ul className="divide-y divide-gray-200">
          {members.map((member) => (
            <li key={member.id} className="p-6">
              <div className="flex items-center justify-between">
                <div className="flex items-center">
                  <div className="h-10 w-10 rounded-full bg-indigo-100 flex items-center justify-center">
                    <span className="text-indigo-600 font-medium text-sm">
                      {member.userName.split(' ').map(n => n[0]).join('')}
                    </span>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-900">{member.userName}</p>
                    <p className="text-sm text-gray-500">{member.userEmail}</p>
                  </div>
                </div>
                <div className="flex items-center space-x-4">
                  <select
                    value={member.role}
                    onChange={(e) => handleUpdateRole(member.id, parseInt(e.target.value))}
                    className="block w-32 rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                  >
                    <option value={ProjectRole.Viewer}>Viewer</option>
                    <option value={ProjectRole.Member}>Member</option>
                    <option value={ProjectRole.Manager}>Manager</option>
                    <option value={ProjectRole.Admin}>Admin</option>
                  </select>
                  <button
                    onClick={() => handleRemoveMember(member.id)}
                    className="text-red-600 hover:text-red-900"
                  >
                    <TrashIcon className="h-5 w-5" />
                  </button>
                </div>
              </div>
            </li>
          ))}
        </ul>
      </div>

      {/* Add Member Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75" onClick={() => setShowModal(false)}></div>
            <div className="inline-block align-bottom bg-white rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full sm:p-6">
              <form onSubmit={handleAddMember}>
                <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">Add Team Member</h3>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">User</label>
                    <select
                      required
                      value={selectedUserId}
                      onChange={(e) => setSelectedUserId(e.target.value)}
                      className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                    >
                      <option value="">Select a user</option>
                      {nonMemberUsers.map((user) => (
                        <option key={user.id} value={user.id}>{user.firstName} {user.lastName} ({user.email})</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Role</label>
                    <select
                      value={selectedRole}
                      onChange={(e) => setSelectedRole(parseInt(e.target.value))}
                      className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                    >
                      <option value={ProjectRole.Viewer}>Viewer</option>
                      <option value={ProjectRole.Member}>Member</option>
                      <option value={ProjectRole.Manager}>Manager</option>
                      <option value={ProjectRole.Admin}>Admin</option>
                    </select>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-cols-2 sm:gap-3">
                  <button type="submit" className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 sm:text-sm">Add</button>
                  <button type="button" onClick={() => setShowModal(false)} className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 sm:mt-0 sm:text-sm">Cancel</button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
