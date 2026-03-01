<script setup lang="ts">
import { ref } from 'vue';
import { groupService } from '@/services/groupService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const resetOptions = ref({
    resetMembers: false,
    resetBudgetGoals: false,
    resetCategories: false,
    resetReceiptsProductsAndSellers: false
});

const errorMsg = ref('');
const isResetting = ref(false);

const resetGroup = async () => {
    if(!confirm("Warning: Resetting will permanently delete the selected data. Continue?")) return;
    
    isResetting.value = true;
    errorMsg.value = '';
    try {
        await groupService.resetGroup(groupId, resetOptions.value);
        router.push(`/groups/${groupId}`);
    } catch (err: any) {
        errorMsg.value = err.data?.message || 'Failed to reset group.';
    } finally {
        isResetting.value = false;
    }
};
</script>

<template>
  <div class="max-w-xl mx-auto p-6 bg-white rounded-lg shadow mt-10 border border-orange-200">
    <h1 class="text-2xl font-bold text-orange-600 mb-2">Reset Group Data</h1>
    <p class="text-gray-600 mb-6">Select the specific data you wish to clear from this group. This action cannot be undone.</p>
    
    <div v-if="errorMsg" class="bg-red-100 text-red-700 p-3 rounded mb-4">{{ errorMsg }}</div>
    
    <form @submit.prevent="resetGroup" class="space-y-4">
        <label class="flex items-center gap-3 p-3 border rounded hover:bg-gray-50 cursor-pointer">
            <input type="checkbox" v-model="resetOptions.resetMembers" class="w-5 h-5 text-orange-600 rounded" />
            <span class="text-gray-800">Reset Members</span>
        </label>
        
        <label class="flex items-center gap-3 p-3 border rounded hover:bg-gray-50 cursor-pointer">
            <input type="checkbox" v-model="resetOptions.resetBudgetGoals" class="w-5 h-5 text-orange-600 rounded" />
            <span class="text-gray-800">Reset Budget Goals</span>
        </label>

        <label class="flex items-center gap-3 p-3 border rounded hover:bg-gray-50 cursor-pointer">
            <input type="checkbox" v-model="resetOptions.resetCategories" class="w-5 h-5 text-orange-600 rounded" />
            <span class="text-gray-800">Reset Categories</span>
        </label>

        <label class="flex items-center gap-3 p-3 border rounded hover:bg-gray-50 cursor-pointer">
            <input type="checkbox" v-model="resetOptions.resetReceiptsProductsAndSellers" class="w-5 h-5 text-orange-600 rounded" />
            <span class="text-gray-800">Reset Receipts, Products & Sellers</span>
        </label>

        <div class="flex gap-4 pt-4 mt-6 border-t">
            <button type="submit" :disabled="isResetting" class="bg-orange-600 text-white px-4 py-2 rounded hover:bg-orange-700 disabled:opacity-50">
                {{ isResetting ? 'Processing...' : 'Execute Reset' }}
            </button>
            <NuxtLink :to="`/groups/${groupId}`" class="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded">Cancel</NuxtLink>
        </div>
    </form>
  </div>
</template>