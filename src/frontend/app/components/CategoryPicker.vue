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
        <UButton
          color="neutral"
          variant="outline"
          class="w-full justify-between"
          trailing-icon="i-heroicons-chevron-down-20-solid"
        >
          <span v-if="selectedCategoryIds.size" class="truncate">
            Categories ({{ selectedCategoryIds.size }})
          </span>
          <span v-else class="text-gray-500 dark:text-gray-400">Add categories</span>
        </UButton>
      </template>
      
      <template #item="{ item }">
        <div class="flex items-center gap-2">
          <div class="w-3 h-3 rounded-full border border-gray-200 dark:border-gray-700" :style="{ backgroundColor: item.colorHex }"></div>
          <span class="truncate">{{ item.label }}</span>
        </div>
      </template>
    </USelectMenu>

    <div v-if="selectedCategoryIds.size > 0" class="flex flex-wrap gap-2 mt-2">
      <UBadge
        v-for="catId in Array.from(selectedCategoryIds)"
        :key="catId"
        color="primary"
        variant="subtle"
        class="flex items-center gap-1.5 px-2 py-1"
      >
        <div 
          class="w-2 h-2 rounded-full" 
          :style="{ backgroundColor: categories.find(c => c.id === catId)?.colorHex }"
        ></div>
        {{ categories.find(c => c.id === catId)?.name }}
        <button 
          @click="emit('toggle', catId)" 
          class="ml-1 text-primary-500 hover:text-primary-700 transition-colors"
          aria-label="Remove category"
        >
          <UIcon name="i-heroicons-x-mark" class="w-3 h-3" />
        </button>
      </UBadge>
    </div>
  </div>
</template>