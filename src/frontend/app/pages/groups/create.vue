<script setup lang="ts">
import { ref } from 'vue';
import { groupService } from '@/services/groupService';

const router = useRouter();

const name = ref('');
const errorMsg = ref('');
const isCreating = ref(false);

const handleCreateGroup = async () => {
    isCreating.value = true;
    errorMsg.value = '';
    
    try {
        const data = await groupService.createGroup({ name: name.value });

        if (data.id) {
            router.push(`/groups/${data.id}`);
        } else {
            router.push('/groups');
        }
    } catch (err: any) {
        errorMsg.value = err.data?.message || 'An unexpected error occurred while creating the group.';
    } finally {
        isCreating.value = false;
    }
};
</script>

<template>
  <div class="max-w-xl mx-auto p-6 bg-white rounded-lg shadow mt-10">
    <h1 class="text-2xl font-bold mb-6 text-gray-800">Create New Group</h1>
    
    <div v-if="errorMsg" class="bg-red-100 text-red-700 p-3 rounded mb-4">
        {{ errorMsg }}
    </div>
    
    <form @submit.prevent="handleCreateGroup">
        <div class="mb-5">
            <label for="groupName" class="block text-gray-700 font-medium mb-2">Group Name</label>
            <input 
                id="groupName"
                v-model="name" 
                type="text" 
                required 
                placeholder="e.g. Family Budget, Vacation Fund..."
                class="w-full border-gray-300 rounded-md shadow-sm p-2 border focus:ring-blue-500 focus:border-blue-500 outline-none" 
            />
            <p class="text-xs text-gray-500 mt-2">You will be automatically set as the owner of this group.</p>
        </div>
        
        <div class="flex gap-4 mt-6 pt-4 border-t border-gray-100">
            <button 
                type="submit" 
                :disabled="isCreating" 
                class="bg-blue-600 text-white px-5 py-2 rounded hover:bg-blue-700 disabled:opacity-50 transition-colors font-medium cursor-pointer">
                {{ isCreating ? 'Creating...' : 'Create Group' }}
            </button>
            <NuxtLink 
                to="/groups" 
                class="px-5 py-2 text-gray-600 hover:bg-gray-100 rounded transition-colors font-medium">
                Cancel
            </NuxtLink>
        </div>
    </form>
  </div>
</template>