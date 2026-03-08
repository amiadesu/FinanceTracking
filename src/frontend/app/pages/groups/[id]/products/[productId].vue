<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from '#imports';
import { productService } from '~/services/productService';
import { receiptService } from '~/services/receiptService';
import { categoryService } from '~/services/categoryService';
import type { ProductDataDto } from '~/services/productService';
import type { ReceiptDto } from '~/services/receiptService';
import type { CategoryDto } from '~/services/categoryService';
import CategoryPicker from '~/components/CategoryPicker.vue';
import type { TableColumn, FormSubmitEvent } from '@nuxt/ui';
import * as v from 'valibot';
import { productDataSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import FormGlobalErrors from '~/components/FormGlobalErrors.vue';

type Schema = v.InferOutput<typeof productDataSchema>;

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const productId = Number(route.params.productId);

const product = ref<ProductDataDto | null>(null);
const categories = ref<CategoryDto[]>([]);
const receipts = ref<ReceiptDto[]>([]);

const loading = ref(false);
const error = ref<string | null>(null);

const isModalOpen = ref(false);
const loadingReceipts = ref(false);
const isSubmitting = ref(false);

const editDto = reactive<{
  name: string;
  description?: string;
  categoryIds: Set<number>;
}>({
  name: '',
  description: undefined,
  categoryIds: new Set<number>(),
});

const { isFormValid, unmappedErrors, touch } = useFormValidation(productDataSchema, editDto);

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
    const [productData, categoriesResponse] = await Promise.all([
      productService.getProduct(groupId, productId),
      categoryService.getCategories(groupId)
    ]);
    
    product.value = productData;
    categories.value = categoriesResponse.categories;
    
    if (product.value) {
      editDto.name = product.value.name ?? '';
      editDto.description = product.value.description ?? undefined;
      
      editDto.categoryIds.clear();
      product.value.categories.forEach(catName => {
        const found = categories.value.find(c => c.name === catName);
        if (found) editDto.categoryIds.add(found.id);
      });
    }
  } catch (err: any) {
    error.value = err.message || 'Failed to load product details';
  } finally {
    loading.value = false;
  }
}

async function fetchReceipts() {
  isModalOpen.value = true;
  if (receipts.value.length > 0) return;
  
  loadingReceipts.value = true;
  try {
    const receiptsDtos = await receiptService.getFilteredReceipts(groupId, { productDataId: productId });
    receipts.value = receiptsDtos;
  } catch (err: any) {
    alert(err.message || 'Error loading associated receipts');
  } finally {
    loadingReceipts.value = false;
  }
}

function toggleCategory(categoryId: number) {
  if (editDto.categoryIds.has(categoryId)) {
    editDto.categoryIds.delete(categoryId);
  } else {
    if (editDto.categoryIds.size >= 5) {
      alert('A product can have a maximum of 5 categories.');
      return;
    }
    editDto.categoryIds.add(categoryId);
  }
  touch();
}

async function save(event: FormSubmitEvent<Schema>) {
  if (editDto.categoryIds.size > 5) {
    alert('A product can have a maximum of 5 categories.');
    return;
  }

  isSubmitting.value = true;
  try {
    const updated = await productService.updateProduct(groupId, productId, {
      name: event.data.name,
      description: event.data.description || null,
      categoryIds: Array.from(editDto.categoryIds)
    });
    
    product.value = updated;
    
    editDto.name = updated.name ?? '';
    editDto.description = updated.description ?? undefined;

    alert('Product updated successfully.');
  } catch (err: any) {
    alert(err.message || 'Error updating product');
  } finally {
    isSubmitting.value = false;
  }
}

async function remove() {
  if (!confirm('Are you sure you want to delete this product? Ensure no receipts are currently using it.')) return;
  isSubmitting.value = true;
  try {
    await productService.deleteProduct(groupId, productId);
    router.push(`/groups/${groupId}/products`);
  } catch (err: any) {
    alert(err.message || 'Error deleting product. It might still be in use.');
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(() => load());
</script>

<template>
  <div class="w-full lg:max-w-4xl md:max-w-2xl sm:max-w-lg mx-auto p-4 mt-2">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
      <div class="flex items-center gap-3">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Manage Product</h1>
        <UBadge color="neutral" variant="subtle" size="md">#{{ productId }}</UBadge>
      </div>
      <UButton 
        :to="`/groups/${groupId}/products`" 
        color="secondary" 
        variant="outline" 
        icon="i-heroicons-arrow-left"
      >
        Back to Products
      </UButton>
    </div>

    <div v-if="loading" class="text-gray-500 animate-pulse flex items-center gap-2 mb-4">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin w-5 h-5" />
      Loading product data...
    </div>
    
    <UAlert 
      v-else-if="error" 
      color="error" 
      variant="soft" 
      icon="i-heroicons-exclamation-triangle"
      :title="error" 
      class="mb-4" 
    />

    <UCard v-else-if="product" class="shadow-sm w-full max-w-3xl mx-auto mt-8">
      <UForm :schema="productDataSchema" :state="editDto" class="flex flex-col gap-6" @submit="save">
        <div class="flex flex-col gap-4">
          <UFormField label="Product Name" name="name" required>
            <UInput 
              v-model="editDto.name" 
              placeholder="e.g. Organic Milk" 
              class="w-full"
            />
          </UFormField>
          
          <UFormField label="Description" name="description">
            <UTextarea 
              v-model="editDto.description" 
              :rows="3" 
              placeholder="Optional details..."
              class="w-full"
            />
          </UFormField>

          <div class="mt-2">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Categories (Max 5)
            </label>
            <CategoryPicker 
              :categories="categories"
              :selectedCategoryIds="editDto.categoryIds"
              @toggle="toggleCategory"
            />
            <p class="text-xs text-gray-500 mt-2">
              Note: Updating categories here will change them for all past receipts containing this product.
            </p>
          </div>

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
              {{ isSubmitting ? 'Saving...' : 'Update Product' }}
            </UButton>

            <UButton 
              type="button"
              @click="fetchReceipts" 
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
            Delete Product
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
              <div class="flex flex-col items-center justify-center py-6 text-center h-full">
                <span class="text-sm text-gray-500 dark:text-gray-400">
                  No receipts currently use this product. Safe to delete.
                </span>
              </div>
            </template>
          </UTable>
        </UCard>
      </template>
    </UModal>
  </div>
</template>