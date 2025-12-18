import api from './api';
import { Task } from '../types';

export const taskService = {
  async getTasks(organizationId: number): Promise<Task[]> {
    const response = await api.get(`/tasks/organization/${organizationId}`);
    return response.data;
  },

  async getTask(id: number): Promise<Task> {
    const response = await api.get(`/tasks/${id}`);
    return response.data;
  },

  async createTask(task: Partial<Task>): Promise<Task> {
    const response = await api.post('/tasks', task);
    return response.data;
  },

  async updateTask(id: number, task: Partial<Task>): Promise<Task> {
    const response = await api.put(`/tasks/${id}`, task);
    return response.data;
  },

  async deleteTask(id: number): Promise<void> {
    await api.delete(`/tasks/${id}`);
  },

  async assignTask(id: number, userId: number): Promise<Task> {
    const response = await api.post(`/tasks/${id}/assign`, { userId });
    return response.data;
  },

  async completeTask(id: number): Promise<Task> {
    const response = await api.post(`/tasks/${id}/complete`);
    return response.data;
  },

  async finalizeTask(id: number): Promise<Task> {
    const response = await api.post(`/tasks/${id}/finalize`);
    return response.data;
  },

  async getTaskPool(organizationId: number): Promise<Task[]> {
    const response = await api.get(`/tasks/organization/${organizationId}/pool`);
    return response.data;
  },
};
