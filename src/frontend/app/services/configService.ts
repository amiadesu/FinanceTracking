export interface GroupConfigDto {
  maxGroupsPerUser: number;
  maxMembersPerGroup: number;
  maxCategoriesPerGroup: number;
  maxReceiptsPerGroup: number;
  maxSellersPerGroup: number;
  maxBudgetGoalsPerGroup: number;
}

export interface ReceiptConfigDto {
  maxProductsPerReceipt: number;
  maxCategoriesPerProduct: number;
}

export interface CategoryConfigDto {
  defaultCategoryColor: string;
}

export interface SystemConfigDto {
  infiniteLimit: number;
  groupLimits: GroupConfigDto;
  receiptLimits: ReceiptConfigDto;
  categoryRules: CategoryConfigDto;
  groupRoles: Record<string, number>;
}

import { useApiFetch } from '@/utils/useApiFetch';

export const configService = {
  getSystemConfiguration() {
    return useApiFetch<SystemConfigDto>('/api/config', { method: 'GET' });
  }
};