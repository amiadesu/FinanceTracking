<script setup lang="ts">
import { computed } from 'vue';
import type { CategoryDto } from '~/services/categoryService';

const props = withDefaults(defineProps<{
  categories: CategoryDto[];
  modelValue: number | null;
  disabled?: boolean;
  clearable?: boolean;
}>(), {
  disabled: false,
  clearable: false
});

const emit = defineEmits<{
  (e: 'update:modelValue', categoryId: number | null): void;
}>();

const categoryItems = computed(() => {
  return props.categories.map(c => ({
    id: c.id,
    label: c.name,
    colorHex: c.colorHex
  }));
});

const selectedCategory = computed({
  get: () => {
    if (!props.modelValue) return null;
    return categoryItems.value.find(c => c.id === props.modelValue) || null;
  },
  set: (val: any) => {
    emit('update:modelValue', val ? val.id : null);
  }
});
</script>

<template>
  <USelectMenu
    v-model="selectedCategory!"
    :items="categoryItems"
    :disabled="disabled"
    class="w-full"
  >
    <template #default>
      <div class="flex items-center justify-between w-full truncate">
        <div v-if="selectedCategory" class="flex items-center gap-2 truncate">
          <div class="w-3 h-3 rounded-full border border-gray-200 dark:border-gray-700 shrink-0" :style="{ backgroundColor: selectedCategory.colorHex }"></div>
          <span class="truncate">{{ selectedCategory.label }}</span>
        </div>
        
        <span v-else class="text-gray-500 dark:text-gray-400 truncate">Select a category</span>
        
        <button 
          v-if="selectedCategory && clearable"
          type="button"
          class="ml-2 text-gray-400 hover:text-red-500 transition-colors shrink-0 flex items-center justify-center relative z-10"
          @click.stop.prevent="emit('update:modelValue', null)"
          aria-label="Clear category"
        >
          <UIcon name="i-heroicons-x-mark" class="w-4 h-4" />
        </button>
      </div>
    </template>
    
    <template #item="{ item }">
      <div class="flex items-center gap-2 max-w-full">
        <div class="w-3 h-3 rounded-full border border-gray-200 dark:border-gray-700 shrink-0" :style="{ backgroundColor: item.colorHex }"></div>
        <span class="truncate">{{ item.label }}</span>
      </div>
    </template>
  </USelectMenu>
</template>