export interface ProductDataDto {
  id: number;
  groupId: number;
  name: string;
  description?: string;
  categories: string[];
  createdDate: string;
  updatedDate: string;
}

export interface UpdateProductDataDto {
  name?: string | null;
  description?: string | null;
  categoryIds?: number[] | null;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const productService = {
  getProducts(groupId: number) {
    return useApiFetch<ProductDataDto[]>(`/api/groups/${groupId}/products`, { method: 'GET' });
  },

  getProduct(groupId: number, productId: number) {
    return useApiFetch<ProductDataDto>(`/api/groups/${groupId}/products/${productId}`, { method: 'GET' });
  },

  updateProduct(groupId: number, productId: number, dto: UpdateProductDataDto) {
    return useApiFetch<ProductDataDto>(`/api/groups/${groupId}/products/${productId}`, {
      method: 'PATCH',
      body: dto,
    });
  },

  deleteProduct(groupId: number, productId: number) {
    return useApiFetch(`/api/groups/${groupId}/products/${productId}`, {
      method: 'DELETE',
    });
  }
};