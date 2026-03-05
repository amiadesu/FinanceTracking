<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from '#imports';
import { groupHistoryService } from '~/services/groupHistoryService';
import type { GroupHistoryDto } from '~/services/groupHistoryService';
import type { TableColumn } from '@nuxt/ui';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const historyEntries = ref<GroupHistoryDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const currentPage = ref(1);
const pageSize = ref(20);
const totalCount = ref(0);

// Nuxt UI Table Columns definition
const columns: TableColumn<GroupHistoryDto>[] = [
  {
    accessorKey: 'changedAt',
    header: 'Date',
    cell: ({ row }) => {
      const dateVal = row.getValue('changedAt') as string;
      if (!dateVal) return '-';
      return new Date(dateVal).toLocaleString();
    }
  },
  {
    accessorKey: 'changedByUserName',
    header: 'Action By'
  },
  {
    accessorKey: 'targetUserName',
    header: 'Target User',
    cell: ({ row }) => {
      const targetUser = row.getValue('targetUserName') as string;
      return targetUser || '-';
    }
  },
  {
    accessorKey: 'note',
    header: 'Note'
  },
  {
    id: 'details',
    header: 'Details',
    cell: ({ row }) => {
      const original = row.original;
      const detailsContent = [];

      // Build out the dynamic details using Vue's 'h' render function
      if (original.roleIdBefore !== original.roleIdAfter) {
        detailsContent.push(
          h('div', [
            h('span', { class: 'font-medium text-gray-700' }, 'Role: '),
            `${original.roleIdBefore ?? 'None'} ➔ ${original.roleIdAfter ?? 'None'}`
          ])
        );
      }

      if (original.activeBefore !== original.activeAfter) {
        detailsContent.push(
          h('div', [
            h('span', { class: 'font-medium text-gray-700' }, 'Active: '),
            `${original.activeBefore} ➔ ${original.activeAfter}`
          ])
        );
      }

      if (original.nameBefore !== original.nameAfter) {
        detailsContent.push(
          h('div', [
            h('span', { class: 'font-medium text-gray-700' }, 'Name: '),
            `${original.nameBefore} ➔ ${original.nameAfter}`
          ])
        );
      }

      return h('div', { class: 'text-xs text-gray-500 space-y-1' }, detailsContent);
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

watch(currentPage, () => {
  loadData();
});

onMounted(() => loadData());
</script>

<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Group History</h1>
      <UButton 
        variant="link" 
        color="primary" 
        @click="() => router.push(`/groups/${groupId}`)"
      >
        Back to Group
      </UButton>
    </div>

    <UAlert v-if="error" color="error" variant="soft" :title="error" class="mb-4" />

    <UTable
      sticky
      :data="historyEntries" 
      :columns="columns" 
      :loading="loading"
      class="w-full mt-4 max-h-78"
    >
      <template #empty-state>
        <div class="flex flex-col items-center justify-center py-6 gap-3">
          <span class="italic text-sm text-gray-500">No history records found for this group.</span>
        </div>
      </template>
    </UTable>

    <div v-if="totalCount > 0" class="flex justify-between items-center mt-6">
      <p class="text-sm text-gray-500">
        Showing {{ (currentPage - 1) * pageSize + 1 }} to {{ Math.min(currentPage * pageSize, totalCount) }} of {{ totalCount }} results
      </p>
      
      <UPagination
        v-model="currentPage"
        :page-count="pageSize"
        :total="totalCount"
      />
    </div>
  </div>
</template>