export interface SellerDto {
  id: string;
  groupId: number | null;
  name: string;
  description?: string;
  createdDate: string;
  updatedDate: string;
}

export interface SellerListResponseDto {
  currentCount: number;
  maxAllowed: number;
  sellers: SellerDto[];
}

export interface UpdateSellerDto {
  name?: string | null;
  description?: string | null;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const sellerService = {
  getSellers(groupId: number) {
    return useApiFetch<SellerListResponseDto>(`/api/groups/${groupId}/sellers`, { method: 'GET' });
  },

  getSeller(groupId: number, sellerId: string) {
    return useApiFetch<SellerDto>(`/api/groups/${groupId}/sellers/${sellerId}`, { method: 'GET' });
  },

  updateSeller(groupId: number, sellerId: string, dto: UpdateSellerDto) {
    return useApiFetch<SellerDto>(`/api/groups/${groupId}/sellers/${sellerId}`, {
      method: 'PATCH',
      body: dto,
    });
  },

  deleteSeller(groupId: number, sellerId: string) {
    return useApiFetch(`/api/groups/${groupId}/sellers/${sellerId}`, {
      method: 'DELETE',
    });
  }
};
