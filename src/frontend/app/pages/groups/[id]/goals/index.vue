<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue';
import { useRoute } from '#imports';
import * as v from 'valibot';
import { budgetGoalSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import { budgetGoalService } from '~/services/budgetGoalService';
import type { BudgetGoalDto, CreateBudgetGoalDto } from '~/services/budgetGoalService';
import { useLimitDisplay } from '~/composables/useLimitDisplay';
import type { FormSubmitEvent } from '@nuxt/ui';
import { formatDate } from '@/utils/formatDate';
import FormGlobalErrors from "~/components/FormGlobalErrors.vue";

type Schema = v.InferOutput<typeof budgetGoalSchema>;

const route = useRoute();
const groupId = Number(route.params.id);

const goals = ref<BudgetGoalDto[]>([]);
const currentCount = ref(0);
const maxAllowed = ref(0);

const loading = ref(false);
const isSubmitting = ref(false);
const error = ref<string | null>(null);

const newGoal = reactive<CreateBudgetGoalDto>({
  targetAmount: 0,
  startDate: '',
  endDate: ''
});

const limitDisplay = useLimitDisplay(currentCount, maxAllowed);

const { isFormValid, unmappedErrors, touch } = useFormValidation(budgetGoalSchema, newGoal);

async function loadGoals() {
  loading.value = true;
  error.value = null;
  try {
    const budgetGoalsResponse = await budgetGoalService.getGoals(groupId);
    currentCount.value = budgetGoalsResponse.currentCount;
    maxAllowed.value = budgetGoalsResponse.maxAllowed;
    goals.value = budgetGoalsResponse.budgetGoals;
  } catch (err: any) {
    error.value = err.message || 'Failed to load goals';
  } finally {
    loading.value = false;
  }
}

async function createGoal(event: FormSubmitEvent<Schema>) {
  isSubmitting.value = true;
  try {
    await budgetGoalService.createGoal(groupId, event.data);
    newGoal.targetAmount = 0;
    newGoal.startDate = '';
    newGoal.endDate = '';
    await loadGoals();
  } catch (err: any) {
    alert(err.message || 'Error creating goal');
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(() => {
  loadGoals();
});

const columns = computed(() => [
  { accessorKey: 'id', header: 'ID' },
  { accessorKey: 'targetAmount', header: 'Target' },
  { 
    id: 'period', 
    header: 'Period',
    cell: ({ row }: any) => {
      const start = row.original?.startDate || row.getValue('startDate');
      const end = row.original?.endDate || row.getValue('endDate');
      return `${formatDate(start)} → ${formatDate(end)}`;
    }
  },
  { id: 'actions', header: '' }
]);
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-4">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Budget Goals</h1>
        <UBadge v-if="!loading" color="neutral" variant="subtle" class="font-mono">
          Capacity: {{ limitDisplay }}
        </UBadge>
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
      v-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-6" 
    />

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start lg:items-stretch w-full max-w-full">
      
      <div class="lg:col-span-2 w-full">
        <UCard 
          class="shadow-sm flex flex-col lg:h-100"
          :ui="{ body: 'p-0 sm:p-0 flex-1 overflow-hidden flex flex-col' }"
        >
          <template #header>
            <h2 class="font-semibold text-gray-900 dark:text-white">Existing Goals</h2>
          </template>
          
          <UTable 
            sticky
            :data="goals" 
            :columns="columns" 
            :loading="loading"
            class="flex-1 min-h-0 overflow-y-auto"
          >
            <template #actions-cell="{ row }">
              <UButton 
                :to="`/groups/${groupId}/goals/${row.original?.id || row.id}`"
                color="primary" 
                variant="outline" 
                icon="i-heroicons-cog"
                size="sm"
              >
                Manage
              </UButton>
            </template>

            <template #empty>
              <div class="flex flex-col items-center justify-center py-12">
                <span class="text-gray-500 dark:text-gray-400">No budget goals found.</span>
              </div>
            </template>
          </UTable>
        </UCard>
      </div>

      <div class="lg:col-span-1 w-full lg:max-w-sm">
        <UCard 
          class="shadow-sm flex flex-col lg:h-100" 
          :ui="{ 
            body: 'flex-1 flex flex-col min-h-0'
          }"
        >
          <template #header>
            <h2 class="font-semibold text-gray-900 dark:text-white">Create Goal</h2>
          </template>

          <UForm 
            :schema="budgetGoalSchema" 
            :state="newGoal" 
            class="flex flex-col flex-1 min-h-0" 
            @submit="createGoal"
          >
            <div class="flex flex-col flex-1 overflow-y-auto pr-2 pb-2">
              <div class="flex flex-col gap-6 flex-1">
                <UFormField label="Target Amount" name="targetAmount" required>
                  <UInput type="number" v-model.number="newGoal.targetAmount" class="w-full" placeholder="e.g. 5000" />
                </UFormField>

                <UFormField label="Start Date" name="startDate" required>
                  <UInput type="date" v-model="newGoal.startDate" @change="touch" class="w-full" />
                </UFormField>

                <UFormField label="End Date" name="endDate" required>
                  <UInput type="date" v-model="newGoal.endDate" @change="touch" class="w-full" />
                </UFormField>
              </div>
              
              <FormGlobalErrors class="" :errors="unmappedErrors" />
            </div>

            <div class="mt-auto pt-6">
              <USeparator class="mb-6" />

              <div class="flex flex-wrap items-center gap-3 justify-between">
                <UButton 
                  type="submit" 
                  color="primary" 
                  class="w-full justify-center"
                  :loading="isSubmitting"
                  :disabled="isSubmitting || !isFormValid"
                >
                  {{ isSubmitting ? 'Creating...' : 'Create Goal' }}
                </UButton>
              </div>
            </div>
          </UForm>
        </UCard>
      </div>
    </div>
  </div>
</template>