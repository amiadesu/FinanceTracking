<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from '#imports';
import { categoryService } from '~/services/categoryService';
import type { CategoryDto, UpdateCategoryDto } from '~/services/categoryService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const categoryId = Number(route.params.categoryId);

const category = ref<CategoryDto | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const editDto = reactive<UpdateCategoryDto>({
  name: undefined,
  colorHex: undefined,
});

function normalizeColor(hex?: string) {
  if (!hex) return '#000000';
  const s = hex.trim();
  return s.startsWith('#') ? s : `#${s}`;
}

async function load() {
  loading.value = true;
  error.value = null;
  try {
    const res = await categoryService.getCategory(groupId, categoryId);
    if (res) {
      res.colorHex = normalizeColor(res.colorHex);
    }
    category.value = res;
    if (category.value) editDto.colorHex = category.value.colorHex;
  } catch (err: any) {
    error.value = err.message || 'Failed to load category';
  } finally {
    loading.value = false;
  }
}

async function save() {
  if (!category.value) return;
  try {
    const payload: UpdateCategoryDto = { ...editDto };
    if (payload.colorHex) payload.colorHex = normalizeColor(payload.colorHex);
    const updated = await categoryService.updateCategory(groupId, categoryId, payload);
    category.value = updated;
    await load();
    alert('Category updated successfully.');
  } catch (err: any) {
    alert(err.message || 'Error updating category');
  }
}

async function remove() {
  if (!confirm(`Are you sure you want to delete "${category.value?.name}"?`)) return;
  try {
    await categoryService.deleteCategory(groupId, categoryId);
    router.push(`/groups/${groupId}/categories`);
  } catch (err: any) {
    alert(err.message || 'Error deleting category');
  }
}

onMounted(load);

function onEditColorInput(e: Event) {
  const v = (e.target as HTMLInputElement).value;
  editDto.colorHex = normalizeColor(v);
}
</script>

<template>
  <div class="max-w-3xl mx-auto p-4 mt-8">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Manage Category</h1>
      <UButton 
        :to="`/groups/${groupId}/categories`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Categories
      </UButton>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2 mb-4">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading category data...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <UCard v-else-if="category" class="shadow-sm">
      <div class="flex flex-col gap-6">
        <div>
          <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Category Name</span>
          <div class="flex items-center gap-3 mt-1">
            <p class="text-xl font-semibold text-gray-900 dark:text-white">{{ category.name }}</p>
            <UBadge v-if="category.isSystem" color="warning" variant="soft" size="sm">System Component</UBadge>
          </div>
        </div>

        <div v-if="!category.isSystem" class="flex flex-col gap-4 max-w-sm">
          <UFormField label="Edit Name">
            <UInput v-model="editDto.name" :placeholder="category.name" class="w-full" />
          </UFormField>

          <UFormField label="Edit Color">
            <div class="flex items-center gap-3 mt-1">
              <div class="w-10 h-10 rounded-lg border border-gray-200 dark:border-gray-700 shadow-sm relative overflow-hidden focus-within:ring-2 focus-within:ring-primary-500 transition-shadow">
                <input
                  type="color"
                  :value="editDto.colorHex ?? '#000000'"
                  @input="onEditColorInput"
                  class="absolute -inset-2 w-[200%] h-[200%] cursor-pointer opacity-0"
                />
                <div class="absolute inset-0 pointer-events-none" :style="{ backgroundColor: editDto.colorHex }"></div>
              </div>
              <span class="text-sm font-mono text-gray-600 dark:text-gray-400 uppercase">
                {{ editDto.colorHex }}
              </span>
            </div>
          </UFormField>
        </div>
        
        <UAlert 
          v-else
          color="warning" 
          variant="soft" 
          icon="i-heroicons-shield-exclamation"
          title="Protected Category"
          description="System categories cannot be edited or deleted."
        />
      </div>

      <template #footer v-if="!category.isSystem">
        <div class="flex flex-wrap items-center gap-3 justify-between">
          <div class="flex gap-3">
            <UButton @click="save" color="primary">
              Save Changes
            </UButton>
          </div>

          <UButton @click="remove" color="error" variant="outline">
            Delete Category
          </UButton>
        </div>
      </template>
    </UCard>
  </div>
</template>