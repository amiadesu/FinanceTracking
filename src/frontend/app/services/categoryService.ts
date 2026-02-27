export interface CategoryDto {
  id: number;
  groupId: number | null;
  name: string;
  colorHex: string;
  isSystem: boolean;
  createdDate: string;
  updatedDate: string;
}

export interface CreateCategoryDto {
  name: string;
  colorHex: string;
}

export interface UpdateCategoryDto {
  name?: string;
  colorHex?: string;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const categoryService = {
  createCategory(groupId: number, dto: CreateCategoryDto) {
    return useApiFetch<CategoryDto>(`/api/groups/${groupId}/categories`, {
      method: 'POST',
      body: dto,
    });
  },

  getCategories(groupId: number) {
    return useApiFetch<CategoryDto[]>(`/api/groups/${groupId}/categories`, { method: 'GET' });
  },

  getCategory(groupId: number, categoryId: number) {
    return useApiFetch<CategoryDto>(`/api/groups/${groupId}/categories/${categoryId}`, { method: 'GET' });
  },

  updateCategory(groupId: number, categoryId: number, dto: UpdateCategoryDto) {
    return useApiFetch<CategoryDto>(`/api/groups/${groupId}/categories/${categoryId}`, {
      method: 'PATCH',
      body: dto,
    });
  },

  deleteCategory(groupId: number, categoryId: number) {
    return useApiFetch(`/api/groups/${groupId}/categories/${categoryId}`, { method: 'DELETE' });
  },
};
