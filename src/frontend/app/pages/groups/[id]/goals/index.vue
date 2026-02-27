<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from '#imports';
import { budgetGoalService } from '~/services/budgetGoalService';
import type { BudgetGoalDto, CreateBudgetGoalDto } from '~/services/budgetGoalService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const goals = ref<BudgetGoalDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

// form state
const newGoal: CreateBudgetGoalDto = {
  targetAmount: 0,
  startDate: '',
  endDate: ''
};

async function loadGoals() {
  loading.value = true;
  error.value = null;
  try {
    const res = await budgetGoalService.getGoals(groupId);
    goals.value = res;
  } catch (err: any) {
    error.value = err.message || 'Failed to load goals';
  } finally {
    loading.value = false;
  }
}

async function createGoal() {
  try {
    await budgetGoalService.createGoal(groupId, newGoal);
    // reset form
    newGoal.targetAmount = 0;
    newGoal.startDate = '';
    newGoal.endDate = '';
    await loadGoals();
  } catch (err: any) {
    alert(err.message || 'error creating');
  }
}

function goTo(id: number) {
  router.push(`/groups/${groupId}/goals/${id}`);
}

onMounted(() => {
  loadGoals();
});
</script>

<template>
  <div class="p-4">
    <h1 class="text-xl font-bold">Budget Goals</h1>
    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <table v-if="!loading" class="w-full mt-4 table-auto border-collapse">
      <thead>
        <tr>
          <th class="border px-2 py-1">ID</th>
          <th class="border px-2 py-1">Target</th>
          <th class="border px-2 py-1">Period</th>
          <th class="border px-2 py-1">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="g in goals" :key="g.id">
          <td class="border px-2 py-1">{{ g.id }}</td>
          <td class="border px-2 py-1">{{ g.targetAmount }}</td>
          <td class="border px-2 py-1">{{ g.startDate }} → {{ g.endDate }}</td>
          <td class="border px-2 py-1">
            <button @click="goTo(g.id)" class="text-blue-600 underline">View</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div class="mt-6 border-t pt-4">
      <h2 class="font-semibold">Create new goal</h2>
      <div class="flex flex-col gap-2 max-w-md">
        <label class="flex flex-col">
          Target amount
          <input type="number" v-model.number="newGoal.targetAmount" class="border p-1" />
        </label>
        <label class="flex flex-col">
          Start date
          <input type="date" v-model="newGoal.startDate" class="border p-1" />
        </label>
        <label class="flex flex-col">
          End date
          <input type="date" v-model="newGoal.endDate" class="border p-1" />
        </label>
        <button @click="createGoal" class="bg-blue-600 text-white px-3 py-1 rounded">Create</button>
      </div>
    </div>
  </div>
</template>
