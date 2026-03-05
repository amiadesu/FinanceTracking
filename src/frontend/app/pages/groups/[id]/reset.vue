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
  selectedOptions: v.array(v.string())
});

type Schema = v.InferOutput<typeof schema>;

const state = reactive({
  selectedOptions: [] as string[]
});

const options = [
    { label: 'Reset Members', value: 'resetMembers' },
    { label: 'Reset Budget Goals', value: 'resetBudgetGoals' },
    { label: 'Reset Categories', value: 'resetCategories' },
    { label: 'Reset Receipts, Products & Sellers', value: 'resetReceiptsProductsAndSellers' }
];

const errorMsg = ref('');
const isResetting = ref(false);

const resetGroup = async (event: FormSubmitEvent<Schema>) => {
    if(!confirm("Warning: Resetting will permanently delete the selected data. Continue?")) return;
    
    isResetting.value = true;
    errorMsg.value = '';

    const payload = {
        resetMembers: event.data.selectedOptions.includes('resetMembers'),
        resetBudgetGoals: event.data.selectedOptions.includes('resetBudgetGoals'),
        resetCategories: event.data.selectedOptions.includes('resetCategories'),
        resetReceiptsProductsAndSellers: event.data.selectedOptions.includes('resetReceiptsProductsAndSellers')
    };

    try {
        await groupService.resetGroup(groupId, payload);
        router.push(`/groups/${groupId}`);
    } catch (err: any) {
        errorMsg.value = err.data?.message || 'Failed to reset group.';
    } finally {
        isResetting.value = false;
    }
};
</script>

<template>
  <div class="max-w-xl mx-auto mt-10">
    <UCard class="ring-1 ring-orange-200 dark:ring-orange-800 shadow">
      <template #header>
        <div class="text-left">
          <h1 class="text-2xl font-bold text-orange-600 dark:text-orange-500 mb-2">Reset Group Data</h1>
          <p class="text-gray-600 dark:text-gray-400">Select the specific data you wish to clear from this group. This action cannot be undone.</p>
        </div>
      </template>
      
      <UAlert 
        v-if="errorMsg" 
        color="error" 
        variant="soft" 
        icon="i-heroicons-exclamation-triangle"
        :title="errorMsg" 
        class="mb-4" 
      />
      
      <UForm :schema="schema" :state="state" class="space-y-6" @submit="resetGroup">
          
          <UFormField name="selectedOptions" class="w-full text-left">
            <UCheckboxGroup
                v-model="state.selectedOptions"
                :items="options"
                color="warning"
                variant="card"
                class="space-y-3"
            />
          </UFormField>

          <div class="flex items-center gap-4 pt-4 mt-6 border-t dark:border-gray-800">
              <UButton type="submit" color="warning" :loading="isResetting">
                  {{ isResetting ? 'Processing...' : 'Execute Reset' }}
              </UButton>
              <UButton :to="`/groups/${groupId}`" color="secondary" variant="outline">
                  Cancel
              </UButton>
          </div>
      </UForm>
    </UCard>
  </div>
</template>