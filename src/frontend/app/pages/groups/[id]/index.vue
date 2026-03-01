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
  <div class="max-w-6xl mx-auto p-6">
    <div v-if="loading" class="text-gray-500 animate-pulse">Loading group data...</div>
    <div v-else-if="error" class="bg-red-100 text-red-700 p-4 rounded-md">Error: {{ error }}</div>
    
    <div v-else-if="group">
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 border-b pb-4">
        <div>
            <h1 class="text-3xl font-bold text-gray-800 flex items-center gap-3">
                {{ group.name }}
                <span v-if="group.isPersonal" class="bg-green-100 text-green-800 text-sm px-2 py-1 rounded-full font-normal">Personal</span>
            </h1>
            <p class="text-sm text-gray-500 mt-1">Created on {{ new Date(group.createdDate).toLocaleDateString() }}</p>
        </div>
        
        <div class="flex gap-2 mt-4 md:mt-0">
            <NuxtLink :to="`/groups/${groupId}/edit`" class="bg-blue-50 text-blue-600 hover:bg-blue-100 px-3 py-1.5 rounded text-sm font-medium">Edit</NuxtLink>
            <NuxtLink :to="`/groups/${groupId}/reset`" class="bg-orange-50 text-orange-600 hover:bg-orange-100 px-3 py-1.5 rounded text-sm font-medium">Reset</NuxtLink>
            <button v-if="!group.isPersonal" @click="handleLeaveGroup" class="bg-gray-100 text-gray-700 hover:bg-gray-200 px-3 py-1.5 rounded text-sm font-medium">Leave</button>
            <button v-if="!group.isPersonal" @click="handleDeleteGroup" class="bg-red-50 text-red-600 hover:bg-red-100 px-3 py-1.5 rounded text-sm font-medium">Delete</button>
        </div>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        <NuxtLink :to="`/groups/${groupId}/members`" class="p-4 bg-white border rounded-lg shadow-sm hover:border-blue-500 hover:shadow-md transition text-center font-medium text-gray-700">👥 Members</NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/invitations`" class="p-4 bg-white border rounded-lg shadow-sm hover:border-blue-500 hover:shadow-md transition text-center font-medium text-gray-700">✉️ Invitations</NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/history`" class="p-4 bg-white border rounded-lg shadow-sm hover:border-blue-500 hover:shadow-md transition text-center font-medium text-gray-700">📜 History</NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/categories`" class="p-4 bg-white border rounded-lg shadow-sm hover:border-blue-500 hover:shadow-md transition text-center font-medium text-gray-700">📁 Categories</NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/goals`" class="p-4 bg-white border rounded-lg shadow-sm hover:border-blue-500 hover:shadow-md transition text-center font-medium text-gray-700">🎯 Budget Goals</NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/receipts`" class="p-4 bg-white border rounded-lg shadow-sm hover:border-blue-500 hover:shadow-md transition text-center font-medium text-gray-700">🧾 Receipts</NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/sellers`" class="p-4 bg-white border rounded-lg shadow-sm hover:border-blue-500 hover:shadow-md transition text-center font-medium text-gray-700">🏪 Sellers</NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/products`" class="p-4 bg-white border rounded-lg shadow-sm hover:border-blue-500 hover:shadow-md transition text-center font-medium text-gray-700">🛍️ Products</NuxtLink>
      </div>
    </div>
  </div>
</template>