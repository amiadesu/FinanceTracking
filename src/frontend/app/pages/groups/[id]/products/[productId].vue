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

const route = useRoute();
const router = useRouter();
const groupId = Number(route.params.id);
const productId = Number(route.params.productId);

const product = ref<ProductDataDto | null>(null);
const categories = ref<CategoryDto[]>([]);
const receipts = ref<ReceiptDto[]>([]);

const loading = ref(false);
const error = ref<string | null>(null);

const showReceipts = ref(false);
const loadingReceipts = ref(false);

const editDto = reactive({
  name: '',
  description: '' as string | null,
  categoryIds: new Set<number>(),
});

async function load() {
  loading.value = true;
  error.value = null;
  try {
    const [productData, categoriesData] = await Promise.all([
      productService.getProduct(groupId, productId),
      categoryService.getCategories(groupId)
    ]);
    
    product.value = productData;
    categories.value = categoriesData;
    
    if (product.value) {
      editDto.name = product.value.name;
      editDto.description = product.value.description || null;
      
      // Map existing category names to their respective IDs for the picker
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
  showReceipts.value = !showReceipts.value;
  if (!showReceipts.value || receipts.value.length > 0) return;
  
  loadingReceipts.value = true;
  try {
    receipts.value = await receiptService.getReceipts(groupId, { productDataId: productId });
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
    // Optional: enforce max categories limit on the UI side as well
    if (editDto.categoryIds.size >= 5) {
      alert('A product can have a maximum of 5 categories.');
      return;
    }
    editDto.categoryIds.add(categoryId);
  }
}

async function save() {
  if (!editDto.name.trim()) {
    alert('Product name is required.');
    return;
  }

  try {
    const updated = await productService.updateProduct(groupId, productId, {
      name: editDto.name,
      description: editDto.description,
      categoryIds: Array.from(editDto.categoryIds)
    });
    
    product.value = updated;
    alert('Product updated successfully.');
  } catch (err: any) {
    alert(err.message || 'Error updating product');
  }
}

async function remove() {
  if (!confirm('Delete this product? Ensure no receipts are currently using it.')) return;
  try {
    await productService.deleteProduct(groupId, productId);
    router.push(`/groups/${groupId}/products`);
  } catch (err: any) {
    alert(err.message || 'Error deleting product. It might still be in use.');
  }
}

onMounted(() => load());
</script>

<template>
  <div class="p-4 max-w-3xl">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Manage Product #{{ productId }}</h1>
      <button @click="() => router.back()" class="text-blue-600 underline text-sm">
        ← Back
      </button>
    </div>

    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <div v-if="product && !loading" class="space-y-6">
      
      <div class="border rounded p-4 bg-gray-50 flex flex-col gap-4">
        <label class="flex flex-col">
          <span class="font-semibold text-gray-700 text-sm">Product Name</span>
          <input type="text" v-model="editDto.name" class="border p-2 mt-1 rounded" placeholder="e.g. Organic Milk" />
        </label>
        
        <label class="flex flex-col">
          <span class="font-semibold text-gray-700 text-sm">Description</span>
          <textarea v-model="editDto.description" rows="3" class="border p-2 mt-1 rounded" placeholder="Optional details..."></textarea>
        </label>

        <div>
          <span class="font-semibold text-gray-700 text-sm block mb-1">Categories (Max 5)</span>
          <CategoryPicker 
            :categories="categories"
            :selectedCategoryIds="editDto.categoryIds"
            @toggle="toggleCategory"
          />
          <p class="text-xs text-gray-500 mt-2">
            Note: Updating categories here will change them for all past receipts containing this product.
          </p>
        </div>

        <div class="flex gap-2 pt-2 border-t mt-2">
          <button @click="save" class="bg-green-600 text-white px-4 py-2 rounded font-semibold mt-2">
            Update Product
          </button>
          <button @click="remove" class="bg-red-600 text-white px-4 py-2 rounded font-semibold ml-auto mt-2">
            Delete Product
          </button>
        </div>
      </div>

      <div class="border-t pt-4">
        <button @click="fetchReceipts" class="text-blue-600 underline font-semibold mb-3">
          {{ showReceipts ? 'Hide Associated Receipts' : 'View Associated Receipts' }}
        </button>

        <div v-if="showReceipts" class="border rounded p-3 bg-white shadow-sm">
          <div v-if="loadingReceipts" class="text-gray-500">Loading receipts...</div>
          <div v-else-if="receipts.length === 0" class="text-gray-500">
            No receipts currently use this product. Safe to delete.
          </div>
          <table v-else class="w-full text-sm table-auto border-collapse">
            <thead>
              <tr class="bg-gray-100">
                <th class="border px-2 py-1 text-left">Receipt ID</th>
                <th class="border px-2 py-1 text-left">Date</th>
                <th class="border px-2 py-1 text-right">Total</th>
                <th class="border px-2 py-1 text-center">Action</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in receipts" :key="r.id">
                <td class="border px-2 py-1">#{{ r.id }}</td>
                <td class="border px-2 py-1">{{ new Date(r.paymentDate).toLocaleDateString() }}</td>
                <td class="border px-2 py-1 text-right">{{ r.totalAmount.toFixed(2) }}</td>
                <td class="border px-2 py-1 text-center">
                  <NuxtLink :to="`/groups/${groupId}/receipts/${r.id}`" class="text-blue-600 underline">View</NuxtLink>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</template>