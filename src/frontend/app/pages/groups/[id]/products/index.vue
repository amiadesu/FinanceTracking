<script setup lang="ts">
import { ref, onMounted, h } from 'vue';
import { useRoute, useRouter } from '#imports';
import { productService } from '~/services/productService';
import type { ProductDataDto } from '~/services/productService';
import { categoryService } from '~/services/categoryService';
import type { CategoryDto } from '~/services/categoryService';
import type { TableColumn } from '@nuxt/ui';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const products = ref<ProductDataDto[]>([]);
const categories = ref<CategoryDto[]>([]);
const systemCategories = ref<CategoryDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const columns: TableColumn<ProductDataDto>[] = [
  { accessorKey: 'id', header: 'ID' },
  { 
    id: 'name',
    header: 'Name',
    cell: ({ row }) => h(
      'span', 
      { class: 'text-gray-900 dark:text-white whitespace-normal wrap-break-words sm:break-all min-w-30 max-w-60 sm:max-w-90 font-medium' }, 
      row.original.name || '-'
    )
  },
  { 
    accessorKey: 'description', 
    header: 'Description',
    cell: ({ row }) => {
      const desc = row.original.description;
      return h(
        'span', 
        { class: 'whitespace-normal wrap-break-words sm:break-all min-w-30 max-w-60 sm:max-w-90 font-medium' }, 
        desc ? (desc.length > 50 ? desc.substring(0, 50) + '...' : desc) : '-'
      );
    }
  },
  { id: 'categories', header: 'Categories' },
  { id: 'actions', header: '' }
];

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const [productsData, categoriesData, systemCats] = await Promise.all([
      productService.getProducts(groupId),
      categoryService.getCategories(groupId),
      categoryService.getSystemCategories(groupId)
    ]);

    products.value = productsData;
    categories.value = categoriesData.categories;
    systemCategories.value = systemCats;
  } catch (err: any) {
    error.value = err.message || 'Failed to load products';
  } finally {
    loading.value = false;
  }
}

function getCategoryColor(catName: string) {
  const cat = [...categories.value, ...systemCategories.value].find(c => c.name === catName);
  return cat?.colorHex || '#9ca3af';
}

onMounted(() => loadData());
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Products Catalog</h1>
      </div>
      
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
      class="mb-4" 
    />

    <UCard :ui="{ body: 'p-0 sm:p-0 flex-1 flex flex-col min-h-0' }" class="shadow-sm overflow-hidden flex flex-col w-full lg:h-100 max-w-full">
      <UTable 
        sticky
        :data="products" 
        :columns="columns" 
        :loading="loading" 
        class="w-full flex-1 min-h-0 overflow-y-auto"
      >
        <template #categories-cell="{ row }">
          <div class="flex flex-wrap gap-1.5 max-w-60 sm:max-w-90">
            <div 
              v-for="cat in row.original.categories" 
              :key="cat" 
              class="inline-flex items-center gap-1.5 px-2 py-1 rounded-md bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 max-w-full"
              :title="cat"
            >
              <span 
                class="w-2.5 h-2.5 rounded-full shrink-0 shadow-sm" 
                :style="{ backgroundColor: getCategoryColor(cat) }"
              ></span>
              <span class="text-xs font-medium text-gray-700 dark:text-gray-300 truncate">{{ cat }}</span>
            </div>
            <span v-if="row.original.categories.length === 0" class="text-gray-400 text-xs italic">-</span>
          </div>
        </template>

        <template #actions-cell="{ row }">
          <div class="text-right">
            <UButton
              :to="`/groups/${groupId}/products/${row.original.id}`"
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
            <span class="text-gray-500 dark:text-gray-400">No products exist in this group yet.</span>
          </div>
        </template>
      </UTable>
    </UCard>
  </div>
</template>