export interface ReceiptProductDto {
  id: number;
  name: string;
  categories: string[];
  price: number;
  quantity: number;
}

export interface ReceiptDto {
  id: number;
  groupId: number;
  createdByUserId?: string;
  sellerId: string;
  sellerName?: string;
  totalAmount: number;
  paymentDate: string;
  createdDate: string;
  updatedDate: string;
  products: ReceiptProductDto[];
}

export interface ReceiptListResponseDto {
  currentCount: number;
  maxAllowed: number;
  receipts: ReceiptDto[];
}

export interface CreateReceiptProductDto {
  name: string;
  categories: string[];
  price: number;
  quantity: number;
}

export interface CreateReceiptDto {
  sellerId: string;
  paymentDate: string;
  products: CreateReceiptProductDto[];
}

export interface UpdateReceiptProductDto {
  id?: number;
  name?: string;
  categories?: string[];
  price?: number;
  quantity?: number;
}

export interface UpdateReceiptDto {
  sellerId?: string;
  paymentDate?: string;
  products?: UpdateReceiptProductDto[];
}

import { useApiFetch } from '@/utils/useApiFetch';

export const receiptService = {
  createReceipt(groupId: number, dto: CreateReceiptDto) {
    return useApiFetch<ReceiptDto>(`/api/groups/${groupId}/receipts`, {
      method: 'POST',
      body: dto,
    });
  },

  getFilteredReceipts(groupId: number, params: { sellerId?: string; productDataId?: number }) {
    const query = new URLSearchParams();
    if (params.sellerId) query.append('sellerId', params.sellerId.toString());
    if (params.productDataId) query.append('productDataId', params.productDataId.toString());

    const queryString = query.toString() ? `?${query.toString()}` : '';

    return useApiFetch<ReceiptDto[]>(`/api/groups/${groupId}/receipts${queryString}`, { method: 'GET' });
  },

  getReceipts(groupId: number) {
    return useApiFetch<ReceiptListResponseDto>(`/api/groups/${groupId}/receipts`, { method: 'GET' });
  },

  getReceipt(groupId: number, receiptId: number) {
    return useApiFetch<ReceiptDto>(`/api/groups/${groupId}/receipts/${receiptId}`, {
      method: 'GET',
    });
  },

  async uploadReceiptXml(groupId: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return useApiFetch<CreateReceiptDto>(`/api/groups/${groupId}/receipts/upload-xml`, {
      method: 'POST',
      body: formData,
    });
  },

  updateReceipt(groupId: number, receiptId: number, dto: UpdateReceiptDto) {
    return useApiFetch<ReceiptDto>(`/api/groups/${groupId}/receipts/${receiptId}`, {
      method: 'PATCH',
      body: dto,
    });
  },

  deleteReceipt(groupId: number, receiptId: number) {
    return useApiFetch(`/api/groups/${groupId}/receipts/${receiptId}`, {
      method: 'DELETE',
    });
  },
};
