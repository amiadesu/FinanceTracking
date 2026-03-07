<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRoute } from 'vue-router';
import { invitationService } from '~/services/invitationService';
import * as v from 'valibot';
import { inviteSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import type { FormSubmitEvent } from '@nuxt/ui';
import FormGlobalErrors from '~/components/FormGlobalErrors.vue';

const route = useRoute()
const groupId = Number(route.params.id)

type Schema = v.InferOutput<typeof inviteSchema>

const state = reactive({
  targetUserIdentifier: '',
  note: ''
})

const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const { isFormValid, unmappedErrors, touch } = useFormValidation(inviteSchema, state);

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
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Send Invitation</h1>
        <UBadge color="neutral" variant="subtle" size="md">#{{ groupId }}</UBadge>
      </div>
      
      <UButton 
        :to="`/groups/${groupId}/invitations`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Invitations
      </UButton>
    </div>

    <UAlert 
      v-if="errorMessage" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="errorMessage" 
      class="mb-4" 
    />

    <UAlert 
      v-if="successMessage" 
      color="success" 
      variant="soft" 
      icon="i-heroicons-check-circle"
      :title="successMessage" 
      class="mb-4" 
    />

    <UCard class="shadow-sm w-full max-w-3xl mx-auto mt-8">
      <UForm :schema="inviteSchema" :state="state" class="flex flex-col gap-6" @submit="submitInvite">
        <UFormField label="Email or Username" name="targetUserIdentifier" required>
          <UInput 
            v-model="state.targetUserIdentifier" 
            placeholder="e.g., john_doe or john@example.com" 
            class="w-full"
          />
        </UFormField>

        <UFormField label="Note (Optional)" name="note">
          <UTextarea 
            v-model="state.note" 
            placeholder="Come join my finance group!" 
            class="w-full"
            :rows="3"
          />
        </UFormField>

        <FormGlobalErrors :errors="unmappedErrors" />

        <USeparator />

        <div class="flex items-center gap-3">
          <UButton type="submit" color="primary" :loading="isSubmitting" :disabled="isSubmitting || !isFormValid">
            {{ isSubmitting ? 'Sending...' : 'Send Invitation' }}
          </UButton>
        </div>
      </UForm>
    </UCard>
  </div>
</template>