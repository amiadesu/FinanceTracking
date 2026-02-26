<template>
  <div class="p-6 max-w-md mx-auto">
    <h1 class="text-2xl font-bold mb-4">Invite to Group #{{ groupId }}</h1>

    <form @submit.prevent="submitInvite" class="space-y-4">
      <div>
        <label class="block font-medium mb-1">Email or Username</label>
        <input 
          v-model="targetUserIdentifier" 
          type="text" 
          required 
          class="w-full border p-2 rounded"
          placeholder="e.g., john_doe or john@example.com"
        />
      </div>

      <div>
        <label class="block font-medium mb-1">Note (Optional)</label>
        <textarea 
          v-model="note" 
          class="w-full border p-2 rounded"
          placeholder="Come join my finance group!"
        ></textarea>
      </div>

      <div v-if="errorMessage" class="text-red-500">{{ errorMessage }}</div>
      <div v-if="successMessage" class="text-green-500">{{ successMessage }}</div>

      <button 
        type="submit" 
        :disabled="isSubmitting"
        class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 disabled:opacity-50"
      >
        {{ isSubmitting ? 'Sending...' : 'Send Invitation' }}
      </button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { invitationService } from '~/services/invitationService'

const route = useRoute()
const groupId = Number(route.params.id)

const targetUserIdentifier = ref('')
const note = ref('')
const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const submitInvite = async () => {
  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await invitationService.createInvitation(groupId, targetUserIdentifier.value, note.value)
    successMessage.value = 'Invitation sent successfully!'
    targetUserIdentifier.value = ''
    note.value = ''
  } catch (err: any) {
    // If your backend throws one of your custom exceptions, the message will show here
    errorMessage.value = err.data?.message || 'An error occurred while sending the invitation.'
  } finally {
    isSubmitting.value = false
  }
}
</script>