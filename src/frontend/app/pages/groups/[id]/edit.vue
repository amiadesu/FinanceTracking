<script setup lang="ts">
import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { groupService } from '@/services/groupService';
import * as v from 'valibot';
import { groupEditSchema } from '~/schemas/schemas';
import type { FormSubmitEvent } from '@nuxt/ui';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

type Schema = v.InferOutput<typeof groupEditSchema>;

const state = reactive({
  name: ''
});

const errorMsg = ref('');
const isSaving = ref(false);

const isFormValid = computed(() => {
  return v.safeParse(groupEditSchema, state).success;
});

const group = await groupService.getGroup(groupId);
if (group) state.name = group.name;

async function onSubmit(event: FormSubmitEvent<Schema>) {
    isSaving.value = true;
    errorMsg.value = '';
    
    try {
        await groupService.updateGroup(groupId, { name: event.data.name });
        router.push(`/groups/${groupId}`);
    } catch (err: any) {
        errorMsg.value = err.data?.message || 'Failed to update group.';
    } finally {
        isSaving.value = false;
    }
}
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Edit Group</h1>
      <UButton 
        :to="`/groups/${groupId}`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Group
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

    <UCard class="shadow-sm w-full max-w-3xl mx-auto mt-8">
      <UForm :schema="groupEditSchema" :state="state" class="flex flex-col gap-6" @submit="onSubmit">
          
        <UFormField label="Group Name" name="name">
            <UInput v-model="state.name" type="text" class="w-full" />
        </UFormField>
        
        <USeparator />

        <div class="flex items-center gap-3">
            <UButton type="submit" color="primary" :loading="isSaving" :disabled="isSaving || !isFormValid">
                {{ isSaving ? 'Saving...' : 'Save Changes' }}
            </UButton>
        </div>

      </UForm>
    </UCard>
  </div>
</template>