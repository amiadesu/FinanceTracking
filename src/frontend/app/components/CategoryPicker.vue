<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import type { CategoryDto } from '~/services/categoryService';

const props = defineProps<{
  categories: CategoryDto[];
  selectedCategoryIds: Set<number>;
}>();

const emit = defineEmits<{
  (e: 'toggle', categoryId: number): void;
}>();

const isOpen = ref(false);
const filterText = ref('');
const pickerRef = ref<HTMLElement | null>(null);

const filteredCategories = computed(() => {
  if (!filterText.value) return props.categories;
  return props.categories.filter(c =>
    c.name.toLowerCase().includes(filterText.value.toLowerCase())
  );
});

function toggleCategory(categoryId: number) {
  emit('toggle', categoryId);
}

function handleClickOutside(event: MouseEvent) {
  if (isOpen.value && pickerRef.value && !pickerRef.value.contains(event.target as Node)) {
    isOpen.value = false;
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside);
});

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside);
});
</script>

<template>
  <div ref="pickerRef" class="relative">
    <button
      type="button"
      class="text-blue-600 underline text-sm"
      @click="isOpen = !isOpen"
    >
      {{ selectedCategoryIds.size > 0 ? `Categories (${selectedCategoryIds.size})` : 'Add categories' }}
    </button>

    <div
      v-if="isOpen"
      class="absolute top-full left-0 mt-1 bg-white border rounded shadow-lg p-3 z-10 w-64"
    >
      <div class="flex justify-between items-center mb-2">
        <input
          type="text"
          v-model="filterText"
          placeholder="Search categories..."
          class="border p-1 w-full text-sm"
        />
        <button type="button" @click="isOpen = false" class="ml-2 text-gray-500 hover:text-gray-800 font-bold px-2">
          &times;
        </button>
      </div>
      
      <div class="max-h-48 overflow-y-auto">
        <div v-if="filteredCategories.length === 0" class="text-gray-500 text-sm py-2">
          No categories found
        </div>
        <label
          v-for="cat in filteredCategories"
          :key="cat.id"
          class="flex items-center gap-2 p-2 hover:bg-gray-100 cursor-pointer"
        >
          <input
            type="checkbox"
            :checked="selectedCategoryIds.has(cat.id)"
            @change="toggleCategory(cat.id)"
            class="w-4 h-4"
          />
          <div class="w-4 h-4 rounded border" :style="{ backgroundColor: cat.colorHex }"></div>
          <span class="text-sm">{{ cat.name }}</span>
        </label>
      </div>
    </div>

    <div v-if="selectedCategoryIds.size > 0" class="mt-2 flex flex-wrap gap-1">
      <span
        v-for="catId in Array.from(selectedCategoryIds)"
        :key="catId"
        class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded"
      >
        {{ categories.find(c => c.id === catId)?.name }}
      </span>
    </div>
  </div>
</template>