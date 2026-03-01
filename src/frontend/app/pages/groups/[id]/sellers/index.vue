<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from '#imports';
import { sellerService } from '~/services/sellerService';
import type { SellerDto } from '~/services/sellerService';
import { useLimitDisplay } from '~/composables/useLimitDisplay';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const sellers = ref<SellerDto[]>([]);
const currentCount = ref(0);
const maxAllowed = ref(0);

const loading = ref(false);
const error = ref<string | null>(null);

const limitDisplay = useLimitDisplay(currentCount, maxAllowed);

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const [sellersResponse] = await Promise.all([
      sellerService.getSellers(groupId)
    ]);

    currentCount.value = sellersResponse.currentCount;
    maxAllowed.value = sellersResponse.maxAllowed;
    sellers.value = sellersResponse.sellers;
  } catch (err: any) {
    error.value = err.message || 'Failed to load sellers';
  } finally {
    loading.value = false;
  }
}

function goToSeller(id: number) {
  router.push(`/groups/${groupId}/sellers/${id}`);
}

onMounted(() => loadData());
</script>

<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Sellers</h1>
      <p class="text-sm text-gray-600 mt-1" v-if="!loading">
        Capacity: <span class="font-mono bg-gray-100 px-1 rounded">{{ limitDisplay }}</span>
      </p>
      <button @click="() => router.push(`/groups/${groupId}/receipts`)" class="text-blue-600 underline text-sm">
        Back to Receipts
      </button>
    </div>

    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <table v-if="!loading && sellers.length > 0" class="w-full mt-4 table-auto border-collapse">
      <thead>
        <tr>
          <th class="border px-2 py-1 text-left">ID</th>
          <th class="border px-2 py-1 text-left">Name</th>
          <th class="border px-2 py-1 text-left">Description</th>
          <th class="border px-2 py-1">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="s in sellers" :key="s.id">
          <td class="border px-2 py-1">{{ s.id }}</td>
          <td class="border px-2 py-1">{{ s.name || '-' }}</td>
          <td class="border px-2 py-1 text-gray-600 text-sm truncate max-w-xs">{{ s.description || '-' }}</td>
          <td class="border px-2 py-1 text-center">
            <button @click="goToSeller(s.id)" class="text-blue-600 underline">Manage</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="!loading && sellers.length === 0" class="text-gray-500 mt-4">No sellers yet.</div>
  </div>
</template>