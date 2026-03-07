<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from '#imports';
import { groupService } from '@/services/groupService';
import type { FormSubmitEvent } from '@nuxt/ui';
import * as v from 'valibot';
import { groupEditSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import FormGlobalErrors from '~/components/FormGlobalErrors.vue';

type Schema = v.InferOutput<typeof groupEditSchema>;

const router = useRouter();

const isCreating = ref(false);
const errorMsg = ref<string | null>(null);

const state = reactive({
    name: ''
});

const { isFormValid, unmappedErrors, touch } = useFormValidation(groupEditSchema, state);

const handleCreateGroup = async (event: FormSubmitEvent<Schema>) => {
    isCreating.value = true;
    errorMsg.value = null;
    
    try {
        const data = await groupService.createGroup({ name: event.data.name });

        if (data.id) {
            router.push(`/groups/${data.id}`);
        } else {
            router.push('/groups');
        }
    } catch (err: any) {
        errorMsg.value = err.data?.message || err.message || 'An unexpected error occurred while creating the group.';
    } finally {
        isCreating.value = false;
    }
};
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Create New Group</h1>
      </div>
      
      <UButton 
        to="/groups" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Groups
      </UButton>
    </div>

    <UAlert 
      v-if="errorMsg" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="errorMsg" 
      class="mb-4" 
    />

    <UCard class="shadow-sm w-full max-w-xl mx-auto mt-8">
        <UForm :schema="groupEditSchema" :state="state" class="flex flex-col gap-6" @submit="handleCreateGroup">
            <div class="flex flex-col gap-4">
                <UFormField label="Group Name" name="name" required>
                    <UInput 
                        v-model="state.name" 
                        placeholder="e.g. Family Budget, Vacation Fund..." 
                        class="w-full"
                    />
                </UFormField>
                <p class="text-xs text-gray-500">You will be automatically set as the owner of this group.</p>

                <FormGlobalErrors :errors="unmappedErrors" />
            </div>

            <USeparator />

            <div class="flex flex-wrap items-center gap-3">
                <UButton 
                    type="submit" 
                    color="primary"
                    :loading="isCreating"
                    :disabled="isCreating || !isFormValid"
                >
                    {{ isCreating ? 'Creating...' : 'Create Group' }}
                </UButton>

                <UButton 
                    to="/groups" 
                    color="secondary" 
                    variant="outline"
                    :disabled="isCreating"
                >
                    Cancel
                </UButton>
            </div>
        </UForm>
    </UCard>
  </div>
</template>