<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from '#imports';
import { receiptService } from '~/services/receiptService';
import { categoryService } from '~/services/categoryService';
import type { ReceiptDto, CreateReceiptDto } from '~/services/receiptService';
import type { CategoryDto } from '~/services/categoryService';
import CategoryPicker from '~/components/CategoryPicker.vue';

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

const receipts = ref<ReceiptDto[]>([]);
const categories = ref<CategoryDto[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

function generateUid() {
  return Math.random().toString(36).substring(2) + Date.now().toString(36);
}

const newReceipt = reactive<{
  paymentDate: string;
  sellerId: number;
  products: FormProduct[];
}>({
  paymentDate: new Date().toISOString().split('T')[0]!,
  sellerId: 0,
  products: [{
    _uid: generateUid(),
    name: '',
    price: 0,
    quantity: 1,
    categoryIds: new Set()
  }]
});

async function loadData() {
  loading.value = true;
  error.value = null;
  try {
    const [receiptsData, categoriesData] = await Promise.all([
      receiptService.getReceipts(groupId),
      categoryService.getCategories(groupId)
    ]);
    receipts.value = receiptsData;
    categories.value = categoriesData;
  } catch (err: any) {
    error.value = err.message || 'Failed to load data';
  } finally {
    loading.value = false;
  }
}

function addProduct() {
  newReceipt.products.push({
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
}

async function createReceipt() {
  if (!newReceipt.sellerId) {
    alert('Seller ID is required');
    return;
  }
  
  if (newReceipt.products.length === 0) {
    alert('A receipt must have at least one product.');
    return;
  }

  if (newReceipt.products.some(p => !p.name || p.price <= 0 || p.quantity <= 0)) {
    alert('Please fill all product fields with valid values');
    return;
  }
  
  try {
    const receiptToSend: CreateReceiptDto = {
      paymentDate: new Date(newReceipt.paymentDate).toISOString(),
      sellerId: newReceipt.sellerId,
      products: newReceipt.products.map(p => ({
        name: p.name,
        price: p.price,
        quantity: p.quantity,
        categories: Array.from(p.categoryIds)
          .map(id => categories.value.find(c => c.id === id)?.name)
          .filter((name): name is string => !!name)
      }))
    };
    
    await receiptService.createReceipt(groupId, receiptToSend);
    
    newReceipt.paymentDate = new Date().toISOString().split('T')[0]!;
    newReceipt.sellerId = 0;
    newReceipt.products = [{
      _uid: generateUid(),
      name: '',
      price: 0,
      quantity: 1,
      categoryIds: new Set()
    }];
    
    await loadData();
  } catch (err: any) {
    alert(err.message || 'Error creating receipt');
  }
}

function goToReceipt(id: number) {
  router.push(`/groups/${groupId}/receipts/${id}`);
}

onMounted(() => loadData());
</script>

<template>
  <div class="p-4">
    <h1 class="text-xl font-bold">Receipts</h1>
    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <table v-if="!loading && receipts.length > 0" class="w-full mt-4 table-auto border-collapse">
      <thead>
        <tr>
          <th class="border px-2 py-1">ID</th>
          <th class="border px-2 py-1">Date</th>
          <th class="border px-2 py-1">Seller</th>
          <th class="border px-2 py-1">Total</th>
          <th class="border px-2 py-1">Products</th>
          <th class="border px-2 py-1">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="r in receipts" :key="r.id">
          <td class="border px-2 py-1">{{ r.id }}</td>
          <td class="border px-2 py-1">{{ new Date(r.paymentDate).toLocaleDateString() }}</td>
          <td class="border px-2 py-1">{{ r.sellerName ? r.sellerName : (r.sellerId ?? '-') }}</td>
          <td class="border px-2 py-1">{{ r.totalAmount.toFixed(2) }}</td>
          <td class="border px-2 py-1">{{ r.products.length }}</td>
          <td class="border px-2 py-1">
            <button @click="goToReceipt(r.id)" class="text-blue-600 underline">View</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="!loading && receipts.length === 0" class="text-gray-500 mt-4">No receipts yet</div>

    <div class="mt-6 border-t pt-4">
      <h2 class="font-semibold">Create new receipt</h2>
      <div class="flex flex-col gap-4 max-w-2xl">
        <label class="flex flex-col">
          Payment Date
          <input type="date" v-model="newReceipt.paymentDate" class="border p-1" />
        </label>

        <label class="flex flex-col">
          Seller ID
          <input type="number" v-model.number="newReceipt.sellerId" class="border p-1" />
        </label>

        <div class="border rounded p-4 bg-gray-50">
          <h3 class="font-semibold mb-3">Products</h3>
          
          <div v-for="product in newReceipt.products" :key="product._uid" class="mb-4 border-b pb-4 last:border-b-0">
            <div class="grid grid-cols-2 gap-3">
              <label class="flex flex-col">
                Product Name
                <input type="text" v-model="product.name" class="border p-1" placeholder="e.g., Milk" />
              </label>
              <label class="flex flex-col">
                Price
                <input type="number" v-model.number="product.price" step="0.01" class="border p-1" />
              </label>
              <label class="flex flex-col">
                Quantity
                <input type="number" v-model.number="product.quantity" step="0.01" class="border p-1" />
              </label>
            </div>

            <div class="mt-3">
              <CategoryPicker 
                :categories="categories"
                :selectedCategoryIds="product.categoryIds"
                @toggle="(catId) => toggleProductCategory(product, catId)"
              />
            </div>

            <button
              v-if="newReceipt.products.length > 1"
              type="button"
              @click="removeProduct(product._uid)"
              class="text-red-600 text-sm mt-2 underline"
            >
              Remove product
            </button>
          </div>

          <button type="button" @click="addProduct" class="text-blue-600 underline text-sm mt-3">
            + Add product
          </button>
        </div>

        <button @click="createReceipt" class="bg-blue-600 text-white px-3 py-2 rounded">
          Create Receipt
        </button>
      </div>
    </div>
  </div>
</template>