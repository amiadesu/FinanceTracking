<script setup lang="ts">
import { ref } from 'vue';
import { groupService } from '@/services/groupService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const name = ref('');
const errorMsg = ref('');
const isSaving = ref(false);

const group = await groupService.getGroup(groupId);
if (group) name.value = group.name;

const updateGroup = async () => {
    isSaving.value = true;
    errorMsg.value = '';
    try {
        await groupService.updateGroup(groupId, { name: name.value });
        router.push(`/groups/${groupId}`);
    } catch (err: any) {
        errorMsg.value = err.data?.message || 'Failed to update group.';
    } finally {
        isSaving.value = false;
    }
};
</script>

<template>
  <div class="max-w-xl mx-auto p-6 bg-white rounded-lg shadow mt-10">
    <h1 class="text-2xl font-bold mb-6">Edit Group</h1>
    <div v-if="errorMsg" class="bg-red-100 text-red-700 p-3 rounded mb-4">{{ errorMsg }}</div>
    
    <form @submit.prevent="updateGroup">
        <div class="mb-4">
            <label class="block text-gray-700 font-medium mb-2">Group Name</label>
            <input 
                v-model="name" 
                type="text" 
                required 
                class="w-full border-gray-300 rounded-md shadow-sm p-2 border focus:ring-blue-500 focus:border-blue-500" 
            />
        </div>
        <div class="flex gap-4">
            <button type="submit" :disabled="isSaving" class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50">
                {{ isSaving ? 'Saving...' : 'Save Changes' }}
            </button>
            <NuxtLink :to="`/groups/${groupId}`" class="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded">Cancel</NuxtLink>
        </div>
    </form>
  </div>
</template>