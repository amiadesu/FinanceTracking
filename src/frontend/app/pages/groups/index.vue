<script setup lang="ts">
import { ref } from 'vue';
import { type GroupDto, groupService } from '@/services/groupService';
import { useLimitDisplay } from '@/composables/useLimitDisplay';

const groups = ref<GroupDto[]>([]);
const currentCount = ref(0);
const maxAllowed = ref(0);

const loading = ref(false);
const error = ref<string | null>(null);

const limitDisplay = useLimitDisplay(currentCount, maxAllowed);

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const [groupsResponse] = await Promise.all([
      groupService.getMyGroups()
    ]);
    
    currentCount.value = groupsResponse.currentCount;
    maxAllowed.value = groupsResponse.maxAllowed;
    groups.value = groupsResponse.groups;
  } catch (err: any) {
    error.value = err.message || 'Failed to load groups';
  } finally {
    loading.value = false;
  }
}

onMounted(() => loadData());
</script>

<template>
  <div class="max-w-6xl mx-auto p-6">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold text-gray-800">My Groups</h1>
      <p class="text-sm text-gray-600" v-if="!loading">
        {{ limitDisplay }}
      </p>
      <NuxtLink 
        to="/groups/create" 
        class="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded shadow">
        + Create Group
      </NuxtLink>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse">
      Loading your groups...
    </div>
    <div v-else-if="error" class="bg-red-100 text-red-700 p-4 rounded-md">
      Error loading groups: {{ error }}
    </div>
    
    <div v-else-if="groups && groups.length === 0" class="text-gray-500 italic">
      You don't have any groups yet. Create one to get started!
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div 
        v-for="group in groups" 
        :key="group.id" 
        class="bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition-shadow p-5 flex flex-col justify-between">
        
        <div>
          <div class="flex justify-between items-start mb-2">
            <h2 class="text-xl font-semibold text-gray-900 line-clamp-1">
              {{ group.name }}
            </h2>
            <span v-if="group.isPersonal" class="bg-green-100 text-green-800 text-xs px-2 py-1 rounded-full">
              Personal
            </span>
          </div>
          
          <p class="text-sm text-gray-500 mb-4">
            Created: {{ new Date(group.createdDate).toLocaleDateString() }}
          </p>
        </div>

        <NuxtLink 
          :to="`/groups/${group.id}`" 
          class="block text-center w-full bg-gray-100 hover:bg-gray-200 text-gray-800 font-medium py-2 px-4 rounded transition-colors">
          View Details
        </NuxtLink>
      </div>
    </div>
  </div>
</template>