<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from '#imports';
import { groupHistoryService } from '~/services/groupHistoryService';
import type { GroupHistoryDto } from '~/services/groupHistoryService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const historyEntries = ref<GroupHistoryDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

// Pagination State
const currentPage = ref(1);
const pageSize = ref(20);
const totalPages = ref(1);
const totalCount = ref(0);

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const response = await groupHistoryService.getGroupHistory(
      groupId, 
      currentPage.value, 
      pageSize.value
    );

    historyEntries.value = response.groupHistoryEntries;
    currentPage.value = response.pageNumber;
    totalPages.value = response.totalPages;
    totalCount.value = response.totalCount;
  } catch (err: any) {
    error.value = err.message || 'Failed to load group history';
  } finally {
    loading.value = false;
  }
}

function prevPage() {
  if (currentPage.value > 1) {
    currentPage.value--;
    loadData();
  }
}

function nextPage() {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
    loadData();
  }
}

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleString();
}

onMounted(() => loadData());
</script>

<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Group History</h1>
      <button @click="() => router.push(`/groups/${groupId}`)" class="text-blue-600 underline text-sm">
        Back to Group
      </button>
    </div>

    <div v-if="loading" class="text-gray-600 mb-4">Loading history...</div>
    <div v-if="error" class="text-red-600 mb-4">{{ error }}</div>

    <div class="overflow-x-auto">
      <table v-if="!loading && historyEntries.length > 0" class="w-full mt-4 table-auto border-collapse text-sm">
        <thead class="bg-gray-50">
          <tr>
            <th class="border px-3 py-2 text-left font-medium">Date</th>
            <th class="border px-3 py-2 text-left font-medium">Action By</th>
            <th class="border px-3 py-2 text-left font-medium">Target User</th>
            <th class="border px-3 py-2 text-left font-medium">Note</th>
            <th class="border px-3 py-2 text-left font-medium">Details</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="entry in historyEntries" :key="entry.id" class="hover:bg-gray-50">
            <td class="border px-3 py-2 whitespace-nowrap">{{ formatDate(entry.changedAt) }}</td>
            <td class="border px-3 py-2">{{ entry.changedByUserName }}</td>
            <td class="border px-3 py-2">{{ entry.targetUserName || '-' }}</td>
            <td class="border px-3 py-2">{{ entry.note }}</td>
            <td class="border px-3 py-2 text-xs text-gray-600">
              <div v-if="entry.roleIdBefore !== entry.roleIdAfter">
                Role: {{ entry.roleIdBefore ?? 'None' }} ➔ {{ entry.roleIdAfter ?? 'None' }}
              </div>
              <div v-if="entry.activeBefore !== entry.activeAfter">
                Active: {{ entry.activeBefore }} ➔ {{ entry.activeAfter }}
              </div>
              <div v-if="entry.nameBefore !== entry.nameAfter">
                Name: {{ entry.nameBefore }} ➔ {{ entry.nameAfter }}
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="!loading && historyEntries.length === 0" class="text-gray-500 mt-4 text-center py-8 bg-gray-50 rounded">
      No history records found for this group.
    </div>

    <div v-if="totalPages > 1" class="flex items-center justify-between mt-6">
      <p class="text-sm text-gray-600">
        Showing page <span class="font-semibold">{{ currentPage }}</span> of <span class="font-semibold">{{ totalPages }}</span> 
        ({{ totalCount }} total records)
      </p>
      <div class="flex space-x-2">
        <button 
          @click="prevPage" 
          :disabled="currentPage === 1"
          class="px-3 py-1 border rounded text-sm disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50">
          Previous
        </button>
        <button 
          @click="nextPage" 
          :disabled="currentPage === totalPages"
          class="px-3 py-1 border rounded text-sm disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50">
          Next
        </button>
      </div>
    </div>

  </div>
</template>