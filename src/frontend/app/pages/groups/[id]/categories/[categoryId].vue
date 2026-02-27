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

// hidden color input ref
const editColorInput = ref<HTMLInputElement | null>(null);

// edit form
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
    // initialize edit form with current color so picker shows the right value
    if (category.value) editDto.colorHex = category.value.colorHex;
  } catch (err: any) {
    error.value = err.message || 'failed';
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
    alert('updated');
  } catch (err: any) {
    alert(err.message || 'fail');
  }
}

async function remove() {
  if (!confirm('Delete this category?')) return;
  try {
    await categoryService.deleteCategory(groupId, categoryId);
    router.push(`/groups/${groupId}/categories`);
  } catch (err: any) {
    alert(err.message || 'fail');
  }
}

onMounted(load);

function onEditColorInput(e: Event) {
  const v = (e.target as HTMLInputElement).value;
  editDto.colorHex = normalizeColor(v);
}
</script>

<template>
  <div class="p-4">
    <h1 class="text-xl font-bold">Category #{{ categoryId }}</h1>
    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>
    <div v-if="category">
      <p><strong>Name:</strong> {{ category.name }}</p>
      <p>
        <strong>Color:</strong>
        <div class="flex items-center gap-2">
          <div :style="{ backgroundColor: category.colorHex }" class="w-8 h-8 border"></div>
          <span>{{ category.colorHex }}</span>
        </div>
      </p>
      <p><strong>System:</strong> {{ category.isSystem ? 'Yes' : 'No' }}</p>
      <p><strong>Created:</strong> {{ category.createdDate }}</p>
      <p><strong>Updated:</strong> {{ category.updatedDate }}</p>

      <div v-if="!category.isSystem" class="mt-4 border-t pt-2">
        <h2 class="font-semibold">Edit</h2>
        <div class="flex flex-col gap-2 max-w-md">
          <label class="flex flex-col">
            New name
            <input type="text" v-model="editDto.name" class="border p-1" />
          </label>
          <label class="flex flex-col">
            New color
                <div class="flex items-center gap-2">
                  <input
                    ref="editColorInput"
                    type="color"
                    :value="editDto.colorHex ?? '#000000'"
                    @input="(e) => onEditColorInput(e)"
                    class="color-picker"
                  />
                  <span v-if="editDto.colorHex" class="text-sm text-gray-600">{{ editDto.colorHex }}</span>
                </div>
          </label>
          <button @click="save" class="bg-green-600 text-white px-3 py-1 rounded">Save</button>
          <button @click="remove" class="bg-red-600 text-white px-3 py-1 rounded">Delete</button>
        </div>
      </div>

      <div v-if="category.isSystem" class="mt-4 p-2 bg-yellow-100 border border-yellow-300 text-yellow-800">
        System categories cannot be edited or deleted.
      </div>
    </div>
  </div>
</template>

<style scoped>
.color-picker {
  width: 36px;
  height: 36px;
  padding: 0;
  border: none;
  border-radius: 8px;
  box-shadow: none;
  cursor: pointer;
  -webkit-appearance: none;
  appearance: none;
  background: transparent;
}
.color-picker::-webkit-color-swatch-wrapper { padding: 0; }
.color-picker::-webkit-color-swatch { border: none; border-radius: 8px; }

</style>
