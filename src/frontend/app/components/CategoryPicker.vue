<script setup lang="ts">
import { computed } from 'vue';
import type { CategoryDto } from '~/services/categoryService';

const props = defineProps<{
  categories: CategoryDto[];
  selectedCategoryIds: Set<number>;
}>();

const emit = defineEmits<{
  (e: 'toggle', categoryId: number): void;
}>();

const categoryItems = computed(() => {
  return props.categories.map(c => ({
    id: c.id,
    label: c.name,
    colorHex: c.colorHex
  }));
});

const selectedItems = computed({
  get: () => categoryItems.value.filter(c => props.selectedCategoryIds.has(c.id)),
  set: (newSelection) => {
    const oldSet = props.selectedCategoryIds;
    const newSet = new Set(newSelection.map(c => c.id));
    
    for (const id of newSet) {
      if (!oldSet.has(id)) emit('toggle', id);
    }
    for (const id of oldSet) {
      if (!newSet.has(id)) emit('toggle', id);
    }
  }
});
</script>

<template>
  <div class="space-y-3">
    <USelectMenu
      v-model="selectedItems"
      :items="categoryItems"
      multiple
      class="w-full"
    >
      <template #default>
        <span v-if="selectedCategoryIds.size" class="truncate">
          Categories ({{ selectedCategoryIds.size }})
        </span>
        <span v-else class="text-gray-500 dark:text-gray-400">Add categories</span>
      </template>
      
      <template #item="{ item }">
        <div class="flex items-center gap-2 max-w-full">
          <div class="w-3 h-3 rounded-full border border-gray-200 dark:border-gray-700 shrink-0" :style="{ backgroundColor: item.colorHex }"></div>
          <span class="truncate">{{ item.label }}</span>
        </div>
      </template>
    </USelectMenu>

    <div v-if="selectedCategoryIds.size > 0" class="flex flex-wrap gap-2 mt-2">
      <div
        v-for="catId in Array.from(selectedCategoryIds)"
        :key="catId"
        class="inline-flex items-center gap-1.5 px-2 py-1 rounded-md bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 max-w-full"
        :title="categories.find(c => c.id === catId)?.name"
      >
        <div 
          class="w-2.5 h-2.5 rounded-full shrink-0 shadow-sm" 
          :style="{ backgroundColor: categories.find(c => c.id === catId)?.colorHex || '#9ca3af' }"
        ></div>
        
        <span class="text-xs font-medium text-gray-700 dark:text-gray-300 truncate max-w-30 sm:max-w-60">
          {{ categories.find(c => c.id === catId)?.name }}
        </span>

        <button 
          type="button"
          @click="emit('toggle', catId)" 
          class="ml-1 text-gray-400 hover:text-red-500 transition-colors shrink-0 flex items-center justify-center"
          aria-label="Remove category"
        >
          <UIcon name="i-heroicons-x-mark" class="w-3 h-3" />
        </button>
      </div>
    </div>
  </div>
</template>