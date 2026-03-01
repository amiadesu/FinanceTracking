<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from '#imports';
import { groupMemberService, type GroupMemberDto } from '~/services/groupMemberService';
import { useConfigStore } from '~/stores/useConfigStore';
import { useLimitDisplay } from '~/composables/useLimitDisplay';

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const configStore = useConfigStore();

const members = ref<GroupMemberDto[]>([]);
const currentCount = ref(0);
const maxAllowed = ref(0);

const myRole = ref<number | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const ownerRoleId = computed(() => configStore.config?.groupRoles['Owner']);
const adminRoleId = computed(() => configStore.config?.groupRoles['Admin']);

const canManage = computed(() => myRole.value === ownerRoleId.value || myRole.value === adminRoleId.value);

const limitDisplay = useLimitDisplay(currentCount, maxAllowed);

function getRoleName(roleValue: number) {
  if (!configStore.config) return 'Unknown';
  
  const roleEntry = Object.entries(configStore.config.groupRoles).find(([_, val]) => val === roleValue);
  return roleEntry ? roleEntry[0] : 'Unknown';
}

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    await configStore.fetchConfig();

    const [membersResponse, roleData] = await Promise.all([
      groupMemberService.getMembers(groupId),
      groupMemberService.getMyRole(groupId)
    ]);
    
    currentCount.value = membersResponse.currentCount;
    maxAllowed.value = membersResponse.maxAllowed;
    members.value = membersResponse.groupMembers;
    
    myRole.value = roleData.role;
  } catch (err: any) {
    error.value = err.message || 'Failed to load group members';
  } finally {
    loading.value = false;
  }
}

function goToMember(userId: string) {
  router.push(`/groups/${groupId}/members/${userId}`);
}

onMounted(() => loadData());
</script>

<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Group Members</h1>
      <p class="text-sm text-gray-600 mt-1" v-if="!loading">
        Capacity: <span class="font-mono bg-gray-100 px-1 rounded">{{ limitDisplay }}</span>
      </p>
      <button @click="() => router.back()" class="text-blue-600 underline text-sm">
        Back
      </button>
    </div>

    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <table v-if="!loading && members.length > 0" class="w-full mt-4 table-auto border-collapse">
      <thead>
        <tr class="bg-gray-100">
          <th class="border px-2 py-1 text-left">Name</th>
          <th class="border px-2 py-1 text-left">Role</th>
          <th class="border px-2 py-1 text-left">Joined</th>
          <th class="border px-2 py-1 text-center">Status</th>
          <th v-if="canManage" class="border px-2 py-1">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="m in members" :key="m.userId">
          <td class="border px-2 py-1">{{ m.userName }}</td>
          <td class="border px-2 py-1 font-semibold">{{ getRoleName(m.role) }}</td>
          <td class="border px-2 py-1 text-sm text-gray-600">{{ new Date(m.joinedDate).toLocaleDateString() }}</td>
          <td class="border px-2 py-1 text-center text-sm">
            <span :class="m.active ? 'text-green-600' : 'text-red-600'">{{ m.active ? 'Active' : 'Inactive' }}</span>
          </td>
          <td v-if="canManage" class="border px-2 py-1 text-center">
            <button @click="goToMember(m.userId)" class="text-blue-600 underline">Manage</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>