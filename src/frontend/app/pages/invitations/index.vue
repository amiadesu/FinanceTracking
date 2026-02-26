<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">My Invitations</h1>

    <div v-if="pending" class="text-gray-500">Loading invitations...</div>
    <div v-else-if="error" class="text-red-500">{{ error.message }}</div>
    
    <div v-else-if="invitations?.length === 0" class="text-gray-500">
      You have no pending invitations.
    </div>

    <ul v-else class="space-y-4">
      <li 
        v-for="invitation in invitations" 
        :key="invitation.id" 
        class="border p-4 rounded shadow-sm"
      >
        <div>
          <strong>{{ invitation.groupName }}</strong>
          <p class="text-sm text-gray-600">
            Invited by: {{ invitation.invitedByUserName }}
          </p>
          <p v-if="invitation.note" class="italic mt-2">"{{ invitation.note }}"</p>
        </div>
        
        <div class="mt-4 flex gap-2">
          <button 
            @click="handleAccept(invitation.id)" 
            class="bg-green-500 text-white px-3 py-1 rounded hover:bg-green-600"
          >
            Accept
          </button>
          <button 
            @click="handleReject(invitation.id)" 
            class="bg-red-500 text-white px-3 py-1 rounded hover:bg-red-600"
          >
            Reject
          </button>
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { invitationService } from '~/services/invitationService'

const { data: invitations, pending, error, refresh } = useAsyncData('my-invitations', () => 
  invitationService.getPendingInvitations()
)

const handleAccept = async (id: string) => {
  try {
    await invitationService.acceptInvitation(id)
    alert('Invitation accepted!')
    refresh()
  } catch (err: any) {
    alert(err.data?.message || 'Failed to accept invitation')
  }
}

const handleReject = async (id: string) => {
  try {
    await invitationService.rejectInvitation(id)
    alert('Invitation rejected!')
    refresh()
  } catch (err: any) {
    alert(err.data?.message || 'Failed to reject invitation')
  }
}
</script>