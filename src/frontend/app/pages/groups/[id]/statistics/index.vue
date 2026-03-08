<script setup lang="ts">
import { ref, onMounted, reactive, computed, h } from 'vue';
import type { TableColumn } from '@nuxt/ui';
import { useRoute } from '#imports';
import * as v from 'valibot';
import { statisticsFilterSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import { statisticsService } from '~/services/statisticsService';
import { sellerService } from '~/services/sellerService';
import { categoryService } from '~/services/categoryService';
import type { ProductStatisticDto, SpendingHistoryDataPointDto, StatisticsFilterDto } from '~/services/statisticsService';
import type { SellerDto } from '~/services/sellerService';
import type { CategoryDto } from '~/services/categoryService';
import type { FormSubmitEvent } from '@nuxt/ui';
import { formatDate } from '@/utils/formatDate';
import FormGlobalErrors from "~/components/FormGlobalErrors.vue";
import SingleCategoryPicker from '~/components/SingleCategoryPicker.vue';
import SellerPicker from '~/components/SellerPicker.vue';

type Schema = v.InferOutput<typeof statisticsFilterSchema>;

const route = useRoute();
const groupId = Number(route.params.id);

const topProducts = ref<ProductStatisticDto[]>([]);
const spendingHistory = ref<SpendingHistoryDataPointDto[]>([]);
const availableSellers = ref<SellerDto[]>([]);
const availableCategories = ref<CategoryDto[]>([]);
const availableSystemCategories = ref<CategoryDto[]>([]);

const loadingDependencies = ref(false);
const isSubmitting = ref(false);
const error = ref<string | null>(null);

const today = new Date();
const firstDay = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0];
const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0).toISOString().split('T')[0];

const groupBy = ref<'day' | 'month' | 'year'>('day');
const MAX_DONUT_ITEMS = 5;

const filter = reactive<StatisticsFilterDto>({
  startDate: firstDay!,
  endDate: lastDay!,
  isPersonalBudgetOnly: false,
  sellerId: null,
  categoryId: null,
  top: 10
});

const { isFormValid, unmappedErrors, touch } = useFormValidation(statisticsFilterSchema, filter);

const spendingChartData = computed(() => {
  if (!spendingHistory.value.length) return [];

  const aggregated = spendingHistory.value.reduce((acc, curr) => {
    const date = new Date(curr.date);
    let key = '';

    if (groupBy.value === 'year') {
      key = `${date.getFullYear()}`;
    } else if (groupBy.value === 'month') {
      key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
    } else {
      key = formatDate(curr.date)!;
    }

    acc[key] = (acc[key] || 0) + curr.totalSpent;
    return acc;
  }, {} as Record<string, number>);

  const data = Object.entries(aggregated).map(([date, amount]) => ({
    date,
    amount
  }));

  if (data.length === 1) {
    return [
      { date: data[0]!.date, amount: data[0]!.amount },
      { date: data[0]!.date, amount: data[0]!.amount }
    ];
  }

  return data;
});

const xLineChartFormatter = (tick: number): string => {
  const label = spendingChartData.value[tick]?.date ?? '';
  return label;
};

const lineCategories = {
  amount: { name: 'Total Spent', color: '#3b82f6' }
};

const totalSpentSum = computed(() => {
  return topProducts.value.reduce((sum, p) => sum + p.totalSpent, 0);
});

const processedDonutData = computed(() => {
  if (!topProducts.value.length) return [];
  
  const sorted = [...topProducts.value].sort((a, b) => b.totalSpent - a.totalSpent);
  
  if (sorted.length <= MAX_DONUT_ITEMS) {
    return sorted;
  }
  
  const topItems = sorted.slice(0, MAX_DONUT_ITEMS);
  const otherTotal = sorted.slice(MAX_DONUT_ITEMS).reduce((sum, item) => sum + item.totalSpent, 0);
  
  return [
    ...topItems,
    { productName: 'Other', totalSpent: otherTotal } as ProductStatisticDto 
  ];
});

const donutChartData = computed<number[]>(() => {
  return processedDonutData.value.map(p => p.totalSpent);
});

const chartColors = [
  '#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', 
  '#ec4899', '#06b6d4', '#14b8a6', '#f97316', '#6366f1'
];

const productColumns: TableColumn<ProductStatisticDto>[] = [
  {
    accessorKey: 'productName',
    header: 'Product',
    cell: ({ row }) => h(
      'span',
      { class: 'text-gray-900 dark:text-white font-medium whitespace-normal break-words max-w-xs' },
      row.original.productName || '-'
    )
  },
  {
    accessorKey: 'totalSpent',
    header: 'Total Spent',
    cell: ({ row }) => h(
      'span',
      { class: 'font-semibold tabular-nums' },
      row.original.totalSpent.toFixed(2)
    )
  }
];

const historyColumns: TableColumn<SpendingHistoryDataPointDto>[] = [
  {
    accessorKey: 'date',
    header: 'Date',
    cell: ({ row }) => h(
      'span',
      { class: 'text-gray-900 dark:text-white font-medium' },
      formatDate(row.original.date) || row.original.date
    )
  },
  {
    accessorKey: 'totalSpent',
    header: 'Amount Spent',
    cell: ({ row }) => h(
      'span',
      { class: 'font-semibold tabular-nums' },
      row.original.totalSpent.toFixed(2)
    )
  }
];

const donutCategories = computed(() => {
  const cats: Record<string, { name: string; color: string }> = {};
  
  processedDonutData.value.forEach((p, index) => {
    const color = p.productName === 'Other' ? '#9ca3af' : chartColors[index % chartColors.length];
    
    cats[p.productName] = { 
      name: p.productName, 
      color: color!
    };
  });
  
  return cats;
});

function getDonutCategoryColor(label?: string) {
  if (!label) return '#000000';
  return donutCategories.value[label]?.color || '#000000';
}

async function loadDependencies() {
  loadingDependencies.value = true;
  try {
    const [sellersRes, categoriesRes, systemCats] = await Promise.all([
      sellerService.getSellers(groupId),
      categoryService.getCategories(groupId),
      categoryService.getSystemCategories(groupId)
    ]);
    availableSellers.value = sellersRes.sellers;
    availableCategories.value = categoriesRes.categories;
    availableSystemCategories.value = systemCats;
  } catch (err: any) {
    error.value = 'Failed to load filters data.';
  } finally {
    loadingDependencies.value = false;
  }
}

async function fetchStatistics(event?: FormSubmitEvent<Schema>) {
  isSubmitting.value = true;
  error.value = null;
  
  try {
    const filterData = event ? event.data : filter;

    const [products, history] = await Promise.all([
      statisticsService.getTopProducts(groupId, filterData),
      statisticsService.getSpendingHistory(groupId, filterData)
    ]);
    
    topProducts.value = products;
    spendingHistory.value = history;
  } catch (err: any) {
    error.value = err.message || 'Failed to fetch statistics';
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(async () => {
  await loadDependencies();
  await fetchStatistics();
});
</script>

<template>
  <div class="w-full lg:max-w-6xl md:max-w-4xl sm:max-w-2xl mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Analysis & Statistics</h1>
      <UButton 
        :to="`/groups/${groupId}`"
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Group
      </UButton>
    </div>

    <UAlert 
      v-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-6" 
    />

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start lg:items-stretch w-full max-w-full">
      
      <div class="lg:col-span-2 w-full flex flex-col gap-6">
        
        <UCard class="shadow-sm" :ui="{ header: 'overflow-visible' }">
          <template #header>
            <div class="flex justify-between items-center">
              <h2 class="font-semibold text-gray-900 dark:text-white">Spending History</h2>
              <USelect
                v-model="groupBy"
                :items="[
                  { label: 'By Day', value: 'day' },
                  { label: 'By Month', value: 'month' },
                  { label: 'By Year', value: 'year' }
                ]"
                value-key="value"
                size="xs"
                :portal="true"
              />
            </div>
          </template>

          <div class="w-full min-h-80 flex flex-col items-center justify-center pt-8 pb-16">
            <span v-if="isSubmitting" class="text-gray-500 animate-pulse">Loading data...</span>
            <span v-else-if="!spendingHistory.length" class="text-gray-500">No spending data found for this period.</span>
            
            <LineChart 
              v-else
              :data="spendingChartData"
              :height="300"
              index="date" 
              :categories="lineCategories"
              :curve-type="CurveType.Linear"
              :legend-position="LegendPosition.TopRight"
              :hide-legend="false"
              :x-formatter="xLineChartFormatter"
              :y-grid-line="true"
              :show-y-axis="true" 
              :y-num-ticks="2"
              :x-num-ticks="4"
              class="w-full h-full" 
            >
              <template #tooltip="{ values }">
                <div class="flex flex-col gap-1 p-1 text-sm min-w-36 max-w-56">
                  <span class="text-gray-400 dark:text-gray-500 text-xs font-medium">{{ values?.date }}</span>
                  <div v-for="(cat, key) in lineCategories" :key="key" class="flex items-center gap-2">
                    <span class="size-2.5 rounded-full shrink-0" :style="{ backgroundColor: cat.color }" />
                    <span class="text-gray-700 dark:text-gray-300 font-medium">{{ cat.name }}</span>
                    <span class="text-gray-900 dark:text-white font-bold ml-auto pl-3">{{ values?.[key] != null ? Number(values[key]).toFixed(2) : '' }}</span>
                  </div>
                </div>
              </template>
            </LineChart>
          </div>
        </UCard>

        <UCard class="shadow-sm">
          <template #header>
            <h2 class="font-semibold text-gray-900 dark:text-white">Most Common Products</h2>
          </template>
          
          <div class="w-full min-h-80 flex flex-col items-center justify-center pt-8 pb-4">
            <span v-if="isSubmitting" class="text-gray-500 animate-pulse">Loading data...</span>
            <span v-else-if="!topProducts.length" class="text-gray-500">No products found for this period.</span>
            
            <div v-else class="w-full">
              <DonutChart 
                :data="donutChartData" 
                :categories="donutCategories"
                :type="DonutType.Full"
                :hide-legend="false"
                :legend-position="LegendPosition.BottomCenter"
                :height="280"
                :radius="5"
                class="w-full"
                :style="{ '--vis-donut-background-color': 'transparent' }"
              >
                <div class="flex flex-col items-center justify-center pointer-events-none">
                  <span class="text-xs text-dimmed uppercase tracking-wider font-semibold">Total</span>
                  <span class="text-2xl font-bold text-default">
                    {{ totalSpentSum.toFixed(2) }}
                  </span>
                </div>
                
                <template #tooltip="{ values }">
                  <div class="flex flex-col gap-1 p-1 text-sm min-w-36 max-w-56">
                    <div class="flex items-center gap-2">
                      <span
                        class="size-2.5 rounded-full shrink-0"
                        :style="{ backgroundColor: getDonutCategoryColor(values?.label) }"
                      />
                      <span class="text-gray-700 dark:text-gray-300 font-medium leading-snug wrap-break-words">{{ values?.label }}</span>
                    </div>
                    <span class="text-gray-900 dark:text-white font-bold pl-4">{{ values ? Number(values[values?.label]).toFixed(2) : '' }}</span>
                  </div>
                </template>
              </DonutChart>
            </div>
          </div>
        </UCard>

      </div>

      <div class="lg:col-span-1 w-full">
        <UCard class="shadow-sm sticky top-4">
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-funnel" class="w-5 h-5 text-gray-500" />
              <h2 class="font-semibold text-gray-900 dark:text-white">Filters</h2>
            </div>
          </template>

          <UForm 
            :schema="statisticsFilterSchema" 
            :state="filter" 
            class="flex flex-col gap-5" 
            @submit="fetchStatistics"
          >
            <div class="grid grid-cols-2 gap-4">
              <UFormField label="Start Date" name="startDate" required>
                <UInput type="date" v-model="filter.startDate" @change="touch" class="w-full" />
              </UFormField>
              
              <UFormField label="End Date" name="endDate" required>
                <UInput type="date" v-model="filter.endDate" @change="touch" class="w-full" />
              </UFormField>
            </div>

            <USeparator />

            <UFormField label="Filter by Category" name="categoryId">
              <SingleCategoryPicker 
                v-model="filter.categoryId!" 
                :categories="availableCategories"
                :systemCategories="availableSystemCategories"
                :disabled="loadingDependencies"
                clearable
              />
            </UFormField>

            <UFormField label="Filter by Seller" name="sellerId">
              <SellerPicker 
                v-model="filter.sellerId!" 
                :sellers="availableSellers" 
                :can-create="false" 
                clearable
              />
            </UFormField>

            <UFormField label="Top Products (1-100)" name="top">
              <UInput type="number" v-model.number="filter.top" min="1" max="100" class="w-full" />
            </UFormField>

            <UFormField name="isPersonalBudgetOnly">
              <USwitch 
                v-model="filter.isPersonalBudgetOnly" 
                label="Show my personal budget only" 
              />
            </UFormField>

            <FormGlobalErrors :errors="unmappedErrors" />

            <div class="mt-4 pt-4 border-t border-gray-200 dark:border-gray-800">
              <UButton 
                type="submit" 
                color="primary" 
                class="w-full justify-center"
                :loading="isSubmitting"
                :disabled="isSubmitting || !isFormValid"
              >
                Apply Filters
              </UButton>
            </div>
          </UForm>
        </UCard>
      </div>

    </div>

    <!-- Data Tables -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6 w-full max-w-full">

      <!-- Spending History Table -->
      <div>
        <h2 class="font-semibold text-gray-900 dark:text-white mb-2 px-1">Spending History</h2>
        <UCard :ui="{ body: 'p-0 sm:p-0' }" class="shadow-sm overflow-hidden">
          <UTable
            sticky
            :data="spendingHistory"
            :columns="historyColumns"
            :loading="isSubmitting"
            class="h-72 w-full"
          >
            <template #empty>
              <div class="flex flex-col items-center justify-center py-10">
                <span class="text-gray-500 dark:text-gray-400">No spending data found for this period.</span>
              </div>
            </template>
          </UTable>
        </UCard>
      </div>

      <!-- Top Products Table -->
      <div>
        <h2 class="font-semibold text-gray-900 dark:text-white mb-2 px-1">Top Products</h2>
        <UCard :ui="{ body: 'p-0 sm:p-0' }" class="shadow-sm overflow-hidden">
          <UTable
            sticky
            :data="topProducts"
            :columns="productColumns"
            :loading="isSubmitting"
            class="h-72 w-full"
          >
            <template #empty>
              <div class="flex flex-col items-center justify-center py-10">
                <span class="text-gray-500 dark:text-gray-400">No products found for this period.</span>
              </div>
            </template>
          </UTable>
        </UCard>
      </div>

    </div>
  </div>
</template>