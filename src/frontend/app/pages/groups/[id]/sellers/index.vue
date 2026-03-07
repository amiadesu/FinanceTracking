<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from '#imports';
import { sellerService } from '~/services/sellerService';
import type { SellerDto } from '~/services/sellerService';
import { useLimitDisplay } from '~/composables/useLimitDisplay';
import type { TableColumn } from '@nuxt/ui';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const sellers = ref<SellerDto[]>([]);
const currentCount = ref(0);
const maxAllowed = ref(0);

const loading = ref(false);
const error = ref<string | null>(null);

const limitDisplay = useLimitDisplay(currentCount, maxAllowed);

const columns: TableColumn<SellerDto>[] = [
  { accessorKey: 'id', header: 'ID' },
  { 
    accessorKey: 'name', 
    header: 'Name',
    cell: ({ row }) => row.getValue('name') || '-' 
  },
  { 
    accessorKey: 'description', 
    header: 'Description',
    cell: ({ row }) => {
      const desc = row.getValue('description') as string;
      return desc ? (desc.length > 50 ? desc.substring(0, 50) + '...' : desc) : '-';
    }
  },
  { id: 'actions', header: '' }
];

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const [sellersResponse] = await Promise.all([
      sellerService.getSellers(groupId)
    ]);

    currentCount.value = sellersResponse.currentCount;
    maxAllowed.value = sellersResponse.maxAllowed;
    sellers.value = sellersResponse.sellers;
  } catch (err: any) {
    error.value = err.message || 'Failed to load sellers';
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
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Sellers</h1>
        <UBadge v-if="!loading" color="neutral" variant="subtle" size="md">
          Capacity: {{ limitDisplay }}
        </UBadge>
      </div>
      
      <UButton 
        :to="`/groups/${groupId}/receipts`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Receipts
      </UButton>
    </div>
    
    <UAlert 
      v-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <UCard v-else :ui="{ body: 'p-0 sm:p-0' }" class="shadow-sm overflow-hidden lg:h-100 w-full max-w-full">
      <UTable sticky :data="sellers" :columns="columns" :loading="loading" class="lg:h-100 w-full">
        <template #actions-cell="{ row }">
          <div class="text-right">
            <UButton
              :to="`/groups/${groupId}/sellers/${row.original.id}`"
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
          <div class="flex flex-col items-center justify-center py-12">
            <span class="text-gray-500 dark:text-gray-400">No sellers found.</span>
          </div>
        </template>
      </UTable>
    </UCard>
  </div>
</template>