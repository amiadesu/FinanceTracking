<script setup lang="ts">
import { ref, onMounted, watch, h } from 'vue';
import { useRoute } from '#imports';
import { groupHistoryService } from '~/services/groupHistoryService';
import type { GroupHistoryDto } from '~/services/groupHistoryService';
import type { TableColumn } from '@nuxt/ui';

const route = useRoute();
const groupId = Number(route.params.id);

const historyEntries = ref<GroupHistoryDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const currentPage = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);

const columns: TableColumn<GroupHistoryDto>[] = [
  {
    accessorKey: 'changedAt',
    header: 'Date',
    cell: ({ row }) => {
      const dateVal = row.original.changedAt;
      if (!dateVal) return '-';
      return new Date(dateVal).toLocaleString();
    }
  },
  {
    id: 'changedByUserName',
    header: 'Action By',
    cell: ({ row }) => h(
      'span', 
      { class: 'text-gray-900 dark:text-white whitespace-normal break-words sm:break-all min-w-30 max-w-60 sm:max-w-90 font-medium' }, 
      row.original.changedByUserName || '-'
    )
  },
  {
    id: 'targetUserName',
    header: 'Target User',
    cell: ({ row }) => h(
      'span', 
      { class: 'text-gray-900 dark:text-white whitespace-normal break-words sm:break-all min-w-30 max-w-60 sm:max-w-90 font-medium' }, 
      row.original.targetUserName || '-'
    )
  },
  {
    id: 'note',
    header: 'Note',
    cell: ({ row }) => {
      const note = row.original.note;
      return h(
        'span', 
        { class: 'whitespace-normal break-words sm:break-all min-w-30 max-w-60 sm:max-w-90 font-medium' }, 
        note ? (note.length > 50 ? note.substring(0, 50) + '...' : note) : '-'
      );
    }
  },
  {
    id: 'details',
    header: 'Details',
    cell: ({ row }) => {
      const original = row.original;
      const detailsContent = [];
      
      const wrapClass = 'whitespace-normal break-words sm:break-all min-w-30 max-w-60 sm:max-w-90';

      if (original.roleIdBefore !== original.roleIdAfter) {
        detailsContent.push(
          h('div', { class: wrapClass }, [
            h('span', { class: 'font-medium text-gray-700 dark:text-gray-300' }, 'Role: '),
            `${original.roleIdBefore ?? 'None'} ➔ ${original.roleIdAfter ?? 'None'}`
          ])
        );
      }

      if (original.activeBefore !== original.activeAfter) {
        detailsContent.push(
          h('div', { class: wrapClass }, [
            h('span', { class: 'font-medium text-gray-700 dark:text-gray-300' }, 'Active: '),
            `${original.activeBefore} ➔ ${original.activeAfter}`
          ])
        );
      }

      if (original.nameBefore !== original.nameAfter) {
        detailsContent.push(
          h('div', { class: wrapClass }, [
            h('span', { class: 'font-medium text-gray-700 dark:text-gray-300' }, 'Name: '),
            `${original.nameBefore} ➔ ${original.nameAfter}`
          ])
        );
      }

      return h('div', { class: 'text-xs text-gray-500 dark:text-gray-400 space-y-1' }, detailsContent);
    }
  }
];

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
    totalCount.value = response.totalCount;
  } catch (err: any) {
    error.value = err.message || 'Failed to load group history';
  } finally {
    loading.value = false;
  }
}

watch(currentPage, (newPage, oldPage) => {
  if (newPage !== oldPage) {
    loadData();
  }
});

onMounted(() => loadData());
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Group History</h1>
        <UBadge color="neutral" variant="subtle" size="md">#{{ groupId }}</UBadge>
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
      class="mb-4" 
    />

    <UCard 
      :ui="{ body: 'p-0 sm:p-0 flex-1 flex flex-col min-h-0' }" 
      class="shadow-sm overflow-hidden flex flex-col w-full lg:h-100 max-w-full"
    >
      <UTable
        sticky
        :data="historyEntries" 
        :columns="columns" 
        :loading="loading"
        class="w-full flex-1 min-h-0 overflow-y-auto"
      >
        <template #empty>
          <div class="flex flex-col items-center justify-center py-12 gap-3 h-full">
            <span class="text-gray-500 dark:text-gray-400">No history records found for this group.</span>
          </div>
        </template>
      </UTable>

      <div v-if="totalCount > 0" class="flex flex-col sm:flex-row justify-between items-center p-4 border-t dark:border-gray-800 gap-4 mt-auto">
        <p class="text-sm text-gray-500">
          Showing {{ (currentPage - 1) * pageSize + 1 }} to {{ Math.min(currentPage * pageSize, totalCount) }} of {{ totalCount }} results
        </p>
        
        <UPagination
          v-model:page="currentPage"
          :items-per-page="pageSize"
          :total="totalCount"
        />
      </div>
    </UCard>
  </div>
</template>