<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from '#imports';
import { budgetGoalService } from '~/services/budgetGoalService';
import type { BudgetGoalDto, UpdateBudgetGoalDto, BudgetGoalProgressDto } from '~/services/budgetGoalService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const goalId = Number(route.params.goalId);

const goal = ref<BudgetGoalDto | null>(null);
const progress = ref<BudgetGoalProgressDto | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

// edit form
const editDto: UpdateBudgetGoalDto = {
  targetAmount: undefined,
  startDate: undefined,
  endDate: undefined,
};

async function load() {
  loading.value = true;
  error.value = null;
  try {
    goal.value = await budgetGoalService.getGoal(groupId, goalId);
    progress.value = await budgetGoalService.getProgress(groupId, goalId);
  } catch (err: any) {
    error.value = err.message || 'failed';
  } finally {
    loading.value = false;
  }
}

async function save() {
  if (!goal.value) return;
  try {
    const updated = await budgetGoalService.updateGoal(groupId, goalId, editDto);
    goal.value = updated;
    await load();
    alert('updated');
  } catch (err: any) {
    alert(err.message || 'fail');
  }
}

async function remove() {
  if (!confirm('Delete this goal?')) return;
  try {
    await budgetGoalService.deleteGoal(groupId, goalId);
    router.push(`/groups/${groupId}/goals`);
  } catch (err: any) {
    alert(err.message || 'fail');
  }
}

onMounted(load);
</script>

<template>
  <div class="p-4">
    <h1 class="text-xl font-bold">Budget goal #{{ goalId }}</h1>
    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>
    <div v-if="goal && progress">
      <p>Target: {{ goal.targetAmount }}</p>
      <p>Period: {{ goal.startDate }} → {{ goal.endDate }}</p>
      <p>Created: {{ goal.createdDate }}</p>
      <p>Updated: {{ goal.updatedDate }}</p>
      <p>Progress: {{ progress.currentAmount }} / {{ progress.targetAmount }}</p>

      <div class="mt-4 border-t pt-2">
        <h2 class="font-semibold">Edit</h2>
        <div class="flex flex-col gap-2 max-w-md">
          <label class="flex flex-col">
            New target amount
            <input type="number" v-model.number="editDto.targetAmount" class="border p-1" />
          </label>
          <label class="flex flex-col">
            New start date
            <input type="date" v-model="editDto.startDate" class="border p-1" />
          </label>
          <label class="flex flex-col">
            New end date
            <input type="date" v-model="editDto.endDate" class="border p-1" />
          </label>
          <button @click="save" class="bg-green-600 text-white px-3 py-1 rounded">Save</button>
          <button @click="remove" class="bg-red-600 text-white px-3 py-1 rounded">Delete</button>
        </div>
      </div>
    </div>
  </div>
</template>