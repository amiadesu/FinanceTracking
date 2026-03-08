<script setup lang="ts">
import { ref, onMounted, reactive, computed, h } from 'vue';
import { useRoute } from '#imports';
import * as v from 'valibot';
import { useAppToast } from '~/composables/useAppToast';
import { categorySchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import { categoryService } from '~/services/categoryService';
import type { CategoryDto, CreateCategoryDto } from '~/services/categoryService';
import { useLimitDisplay } from '~/composables/useLimitDisplay';
import type { FormSubmitEvent } from '@nuxt/ui';
import type { TableColumn } from '@nuxt/ui';
import FormGlobalErrors from "~/components/FormGlobalErrors.vue";

const { showSuccess, showError } = useAppToast();

type Schema = v.InferOutput<typeof categorySchema>;

const route = useRoute();
const groupId = Number(route.params.id);

const categories = ref<CategoryDto[]>([]);
const currentCount = ref(0);
const maxAllowed = ref(0);

const loading = ref(false);
const isSubmitting = ref(false);
const error = ref<string | null>(null);

const newCategory = reactive<CreateCategoryDto>({
  name: '',
  colorHex: '#000000'
});

const limitDisplay = useLimitDisplay(currentCount, maxAllowed);

function normalizeColor(hex?: string) {
  if (!hex) return '#000000';
  const s = hex.trim();
  return s.startsWith('#') ? s : `#${s}`;
}

function onNewColorInput(e: Event) {
  const v = (e.target as HTMLInputElement).value;
  newCategory.colorHex = normalizeColor(v);
}

const { isFormValid, unmappedErrors, touch } = useFormValidation(categorySchema, newCategory);

async function loadCategories() {
  loading.value = true;
  error.value = null;
  try {
    const categoriesResponse = await categoryService.getCategories(groupId);
    
    currentCount.value = categoriesResponse.currentCount;
    maxAllowed.value = categoriesResponse.maxAllowed;
    categories.value = categoriesResponse.categories.map(c => ({ 
      ...c, 
      colorHex: normalizeColor(c.colorHex) 
    }));
  } catch (err: any) {
    error.value = err.message || 'Failed to load categories';
  } finally {
    loading.value = false;
  }
}

async function createCategory(event: FormSubmitEvent<Schema>) {
  isSubmitting.value = true;
  try {
    const payload = { ...event.data, colorHex: normalizeColor(event.data.colorHex) };
    await categoryService.createCategory(groupId, payload);
    
    newCategory.name = '';
    newCategory.colorHex = '#000000';
    await loadCategories();
    showSuccess('Category created successfully.');
  } catch (err: any) {
    showError(err.message || 'Error creating category');
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(() => {
  loadCategories();
});

const columns: TableColumn<CategoryDto>[] = [
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
    accessorKey: 'colorHex', 
    header: 'Color',
    cell: ({ row }) => {
      const hex = row.original.colorHex;
      return h('div', { class: 'flex items-center gap-2' }, [
        h('div', { 
          class: 'w-4 h-4 rounded-full border border-gray-200 dark:border-gray-700 shadow-sm',
          style: { backgroundColor: hex }
        }),
        h('span', { class: 'text-sm font-mono text-gray-600 dark:text-gray-400' }, hex)
      ]);
    }
  },
  { 
    accessorKey: 'isSystem', 
    header: 'Type',
    cell: ({ row }) => {
      const isSystem = row.original.isSystem;
      return h('span', {
        class: isSystem 
          ? 'inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ring-1 ring-inset bg-amber-50 text-amber-600 ring-amber-500/10 dark:bg-amber-400/10 dark:text-amber-400 dark:ring-amber-400/20'
          : 'inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ring-1 ring-inset bg-primary-50 text-primary-600 ring-primary-500/10 dark:bg-primary-400/10 dark:text-primary-400 dark:ring-primary-400/20'
      }, isSystem ? 'System' : 'Custom');
    }
  },
  { id: 'actions', header: '' }
];
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-4">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Categories</h1>
        <UBadge v-if="!loading" color="neutral" variant="subtle" class="font-mono">
          Capacity: {{ limitDisplay }}
        </UBadge>
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
      class="mb-6" 
    />

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start lg:items-stretch w-full max-w-full">
      
      <div class="lg:col-span-2 w-full">
        <UCard 
          class="shadow-sm flex flex-col lg:h-100" 
          :ui="{ body: 'p-0 sm:p-0 flex-1 overflow-hidden flex flex-col' }"
        >
          <template #header>
            <h2 class="font-semibold text-gray-900 dark:text-white">Existing Categories</h2>
          </template>
          
          <UTable
            sticky
            :data="categories" 
            :columns="columns" 
            :loading="loading"
            class="flex-1 min-h-0 overflow-y-auto"
          >
            <template #actions-cell="{ row }">
              <UButton 
                :to="`/groups/${groupId}/categories/${row.original?.id || row.id}`"
                color="primary" 
                variant="outline" 
                icon="i-heroicons-cog"
                size="sm"
              >
                Manage
              </UButton>
            </template>

            <template #empty>
              <div class="flex flex-col items-center justify-center py-12">
                <span class="text-gray-500 dark:text-gray-400">No categories found.</span>
              </div>
            </template>
          </UTable>
        </UCard>
      </div>

      <div class="lg:col-span-1 w-full lg:max-w-sm">
        <UCard 
          class="shadow-sm flex flex-col lg:h-100"
          :ui="{ body: 'flex-1 flex flex-col min-h-0' }"
        >
          <template #header>
            <h2 class="font-semibold text-gray-900 dark:text-white">Create Category</h2>
          </template>

          <UForm 
            :schema="categorySchema" 
            :state="newCategory" 
            class="flex flex-col flex-1 min-h-0" 
            @submit="createCategory"
          >
            <div class="flex flex-col flex-1 overflow-y-auto pr-2 pb-2">
              <div class="flex flex-col gap-6 flex-1">
                <UFormField label="Name" name="name" required>
                  <UInput v-model="newCategory.name" placeholder="e.g. Groceries" class="w-full" />
                </UFormField>

                <UFormField label="Color" name="colorHex" required>
                  <div class="flex items-center gap-3 mt-1 w-full">
                    <div class="w-10 h-10 rounded-lg border border-gray-200 dark:border-gray-700 shadow-sm relative overflow-hidden focus-within:ring-2 focus-within:ring-primary-500 transition-shadow">
                      <input
                        type="color"
                        :value="newCategory.colorHex ?? '#000000'"
                        @input="onNewColorInput"
                        class="absolute -inset-2 w-[200%] h-[200%] cursor-pointer opacity-0"
                      />
                      <div class="absolute inset-0 pointer-events-none" :style="{ backgroundColor: newCategory.colorHex }"></div>
                    </div>
                    <span class="text-sm font-mono text-gray-600 dark:text-gray-400 uppercase">
                      {{ newCategory.colorHex }}
                    </span>
                  </div>
                </UFormField>
              </div>

              <FormGlobalErrors :errors="unmappedErrors" />
            </div>

            <div class="mt-auto pt-6">
              <USeparator class="mb-6" />

              <div class="flex flex-wrap items-center gap-3 justify-between">
                <UButton 
                  type="submit" 
                  color="primary" 
                  class="w-full justify-center"
                  :loading="isSubmitting"
                  :disabled="isSubmitting || !isFormValid"
                >
                  {{ isSubmitting ? 'Creating...' : 'Create Category' }}
                </UButton>
              </div>
            </div>
          </UForm>
        </UCard>
      </div>
    </div>
  </div>
</template>