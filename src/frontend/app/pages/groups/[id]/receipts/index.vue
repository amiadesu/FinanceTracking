<script setup lang="ts">
import { ref, onMounted, h } from 'vue';
import { useRoute, useRouter } from '#imports';
import { receiptService } from '~/services/receiptService';
import { categoryService } from '~/services/categoryService';
import { sellerService } from '~/services/sellerService';
import type { ReceiptDto } from '~/services/receiptService';
import type { CategoryDto } from '~/services/categoryService';
import type { SellerDto } from '~/services/sellerService';
import { useLimitDisplay } from '~/composables/useLimitDisplay';
import type { TableColumn } from '@nuxt/ui';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const receipts = ref<ReceiptDto[]>([]);
const categories = ref<CategoryDto[]>([]);
const sellers = ref<SellerDto[]>([]);
const currentCount = ref(0);
const maxAllowed = ref(0);

const loading = ref(false);
const error = ref<string | null>(null);

const limitDisplay = useLimitDisplay(currentCount, maxAllowed);

const columns: TableColumn<ReceiptDto>[] = [
  { accessorKey: 'id', header: 'ID' },
  { 
    accessorKey: 'paymentDate', 
    header: 'Date',
    cell: ({ row }) => new Date(row.original.paymentDate).toLocaleDateString()
  },
  { 
    id: 'seller', 
    header: 'Seller',
    cell: ({ row }) => h(
      'div', 
      { class: 'whitespace-normal break-words sm:break-all min-w-[120px] max-w-[250px] font-medium text-gray-900 dark:text-white' }, 
      row.original.sellerName ? row.original.sellerName : (row.original.sellerId ?? '-')
    )
  },
  { 
    accessorKey: 'totalAmount', 
    header: 'Total',
    cell: ({ row }) => row.original.totalAmount.toFixed(2)
  },
  { 
    id: 'productCount', 
    header: 'Products',
    cell: ({ row }) => row.original.products.length.toString()
  },
  { id: 'actions', header: '' }
];

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const [receiptsResponse, categoriesResponse, sellersResponse] = await Promise.all([
      receiptService.getReceipts(groupId),
      categoryService.getCategories(groupId),
      sellerService.getSellers(groupId)
    ]);

    currentCount.value = receiptsResponse.currentCount;
    maxAllowed.value = receiptsResponse.maxAllowed;
    receipts.value = receiptsResponse.receipts;
    categories.value = categoriesResponse.categories;
    sellers.value = sellersResponse.sellers;
  } catch (err: any) {
    error.value = err.message || 'Failed to load data';
  } finally {
    loading.value = false;
  }
}

onMounted(() => loadData());
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Receipts</h1>
        <UBadge v-if="!loading" color="neutral" variant="subtle" size="md">
          Capacity: {{ limitDisplay }}
        </UBadge>
      </div>
      
      <div class="flex items-center gap-3">
        <UButton 
          :to="`/groups/${groupId}/receipts/create`" 
          color="primary" 
          icon="i-heroicons-plus"
        >
          Create Receipt
        </UButton>
        <UButton 
          :to="`/groups/${groupId}`" 
          color="secondary" 
          variant="outline" 
          icon="i-heroicons-arrow-left"
        >
          Back to Group
        </UButton>
      </div>
    </div>
    
    <UAlert 
      v-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <UCard :ui="{ body: 'p-0 sm:p-0 flex-1 flex flex-col min-h-0' }" class="shadow-sm overflow-hidden flex flex-col w-full lg:h-100 max-w-full">
      <UTable 
        sticky 
        :data="receipts" 
        :columns="columns" 
        :loading="loading" 
        class="w-full flex-1 min-h-0 overflow-y-auto"
      >
        <template #actions-cell="{ row }">
          <div class="text-right">
            <UButton 
              :to="`/groups/${groupId}/receipts/${row.original.id}`" 
              color="primary" 
              variant="outline" 
              size="sm"
              icon="i-heroicons-cog"
            >
              Manage
            </UButton>
          </div>
        </template>
        
        <template #empty>
          <div class="flex flex-col items-center justify-center py-12 h-full">
            <span class="text-gray-500 dark:text-gray-400">No receipts found.</span>
          </div>
        </template>
      </UTable>
    </UCard>
  </div>
</template>