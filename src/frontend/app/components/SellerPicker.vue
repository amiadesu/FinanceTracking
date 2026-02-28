<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import type { SellerDto } from '~/services/sellerService';

const props = defineProps<{
  sellers: SellerDto[];
  modelValue: number | null;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', sellerId: number | null): void;
}>();

const isOpen = ref(false);
const filterText = ref('');
const customId = ref<number | ''>('');
const pickerRef = ref<HTMLElement | null>(null);

const filteredSellers = computed(() => {
  if (!filterText.value) return props.sellers;
  const lowerFilter = filterText.value.toLowerCase();
  return props.sellers.filter(s =>
    (s.name && s.name.toLowerCase().includes(lowerFilter)) ||
    s.id.toString().includes(lowerFilter)
  );
});

const selectedSellerDisplay = computed(() => {
  if (!props.modelValue) return 'Select a Seller';
  const found = props.sellers.find(s => s.id === props.modelValue);
  if (found) {
    return found.name ? `${found.name} (ID: ${found.id})` : `ID: ${found.id}`;
  }
  return `ID: ${props.modelValue} (New)`;
});

function selectSeller(sellerId: number) {
  emit('update:modelValue', sellerId);
  isOpen.value = false;
  filterText.value = '';
}

function applyCustomId() {
  if (typeof customId.value === 'number' && customId.value > 0) {
    emit('update:modelValue', customId.value);
    isOpen.value = false;
    customId.value = '';
  }
}

function handleClickOutside(event: MouseEvent) {
  if (isOpen.value && pickerRef.value && !pickerRef.value.contains(event.target as Node)) {
    isOpen.value = false;
  }
}

onMounted(() => document.addEventListener('mousedown', handleClickOutside));
onBeforeUnmount(() => document.removeEventListener('mousedown', handleClickOutside));
</script>

<template>
  <div ref="pickerRef" class="relative">
    <button
      type="button"
      class="border p-1 w-full text-left bg-white min-h-[34px] flex justify-between items-center"
      @click="isOpen = !isOpen"
    >
      <span :class="{ 'text-gray-500': !modelValue }">{{ selectedSellerDisplay }}</span>
      <span class="text-gray-400 text-xs">▼</span>
    </button>

    <div
      v-if="isOpen"
      class="absolute top-full left-0 mt-1 bg-white border rounded shadow-lg p-3 z-20 w-72"
    >
      <div class="flex justify-between items-center mb-2">
        <input
          type="text"
          v-model="filterText"
          placeholder="Search by name or ID..."
          class="border p-1 w-full text-sm"
        />
        <button type="button" @click="isOpen = false" class="ml-2 text-gray-500 hover:text-gray-800 font-bold px-2">
          &times;
        </button>
      </div>
      
      <div class="max-h-48 overflow-y-auto mb-3 border-b pb-2">
        <div v-if="filteredSellers.length === 0" class="text-gray-500 text-sm py-2 px-1">
          No existing sellers found
        </div>
        <button
          v-for="seller in filteredSellers"
          :key="seller.id"
          type="button"
          @click="selectSeller(seller.id)"
          class="w-full text-left px-2 py-1 hover:bg-gray-100 text-sm flex flex-col"
        >
          <span class="font-semibold">{{ seller.name || 'Unnamed Seller' }}</span>
          <span class="text-xs text-gray-500">ID: {{ seller.id }}</span>
        </button>
      </div>

      <div class="pt-1">
        <label class="block text-xs font-semibold text-gray-600 mb-1">Or enter a new ID manually:</label>
        <div class="flex gap-2">
          <input
            type="number"
            v-model.number="customId"
            placeholder="New ID..."
            class="border p-1 text-sm flex-1"
            @keyup.enter="applyCustomId"
          />
          <button 
            type="button" 
            @click="applyCustomId"
            class="bg-blue-600 text-white px-2 py-1 text-sm rounded"
            :disabled="!customId"
          >
            Apply
          </button>
        </div>
      </div>
    </div>
  </div>
</template>