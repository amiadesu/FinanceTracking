export interface StatisticsFilterDto {
  startDate: string;
  endDate: string;
  isPersonalBudgetOnly: boolean;
  sellerId?: string | null;
  categoryId?: number | null;
  top: number;
}

export interface ProductStatisticDto {
  productName: string;
  totalQuantity: number;
  totalSpent: number;
}

export interface SpendingHistoryDataPointDto {
  date: string;
  totalSpent: number;
}

import { useApiFetch } from '@/utils/useApiFetch';

function buildQueryString(filter: StatisticsFilterDto): string {
  const params = new URLSearchParams();
  params.append('startDate', filter.startDate);
  params.append('endDate', filter.endDate);
  params.append('personalOnly', filter.isPersonalBudgetOnly.toString());
  params.append('top', filter.top.toString());

  if (filter.sellerId) params.append('sellerId', filter.sellerId);
  if (filter.categoryId) params.append('categoryId', filter.categoryId.toString());

  return params.toString();
}

export const statisticsService = {
  getTopProducts(groupId: number, filter: StatisticsFilterDto) {
    const query = buildQueryString(filter);
    return useApiFetch<ProductStatisticDto[]>(`/api/groups/${groupId}/statistics/top-products?${query}`, { 
      method: 'GET' 
    });
  },

  getSpendingHistory(groupId: number, filter: StatisticsFilterDto) {
    const query = buildQueryString(filter);
    return useApiFetch<SpendingHistoryDataPointDto[]>(`/api/groups/${groupId}/statistics/spending-history?${query}`, { 
      method: 'GET' 
    });
  }
};