<script setup lang="ts">
import { groupService } from '@/services/groupService';
import { type GroupDto } from '@/services/groupService';

const group = ref<GroupDto | null>(null);

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const loading = ref(false);
const error = ref<string | null>(null);

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const [groupData] = await Promise.all([
      groupService.getGroup(groupId)
    ]);
    
    group.value = groupData;
  } catch (err: any) {
    error.value = err.message || 'Failed to load group';
  } finally {
    loading.value = false;
  }
}

const handleLeaveGroup = async () => {
    if(!confirm("Are you sure you want to leave this group?")) return;
    try {
        await groupService.leaveGroup(groupId);
        router.push('/groups');
    } catch (err: any) {
        alert("Failed to leave group.");
    }
};

const handleDeleteGroup = async () => {
    if(!confirm("Are you sure you want to permanently delete this group?")) return;
    try {
        await groupService.deleteGroup(groupId);
        router.push('/groups');
    } catch (err) {
        alert("Failed to delete group.");
    }
};

onMounted(() => loadData());
</script>

<template>
  <UContainer class="max-w-6xl py-6">
    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading group data...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="'Error: ' + error" 
    />
    
    <div v-else-if="group">
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 border-b dark:border-gray-800 pb-4">
        <div>
            <h1 class="text-3xl font-bold text-gray-800 dark:text-gray-100 flex items-center gap-3">
                {{ group.name }}
                <UBadge v-if="group.isPersonal" color="info" variant="soft" size="sm" class="font-normal">
                  Personal
                </UBadge>
            </h1>
            <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
              Created on {{ new Date(group.createdDate).toLocaleDateString() }}
            </p>
        </div>
        
        <div class="flex gap-2 mt-4 md:mt-0">
            <UButton :to="`/groups/${groupId}/edit`" color="primary" variant="soft" size="sm">
              Edit
            </UButton>
            <UButton :to="`/groups/${groupId}/reset`" color="warning" variant="soft" size="sm">
              Reset
            </UButton>
            <UButton v-if="!group.isPersonal" @click="handleLeaveGroup" color="neutral" variant="soft" size="sm">
              Leave
            </UButton>
            <UButton v-if="!group.isPersonal" @click="handleDeleteGroup" color="error" variant="soft" size="sm">
              Delete
            </UButton>
        </div>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        <NuxtLink :to="`/groups/${groupId}/members`" class="block group">
          <UCard :ui="{ body: 'p-4 sm:p-4' }" class="h-full hover:ring-2 hover:ring-blue-500 hover:shadow-md transition text-center font-medium text-gray-700 dark:text-gray-200 cursor-pointer">
            👥 Members
          </UCard>
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/invitations`" class="block group">
          <UCard :ui="{ body: 'p-4 sm:p-4' }" class="h-full hover:ring-2 hover:ring-blue-500 hover:shadow-md transition text-center font-medium text-gray-700 dark:text-gray-200 cursor-pointer">
            ✉️ Invitations
          </UCard>
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/history`" class="block group">
          <UCard :ui="{ body: 'p-4 sm:p-4' }" class="h-full hover:ring-2 hover:ring-blue-500 hover:shadow-md transition text-center font-medium text-gray-700 dark:text-gray-200 cursor-pointer">
            📜 History
          </UCard>
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/categories`" class="block group">
          <UCard :ui="{ body: 'p-4 sm:p-4' }" class="h-full hover:ring-2 hover:ring-blue-500 hover:shadow-md transition text-center font-medium text-gray-700 dark:text-gray-200 cursor-pointer">
            📁 Categories
          </UCard>
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/goals`" class="block group">
          <UCard :ui="{ body: 'p-4 sm:p-4' }" class="h-full hover:ring-2 hover:ring-blue-500 hover:shadow-md transition text-center font-medium text-gray-700 dark:text-gray-200 cursor-pointer">
            🎯 Budget Goals
          </UCard>
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/receipts`" class="block group">
          <UCard :ui="{ body: 'p-4 sm:p-4' }" class="h-full hover:ring-2 hover:ring-blue-500 hover:shadow-md transition text-center font-medium text-gray-700 dark:text-gray-200 cursor-pointer">
            🧾 Receipts
          </UCard>
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/sellers`" class="block group">
          <UCard :ui="{ body: 'p-4 sm:p-4' }" class="h-full hover:ring-2 hover:ring-blue-500 hover:shadow-md transition text-center font-medium text-gray-700 dark:text-gray-200 cursor-pointer">
            🏪 Sellers
          </UCard>
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/products`" class="block group">
          <UCard :ui="{ body: 'p-4 sm:p-4' }" class="h-full hover:ring-2 hover:ring-blue-500 hover:shadow-md transition text-center font-medium text-gray-700 dark:text-gray-200 cursor-pointer">
            🛍️ Products
          </UCard>
        </NuxtLink>
      </div>
    </div>
  </UContainer>
</template>