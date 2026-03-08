export interface GroupHistoryDto {
  id: number;
  note: string;
  roleIdBefore: number | null;
  roleIdAfter: number | null;
  activeBefore: boolean | null;
  activeAfter: boolean | null;
  nameBefore: string | null;
  nameAfter: string | null;
  changedAt: string;
  targetUserName: string;
  changedByUserName: string;
}

export interface GroupHistoryListResponseDto {
  countOnPage: number;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  groupHistoryEntries: GroupHistoryDto[];
}

import { useApiFetch } from '@/utils/useApiFetch';

export const groupHistoryService = {
  getGroupHistory(groupId: number, pageNumber: number = 1, pageSize: number = 20) {
    const query = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    }).toString();

    return useApiFetch<GroupHistoryListResponseDto>(`/api/groups/${groupId}/history?${query}`, {
      method: 'GET',
    });
  },

  exportGroupHistory(groupId: number, fileType: 'xlsx' | 'docx') {
    const query = new URLSearchParams({ fileType }).toString();
    
    return useApiFetch<Blob>(`/api/groups/${groupId}/history/export?${query}`, {
      method: 'GET',
      responseType: 'blob', 
    });
  }
};