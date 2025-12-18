export interface Organization {
  id: number;
  name: string;
  ownerId?: number;
  createdAt: string;
}

export interface User {
  id: number;
  name: string;
  email: string;
  role: 'admin' | 'user';
  points: number;
  organizationId: number;
  createdAt: string;
}

export interface Task {
  id: number;
  title: string;
  description?: string;
  status: 'uncompleted' | 'completed' | 'finalized';
  points: number;
  assignedTo?: number;
  organizationId: number;
  createdAt: string;
  updatedAt: string;
}

export interface TaskHistory {
  id: number;
  taskId: number;
  userId: number;
  action: string;
  details?: string;
  organizationId: number;
  createdAt: string;
}

export interface UserInvitation {
  id: number;
  email: string;
  token: string;
  role: 'admin' | 'user';
  organizationId: number;
  expiresAt: string;
  accepted: boolean;
  createdAt: string;
}
