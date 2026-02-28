export interface SellerDto {
  id: number;
  groupId: number | null;
  name: string;
  description?: string;
  createdDate: string;
  updatedDate: string;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const sellerService = {
  getSellers(groupId: number) {
    return useApiFetch<SellerDto[]>(`/api/groups/${groupId}/sellers`, { method: 'GET' });
  },

  getSeller(groupId: number, sellerId: number) {
    return useApiFetch<SellerDto>(`/api/groups/${groupId}/sellers/${sellerId}`, { method: 'GET' });
  },
};
