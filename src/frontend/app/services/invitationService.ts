export interface InvitationResponseDto {
  id: string;
  groupId: number;
  groupName: string;
  invitedByUserId: string;
  invitedByUserName: string;
  targetUserId: string;
  targetUserName: string;
  note: string;
  status: string;
  createdDate: string;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const invitationService = {
  getPendingInvitations() {
    return useApiFetch<InvitationResponseDto[]>('/api/invitations', { method: 'GET' })
  },
  getGroupInvitations(groupId: number) {
    return useApiFetch<InvitationResponseDto[]>(`/api/groups/${groupId}/invitations`, { method: 'GET' })
  },
  getSpecificInvitation(id: string) {
    return useApiFetch<InvitationResponseDto>(`/api/invitations/${id}`, { method: 'GET' })
  },
  acceptInvitation(id: string) {
    return useApiFetch(`/api/invitations/${id}/accept`, { method: 'POST' })
  },
  rejectInvitation(id: string) {
    return useApiFetch(`/api/invitations/${id}/reject`, { method: 'POST' })
  },

  createInvitation(groupId: number, targetUserIdentifier: string, note: string) {
    return useApiFetch<InvitationResponseDto>(`/api/groups/${groupId}/invitations`, {
      method: 'POST',
      body: { targetUserIdentifier, note }
    })
  },
  cancelInvitation(groupId: number, invitationId: string) {
    return useApiFetch(`/api/groups/${groupId}/invitations/${invitationId}`, { method: 'DELETE' })
  },

  getPendingCount() {
    return useApiFetch<{ count: number }>(`/api/invitations/pending/count`, { method: 'GET' })
  }
}