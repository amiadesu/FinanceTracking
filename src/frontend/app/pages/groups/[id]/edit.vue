<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { groupService } from '@/services/groupService';
import * as v from 'valibot';
import type { FormSubmitEvent } from '@nuxt/ui';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const schema = v.object({
  name: v.pipe(
    v.string(), 
    v.minLength(1, 'Group name is required'),
    v.maxLength(100, 'Group name must be less than 100 characters')
)
});

type Schema = v.InferOutput<typeof schema>;

const state = reactive({
  name: ''
});

const errorMsg = ref('');
const isSaving = ref(false);

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
  <div class="max-w-xl mx-auto mt-10">
    <UCard class="shadow-sm">
      <template #header>
        <h1 class="text-xl font-semibold text-gray-900 dark:text-white">Edit Group</h1>
      </template>

      <UAlert 
        v-if="errorMsg" 
        color="error" 
        variant="soft" 
        icon="i-heroicons-exclamation-triangle"
        :title="errorMsg" 
        class="mb-6" 
      />
      
      <UForm :schema="schema" :state="state" class="space-y-6" @submit="onSubmit">
          
          <UFormField label="Group Name" name="name">
              <UInput v-model="state.name" type="text" />
          </UFormField>
          
          <div class="flex items-center gap-3">
              <UButton type="submit" color="primary" :loading="isSaving">
                  {{ isSaving ? 'Saving...' : 'Save Changes' }}
              </UButton>
              <UButton :to="`/groups/${groupId}`" color="secondary" variant="outline">
                  Cancel
              </UButton>
          </div>

      </UForm>
    </UCard>
  </div>
</template>