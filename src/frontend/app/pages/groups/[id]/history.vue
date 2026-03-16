<script setup lang="ts">
import { ref, onMounted, watch, h } from 'vue';
import { useRoute } from '#imports';
import { groupHistoryService } from '~/services/groupHistoryService';
import type { GroupHistoryDto } from '~/services/groupHistoryService';
import type { TableColumn } from '@nuxt/ui';
import { badge } from '#build/ui';

const route = useRoute();
const groupId = Number(route.params.id);

const historyEntries = ref<GroupHistoryDto[]>([]);
const loading = ref(false);
const exporting = ref(false);
const error = ref<string | null>(null);

const currentPage = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);

const exportItems = [
  [
    {
      label: 'Export as Excel',
      icon: 'i-heroicons-table-cells',
      onSelect: () => handleExport('xlsx'),
      badge: undefined
    },
    {
      label: 'Export as Word',
      icon: 'i-heroicons-document-text',
      onSelect: () => handleExport('docx'),
      badge: undefined
    }
  ]
];

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

async function handleExport(fileType: string) {
  exporting.value = true;
  error.value = null;
  try {
    const blob = await groupHistoryService.exportGroupHistory(groupId, fileType as 'xlsx' | 'docx');
    
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    
    const dateStr = new Date().toISOString().replace(/[:.]/g, '-');
    link.download = `Group_${groupId}_History_${dateStr}.${fileType}`;
    
    document.body.appendChild(link);
    link.click();
    
    window.URL.revokeObjectURL(url);
    document.body.removeChild(link);
  } catch (err: any) {
    error.value = err.message || `Failed to export history as ${fileType.toUpperCase()}`;
  } finally {
    exporting.value = false;
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
      
      <div class="flex items-center gap-2">
        <UDropdownMenu :items="exportItems" :popper="{ placement: 'bottom-end' }">
          <UButton 
            color="primary" 
            variant="outline" 
            label="Export"
            icon="i-heroicons-arrow-down-tray"
            :loading="exporting"
          >
            <template #trailing>
              <UIcon name="i-heroicons-chevron-down-20-solid" class="w-4 h-4" />
            </template>
          </UButton>

          <template #item="{ item }">
            <span class="truncate">{{ item.label }}</span>
            <UIcon :name="item.icon" class="shrink-0 h-4 w-4 text-gray-400 dark:text-gray-500 ms-auto" />
          </template>
        </UDropdownMenu>

        <UButton 
          :to="`/groups/${groupId}`"
          color="secondary" 
          variant="outline"
          icon="i-heroicons-arrow-left"
        >
          Back to Group
        </UButton>
      </div>
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