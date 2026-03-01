export interface GroupDto {
  id: number;
  name: string;
  isFull: boolean;
  isPersonal: boolean;
  ownerId: string | null;
  createdDate: string;
}

export interface GroupListResponseDto {
  currentCount: number;
  maxAllowed: number;
  groups: GroupDto[];
}

export interface CreateGroupDto {
  name: string;
}

export interface UpdateGroupDto {
  name: string;
}

export interface ResetGroupDto {
  resetMembers: boolean;
  resetBudgetGoals: boolean;
  resetCategories: boolean;
  resetReceiptsProductsAndSellers: boolean;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const groupService = {
  createGroup(dto: CreateGroupDto) {
    return useApiFetch<GroupDto>('/api/groups', {
      method: 'POST',
      body: dto,
    });
  },

  getMyGroups() {
    return useApiFetch<GroupListResponseDto>('/api/groups', {
      method: 'GET',
    });
  },

  getGroup(groupId: number) {
    return useApiFetch<GroupDto>(`/api/groups/${groupId}`, {
      method: 'GET',
    });
  },

  updateGroup(groupId: number, dto: UpdateGroupDto) {
    return useApiFetch<GroupDto>(`/api/groups/${groupId}`, {
      method: 'PATCH',
      body: dto,
    });
  },

  resetGroup(groupId: number, dto: ResetGroupDto) {
    return useApiFetch(`/api/groups/${groupId}/reset`, {
      method: 'PATCH',
      body: dto,
    });
  },

  deleteGroup(groupId: number) {
    return useApiFetch(`/api/groups/${groupId}`, {
      method: 'DELETE',
    });
  },

  leaveGroup(groupId: number) {
    return useApiFetch(`/api/groups/${groupId}/leave`, {
      method: 'POST',
    });
  }
};