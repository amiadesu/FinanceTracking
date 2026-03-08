<script setup lang="ts">
import { ref, onMounted, reactive, h } from 'vue';
import { useRoute, useRouter } from '#imports';
import { useAppToast } from '~/composables/useAppToast';
import { receiptService } from '~/services/receiptService';
import { categoryService } from '~/services/categoryService';
import type { ReceiptDto, UpdateReceiptDto } from '~/services/receiptService';
import type { CategoryDto } from '~/services/categoryService';
import CategoryPicker from '~/components/CategoryPicker.vue';
import { sellerService } from '~/services/sellerService';
import type { SellerDto } from '~/services/sellerService';
import SellerPicker from '~/components/SellerPicker.vue';
import type { TableColumn, FormSubmitEvent } from '@nuxt/ui';
import * as v from 'valibot';
import { receiptSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import FormGlobalErrors from '~/components/FormGlobalErrors.vue';

const { showSuccess, showError, withConfirm } = useAppToast();
type Schema = v.InferOutput<typeof receiptSchema>;

interface FormProduct {
  _uid: string;
  id?: number;
  name: string;
  price: number;
  quantity: number;
  categoryIds: Set<number>;
}

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const receiptId = Number(route.params.receiptId);

const receipt = ref<ReceiptDto | null>(null);
const categories = ref<CategoryDto[]>([]);
const sellers = ref<SellerDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const editMode = ref(false);
const isSubmitting = ref(false);

const editDto = reactive<{
  paymentDate: string;
  sellerId: string;
  products: FormProduct[];
}>({
  paymentDate: '',
  sellerId: '',
  products: [],
});

const { isFormValid, unmappedErrors, touch } = useFormValidation(receiptSchema, editDto);

const productColumns: TableColumn<any>[] = [
  { 
    id: 'name', 
    header: 'Name',
    cell: ({ row }) => h(
      'div', 
      { class: 'whitespace-normal break-words sm:break-all min-w-[120px] max-w-[250px] font-medium' }, 
      row.original.name
    )
  },
  { 
    accessorKey: 'price', 
    header: 'Price',
    cell: ({ row }) => row.original.price.toFixed(2)
  },
  { accessorKey: 'quantity', header: 'Qty' },
  { 
    id: 'subtotal', 
    header: 'Subtotal',
    cell: ({ row }) => (row.original.price * row.original.quantity).toFixed(2)
  },
  { id: 'categories', header: 'Categories' }
];

function generateUid() {
  return Math.random().toString(36).substring(2) + Date.now().toString(36);
}

async function load() {
  loading.value = true;
  error.value = null;
  try {
    const [receiptData, categoriesResponse, sellersResponse] = await Promise.all([
      receiptService.getReceipt(groupId, receiptId),
      categoryService.getCategories(groupId),
      sellerService.getSellers(groupId)
    ]);
    
    receipt.value = receiptData;
    categories.value = categoriesResponse.categories;
    sellers.value = sellersResponse.sellers;
  } catch (err: any) {
    error.value = err.message || 'Failed to load data';
  } finally {
    loading.value = false;
  }
}

function getCategoryColor(catName: string) {
  const cat = categories.value.find(c => c.name === catName);
  return cat?.colorHex || '#9ca3af';
}

function startEdit() {
  if (!receipt.value) return;
  editMode.value = true;
  
  editDto.paymentDate = receipt.value.paymentDate 
    ? receipt.value.paymentDate.split('T')[0]!
    : new Date().toISOString().split('T')[0]!;
    
  editDto.sellerId = receipt.value.sellerId ?? '';
  
  editDto.products = receipt.value.products.map(p => {
    const catIds = new Set<number>();
    p.categories.forEach(catName => {
      const catId = categories.value.find(c => c.name === catName)?.id;
      if (catId !== undefined) catIds.add(catId);
    });

    return {
      _uid: generateUid(),
      id: p.id,
      name: p.name,
      price: p.price,
      quantity: p.quantity,
      categoryIds: catIds,
    };
  });
}

function addProduct() {
  editDto.products.unshift({ 
    _uid: generateUid(),
    name: '',
    price: 0,
    quantity: 1,
    categoryIds: new Set(),
  });
}

function removeProduct(uid: string) {
  if (editDto.products.length <= 1) {
    showError('A receipt must have at least one product.');
    return;
  }
  editDto.products = editDto.products.filter(p => p._uid !== uid);
}

function toggleProductCategory(product: FormProduct, categoryId: number) {
  if (product.categoryIds.has(categoryId)) {
    product.categoryIds.delete(categoryId);
  } else {
    product.categoryIds.add(categoryId);
  }
  touch();
}

async function save(event: FormSubmitEvent<Schema>) {
  if (!receipt.value) return;
  isSubmitting.value = true;
  
  try {
    const receiptToSend: UpdateReceiptDto = {
      paymentDate: event.data.paymentDate,
      sellerId: event.data.sellerId,
      products: event.data.products.map(p => ({
        id: p.id,
        name: p.name,
        price: p.price,
        quantity: p.quantity,
        categories: Array.from(p.categoryIds)
          .map(id => categories.value.find(c => c.id === id)?.name)
          .filter((name): name is string => !!name)
      }))
    };
    
    const updated = await receiptService.updateReceipt(groupId, receiptId, receiptToSend);
    receipt.value = updated;
    editMode.value = false;
    showSuccess('Receipt updated successfully.');
    await load(); 
  } catch (err: any) {
    showError(err.message || 'Error updating receipt');
  } finally {
    isSubmitting.value = false;
  }
}

function remove() {
  withConfirm({
    title: 'Delete Receipt',
    description: 'Are you sure you want to delete this receipt?',
    toastColor: 'error',
    confirmLabel: 'Delete',
    actionColor: 'error',
    successMsg: 'Receipt deleted successfully.',
    onConfirm: async () => {
      isSubmitting.value = true;
      try {
        await receiptService.deleteReceipt(groupId, receiptId);
        router.push(`/groups/${groupId}/receipts`);
      } finally {
        isSubmitting.value = false;
      }
    }
  });
}

function cancelEdit() {
  editMode.value = false;
}

onMounted(() => load());
</script>

<template>
  <div class="w-full lg:max-w-6xl md:max-w-3xl sm:max-w-xl mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Manage Receipt</h1>
        <UBadge color="neutral" variant="subtle" size="md">#{{ receiptId }}</UBadge>
      </div>
      <UButton 
        :to="`/groups/${groupId}/receipts`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Receipts
      </UButton>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2 mb-4">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading receipt data...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <UCard v-else-if="receipt && !editMode" :ui="{ body: 'p-4 sm:p-6 flex-1 flex flex-col min-h-0' }" class="shadow-sm flex flex-col h-200 max-h-[70vh] w-full max-w-full">
      <div class="flex flex-col flex-1 min-h-0 justify-between">
        
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 lg:gap-8 flex-1 min-h-0 mb-6">
          
          <div class="lg:col-span-4 flex flex-col gap-6 shrink-0 overflow-y-auto pr-2 pb-2">
            <div>
              <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Created By</span>
              <p class="text-lg font-semibold text-gray-900 dark:text-white mt-1 wrap-break-words sm:break-all max-w-full">
                {{ receipt.createdByUserName || 'Unknown' }}
              </p>
            </div>
            <div>
              <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Payment Date</span>
              <p class="text-lg font-semibold text-gray-900 dark:text-white mt-1">
                {{ new Date(receipt.paymentDate).toLocaleDateString() }}
              </p>
            </div>
            <div>
              <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Total Amount</span>
              <p class="text-lg font-semibold text-gray-900 dark:text-white mt-1">
                {{ receipt.totalAmount.toFixed(2) }}
              </p>
            </div>
            <div>
              <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Seller</span>
              <p class="text-lg font-semibold text-gray-900 dark:text-white mt-1 wrap-break-words sm:break-all max-w-full">
                {{ receipt.sellerName ? receipt.sellerName : (receipt.sellerId ?? 'Not specified') }}
              </p>
            </div>
            <div>
              <span class="text-sm font-medium text-gray-500 dark:text-gray-400">Created At</span>
              <p class="text-lg font-semibold text-gray-900 dark:text-white mt-1">
                {{ new Date(receipt.createdDate).toLocaleDateString() }}
              </p>
            </div>
          </div>

          <div class="lg:col-span-8 flex flex-col flex-1 min-h-0 border-t lg:border-t-0 lg:border-l border-gray-100 dark:border-gray-800 pt-6 lg:pt-0 lg:pl-8">
            <div class="border rounded-md dark:border-gray-800 overflow-hidden flex flex-col flex-1 min-h-0">
              <div class="p-4 bg-gray-50 dark:bg-gray-900/50 border-b dark:border-gray-800 shrink-0">
                <h2 class="text-sm font-semibold text-gray-900 dark:text-white">
                  Products ({{ receipt.products.length }})
                </h2>
              </div>
              <UTable 
                sticky
                :data="receipt.products" 
                :columns="productColumns" 
                class="w-full flex-1 min-h-0 overflow-y-auto"
              >
                <template #categories-cell="{ row }">
                  <div class="flex flex-wrap gap-1.5 max-w-50 sm:max-w-75">
                    <div 
                      v-for="cat in row.original.categories" 
                      :key="cat" 
                      class="inline-flex items-center gap-1.5 px-2 py-1 rounded-md bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 max-w-full"
                      :title="cat"
                    >
                      <span 
                        class="w-2.5 h-2.5 rounded-full shrink-0 shadow-sm" 
                        :style="{ backgroundColor: getCategoryColor(cat) }"
                      ></span>
                      <span class="text-xs font-medium text-gray-700 dark:text-gray-300 truncate">{{ cat }}</span>
                    </div>
                    <span v-if="row.original.categories.length === 0" class="text-gray-400 text-xs italic">-</span>
                  </div>
                </template>
                <template #empty>
                  <div class="flex flex-col items-center justify-center py-6">
                    <span class="text-sm text-gray-500">No products listed.</span>
                  </div>
                </template>
              </UTable>
            </div>
          </div>
        </div>

        <div class="flex flex-col gap-4 shrink-0">
          <USeparator />
          <div class="flex flex-wrap items-center gap-3 justify-between">
            <UButton @click="startEdit" color="primary" icon="i-heroicons-pencil">
              Edit Receipt
            </UButton>
            <UButton @click="remove" color="error" variant="outline" :disabled="isSubmitting">
              Delete Receipt
            </UButton>
          </div>
        </div>
      </div>
    </UCard>

    <UCard v-else-if="receipt && editMode" :ui="{ body: 'p-4 sm:p-6 flex-1 flex flex-col min-h-0' }" class="shadow-sm flex flex-col h-200 max-h-[70vh] w-full max-w-full">
      <template #header>
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white">Edit Receipt</h2>
      </template>

      <UForm :schema="receiptSchema" :state="editDto" class="flex flex-col flex-1 min-h-0 justify-between" @submit="save">
        
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 lg:gap-8 flex-1 min-h-0 mb-6">
          
          <div class="lg:col-span-4 flex flex-col gap-6 shrink-0 lg:overflow-y-auto pr-1">
            <UFormField label="Payment Date" name="paymentDate" required>
              <UInput type="date" v-model="editDto.paymentDate" @change="touch" class="w-full" />
            </UFormField>

            <UFormField label="Seller" name="sellerId" required>
              <SellerPicker :sellers="sellers" v-model="editDto.sellerId" @change="touch" />
            </UFormField>
          </div>

          <div class="lg:col-span-8 flex flex-col flex-1 min-h-0 border-t lg:border-t-0 lg:border-l border-gray-100 dark:border-gray-800 pt-6 lg:pt-0 lg:pl-8">
            <div class="flex justify-between items-center mb-4 shrink-0">
              <h3 class="font-semibold text-gray-900 dark:text-white">Products ({{ editDto.products.length }})</h3>
              <UButton type="button" @click="addProduct" color="primary" variant="soft" icon="i-heroicons-plus" size="sm">
                Add Product
              </UButton>
            </div>
            
            <div class="space-y-4 flex-1 min-h-0 overflow-y-auto pr-2 pb-2">
              <div 
                v-for="(product, index) in editDto.products" 
                :key="product._uid" 
                class="relative p-4 rounded-lg bg-gray-50 dark:bg-gray-900/50 border border-gray-100 dark:border-gray-800 shrink-0"
              >
                <UButton 
                  v-if="editDto.products.length > 1" 
                  @click="removeProduct(product._uid)" 
                  color="error" 
                  variant="ghost" 
                  icon="i-heroicons-trash" 
                  size="sm" 
                  class="absolute top-3 right-3" 
                />
                
                <div class="grid grid-cols-1 md:grid-cols-12 gap-4 mt-2 pr-8 md:pr-0">
                  <UFormField label="Product Name" :name="`products.${index}.name`" class="md:col-span-6" required>
                    <UInput v-model="product.name" class="w-full" @change="touch" />
                  </UFormField>
                  
                  <UFormField label="Price" :name="`products.${index}.price`" class="md:col-span-3" required>
                    <UInput type="number" v-model.number="product.price" step="0.01" class="w-full" @change="touch" />
                  </UFormField>
                  
                  <UFormField label="Qty" :name="`products.${index}.quantity`" class="md:col-span-3" required>
                    <UInput type="number" v-model.number="product.quantity" step="0.01" class="w-full" @change="touch" />
                  </UFormField>
                </div>

                <div class="mt-4">
                  <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Categories</label>
                  <CategoryPicker 
                    :categories="categories"
                    :selectedCategoryIds="product.categoryIds"
                    @toggle="(catId) => toggleProductCategory(product, catId)"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="flex flex-col gap-4 shrink-0">
          <FormGlobalErrors :errors="unmappedErrors" />
          <USeparator />
          <div class="flex flex-wrap items-center gap-3">
            <UButton 
              type="submit" 
              color="primary" 
              :loading="isSubmitting"
              :disabled="isSubmitting || !isFormValid"
            >
              {{ isSubmitting ? 'Saving...' : 'Save Changes' }}
            </UButton>
            <UButton type="button" @click="cancelEdit" color="secondary" variant="outline" :disabled="isSubmitting">
              Cancel
            </UButton>
          </div>
        </div>
      </UForm>
    </UCard>
  </div>
</template>