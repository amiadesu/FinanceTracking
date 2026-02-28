<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from '#imports';
import { sellerService } from '~/services/sellerService';
import { receiptService } from '~/services/receiptService';
import type { SellerDto, UpdateSellerDto } from '~/services/sellerService';
import type { ReceiptDto } from '~/services/receiptService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const sellerId = Number(route.params.sellerId);

const seller = ref<SellerDto | null>(null);
const receipts = ref<ReceiptDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const showReceipts = ref(false);
const loadingReceipts = ref(false);

const editDto = reactive<UpdateSellerDto>({
  name: null,
  description: null,
});

async function load() {
  loading.value = true;
  error.value = null;
  try {
    seller.value = await sellerService.getSeller(groupId, sellerId);
    if (seller.value) {
      editDto.name = seller.value.name;
      editDto.description = seller.value.description;
    }
  } catch (err: any) {
    error.value = err.message || 'Failed to load seller details';
  } finally {
    loading.value = false;
  }
}

async function fetchReceipts() {
  showReceipts.value = !showReceipts.value;
  if (!showReceipts.value || receipts.value.length > 0) return;
  
  loadingReceipts.value = true;
  try {
    receipts.value = await receiptService.getReceipts(groupId, { sellerId });
  } catch (err: any) {
    alert(err.message || 'Error loading associated receipts');
  } finally {
    loadingReceipts.value = false;
  }
}

async function save() {
  try {
    const updated = await sellerService.updateSeller(groupId, sellerId, {
      name: editDto.name,
      description: editDto.description
    });
    seller.value = updated;
    alert('Seller updated successfully.');
  } catch (err: any) {
    alert(err.message || 'Error updating seller');
  }
}

async function remove() {
  if (!confirm('Delete this seller? Ensure no receipts are using it.')) return;
  try {
    await sellerService.deleteSeller(groupId, sellerId);
    router.push(`/groups/${groupId}/sellers`);
  } catch (err: any) {
    alert(err.message || 'Error deleting seller. It might still be in use.');
  }
}

onMounted(() => load());
</script>

<template>
  <div class="p-4 max-w-3xl">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Manage Seller #{{ sellerId }}</h1>
      <button @click="() => router.back()" class="text-blue-600 underline text-sm">
        ← Back
      </button>
    </div>

    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <div v-if="seller && !loading" class="space-y-6">
      
      <div class="border rounded p-4 bg-gray-50 flex flex-col gap-4">
        <label class="flex flex-col">
          <span class="font-semibold text-gray-700 text-sm">Seller Name</span>
          <input type="text" v-model="editDto.name" class="border p-2 mt-1 rounded" placeholder="e.g. Walmart" />
        </label>
        
        <label class="flex flex-col">
          <span class="font-semibold text-gray-700 text-sm">Description</span>
          <textarea v-model="editDto.description" rows="3" class="border p-2 mt-1 rounded"></textarea>
        </label>

        <div class="flex gap-2 pt-2">
          <button @click="save" class="bg-green-600 text-white px-4 py-2 rounded font-semibold">
            Update Seller
          </button>
          <button @click="remove" class="bg-red-600 text-white px-4 py-2 rounded font-semibold ml-auto">
            Delete Seller
          </button>
        </div>
      </div>

      <div class="border-t pt-4">
        <button @click="fetchReceipts" class="text-blue-600 underline font-semibold mb-3">
          {{ showReceipts ? 'Hide Associated Receipts' : 'View Associated Receipts' }}
        </button>

        <div v-if="showReceipts" class="border rounded p-3 bg-white shadow-sm">
          <div v-if="loadingReceipts" class="text-gray-500">Loading receipts...</div>
          <div v-else-if="receipts.length === 0" class="text-gray-500">
            No receipts currently use this seller. Safe to delete.
          </div>
          <table v-else class="w-full text-sm table-auto border-collapse">
            <thead>
              <tr class="bg-gray-100">
                <th class="border px-2 py-1 text-left">Receipt ID</th>
                <th class="border px-2 py-1 text-left">Date</th>
                <th class="border px-2 py-1 text-right">Total</th>
                <th class="border px-2 py-1 text-center">Action</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in receipts" :key="r.id">
                <td class="border px-2 py-1">#{{ r.id }}</td>
                <td class="border px-2 py-1">{{ new Date(r.paymentDate).toLocaleDateString() }}</td>
                <td class="border px-2 py-1 text-right">{{ r.totalAmount.toFixed(2) }}</td>
                <td class="border px-2 py-1 text-center">
                  <NuxtLink :to="`/groups/${groupId}/receipts/${r.id}`" class="text-blue-600 underline">View</NuxtLink>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</template>