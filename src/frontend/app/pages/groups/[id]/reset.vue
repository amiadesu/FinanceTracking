<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { groupService } from '@/services/groupService';
import * as v from 'valibot';
import { groupResetSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import type { FormSubmitEvent } from '@nuxt/ui';
import FormGlobalErrors from '~/components/FormGlobalErrors.vue';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

type Schema = v.InferOutput<typeof groupResetSchema>;

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

const { isFormValid, unmappedErrors, touch } = useFormValidation(groupResetSchema, state);

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
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Reset Group Data</h1>
      </div>
      
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

    <UCard class="ring-1 ring-orange-200 dark:ring-orange-800 shadow-sm w-full max-w-3xl mx-auto mt-8">
      <UForm :schema="groupResetSchema" :state="state" class="flex flex-col gap-6" @submit="resetGroup">
        <p class="text-sm text-gray-500 dark:text-gray-400">
          Select the specific data you wish to clear from this group. This action cannot be undone.
        </p>
          
        <UFormField name="selectedOptions" class="w-full">
          <UCheckboxGroup
              v-model="state.selectedOptions"
              :items="options"
              color="warning"
              variant="card"
              class="space-y-3"
          />
        </UFormField>

        <FormGlobalErrors :errors="unmappedErrors" />

        <USeparator />

        <div class="flex items-center gap-3">
            <UButton 
              type="submit" 
              color="warning" 
              :loading="isResetting" 
              :disabled="isResetting || !isFormValid || state.selectedOptions.length === 0"
            >
                {{ isResetting ? 'Processing...' : 'Execute Reset' }}
            </UButton>
        </div>
      </UForm>
    </UCard>
  </div>
</template>