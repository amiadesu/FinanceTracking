<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from '#imports';
import { categoryService } from '~/services/categoryService';
import type { CategoryDto, CreateCategoryDto } from '~/services/categoryService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const categories = ref<CategoryDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

// hidden color input ref
const newColorInput = ref<HTMLInputElement | null>(null);

// form state
const newCategory = reactive<CreateCategoryDto>({
  name: '',
  colorHex: '#000000'
});

function normalizeColor(hex?: string) {
  if (!hex) return '#000000';
  const s = hex.trim();
  return s.startsWith('#') ? s : `#${s}`;
}

function onNewColorInput(e: Event) {
  const v = (e.target as HTMLInputElement).value;
  newCategory.colorHex = normalizeColor(v);
}

async function loadCategories() {
  loading.value = true;
  error.value = null;
  try {
    const res = await categoryService.getCategories(groupId);
    categories.value = res.map(c => ({ ...c, colorHex: normalizeColor(c.colorHex) }));
  } catch (err: any) {
    error.value = err.message || 'Failed to load categories';
  } finally {
    loading.value = false;
  }
}

async function createCategory() {
  try {
    const payload = { ...newCategory, colorHex: normalizeColor(newCategory.colorHex) };
    await categoryService.createCategory(groupId, payload);
    // reset form
    newCategory.name = '';
    newCategory.colorHex = '#000000';
    await loadCategories();
  } catch (err: any) {
    alert(err.message || 'error creating');
  }
}

function goTo(id: number) {
  router.push(`/groups/${groupId}/categories/${id}`);
}

onMounted(() => {
  loadCategories();
});
</script>

<template>
  <div class="p-4">
    <h1 class="text-xl font-bold">Categories</h1>
    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <table v-if="!loading" class="w-full mt-4 table-auto border-collapse">
      <thead>
        <tr>
          <th class="border px-2 py-1">ID</th>
          <th class="border px-2 py-1">Name</th>
          <th class="border px-2 py-1">Color</th>
          <th class="border px-2 py-1">System</th>
          <th class="border px-2 py-1">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="c in categories" :key="c.id">
          <td class="border px-2 py-1">{{ c.id }}</td>
          <td class="border px-2 py-1">{{ c.name }}</td>
          <td class="border px-2 py-1">
            <div class="flex items-center gap-2">
              <div :style="{ backgroundColor: c.colorHex }" class="w-6 h-6 border"></div>
              <span>{{ c.colorHex }}</span>
            </div>
          </td>
          <td class="border px-2 py-1">{{ c.isSystem ? 'Yes' : 'No' }}</td>
          <td class="border px-2 py-1">
            <button @click="goTo(c.id)" class="text-blue-600 underline">View</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div class="mt-6 border-t pt-4">
      <h2 class="font-semibold">Create new category</h2>
      <div class="flex flex-col gap-2 max-w-md">
        <label class="flex flex-col">
          Name
          <input type="text" v-model="newCategory.name" class="border p-1" />
        </label>
        <label class="flex flex-col">
          Color
          <div class="flex items-center gap-2">
            <input
              ref="newColorInput"
              type="color"
              :value="newCategory.colorHex ?? '#000000'"
              @input="onNewColorInput"
              class="color-picker"
            />
            <span class="text-sm text-gray-600">{{ newCategory.colorHex }}</span>
          </div>
        </label>

        <button @click="createCategory" class="bg-blue-600 text-white px-3 py-1 rounded">Create</button>
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
