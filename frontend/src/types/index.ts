export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  systemRole: SystemRole;
  companyId?: number;
  companyName?: string;
  managerId?: number;
  managerName?: string;
}

export enum SystemRole {
  Member = 0,
  QA = 1,
  Manager = 2,
  Admin = 3,
  SuperAdmin = 4
}

export enum ProjectRole {
  Viewer = 0,
  Member = 1,
  Manager = 2,
  Admin = 3
}

export interface Company {
  id: number;
  name: string;
  description?: string;
  logo?: string;
  isActive: boolean;
  createdAt: string;
}

export interface Project {
  id: number;
  name: string;
  description?: string;
  key: string;
  startDate?: string;
  endDate?: string;
  isActive: boolean;
  companyId: number;
  createdAt: string;
}

export interface ProjectMember {
  id: number;
  projectId: number;
  userId: number;
  userName: string;
  userEmail: string;
  role: ProjectRole;
}

export interface WorkflowStatus {
  id: number;
  name: string;
  description?: string;
  order: number;
  color: string;
  isCore: boolean;
  coreType?: CoreStatusType;
  projectId: number;
}

export enum CoreStatusType {
  New = 0,
  InProgress = 1,
  Review = 2,
  Done = 3,
  Blocked = 4
}

export enum Priority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export enum WorkItemType {
  Epic = 0,
  Feature = 1,
  Story = 2,
  Task = 3,
  Bug = 4,
  Subtask = 5
}

export interface WorkItem {
  id: number;
  title: string;
  description?: string;
  type: WorkItemType;
  priority: Priority;
  dueDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  itemNumber: number;
  itemKey: string;
  projectId: number;
  statusId: number;
  statusName: string;
  statusColor: string;
  assignedToId?: number;
  assignedToName?: string;
  sprintId?: number;
  sprintName?: string;
  isInBacklog: boolean;
  queueOrder?: number;
  createdById: number;
  createdByName: string;
  createdAt: string;
  commentCount: number;
  attachmentCount: number;
  parentId?: number;
  parentKey?: string;
  childCount: number;
}

export enum SprintStatus {
  Planning = 0,
  Active = 1,
  Completed = 2
}

export interface Sprint {
  id: number;
  name: string;
  goal?: string;
  startDate: string;
  endDate: string;
  status: SprintStatus;
  projectId: number;
  workItemCount: number;
  createdAt: string;
}

export enum FileTicketType {
  Physical = 0,
  Digital = 1
}

export enum FileTicketStatus {
  Created = 0,
  InTransit = 1,
  Received = 2,
  Processing = 3,
  Approved = 4,
  Rejected = 5,
  Completed = 6,
  Lost = 7
}

export interface FileTicket {
  id: number;
  ticketNumber: string;
  title: string;
  description?: string;
  type: FileTicketType;
  status: FileTicketStatus;
  dueDate?: string;
  projectId: number;
  createdById: number;
  createdByName: string;
  currentHolderId?: number;
  currentHolderName?: string;
  createdAt: string;
}

export interface FileTicketTransfer {
  id: number;
  fileTicketId: number;
  fromUserId: number;
  fromUserName: string;
  toUserId: number;
  toUserName: string;
  transferredAt: string;
  receivedAt?: string;
  notes?: string;
}

export interface ActivityLog {
  id: number;
  action: string;
  entityType: string;
  entityId: number;
  oldValue?: string;
  newValue?: string;
  description?: string;
  timestamp: string;
  userId: number;
  userName: string;
  projectId?: number;
  projectName?: string;
}

export interface UserWithProjects extends User {
  projectAssignments: UserProjectAssignment[];
}

export interface UserProjectAssignment {
  projectId: number;
  projectName: string;
  projectKey: string;
  role: ProjectRole;
}

export interface UserSelection {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  systemRole: SystemRole;
  isAssignedToProject: boolean;
  projectRole: ProjectRole | null;
}

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

export interface Dashboard {
  totalWorkItems: number;
  completedWorkItems: number;
  inProgressWorkItems: number;
  blockedWorkItems: number;
  totalSprints: number;
  activeSprints: number;
  totalFileTickets: number;
  pendingFileTickets: number;
  workItemsByStatus: { status: string; color: string; count: number }[];
  workItemsByPriority: { priority: string; count: number }[];
  userWorkload: { userId: number; userName: string; assignedItems: number; completedItems: number }[];
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

export interface Comment {
  id: number;
  content: string;
  workItemId: number;
  authorId: number;
  authorName: string;
  createdAt: string;
}

export interface Attachment {
  id: number;
  fileName: string;
  filePath: string;
  contentType: string;
  fileSize: number;
  workItemId: number;
  uploadedById: number;
  uploadedByName: string;
  createdAt: string;
}
