<script setup lang="ts">
import { ref, onMounted, computed, h, resolveComponent } from 'vue';
import { useRoute, useRouter } from '#imports';
import { groupMemberService, type GroupMemberDto } from '~/services/groupMemberService';
import { useConfigStore } from '~/stores/useConfigStore';
import { useLimitDisplay } from '~/composables/useLimitDisplay';
import type { TableColumn } from '@nuxt/ui';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const configStore = useConfigStore();

const members = ref<GroupMemberDto[]>([]);
const currentCount = ref(0);
const maxAllowed = ref(0);

const myRole = ref<number | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const ownerRoleId = computed(() => configStore.config?.groupRoles['Owner']);
const adminRoleId = computed(() => configStore.config?.groupRoles['Admin']);

const canManage = computed(() => myRole.value === ownerRoleId.value || myRole.value === adminRoleId.value);

const limitDisplay = useLimitDisplay(currentCount, maxAllowed);

function getRoleName(roleValue: number) {
  if (!configStore.config) return 'Unknown';

  const roleEntry = Object.entries(configStore.config.groupRoles).find(([_, val]) => val === roleValue);
  return roleEntry ? roleEntry[0] : 'Unknown';
}

const columns = computed<TableColumn<GroupMemberDto>[]>(() => {
  const UBadge = resolveComponent('UBadge');

  const baseCols: TableColumn<GroupMemberDto>[] = [
    { 
      accessorKey: 'userName', 
      header: 'Name',
      cell: ({ row }) => h('span', { class: 'font-medium text-gray-900 dark:text-white' }, row.original.userName || '-')
    },
    { 
      accessorKey: 'role', 
      header: 'Role',
      cell: ({ row }) => getRoleName(row.original.role)
    },
    { 
      accessorKey: 'joinedDate', 
      header: 'Joined',
      cell: ({ row }) => row.original.joinedDate ? new Date(row.original.joinedDate).toLocaleDateString() : '-'
    },
    { 
      accessorKey: 'active', 
      header: 'Status',
      cell: ({ row }) => h(UBadge as any, {
        color: row.original.active ? 'success' : 'error',
        variant: 'soft',
        size: 'sm'
      }, () => row.original.active ? 'Active' : 'Inactive')
    }
  ];

  if (canManage.value) {
    baseCols.push({ id: 'actions', header: '' });
  }

  return baseCols;
});

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    await configStore.fetchConfig();

    const [membersResponse, roleData] = await Promise.all([
      groupMemberService.getMembers(groupId),
      groupMemberService.getMyRole(groupId)
    ]);

    currentCount.value = membersResponse.currentCount;
    maxAllowed.value = membersResponse.maxAllowed;
    members.value = membersResponse.groupMembers;

    myRole.value = roleData.role;
  } catch (err: any) {
    error.value = err.message || 'Failed to load group members';
  } finally {
    loading.value = false;
  }
}

onMounted(() => loadData());
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Group Members</h1>
        <UBadge color="neutral" variant="subtle" size="md">#{{ groupId }}</UBadge>
        <div v-if="!loading" class="text-sm text-gray-500 flex items-center gap-2 ml-4 border-l pl-4 dark:border-gray-800">
          Capacity: <UBadge color="neutral" variant="soft">{{ limitDisplay }}</UBadge>
        </div>
      </div>
      
      <div class="flex items-center gap-3">
        <UButton :to="`/groups/${groupId}`" color="secondary" variant="outline" icon="i-heroicons-arrow-left">
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

    <UCard :ui="{ body: 'p-0 sm:p-0' }" class="shadow-sm overflow-hidden lg:h-100 w-full max-w-full">
      <UTable :data="members" :columns="columns" :loading="loading" class="w-full">
        <template #actions-cell="{ row }">
          <div class="text-right">
            <UButton
              :to="`/groups/${groupId}/members/${row.original.userId}`"
              color="primary" 
              variant="outline" 
              size="sm"
              icon="i-heroicons-cog"
            >
              Manage
            </UButton>
          </div>
        </template>

        <template #empty>
          <div class="flex flex-col items-center justify-center py-12">
            <span class="text-gray-500 dark:text-gray-400">No members found for this group.</span>
          </div>
        </template>
      </UTable>
    </UCard>
  </div>
</template>