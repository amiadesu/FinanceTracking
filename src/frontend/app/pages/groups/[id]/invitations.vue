<template>
  <div class="p-6 max-w-4xl mx-auto">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold">Group #{{ groupId }} Invitations</h1>
      <NuxtLink 
        :to="`/groups/${groupId}/invite`" 
        class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
      >
        + Send New Invite
      </NuxtLink>
    </div>

    <div v-if="pending" class="text-gray-500">Loading invitations...</div>
    <div v-else-if="error" class="text-red-500">{{ error.message }}</div>
    
    <div v-else-if="invitations?.length === 0" class="text-gray-500">
      No invitations have been sent for this group yet.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="min-w-full bg-white border border-gray-200 shadow-sm rounded">
        <thead class="bg-gray-50">
          <tr>
            <th class="py-3 px-4 text-left font-semibold text-gray-600 border-b">Invited User</th>
            <th class="py-3 px-4 text-left font-semibold text-gray-600 border-b">Invited By</th>
            <th class="py-3 px-4 text-left font-semibold text-gray-600 border-b">Date</th>
            <th class="py-3 px-4 text-left font-semibold text-gray-600 border-b">Status</th>
            <th class="py-3 px-4 text-right font-semibold text-gray-600 border-b">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="invitation in invitations" :key="invitation.id" class="border-b hover:bg-gray-50">
            <td class="py-3 px-4">{{ invitation.targetUserName }}</td>
            <td class="py-3 px-4">{{ invitation.invitedByUserName }}</td>
            <td class="py-3 px-4 text-sm text-gray-500">
              {{ new Date(invitation.createdDate).toLocaleDateString() }}
            </td>
            <td class="py-3 px-4">
              <span 
                class="px-2 py-1 text-xs rounded-full font-medium"
                :class="{
                  'bg-yellow-100 text-yellow-800': invitation.status === 'Pending',
                  'bg-green-100 text-green-800': invitation.status === 'Accepted',
                  'bg-red-100 text-red-800': invitation.status === 'Rejected' || invitation.status === 'Cancelled'
                }"
              >
                {{ invitation.status }}
              </span>
            </td>
            <td class="py-3 px-4 text-right">
              <button 
                v-if="invitation.status === 'Pending'"
                @click="handleCancel(invitation.id)" 
                class="text-red-500 hover:text-red-700 font-medium text-sm"
              >
                Cancel
              </button>
              <span v-else class="text-gray-400 text-sm">-</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { invitationService } from '~/services/invitationService'

const route = useRoute()
const groupId = Number(route.params.id)

const { data: invitations, pending, error, refresh } = useAsyncData(`group-invites-${groupId}`, () => 
  invitationService.getGroupInvitations(groupId)
)

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