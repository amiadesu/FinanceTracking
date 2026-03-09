<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute } from '#imports';
import { budgetGoalService } from '~/services/budgetGoalService';
import type { BudgetGoalProgressDto } from '~/services/budgetGoalService';
import type { ReceiptDto } from '~/services/receiptService';
import { clamp } from '~/utils/clamp';
import type { TableColumn } from '@nuxt/ui';
import { formatDate } from '@/utils/formatDate';

const route = useRoute();
const groupId = Number(route.params.id);
const goalId = Number(route.params.goalId);

const progress = ref<BudgetGoalProgressDto | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const progressPercentageDisplay = computed(() => 
    progress.value != null ? clamp(progress.value.percentageCompleted, 0, 100) : 0
);

async function load() {
  loading.value = true;
  error.value = null;
  try {
    progress.value = await budgetGoalService.getProgress(groupId, goalId);
  } catch (err: any) {
    error.value = err.message || 'Failed to load progress data';
  } finally {
    loading.value = false;
  }
}

onMounted(load);

const progressColor = computed(() => {
  if (!progress.value) return 'primary';
  if (progress.value.percentageCompleted > 100) return 'error';
  if (progress.value.percentageCompleted > 80) return 'warning';
  return 'primary';
});

const receiptColumns: TableColumn<ReceiptDto>[] = [
  { accessorKey: 'id', header: 'Receipt ID' },
  { 
    id: 'paymentDate', 
    header: 'Payment Date',
    cell: ({ row }) => formatDate(row.original.paymentDate)
  },
  { 
    accessorKey: 'sellerName', 
    header: 'Seller',
    cell: ({ row }) => row.original.sellerName || 'Unknown Seller'
  },
  { 
    id: 'totalAmount', 
    header: 'Amount',
    cell: ({ row }) => `${row.original.totalAmount.toFixed(2)}`
  }
];
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Goal Progress #{{ goalId }}</h1>
      <UButton 
        :to="`/groups/${groupId}/goals`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Goals
      </UButton>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2 mb-4">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading progress...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <div v-else-if="progress" class="flex flex-col gap-6">
      <UCard class="shadow-sm w-full">
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-8 mb-6">
          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Period</span>
            <p class="text-lg font-medium text-gray-900 dark:text-white mt-1">
              {{ formatDate(progress.startDate) }} → {{ formatDate(progress.endDate) }}
            </p>
          </div>
          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Current / Target</span>
            <p class="text-xl font-semibold text-gray-900 dark:text-white mt-1">
              {{ progress.currentAmount }} <span class="text-base font-normal text-gray-500">/ {{ progress.targetAmount }}</span>
            </p>
          </div>
          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Status</span>
            <p class="text-lg font-semibold mt-1" :class="`text-${progressColor}-500`">
              {{ progress.percentageCompleted }}% Completed
            </p>
          </div>
        </div>

        <UProgress 
          v-model="progressPercentageDisplay" 
          :max="100"
          :color="progressColor" 
          class="h-3 mb-2" 
        />
        <div class="text-right text-xs text-gray-500">
          <span v-if="progress.percentageCompleted > 100" class="text-red-500 font-medium">Over budget!</span>
          <span v-else>Keep it up!</span>
        </div>
      </UCard>

      <UCard class="shadow-sm w-full max-h-100 flex flex-col" :ui="{ body: 'p-0 sm:p-0 flex-1' }">
        <template #header>
          <div class="flex justify-between items-center">
            <h2 class="font-semibold text-gray-900 dark:text-white">Associated Receipts</h2>
            <UBadge color="neutral" variant="soft">{{ progress.associatedReceipts.length }} Receipts</UBadge>
          </div>
        </template>
        
        <UTable 
          :data="progress.associatedReceipts" 
          :columns="receiptColumns"
          class="min-h-0 overflow-y-auto"
        >
          <template #empty>
            <div class="flex flex-col items-center justify-center py-12">
              <span class="text-gray-500 dark:text-gray-400">No receipts logged for this period yet.</span>
            </div>
          </template>
        </UTable>
      </UCard>
    </div>
  </div>
</template>