export interface GroupMemberDto {
  userId: string;
  userName: string;
  role: number;
  active: boolean;
  joinedDate: string;
}

export interface GroupMemberListResponseDto {
  currentCount: number;
  maxAllowed: number;
  groupMembers: GroupMemberDto[];
}

export interface UpdateGroupMemberRoleDto {
  role?: number | null;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const groupMemberService = {
  getMembers(groupId: number) {
    return useApiFetch<GroupMemberListResponseDto>(`/api/groups/${groupId}/members`, { method: 'GET' });
  },

  getMember(groupId: number, userId: string) {
    return useApiFetch<GroupMemberDto>(`/api/groups/${groupId}/members/${userId}`, { method: 'GET' });
  },

  getMyRole(groupId: number) {
    return useApiFetch<{ role: number }>(`/api/groups/${groupId}/members/me/role`, { method: 'GET' });
  },

  updateRole(groupId: number, userId: string, dto: UpdateGroupMemberRoleDto) {
    return useApiFetch<GroupMemberDto>(`/api/groups/${groupId}/members/${userId}/role`, {
      method: 'POST',
      body: dto,
    });
  },

  transferOwnership(groupId: number, userId: string) {
    return useApiFetch(`/api/groups/${groupId}/members/${userId}/transfer-ownership`, {
      method: 'POST',
    });
  },

  removeMember(groupId: number, userId: string) {
    return useApiFetch(`/api/groups/${groupId}/members/${userId}`, {
      method: 'DELETE',
    });
  }
};