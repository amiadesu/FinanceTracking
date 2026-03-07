<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from '#imports';
import { type GroupDto, groupService } from '@/services/groupService';
import { useLimitDisplay } from '@/composables/useLimitDisplay';

const router = useRouter();

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
  <div class="w-full lg:max-w-6xl md:max-w-4xl sm:max-w-2xl mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">My Groups</h1>
        <UBadge v-if="!loading" color="neutral" variant="subtle" size="md">
          Capacity: {{ limitDisplay }}
        </UBadge>
      </div>

      <div class="flex items-center gap-3">
        <UButton 
          to="/groups/create" 
          color="primary" 
          icon="i-heroicons-plus"
        >
          Create Group
        </UButton>

        <UButton 
          to="/" 
          color="secondary" 
          variant="outline" 
          icon="i-heroicons-arrow-left"
        >
          Back to Home
        </UButton>
      </div>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2 mb-4">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading your groups...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />
    
    <div v-else-if="groups && groups.length === 0" class="flex flex-col items-center justify-center py-12 text-center bg-gray-50 dark:bg-gray-900/50 rounded-lg border border-dashed border-gray-300 dark:border-gray-700">
      <UIcon name="i-heroicons-user-group" class="w-12 h-12 text-gray-400 mb-3" />
      <h3 class="text-lg font-medium text-gray-900 dark:text-white">No groups found</h3>
      <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">You don't have any groups yet. Create one to get started!</p>
      <UButton to="/groups/create" color="primary" variant="soft" class="mt-4" icon="i-heroicons-plus">
        Create First Group
      </UButton>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <UCard 
        v-for="group in groups" 
        :key="group.id" 
        @click="router.push(`/groups/${group.id}`)"
        :ui="{ body: 'flex flex-col h-full justify-between gap-4 relative' }"
        class="flex flex-col h-full shadow-sm hover:shadow-md hover:ring-2 hover:ring-primary-500/50 transition-all group cursor-pointer"
      >
        <UBadge v-if="group.isPersonal" color="success" variant="subtle" size="xs" class="absolute top-4 right-4 shrink-0">
          Personal
        </UBadge>

        <div class="flex flex-col items-center text-center mt-6">
          <h2 
            class="text-xl font-semibold text-gray-900 dark:text-white whitespace-normal wrap-break-words sm:break-all group-hover:text-primary-500 transition-colors w-full px-2 mb-2"
            :title="group.name"
          >
            {{ group.name }}
          </h2>
          
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Created: <span class="font-medium text-gray-700 dark:text-gray-300">{{ new Date(group.createdDate).toLocaleDateString() }}</span>
          </p>
        </div>

        <div class="w-full mt-2 flex items-center justify-center gap-2 py-2 px-4 rounded-md bg-gray-50 dark:bg-gray-800/50 text-sm font-medium text-gray-700 dark:text-gray-300 group-hover:bg-primary-50 dark:group-hover:bg-primary-900/20 group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors">
          <UIcon name="i-heroicons-arrow-right-circle" class="w-5 h-5" />
          View Details
        </div>
      </UCard>
    </div>
  </div>
</template>