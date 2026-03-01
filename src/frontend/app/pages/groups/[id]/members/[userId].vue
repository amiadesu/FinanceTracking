<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from '#imports';
import { groupMemberService } from '~/services/groupMemberService';
import { GroupRole } from '~/constants/roles';
import type { GroupMemberDto } from '~/services/groupMemberService';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const userId = route.params.userId as string;

const member = ref<GroupMemberDto | null>(null);
const myRole = ref<GroupRole | null>(null);
const selectedRole = ref<GroupRole>(GroupRole.Member);

const loading = ref(false);
const error = ref<string | null>(null);

const isOwner = computed(() => myRole.value === GroupRole.Owner);

async function load() {
  loading.value = true;
  error.value = null;
  try {
    const [memberData, roleData] = await Promise.all([
      groupMemberService.getMember(groupId, userId),
      groupMemberService.getMyRole(groupId)
    ]);
    member.value = memberData;
    myRole.value = roleData.role;
    selectedRole.value = memberData.role;
  } catch (err: any) {
    error.value = err.message || 'Failed to load member details';
  } finally {
    loading.value = false;
  }
}

async function updateRole() {
  if (selectedRole.value === GroupRole.Owner) {
    alert('Please use the Transfer Ownership button to make this user an owner.');
    return;
  }
  
  try {
    const updated = await groupMemberService.updateRole(groupId, userId, { role: selectedRole.value });
    member.value = updated;
    alert('Role updated successfully.');
  } catch (err: any) {
    alert(err.message || 'Error updating role');
  }
}

async function transferOwnership() {
  if (!confirm('Are you sure you want to transfer ownership to this user? You will become an Admin.')) return;
  try {
    await groupMemberService.transferOwnership(groupId, userId);
    router.push(`/groups/${groupId}/members`);
  } catch (err: any) {
    alert(err.message || 'Error transferring ownership');
  }
}

async function remove() {
  if (!confirm(`Are you sure you want to remove ${member.value?.userName} from the group?`)) return;
  try {
    await groupMemberService.removeMember(groupId, userId);
    router.push(`/groups/${groupId}/members`);
  } catch (err: any) {
    alert(err.message || 'Error removing member. Check your permission level.');
  }
}

onMounted(() => load());
</script>

<template>
  <div class="p-4 max-w-3xl">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Manage Member</h1>
      <button @click="() => router.back()" class="text-blue-600 underline text-sm">
        ← Back to Members
      </button>
    </div>

    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <div v-if="member && !loading" class="space-y-6">
      
      <div class="border rounded p-4 bg-gray-50 flex flex-col gap-4">
        <div>
          <span class="font-semibold text-gray-700 text-sm block">User</span>
          <p class="text-lg">{{ member.userName }} <span v-if="!member.active" class="text-red-500 text-sm">(Inactive)</span></p>
        </div>
        
        <label class="flex flex-col">
          <span class="font-semibold text-gray-700 text-sm">Change Role</span>
          <select v-model="selectedRole" class="border p-2 mt-1 rounded bg-white max-w-xs" :disabled="member.role === GroupRole.Owner">
            <option :value="GroupRole.Admin">Admin</option>
            <option :value="GroupRole.Member">Member</option>
          </select>
          <span v-if="member.role === GroupRole.Owner" class="text-sm text-gray-500 mt-1">
            Owner roles cannot be changed via dropdown.
          </span>
        </label>

        <div class="flex gap-2 pt-2 border-t mt-2">
          <button v-if="member.role !== GroupRole.Owner" @click="updateRole" class="bg-blue-600 text-white px-4 py-2 rounded font-semibold text-sm">
            Save Role
          </button>
          
          <button v-if="isOwner && member.role !== GroupRole.Owner" @click="transferOwnership" class="bg-yellow-500 text-white px-4 py-2 rounded font-semibold text-sm">
            Transfer Ownership
          </button>

          <button v-if="member.role !== GroupRole.Owner" @click="remove" class="bg-red-600 text-white px-4 py-2 rounded font-semibold text-sm ml-auto">
            Remove from Group
          </button>
        </div>
      </div>
    </div>
  </div>
</template>