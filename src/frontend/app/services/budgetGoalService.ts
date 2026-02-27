export interface BudgetGoalDto {
  id: number;
  groupId: number;
  targetAmount: number;
  startDate: string;
  endDate: string;
  createdDate: string;
  updatedDate: string;
}

export interface BudgetGoalProgressDto {
  goalId: number;
  groupId: number;
  targetAmount: number;
  currentAmount: number;
  startDate: string;
  endDate: string;
}

export interface CreateBudgetGoalDto {
  targetAmount: number;
  startDate: string;
  endDate: string;
}

export interface UpdateBudgetGoalDto {
  targetAmount?: number;
  startDate?: string;
  endDate?: string;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const budgetGoalService = {
  createGoal(groupId: number, dto: CreateBudgetGoalDto) {
    return useApiFetch<BudgetGoalDto>(`/api/groups/${groupId}/budget-goals`, {
      method: 'POST',
      body: dto,
    });
  },

  getGoals(groupId: number) {
    return useApiFetch<BudgetGoalDto[]>(`/api/groups/${groupId}/budget-goals`, { method: 'GET' });
  },

  getGoal(groupId: number, goalId: number) {
    return useApiFetch<BudgetGoalDto>(`/api/groups/${groupId}/budget-goals/${goalId}`, { method: 'GET' });
  },

  getProgress(groupId: number, goalId: number) {
    return useApiFetch<BudgetGoalProgressDto>(
      `/api/groups/${groupId}/budget-goals/${goalId}/progress`,
      { method: 'GET' }
    );
  },

  updateGoal(groupId: number, goalId: number, dto: UpdateBudgetGoalDto) {
    return useApiFetch<BudgetGoalDto>(`/api/groups/${groupId}/budget-goals/${goalId}`, {
      method: 'PATCH',
      body: dto,
    });
  },

  deleteGoal(groupId: number, goalId: number) {
    return useApiFetch(`/api/groups/${groupId}/budget-goals/${goalId}`, { method: 'DELETE' });
  },
};
