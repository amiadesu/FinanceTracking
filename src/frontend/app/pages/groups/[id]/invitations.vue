<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useAsyncData } from '#imports'
import { invitationService } from '~/services/invitationService'
import type { TableColumn } from '@nuxt/ui'

const route = useRoute()
const groupId = Number(route.params.id)

const { data: invitations, pending, error, refresh } = useAsyncData(`group-invites-${groupId}`, () => 
  invitationService.getGroupInvitations(groupId)
)

const columns: TableColumn<any>[] = [
  { 
    accessorKey: 'targetUserName', 
    header: 'Invited User',
    cell: ({ row }) => row.getValue('targetUserName') || '-'
  },
  { 
    accessorKey: 'invitedByUserName', 
    header: 'Invited By',
    cell: ({ row }) => row.getValue('invitedByUserName') || '-'
  },
  { 
    accessorKey: 'createdDate', 
    header: 'Date',
    cell: ({ row }) => {
      const dateVal = row.getValue('createdDate') as string;
      if (!dateVal) return '-';
      return new Date(dateVal).toLocaleDateString();
    }
  },
  { 
    accessorKey: 'status', 
    header: 'Status' 
  },
  { 
    id: 'actions', 
    header: '' 
  } 
];

const handleCancel = async (invitationId: string) => {
  if (!confirm('Are you sure you want to cancel this invitation?')) return
  try {
    await invitationService.cancelInvitation(groupId, invitationId)
    refresh() 
  } catch (err: any) {
    alert(err.data?.message || 'Failed to cancel the invitation.')
  }
}
</script>

<template>
  <div class="max-w-5xl mx-auto p-4 mt-8">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Group Invitations</h1>
        <UBadge color="neutral" variant="subtle" size="md">#{{ groupId }}</UBadge>
      </div>
      
      <div class="flex items-center gap-3">
        <UButton :to="`/groups/${groupId}/invite`" color="primary" icon="i-heroicons-plus">
          Send New Invite
        </UButton>
        <UButton :to="`/groups/${groupId}`" color="secondary" variant="outline" icon="i-heroicons-arrow-left">
          Back to Group
        </UButton>
      </div>
    </div>

    <UAlert 
      v-if="error" 
      color="error" 
      variant="soft" 
      :title="error.message || 'Failed to load invitations'" 
      class="mb-4" 
    />

    <UCard :ui="{ body: 'p-0 sm:p-0' }" class="shadow-sm overflow-hidden">
      <UTable :data="invitations" :columns="columns" :loading="pending" class="w-full">
        <template #status-cell="{ row }">
          <UBadge 
            :color="row.original.status === 'Pending' ? 'warning' : row.original.status === 'Accepted' ? 'success' : 'error'"
            variant="soft"
            size="sm"
          >
            {{ row.original.status }}
          </UBadge>
        </template>

        <template #actions-cell="{ row }">
          <div class="text-right">
            <UButton 
              v-if="row.original.status === 'Pending'"
              @click="handleCancel(row.original.id)" 
              color="error" 
              variant="outline" 
              size="sm"
              icon="i-heroicons-x-mark"
            >
              Cancel
            </UButton>
          </div>
        </template>
      </UTable>
    </UCard>
  </div>
</template>