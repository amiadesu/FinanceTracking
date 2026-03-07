<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from '#imports';
import { receiptService } from '~/services/receiptService';
import { categoryService } from '~/services/categoryService';
import type { CreateReceiptDto } from '~/services/receiptService';
import type { CategoryDto } from '~/services/categoryService';
import CategoryPicker from '~/components/CategoryPicker.vue';
import { sellerService } from '~/services/sellerService';
import type { SellerDto } from '~/services/sellerService';
import SellerPicker from '~/components/SellerPicker.vue';
import type { FormSubmitEvent } from '@nuxt/ui';
import * as v from 'valibot';
import { receiptSchema } from '~/schemas/schemas';
import { useFormValidation } from '~/composables/useFormValidation';
import FormGlobalErrors from '~/components/FormGlobalErrors.vue';

type Schema = v.InferOutput<typeof receiptSchema>;

interface FormProduct {
  _uid: string;
  name: string;
  price: number;
  quantity: number;
  categoryIds: Set<number>;
}

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);

const categories = ref<CategoryDto[]>([]);
const sellers = ref<SellerDto[]>([]);

const loading = ref(false);
const error = ref<string | null>(null);

const xmlFileInput = ref<HTMLInputElement | null>(null);
const isUploading = ref(false);
const isSubmitting = ref(false);

function generateUid() {
  return Math.random().toString(36).substring(2) + Date.now().toString(36);
}

const newReceipt = reactive<{
  paymentDate: string;
  sellerId: string;
  products: FormProduct[];
}>({
  paymentDate: new Date().toISOString().split('T')[0]!,
  sellerId: "",
  products: [{
    _uid: generateUid(),
    name: '',
    price: 0,
    quantity: 1,
    categoryIds: new Set()
  }]
});

const { isFormValid, unmappedErrors, touch } = useFormValidation(receiptSchema, newReceipt);

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const [categoriesResponse, sellersResponse] = await Promise.all([
      categoryService.getCategories(groupId),
      sellerService.getSellers(groupId)
    ]);
    categories.value = categoriesResponse.categories;
    sellers.value = sellersResponse.sellers;
  } catch (err: any) {
    error.value = err.message || 'Failed to load data';
  } finally {
    loading.value = false;
  }
}

function addProduct() {
  newReceipt.products.unshift({ 
    _uid: generateUid(),
    name: '',
    price: 0,
    quantity: 1,
    categoryIds: new Set()
  });
}

function removeProduct(uid: string) {
  if (newReceipt.products.length <= 1) {
    alert('A receipt must have at least one product.');
    return;
  }
  newReceipt.products = newReceipt.products.filter(p => p._uid !== uid);
}

function toggleProductCategory(product: FormProduct, categoryId: number) {
  if (product.categoryIds.has(categoryId)) {
    product.categoryIds.delete(categoryId);
  } else {
    product.categoryIds.add(categoryId);
  }
  touch();
}

async function createReceipt(event: FormSubmitEvent<Schema>) {
  isSubmitting.value = true;
  
  try {
    const receiptToSend: CreateReceiptDto = {
      paymentDate: new Date(event.data.paymentDate).toISOString(),
      sellerId: event.data.sellerId,
      products: event.data.products.map(p => ({
        name: p.name,
        price: p.price,
        quantity: p.quantity,
        categories: Array.from(p.categoryIds)
          .map(id => categories.value.find(c => c.id === id)?.name)
          .filter((name): name is string => !!name)
      }))
    };
    
    const createdReceipt = await receiptService.createReceipt(groupId, receiptToSend);
    router.push(`/groups/${groupId}/receipts/${createdReceipt.id}`);
  } catch (err: any) {
    alert(err.message || 'Error creating receipt');
  } finally {
    isSubmitting.value = false;
  }
}

async function handleXmlUpload(event: Event) {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;

  isUploading.value = true;
  try {
    const parsedData = await receiptService.uploadReceiptXml(groupId, file);

    if (!parsedData || typeof parsedData !== 'object') {
      throw new Error('Unexpected response format from server');
    }

    if (parsedData.paymentDate) {
      newReceipt.paymentDate = parsedData.paymentDate.split('T')[0]!;
    }
    
    if (parsedData.sellerId) {
      newReceipt.sellerId = parsedData.sellerId;
    }

    if (parsedData.products && Array.isArray(parsedData.products)) {
      newReceipt.products = parsedData.products.map((p: any) => {
        const categoryIds = new Set<number>();
        if (p.categories) {
          p.categories.forEach((catName: string) => {
            const found = categories.value.find(c => c.name.toLowerCase() === catName.toLowerCase());
            if (found) categoryIds.add(found.id);
          });
        }

        return {
          _uid: generateUid(),
          name: p.name || '',
          price: p.price || 0,
          quantity: p.quantity || 1,
          categoryIds
        };
      });
    }
    alert(`XML parsed successfully! Loaded ${newReceipt.products.length} products.`);
  } catch (err: any) {
    alert(err.message || 'Failed to parse XML');
  } finally {
    isUploading.value = false;
    if (xmlFileInput.value) xmlFileInput.value.value = '';
    touch();
  }
}

function triggerXmlSelect() {
  xmlFileInput.value?.click();
}

onMounted(() => loadData());
</script>

<template>
    <div class="w-full lg:max-w-6xl md:max-w-3xl sm:max-w-xl mx-auto p-4 mt-2">
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
            <div class="flex items-center gap-3">
                <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Create New Receipt</h1>
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
            Loading data...
        </div>
        
        <UAlert 
            v-else-if="error" 
            color="error" 
            variant="soft" 
            icon="i-heroicons-exclamation-triangle"
            :title="error" 
            class="mb-4" 
        />

        <UCard v-if="!loading && !error" :ui="{ body: 'p-4 sm:p-6 flex-1 flex flex-col min-h-0' }" class="shadow-sm flex flex-col h-200 max-h-[70vh] w-full max-w-full">
            <template #header>
                <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                    <div>
                        <h2 class="text-base font-semibold text-gray-900 dark:text-white">Receipt Details</h2>
                        <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">Manually enter a receipt or import data from an XML file.</p>
                    </div>
                    <div>
                        <input type="file" ref="xmlFileInput" class="hidden" accept=".xml" @change="handleXmlUpload" />
                        <UButton 
                            @click="triggerXmlSelect" 
                            :loading="isUploading"
                            color="neutral" 
                            variant="solid" 
                            icon="i-heroicons-arrow-up-tray"
                        >
                            Import XML
                        </UButton>
                    </div>
                </div>
            </template>

            <UForm :schema="receiptSchema" :state="newReceipt" class="flex flex-col flex-1 min-h-0 justify-between" @submit="createReceipt">
                
                <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 lg:gap-8 flex-1 min-h-0 mb-6">
                    
                    <div class="lg:col-span-4 flex flex-col gap-6 shrink-0 lg:overflow-y-auto pr-1">
                        <UFormField label="Payment Date" name="paymentDate" required>
                            <UInput type="date" v-model="newReceipt.paymentDate" @change="touch" class="w-full" />
                        </UFormField>

                        <UFormField label="Seller" name="sellerId" required>
                            <SellerPicker :sellers="sellers" v-model="newReceipt.sellerId" @change="touch" />
                        </UFormField>
                    </div>

                    <div class="lg:col-span-8 flex flex-col flex-1 min-h-0 border-t lg:border-t-0 lg:border-l border-gray-100 dark:border-gray-800 pt-6 lg:pt-0 lg:pl-8">
                        <div class="flex justify-between items-center mb-4 shrink-0">
                            <h3 class="font-semibold text-gray-900 dark:text-white">Products ({{ newReceipt.products.length }})</h3>
                            <UButton type="button" @click="addProduct" color="primary" variant="soft" icon="i-heroicons-plus" size="sm">
                                Add Product
                            </UButton>
                        </div>
                    
                        <div class="space-y-4 flex-1 min-h-0 overflow-y-auto pr-2 pb-2">
                            <div 
                                v-for="(product, index) in newReceipt.products" 
                                :key="product._uid" 
                                class="relative p-4 rounded-lg bg-gray-50 dark:bg-gray-900/50 border border-gray-100 dark:border-gray-800 shrink-0"
                            >
                                <UButton 
                                    v-if="newReceipt.products.length > 1" 
                                    @click="removeProduct(product._uid)" 
                                    color="error" 
                                    variant="ghost" 
                                    icon="i-heroicons-trash" 
                                    size="sm" 
                                    class="absolute top-3 right-3" 
                                />
                            
                                <div class="grid grid-cols-1 md:grid-cols-12 gap-4 mt-2 pr-8 md:pr-0">
                                    <UFormField label="Product Name" :name="`products.${index}.name`" class="md:col-span-6" required>
                                        <UInput v-model="product.name" placeholder="e.g. Milk" class="w-full" @change="touch" />
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
                            {{ isSubmitting ? 'Creating...' : 'Create Receipt' }}
                        </UButton>
                        <UButton 
                            type="button" 
                            @click="router.push(`/groups/${groupId}/receipts`)" 
                            color="secondary" 
                            variant="outline" 
                            :disabled="isSubmitting"
                        >
                            Cancel
                        </UButton>
                    </div>
                </div>
            </UForm>
        </UCard>
    </div>
</template>