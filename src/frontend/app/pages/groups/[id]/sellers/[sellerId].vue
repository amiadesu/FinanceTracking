<script setup lang="ts">
import { ref, onMounted, reactive, h } from 'vue';
import { useRoute, useRouter } from '#imports';
import { useAppToast } from '~/composables/useAppToast';
import { sellerService } from '~/services/sellerService';
import { receiptService } from '~/services/receiptService';
import type { SellerDto, UpdateSellerDto } from '~/services/sellerService';
import type { ReceiptDto } from '~/services/receiptService';
import type { TableColumn, FormSubmitEvent } from '@nuxt/ui';
import * as v from 'valibot';
import { sellerSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import FormGlobalErrors from '~/components/FormGlobalErrors.vue';

const { showSuccess, showError, withConfirm } = useAppToast();
type Schema = v.InferOutput<typeof sellerSchema>;

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const sellerId = String(route.params.sellerId);

const seller = ref<SellerDto | null>(null);
const receipts = ref<ReceiptDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const isModalOpen = ref(false);
const loadingReceipts = ref(false);
const isSubmitting = ref(false);

const editDto = reactive<UpdateSellerDto>({
  name: '',
  description: null,
});

const { isFormValid, unmappedErrors, touch } = useFormValidation(sellerSchema, editDto);

const receiptColumns: TableColumn<ReceiptDto>[] = [
  { accessorKey: 'id', header: 'Receipt ID' },
  { 
    id: 'creator',
    header: 'Created By',
    cell: ({ row }) => h(
      'div', 
      { class: 'whitespace-normal break-words sm:break-all min-w-[120px] max-w-[250px] font-medium text-gray-900 dark:text-white' }, 
      row.original.createdByUserName || 'Unknown'
    )
  },
  { 
    accessorKey: 'paymentDate', 
    header: 'Date',
    cell: ({ row }) => new Date(row.original.paymentDate).toLocaleDateString()
  },
  { 
    accessorKey: 'totalAmount', 
    header: 'Total',
    cell: ({ row }) => row.original.totalAmount.toFixed(2)
  },
  { id: 'actions', header: '' }
];

async function load() {
  loading.value = true;
  error.value = null;
  try {
    seller.value = await sellerService.getSeller(groupId, sellerId);
    if (seller.value) {
      editDto.name = seller.value.name;
      editDto.description = seller.value.description;
    }
  } catch (err: any) {
    error.value = err.message || 'Failed to load seller details';
  } finally {
    loading.value = false;
  }
}

async function openReceiptsModal() {
  isModalOpen.value = true;
  if (receipts.value.length > 0) return;
  
  loadingReceipts.value = true;
  try {
    receipts.value = await receiptService.getFilteredReceipts(groupId, { sellerId });
  } catch (err: any) {
    showError(err.message || 'Error loading associated receipts');
  } finally {
    loadingReceipts.value = false;
  }
}

async function save(event: FormSubmitEvent<Schema>) {
  isSubmitting.value = true;
  try {
    const updated = await sellerService.updateSeller(groupId, sellerId, event.data);
    seller.value = updated;
    showSuccess('Seller updated successfully.');
  } catch (err: any) {
    showError(err.message || 'Error updating seller');
  } finally {
    isSubmitting.value = false;
  }
}

function remove() {
  withConfirm({
    title: 'Delete Seller',
    description: 'Are you sure you want to delete this seller? Ensure no receipts are using it.',
    toastColor: 'error',
    confirmLabel: 'Delete',
    actionColor: 'error',
    successMsg: 'Seller deleted successfully.',
    errorMsg: 'Error deleting seller. It might still be in use.',
    onConfirm: async () => {
      isSubmitting.value = true;
      try {
        await sellerService.deleteSeller(groupId, sellerId);
        router.push(`/groups/${groupId}/sellers`);
      } finally {
        isSubmitting.value = false;
      }
    }
  });
}

onMounted(() => load());
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Manage Seller</h1>
        <UBadge color="neutral" variant="subtle" size="md">#{{ sellerId }}</UBadge>
      </div>
      <UButton 
        :to="`/groups/${groupId}/sellers`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Sellers
      </UButton>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2 mb-4">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading seller data...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <UCard v-else-if="seller" class="shadow-sm w-full max-w-3xl mx-auto mt-8">
      <UForm :schema="sellerSchema" :state="editDto" class="flex flex-col gap-6" @submit="save">
        <div class="flex flex-col gap-4">
          <UFormField label="Seller Name" name="name">
            <UInput 
              v-model="editDto.name!" 
              placeholder="e.g. Walmart" 
              class="w-full"
            />
          </UFormField>
          
          <UFormField label="Description" name="description">
            <UTextarea 
              v-model="editDto.description!" 
              :rows="3" 
              class="w-full"
            />
          </UFormField>

          <FormGlobalErrors :errors="unmappedErrors" />
        </div>

        <USeparator />

        <div class="flex flex-wrap items-center gap-3 justify-between">
          <div class="flex gap-3">
            <UButton 
              type="submit" 
              color="primary"
              :loading="isSubmitting"
              :disabled="isSubmitting || !isFormValid"
            >
              {{ isSubmitting ? 'Saving...' : 'Update Seller' }}
            </UButton>

            <UButton 
              type="button"
              @click="openReceiptsModal" 
              color="secondary" 
              variant="outline"
              icon="i-heroicons-receipt-percent"
            >
              View Associated Receipts
            </UButton>
          </div>

          <UButton 
            type="button"
            @click="remove" 
            color="error" 
            variant="outline"
            :disabled="isSubmitting"
          >
            Delete Seller
          </UButton>
        </div>
      </UForm>
    </UCard>

    <UModal v-model:open="isModalOpen" class="w-full max-w-3xl">
      <template #content>
        <UCard :ui="{ body: 'p-0 sm:p-0 flex-1 flex flex-col min-h-0' }" class="flex flex-col w-full lg:h-100">
          <template #header>
            <div class="flex items-center justify-between">
              <h3 class="text-base font-semibold leading-6 text-gray-900 dark:text-white">
                Associated Receipts
              </h3>
              <UButton 
                color="neutral" 
                variant="ghost" 
                icon="i-heroicons-x-mark-20-solid" 
                class="-my-1" 
                @click="isModalOpen = false" 
              />
            </div>
          </template>
          
          <UTable
            sticky
            :data="receipts" 
            :columns="receiptColumns" 
            :loading="loadingReceipts"
            class="w-full flex-1 min-h-0 overflow-y-auto"
          >
            <template #actions-cell="{ row }">
              <div class="text-right">
                <UButton 
                  :to="`/groups/${groupId}/receipts/${row.original.id}`" 
                  color="primary" 
                  variant="outline"
                  size="sm"
                  icon="i-heroicons-eye"
                >
                  View
                </UButton>
              </div>
            </template>

            <template #empty>
              <div class="flex flex-col items-center justify-center py-6 text-center">
                <span class="text-sm text-gray-500 dark:text-gray-400">
                  No receipts currently use this seller. Safe to delete.
                </span>
              </div>
            </template>
          </UTable>
        </UCard>
      </template>
    </UModal>
  </div>
</template>