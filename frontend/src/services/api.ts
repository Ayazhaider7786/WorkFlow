import axios from 'axios';
import type { AuthResponse, User, Company, Project, ProjectMember, WorkItem, Sprint, FileTicket, FileTicketTransfer, WorkflowStatus, ActivityLog, Dashboard, ProjectRole, Priority, FileTicketType, FileTicketStatus, SprintStatus, Attachment, Comment, Board } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Flag to prevent multiple refresh attempts
let isRefreshing = false;
let failedQueue: Array<{ resolve: (token: string) => void; reject: (error: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token!);
    }
  });
  failedQueue = [];
};

// Request interceptor
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor with refresh token logic
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return api(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = localStorage.getItem('refreshToken');

      if (!refreshToken) {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        window.location.href = '/login';
        return Promise.reject(error);
      }

      try {
        const response = await axios.post<AuthResponse>(`${API_BASE_URL}/auth/refresh`, {
          refreshToken,
        });

        const { token, refreshToken: newRefreshToken } = response.data;

        localStorage.setItem('token', token);
        localStorage.setItem('refreshToken', newRefreshToken);

        api.defaults.headers.common.Authorization = `Bearer ${token}`;
        originalRequest.headers.Authorization = `Bearer ${token}`;

        processQueue(null, token);

        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

// Auth
export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/auth/login', { email, password }),
  register: (data: { email: string; password: string; firstName: string; lastName: string; companyName?: string }) =>
    api.post<AuthResponse>('/auth/register', data),
  refresh: (refreshToken: string) =>
    api.post<AuthResponse>('/auth/refresh', { refreshToken }),
  logout: (refreshToken: string) =>
    api.post('/auth/logout', { refreshToken }),
};

// Users
export const usersApi = {
  getAll: (params?: { search?: string; unassigned?: boolean }) =>
    api.get<User[]>('/users', { params }),
  getWithProjects: (search?: string) =>
    api.get<User[]>('/users/with-projects', { params: { search } }),
  getById: (id: number) => api.get<User>(`/users/${id}`),
  getMe: () => api.get<User>('/users/me'),
  getManagers: () => api.get<User[]>('/users/managers'),
  create: (data: { email: string; password: string; firstName: string; lastName: string; phone?: string; role?: number; managerId?: number }) =>
    api.post<User>('/users', data),
  update: (id: number, data: { firstName?: string; lastName?: string; phone?: string; role?: number; managerId?: number }) =>
    api.put<User>(`/users/${id}`, data),
  delete: (id: number) => api.delete(`/users/${id}`),
  getMyQueue: (projectId?: number) =>
    api.get('/users/me/queue', { params: { projectId } }),
};

// Companies
export const companiesApi = {
  getAll: () => api.get<Company[]>('/companies'),
  getById: (id: number) => api.get<Company>(`/companies/${id}`),
  create: (data: { name: string; description?: string; logo?: string }) =>
    api.post<Company>('/companies', data),
  update: (id: number, data: { name?: string; description?: string; logo?: string; isActive?: boolean }) =>
    api.put<Company>(`/companies/${id}`, data),
  delete: (id: number) => api.delete(`/companies/${id}`),
};

// Projects
export const projectsApi = {
  getAll: () => api.get<Project[]>('/projects'),
  getById: (id: number) => api.get<Project>(`/projects/${id}`),
  create: (data: { name: string; description?: string; key: string; startDate?: string; endDate?: string; managerId: number }) =>
    api.post<Project>('/projects', data),
  update: (id: number, data: { name?: string; description?: string; startDate?: string; endDate?: string; isActive?: boolean }) =>
    api.put<Project>(`/projects/${id}`, data),
  delete: (id: number) => api.delete(`/projects/${id}`),
  getMembers: (projectId: number) => api.get<ProjectMember[]>(`/projects/${projectId}/members`),
  getAvailableUsers: (projectId: number, params?: { search?: string; unassignedOnly?: boolean }) =>
    api.get<UserSelection[]>(`/projects/${projectId}/available-users`, { params }),
  addMember: (projectId: number, data: { userId: number; role: ProjectRole }) =>
    api.post<ProjectMember>(`/projects/${projectId}/members`, data),
  bulkAddMembers: (projectId: number, data: { userIds: number[]; role: ProjectRole }) =>
    api.post(`/projects/${projectId}/members/bulk`, data),
  updateMember: (projectId: number, memberId: number, data: { role: ProjectRole }) =>
    api.put<ProjectMember>(`/projects/${projectId}/members/${memberId}`, data),
  removeMember: (projectId: number, memberId: number) =>
    api.delete(`/projects/${projectId}/members/${memberId}`),
};

// Workflow Statuses
export const workflowStatusesApi = {
  getAll: (projectId: number) => api.get<WorkflowStatus[]>(`/projects/${projectId}/workflowstatuses`),
  getById: (projectId: number, id: number) => api.get<WorkflowStatus>(`/projects/${projectId}/workflowstatuses/${id}`),
  create: (projectId: number, data: { name: string; description?: string; order: number; color?: string }) =>
    api.post<WorkflowStatus>(`/projects/${projectId}/workflowstatuses`, data),
  update: (projectId: number, id: number, data: { name?: string; description?: string; order?: number; color?: string }) =>
    api.put<WorkflowStatus>(`/projects/${projectId}/workflowstatuses/${id}`, data),
  delete: (projectId: number, id: number) => api.delete(`/projects/${projectId}/workflowstatuses/${id}`),
  reorder: (projectId: number, statusIds: number[]) =>
    api.put<WorkflowStatus[]>(`/projects/${projectId}/workflowstatuses/reorder`, statusIds),
};

// Work Items
export const workItemsApi = {
  getAll: (projectId: number, params?: { sprintId?: number; backlogOnly?: boolean; assignedToId?: number; parentId?: number; type?: number }) =>
    api.get<WorkItem[]>(`/projects/${projectId}/workitems`, { params }),
  getById: (projectId: number, id: number) => api.get<WorkItem>(`/projects/${projectId}/workitems/${id}`),
  getChildren: (projectId: number, id: number) => api.get<WorkItem[]>(`/projects/${projectId}/workitems/${id}/children`),
  create: (projectId: number, data: { title: string; description?: string; type?: number; priority?: Priority; dueDate?: string; estimatedHours?: number; assignedToId?: number; sprintId?: number; parentId?: number }) =>
    api.post<WorkItem>(`/projects/${projectId}/workitems`, data),
  update: (projectId: number, id: number, data: { title?: string; description?: string; type?: number; priority?: Priority; dueDate?: string; estimatedHours?: number; actualHours?: number; statusId?: number; assignedToId?: number; sprintId?: number; isInBacklog?: boolean; queueOrder?: number; parentId?: number }) =>
    api.put<WorkItem>(`/projects/${projectId}/workitems/${id}`, data),
  delete: (projectId: number, id: number) => api.delete(`/projects/${projectId}/workitems/${id}`),
};

// Sprints
export const sprintsApi = {
  getAll: (projectId: number) => api.get<Sprint[]>(`/projects/${projectId}/sprints`),
  getById: (projectId: number, id: number) => api.get<Sprint>(`/projects/${projectId}/sprints/${id}`),
  create: (projectId: number, data: { name: string; goal?: string; startDate: string; endDate: string }) =>
    api.post<Sprint>(`/projects/${projectId}/sprints`, data),
  update: (projectId: number, id: number, data: { name?: string; goal?: string; startDate?: string; endDate?: string; status?: SprintStatus }) =>
    api.put<Sprint>(`/projects/${projectId}/sprints/${id}`, data),
  delete: (projectId: number, id: number) => api.delete(`/projects/${projectId}/sprints/${id}`),
  start: (projectId: number, id: number) => api.post<Sprint>(`/projects/${projectId}/sprints/${id}/start`),
  complete: (projectId: number, id: number) => api.post<Sprint>(`/projects/${projectId}/sprints/${id}/complete`),
};

// File Tickets
export const fileTicketsApi = {
  getAll: (projectId: number, params?: { currentHolderId?: number; status?: FileTicketStatus }) =>
    api.get<FileTicket[]>(`/projects/${projectId}/filetickets`, { params }),
  getById: (projectId: number, id: number) => api.get<FileTicket>(`/projects/${projectId}/filetickets/${id}`),
  create: (projectId: number, data: { title: string; description?: string; type?: FileTicketType; dueDate?: string; currentHolderId?: number }) =>
    api.post<FileTicket>(`/projects/${projectId}/filetickets`, data),
  update: (projectId: number, id: number, data: { title?: string; description?: string; status?: FileTicketStatus; dueDate?: string }) =>
    api.put<FileTicket>(`/projects/${projectId}/filetickets/${id}`, data),
  delete: (projectId: number, id: number) => api.delete(`/projects/${projectId}/filetickets/${id}`),
  transfer: (projectId: number, id: number, data: { toUserId: number; notes?: string }) =>
    api.post<FileTicket>(`/projects/${projectId}/filetickets/${id}/transfer`, data),
  receive: (projectId: number, id: number) =>
    api.post<FileTicket>(`/projects/${projectId}/filetickets/${id}/receive`),
  getTransfers: (projectId: number, id: number) =>
    api.get<FileTicketTransfer[]>(`/projects/${projectId}/filetickets/${id}/transfers`),
};

// Boards
export const boardsApi = {
  getProjectBoards: (projectId: number) => api.get<Board[]>(`/projects/${projectId}/boards`),
  getBoard: (projectId: number, id: number) => api.get<Board>(`/projects/${projectId}/boards/${id}`),
  createBoard: (projectId: number, data: { name: string; isDefault: boolean }) =>
    api.post<Board>(`/projects/${projectId}/boards`, data),
  addColumn: (projectId: number, boardId: number, data: { statusId: number }) =>
    api.post<Board>(`/projects/${projectId}/boards/${boardId}/columns`, data),
  deleteBoard: (projectId: number, id: number) => api.delete(`/projects/${projectId}/boards/${id}`),
};

// Dashboard
export const dashboardApi = {
  get: (projectId: number) => api.get<Dashboard>(`/projects/${projectId}/dashboard`),
};

// Activity Logs
export const activityLogsApi = {
  // System-wide activity logs
  getAll: (params?: { startDate?: string; endDate?: string; userId?: number; projectId?: number; page?: number; pageSize?: number }) =>
    api.get<PaginatedResponse<ActivityLog>>('/activitylogs', { params }),
  // Project-specific activity logs
  getByProject: (projectId: number, params?: { startDate?: string; endDate?: string; userId?: number; entityType?: string; entityId?: number; page?: number; pageSize?: number }) =>
    api.get<PaginatedResponse<ActivityLog>>(`/projects/${projectId}/activitylogs`, { params }),
  // User-specific activity logs
  getByUser: (userId: number, params?: { startDate?: string; endDate?: string; page?: number; pageSize?: number }) =>
    api.get<PaginatedResponse<ActivityLog>>(`/activitylogs/user/${userId}`, { params }),
  getWorkItemLogs: (projectId: number, workItemId: number) =>
    api.get<ActivityLog[]>(`/projects/${projectId}/activitylogs/workitem/${workItemId}`),
  getFileTicketLogs: (projectId: number, fileTicketId: number) =>
    api.get<ActivityLog[]>(`/projects/${projectId}/activitylogs/fileticket/${fileTicketId}`),
};

// Paginated Response interface
export interface PaginatedResponse<T> {
  success: boolean;
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// User Selection interface
export interface UserSelection {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  systemRole: number;
  isAssignedToProject: boolean;
  projectRole: ProjectRole | null;
}

// Comments
export const commentsApi = {
  getAll: (projectId: number, workItemId: number) =>
    api.get<Comment[]>(`/projects/${projectId}/workitems/${workItemId}/comments`),
  create: (projectId: number, workItemId: number, data: { content: string }) =>
    api.post<Comment>(`/projects/${projectId}/workitems/${workItemId}/comments`, data),
  delete: (projectId: number, workItemId: number, commentId: number) =>
    api.delete(`/projects/${projectId}/workitems/${workItemId}/comments/${commentId}`),
};

// Attachments
export const attachmentsApi = {
  getAll: (projectId: number, workItemId: number) =>
    api.get<Attachment[]>(`/projects/${projectId}/workitems/${workItemId}/attachments`),
  upload: (projectId: number, workItemId: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post<Attachment>(`/projects/${projectId}/workitems/${workItemId}/attachments`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  delete: (projectId: number, workItemId: number, attachmentId: number) =>
    api.delete(`/projects/${projectId}/workitems/${workItemId}/attachments/${attachmentId}`),
  getDownloadUrl: (projectId: number, workItemId: number, attachmentId: number) =>
    `${API_BASE_URL}/projects/${projectId}/workitems/${workItemId}/attachments/${attachmentId}/download`,
};

export default api;
