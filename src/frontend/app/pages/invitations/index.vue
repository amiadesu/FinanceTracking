<script setup lang="ts">
import { ref, h } from 'vue'
import { useAsyncData } from '#imports'
import { useAppToast } from '~/composables/useAppToast'
import { invitationService } from '~/services/invitationService'
import type { TableColumn } from '@nuxt/ui'

const { showSuccess, showError } = useAppToast()

const { data: invitations, pending, error, refresh } = useAsyncData('my-invitations', () => 
  invitationService.getPendingInvitations()
)

const processingId = ref<string | null>(null)
const processingAction = ref<'accept' | 'reject' | null>(null)

const columns: TableColumn<any>[] = [
  { 
    id: 'groupName', 
    header: 'Group',
    cell: ({ row }) => h(
      'span', 
      { class: 'text-gray-900 dark:text-white whitespace-normal break-words sm:break-all min-w-30 max-w-60 sm:max-w-90 font-medium' }, 
      row.original.groupName || '-'
    )
  },
  { 
    id: 'invitedByUserName', 
    header: 'Invited By',
    cell: ({ row }) => h(
      'span', 
      { class: 'text-gray-900 dark:text-white whitespace-normal break-words sm:break-all min-w-30 max-w-60 sm:max-w-90 font-medium' }, 
      row.original.invitedByUserName || '-'
    )
  },
  {
    accessorKey: 'note',
    header: 'Note',
    cell: ({ row }) => h(
      'span', 
      { class: 'whitespace-normal break-words sm:break-all min-w-30 max-w-60 sm:max-w-90 font-medium' }, 
      row.original.note || '-'
    )
  },
  { id: 'actions', header: '' }
];

const handleAccept = async (id: string) => {
  processingId.value = id
  processingAction.value = 'accept'
  try {
    await invitationService.acceptInvitation(id)
    showSuccess('Invitation accepted successfully.')
    await refresh()
  } catch (err: any) {
    showError(err.data?.message || 'Failed to accept invitation')
  } finally {
    processingId.value = null
    processingAction.value = null
  }
}

const handleReject = async (id: string) => {
  processingId.value = id
  processingAction.value = 'reject'
  try {
    await invitationService.rejectInvitation(id)
    showSuccess('Invitation rejected successfully.')
    await refresh()
  } catch (err: any) {
    showError(err.data?.message || 'Failed to reject invitation')
  } finally {
    processingId.value = null
    processingAction.value = null
  }
}
</script>

<template>
  <div class="w-full lg:max-w-6xl md:max-w-4xl sm:max-w-2xl mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">My Invitations</h1>
        <UBadge v-if="!pending && invitations" color="neutral" variant="subtle" size="md">
          {{ invitations.length }} Pending
        </UBadge>
      </div>

      <UButton 
        to="/" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Home
      </UButton>
    </div>
    
    <UAlert 
      v-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error.message || 'Failed to load invitations'" 
      class="mb-4" 
    />

    <UCard :ui="{ body: 'p-0 sm:p-0 flex-1 flex flex-col min-h-0' }" class="shadow-sm overflow-hidden flex flex-col w-full h-100 max-w-full">
      <UTable 
        sticky 
        :data="invitations" 
        :columns="columns" 
        :loading="pending" 
        class="w-full flex-1 min-h-0 overflow-y-auto"
      >
        <template #actions-cell="{ row }">
          <div class="flex items-center justify-end gap-2 pr-4">
            <UPopover v-if="row.original.isGroupFull">
              <UButton color="warning" variant="ghost" icon="i-heroicons-exclamation-triangle" size="sm" />
              <template #content>
                <div class="p-3 text-xs max-w-40">
                  This group is currently full. You can't accept this invite.
                </div>
              </template>
            </UPopover>

            <UButton 
              @click="handleAccept(row.original.id)" 
              :disabled="row.original.isGroupFull || processingId === row.original.id"
              :loading="processingId === row.original.id && processingAction === 'accept'"
              color="primary"
              size="sm"
              icon="i-heroicons-check"
            >
              Accept
            </UButton>
            
            <UButton 
              @click="handleReject(row.original.id)" 
              :disabled="processingId === row.original.id"
              :loading="processingId === row.original.id && processingAction === 'reject'"
              color="error" 
              variant="outline"
              size="sm"
              icon="i-heroicons-x-mark"
            >
              Reject
            </UButton>
          </div>
        </template>
        
        <template #empty>
          <div class="flex flex-col items-center justify-center py-12 h-full">
            <UIcon name="i-heroicons-envelope-open" class="w-12 h-12 text-gray-400 mb-3" />
            <span class="text-gray-500 dark:text-gray-400">No pending invitations found.</span>
          </div>
        </template>
      </UTable>
    </UCard>
  </div>
</template>