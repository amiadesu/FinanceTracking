export interface SellerDto {
  id: number;
  groupId: number | null;
  name: string;
  description?: string;
  createdDate: string;
  updatedDate: string;
}

export interface UpdateSellerDto {
  name?: string | null;
  description?: string | null;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const sellerService = {
  getSellers(groupId: number) {
    return useApiFetch<SellerDto[]>(`/api/groups/${groupId}/sellers`, { method: 'GET' });
  },

  getSeller(groupId: number, sellerId: number) {
    return useApiFetch<SellerDto>(`/api/groups/${groupId}/sellers/${sellerId}`, { method: 'GET' });
  },

  updateSeller(groupId: number, sellerId: number, dto: UpdateSellerDto) {
    return useApiFetch<SellerDto>(`/api/groups/${groupId}/sellers/${sellerId}`, {
      method: 'PATCH',
      body: dto,
    });
  },

  deleteSeller(groupId: number, sellerId: number) {
    return useApiFetch(`/api/groups/${groupId}/sellers/${sellerId}`, {
      method: 'DELETE',
    });
  }
};
