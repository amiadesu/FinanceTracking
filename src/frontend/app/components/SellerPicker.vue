<script setup lang="ts">
import { ref, computed } from 'vue';
import type { SellerDto } from '~/services/sellerService';

const props = defineProps<{
  sellers: SellerDto[];
  modelValue: string | null;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', sellerId: string | null): void;
}>();

const searchTerm = ref('');

const sellerItems = computed(() => {
  const items = props.sellers.map(s => ({
    id: s.id,
    label: s.name ? `${s.name}:${s.id}` : s.id, 
    displayName: s.name ? `${s.name}` : `Unnamed Seller`,
    description: `ID: ${s.id}`,
    isCustom: false 
  }));

  const query = searchTerm.value.trim();
  if (query && !props.sellers.some(s => s.id === query)) {
    items.push({
      id: query,
      label: query,
      displayName: `Create custom ID: "${query}"`,
      description: 'New ID',
      isCustom: true
    });
  }

  return items;
});

const selectedSeller = computed({
  get: () => {
    if (!props.modelValue) return null;
    const existing = props.sellers.find(s => s.id === props.modelValue);
    if (existing) {
      return { 
        id: existing.id, 
        label: existing.name || existing.id, 
        displayName: existing.name || 'Unnamed Seller',
        description: `ID: ${existing.id}`, 
        isCustom: false 
      };
    }
    return { 
      id: props.modelValue, 
      label: props.modelValue, 
      displayName: `Custom: ${props.modelValue}`, 
      description: 'New ID', 
      isCustom: true 
    };
  },
  set: (val: any) => {
    if (!val) {
      emit('update:modelValue', null);
    } else {
      emit('update:modelValue', val.id);
      searchTerm.value = '';
    }
  }
});

const selectedSellerDisplay = computed(() => {
  if (!props.modelValue) return 'Select a Seller';
  const found = props.sellers.find(s => s.id === props.modelValue);
  if (found) {
    return found.name ? `${found.name} (ID: ${found.id})` : `ID: ${found.id}`;
  }
  return `ID: ${props.modelValue} (New)`;
});
</script>

<template>
  <USelectMenu
    v-model="selectedSeller!"
    v-model:search-term="searchTerm"
    :items="sellerItems"
    searchable
    class="w-full"
  >
    <template #default>
      <span :class="{ 'text-gray-500 dark:text-gray-400': !modelValue }" class="truncate">
        {{ selectedSellerDisplay }}
      </span>
    </template>
    
    <template #item="{ item }">
      <div class="flex flex-col">
        <span class="font-medium" :class="{ 'text-primary-600 dark:text-primary-400': item.isCustom }">
          {{ item.displayName }}
        </span>
        <span class="text-xs text-gray-500 dark:text-gray-400">
          {{ item.description }}
        </span>
      </div>
    </template>
  </USelectMenu>
</template>