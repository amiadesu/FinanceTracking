<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRoute } from 'vue-router'
import { invitationService } from '~/services/invitationService'
import * as v from 'valibot'
import type { FormSubmitEvent } from '@nuxt/ui'

const route = useRoute()
const groupId = Number(route.params.id)

const schema = v.object({
  targetUserIdentifier: v.pipe(v.string(), v.minLength(1, 'Email or Username is required')),
  note: v.optional(v.pipe(v.string(), v.maxLength(500, 'Note must be less than 500 characters')))
})

type Schema = v.InferOutput<typeof schema>

const state = reactive({
  targetUserIdentifier: '',
  note: ''
})

const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const submitInvite = async (event: FormSubmitEvent<Schema>) => {
  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await invitationService.createInvitation(
        groupId, 
        event.data.targetUserIdentifier, 
        event.data.note || ''
    )
    
    successMessage.value = 'Invitation sent successfully!'
    state.targetUserIdentifier = ''
    state.note = ''
  } catch (err: any) {
    errorMessage.value = err.data?.message || 'An error occurred while sending the invitation.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="max-w-md w-md mx-auto p-6 mt-4">
    <UCard class="shadow-sm">
      <template #header>
        <div class="text-left">
          <h1 class="text-xl font-semibold text-gray-900 dark:text-white">
            Invite to Group #{{ groupId }}
          </h1>
        </div>
      </template>

      <UAlert 
        v-if="errorMessage" 
        color="error" 
        variant="soft" 
        icon="i-heroicons-exclamation-triangle"
        :title="errorMessage" 
        class="mb-6" 
      />

      <UAlert 
        v-if="successMessage" 
        color="success" 
        variant="soft" 
        icon="i-heroicons-check-circle"
        :title="successMessage" 
        class="mb-6" 
      />

      <UForm :schema="schema" :state="state" class="space-y-6" @submit="submitInvite">
        
        <UFormField label="Email or Username" name="targetUserIdentifier" class="w-full text-left">
          <UInput 
            v-model="state.targetUserIdentifier" 
            placeholder="e.g., john_doe or john@example.com" 
            class="w-full"
          />
        </UFormField>

        <UFormField label="Note (Optional)" name="note" class="w-full text-left">
          <UTextarea 
            v-model="state.note" 
            placeholder="Come join my finance group!" 
            class="w-full"
            :rows="3"
          />
        </UFormField>

        <div class="flex items-center gap-3">
          <UButton type="submit" color="primary" :loading="isSubmitting">
            {{ isSubmitting ? 'Sending...' : 'Send Invitation' }}
          </UButton>

          <UButton :to="`/groups/${groupId}/invitations`" color="secondary" variant="outline">
            Back to Invitations
          </UButton>
        </div>
      </UForm>
    </UCard>
  </div>
</template>