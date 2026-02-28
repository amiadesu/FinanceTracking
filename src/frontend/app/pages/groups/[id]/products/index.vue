<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from '#imports';
import { productService } from '~/services/productService';
import type { ProductDataDto } from '~/services/productService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const products = ref<ProductDataDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    products.value = await productService.getProducts(groupId);
  } catch (err: any) {
    error.value = err.message || 'Failed to load products';
  } finally {
    loading.value = false;
  }
}

function goToProduct(id: number) {
  router.push(`/groups/${groupId}/products/${id}`);
}

onMounted(() => loadData());
</script>

<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Products Catalog</h1>
      <button @click="() => router.push(`/groups/${groupId}/receipts`)" class="text-blue-600 underline text-sm">
        Back to Receipts
      </button>
    </div>

    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <table v-if="!loading && products.length > 0" class="w-full mt-4 table-auto border-collapse">
      <thead>
        <tr>
          <th class="border px-2 py-1 text-left">ID</th>
          <th class="border px-2 py-1 text-left">Name</th>
          <th class="border px-2 py-1 text-left">Description</th>
          <th class="border px-2 py-1 text-left">Categories</th>
          <th class="border px-2 py-1 text-center">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="p in products" :key="p.id">
          <td class="border px-2 py-1">{{ p.id }}</td>
          <td class="border px-2 py-1 font-medium">{{ p.name }}</td>
          <td class="border px-2 py-1 text-gray-600 text-sm truncate max-w-xs">{{ p.description || '-' }}</td>
          <td class="border px-2 py-1">
            <span v-for="cat in p.categories" :key="cat" class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded mr-1 inline-block mt-1">
              {{ cat }}
            </span>
          </td>
          <td class="border px-2 py-1 text-center">
            <button @click="goToProduct(p.id)" class="text-blue-600 underline">Manage</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="!loading && products.length === 0" class="text-gray-500 mt-4">No products exist in this group yet.</div>
  </div>
</template>