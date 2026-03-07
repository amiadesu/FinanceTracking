<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { useRoute, useRouter } from '#imports';
import * as v from 'valibot';
import { memberRoleSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import { groupMemberService } from '~/services/groupMemberService';
import type { GroupMemberDto } from '~/services/groupMemberService';
import { useConfigStore } from '~/stores/useConfigStore';
import type { FormSubmitEvent } from '@nuxt/ui';
import FormGlobalErrors from "~/components/FormGlobalErrors.vue";

type Schema = v.InferOutput<typeof memberRoleSchema>;

const configStore = useConfigStore();
const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const userId = route.params.userId as string;

const member = ref<GroupMemberDto | null>(null);
const myRole = ref<number | null>(null);

const loading = ref(false);
const isSubmitting = ref(false);
const error = ref<string | null>(null);

const editDto = reactive({
  role: undefined as { label: string, value: number } | undefined,
});

const ownerRoleId = computed(() => configStore.config?.groupRoles['Owner']);
const isOwner = computed(() => myRole.value === ownerRoleId.value);

const assignableRoles = computed(() => {
  const roles = { ...configStore.config?.groupRoles };
  delete roles['Owner'];
  return Object.entries(roles).map(([label, value]) => ({ 
    label, 
    value 
  }));
});

const { isFormValid, unmappedErrors, touch } = useFormValidation(memberRoleSchema, editDto);

async function load() {
  loading.value = true;
  error.value = null;
  try {
    await configStore.fetchConfig();

    const [memberData, roleData] = await Promise.all([
      groupMemberService.getMember(groupId, userId),
      groupMemberService.getMyRole(groupId)
    ]);
    
    member.value = memberData;
    myRole.value = roleData.role;

    let matchedRole = assignableRoles.value.find(r => r.value === memberData.role);
    if (!matchedRole && memberData.role === ownerRoleId.value) {
      matchedRole = { label: 'Owner', value: memberData.role };
    }
    
    editDto.role = matchedRole || { label: 'Unknown', value: memberData.role };

  } catch (err: any) {
    error.value = err.message || 'Failed to load member details';
  } finally {
    loading.value = false;
  }
}

async function save(event: FormSubmitEvent<Schema>) {
  if (!editDto.role) return;

  if (editDto.role.value === ownerRoleId.value) {
    alert('Please use the Transfer Ownership button to make this user an owner.');
    return;
  }
  
  isSubmitting.value = true;
  try {
    const updated = await groupMemberService.updateRole(groupId, userId, { role: editDto.role.value });
    member.value = updated;
    alert('Role updated successfully.');
  } catch (err: any) {
    alert(err.message || 'Error updating role');
  } finally {
    isSubmitting.value = false;
  }
}

async function transferOwnership() {
  if (!confirm('Are you sure you want to transfer ownership to this user? You will become an Admin.')) return;
  isSubmitting.value = true;
  try {
    await groupMemberService.transferOwnership(groupId, userId);
    router.push(`/groups/${groupId}/members`);
  } catch (err: any) {
    alert(err.message || 'Error transferring ownership');
  } finally {
    isSubmitting.value = false;
  }
}

async function remove() {
  if (!confirm(`Are you sure you want to remove ${member.value?.userName} from the group?`)) return;
  isSubmitting.value = true;
  try {
    await groupMemberService.removeMember(groupId, userId);
    router.push(`/groups/${groupId}/members`);
  } catch (err: any) {
    alert(err.message || 'Error removing member. Check your permission level.');
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(load);
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Manage Member</h1>
      <UButton 
        :to="`/groups/${groupId}/members`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Members
      </UButton>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2 mb-4">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading member data...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <UCard v-else-if="member" class="shadow-sm w-full max-w-3xl mx-auto mt-8">
      <div class="flex flex-col gap-6">
        <div class="text-center">
          <span class="text-sm font-medium text-gray-500 dark:text-gray-400">User</span>
          <div class="flex items-center justify-center gap-3 mt-2">
            <p class="text-xl font-semibold text-gray-900 dark:text-white">{{ member.userName }}</p>
            <UBadge v-if="!member.active" color="error" variant="soft" size="sm">Inactive</UBadge>
          </div>
        </div>
        
        <UForm 
          :state="editDto" 
          class="space-y-6" 
          @submit="save"
        >
          <div class="flex flex-col gap-4">
            <UFormField label="Change Role" name="role">
              <USelectMenu 
                v-model="editDto.role" 
                :items="assignableRoles" 
                :disabled="member.role === ownerRoleId || isSubmitting"
                class="flex items-center gap-3 mt-1 w-full"
              />
              <template #help>
                <span v-if="member.role === ownerRoleId" class="text-xs text-gray-500 mt-1">
                  Owner roles cannot be changed via dropdown.
                </span>
              </template>
            </UFormField>

            <FormGlobalErrors :errors="unmappedErrors" />
          </div>

          <USeparator />

          <div class="flex flex-wrap items-center gap-3 justify-between">
            <div class="flex gap-3">
              <UButton 
                v-if="member.role !== ownerRoleId" 
                type="submit" 
                color="primary"
                :loading="isSubmitting"
                :disabled="isSubmitting || !isFormValid"
              >
                {{ isSubmitting ? 'Saving...' : 'Save Role' }}
              </UButton>
              
              <UButton 
                v-if="isOwner && member.role !== ownerRoleId" 
                type="button"
                @click="transferOwnership" 
                color="warning" 
                variant="subtle"
                :disabled="isSubmitting"
              >
                Transfer Ownership
              </UButton>
            </div>

            <UButton 
              v-if="member.role !== ownerRoleId" 
              type="button"
              @click="remove" 
              color="error" 
              variant="outline"
              :disabled="isSubmitting"
            >
              Remove from Group
            </UButton>
          </div>
        </UForm>
      </div>
    </UCard>
  </div>
</template>