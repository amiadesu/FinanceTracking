<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue';
import { useRoute, useRouter } from '#imports';
import * as v from 'valibot';
import { budgetGoalSchema } from '~/schemas/schemas';
import { budgetGoalService } from '~/services/budgetGoalService';
import type { BudgetGoalDto, UpdateBudgetGoalDto, BudgetGoalProgressDto } from '~/services/budgetGoalService';
import type { FormSubmitEvent } from '@nuxt/ui';

type Schema = v.InferOutput<typeof budgetGoalSchema>;

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const goalId = Number(route.params.goalId);

const goal = ref<BudgetGoalDto | null>(null);
const progress = ref<BudgetGoalProgressDto | null>(null);
const loading = ref(false);
const isSubmitting = ref(false);
const error = ref<string | null>(null);

const editDto = reactive<UpdateBudgetGoalDto>({
  targetAmount: undefined,
  startDate: undefined,
  endDate: undefined,
});

function formatDate(dateString?: string, includeTime: boolean = false) {
  if (!dateString) return '';
  if (includeTime) {
    return new Date(dateString).toLocaleString();
  }
  return dateString.split('T')[0]; 
}

const isFormValid = computed(() => {
  return v.safeParse(budgetGoalSchema, editDto).success;
});

async function load() {
  loading.value = true;
  error.value = null;
  try {
    const [goalData, progressData] = await Promise.all([
      budgetGoalService.getGoal(groupId, goalId),
      budgetGoalService.getProgress(groupId, goalId)
    ]);
    
    goal.value = goalData;
    progress.value = progressData;

    if (goal.value) {
      editDto.targetAmount = goal.value.targetAmount;
      editDto.startDate = formatDate(goal.value.startDate);
      editDto.endDate = formatDate(goal.value.endDate);
    }
  } catch (err: any) {
    error.value = err.message || 'Failed to load goal details';
  } finally {
    loading.value = false;
  }
}

async function save(event: FormSubmitEvent<Schema>) {
  if (!goal.value) return;
  
  isSubmitting.value = true;
  try {
    const payload: UpdateBudgetGoalDto = { ...event.data };
    const updated = await budgetGoalService.updateGoal(groupId, goalId, payload);
    goal.value = updated;
    await load();
    alert('Goal updated successfully.');
  } catch (err: any) {
    alert(err.message || 'Error updating goal');
  } finally {
    isSubmitting.value = false;
  }
}

async function remove() {
  if (!confirm(`Are you sure you want to delete this budget goal?`)) return;
  try {
    await budgetGoalService.deleteGoal(groupId, goalId);
    router.push(`/groups/${groupId}/goals`);
  } catch (err: any) {
    alert(err.message || 'Error deleting goal');
  }
}

onMounted(load);
</script>

<template>
  <div class="max-w-5xl mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Manage Goal #{{ goalId }}</h1>
      <UButton 
        :to="`/groups/${groupId}/goals`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Goals
      </UButton>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2 mb-4">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading goal data...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <UCard v-else-if="goal && progress" class="shadow-sm min-h-125 flex flex-col" :ui="{ body: 'flex-1 flex flex-col p-6 sm:p-10' }">
      <div class="flex flex-col gap-10 flex-1">
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-x-8 gap-y-8">
          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Target Amount</span>
            <p class="text-xl font-semibold text-gray-900 dark:text-white mt-1">{{ goal.targetAmount }}</p>
          </div>
          
          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Start Date</span>
            <p class="text-lg font-medium text-gray-900 dark:text-white mt-1">{{ formatDate(goal.startDate) }}</p>
          </div>

          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Created At</span>
            <p class="text-base text-gray-900 dark:text-white mt-1">{{ formatDate(goal.createdDate, true) }}</p>
          </div>

          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Progress</span>
            <p class="text-xl font-semibold text-gray-900 dark:text-white mt-1">
              {{ progress.currentAmount }} <span class="text-base font-normal text-gray-500 dark:text-gray-400">/ {{ progress.targetAmount }}</span>
            </p>
          </div>

          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">End Date</span>
            <p class="text-lg font-medium text-gray-900 dark:text-white mt-1">{{ formatDate(goal.endDate) }}</p>
          </div>

          <div>
            <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Updated At</span>
            <p class="text-base text-gray-900 dark:text-white mt-1">{{ formatDate(goal.updatedDate, true) }}</p>
          </div>
        </div>

        <USeparator />

        <UForm :schema="budgetGoalSchema" :state="editDto" class="flex-1 flex flex-col" @submit="save">
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-6">Edit Goal</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-8 flex-1">
            <UFormField label="New Target Amount" name="targetAmount" required>
              <UInput type="number" v-model.number="editDto.targetAmount" class="w-full" />
            </UFormField>

            <UFormField label="New Start Date" name="startDate" required>
              <UInput type="date" v-model="editDto.startDate" class="w-full" />
            </UFormField>

            <UFormField label="New End Date" name="endDate" required>
              <UInput type="date" v-model="editDto.endDate" class="w-full" />
            </UFormField>
          </div>

          <div class="mt-8">
            <USeparator class="mb-6" />
            
            <div class="flex flex-wrap items-center gap-3 justify-between">
              <div class="flex gap-3">
                <UButton 
                  type="submit" 
                  color="primary" 
                  :loading="isSubmitting" 
                  :disabled="isSubmitting || !isFormValid"
                >
                  {{ isSubmitting ? 'Saving...' : 'Save Changes' }}
                </UButton>
              </div>

              <UButton 
                @click="remove" 
                color="error" 
                variant="outline" 
                :disabled="isSubmitting"
              >
                Delete Goal
              </UButton>
            </div>
          </div>
        </UForm>
      </div>
    </UCard>
  </div>
</template>