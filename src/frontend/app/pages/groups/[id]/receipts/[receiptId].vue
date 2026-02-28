<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from '#imports';
import { receiptService } from '~/services/receiptService';
import { categoryService } from '~/services/categoryService';
import type { ReceiptDto, UpdateReceiptDto } from '~/services/receiptService';
import type { CategoryDto } from '~/services/categoryService';
import CategoryPicker from '~/components/CategoryPicker.vue';
import { sellerService } from '~/services/sellerService';
import type { SellerDto } from '~/services/sellerService';
import SellerPicker from '~/components/SellerPicker.vue';

interface FormProduct {
  _uid: string; // Unique ID for Vue's v-for key tracking
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
const editDto = reactive<{
  paymentDate?: string;
  sellerId?: number;
  products: FormProduct[];
}>({
  products: [],
});

function generateUid() {
  return Math.random().toString(36).substring(2) + Date.now().toString(36);
}

async function load() {
  loading.value = true;
  error.value = null;
  try {
    const [receiptData, categoriesData, sellersData] = await Promise.all([
      receiptService.getReceipt(groupId, receiptId),
      categoryService.getCategories(groupId),
      sellerService.getSellers(groupId)
    ]);
    receipt.value = receiptData;
    categories.value = categoriesData;
    sellers.value = sellersData;
  } catch (err: any) {
    error.value = err.message || 'Failed to load data';
  } finally {
    loading.value = false;
  }
}

function startEdit() {
  if (!receipt.value) return;
  editMode.value = true;
  
  // Format date for the HTML input (YYYY-MM-DD)
  editDto.paymentDate = receipt.value.paymentDate 
    ? receipt.value.paymentDate.split('T')[0] 
    : undefined;
    
  editDto.sellerId = receipt.value.sellerId;
  
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
  editDto.products.push({
    _uid: generateUid(),
    name: '',
    price: 0,
    quantity: 1,
    categoryIds: new Set(),
  });
}

function removeProduct(uid: string) {
  if (editDto.products.length <= 1) {
    alert('A receipt must have at least one product.');
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
}

async function save() {
  if (!receipt.value || editDto.products.length === 0) {
    alert('A receipt must have at least one product.');
    return;
  }
  
  if (editDto.products.some(p => !p.name || p.price <= 0 || p.quantity <= 0)) {
    alert('Please fill all product fields with valid values');
    return;
  }
  
  try {
    const receiptToSend: UpdateReceiptDto = {
      paymentDate: editDto.paymentDate,
      sellerId: editDto.sellerId,
      products: editDto.products.map(p => ({
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
    alert('Receipt updated successfully.');
    await load(); // Reload to get fresh timestamps/data
  } catch (err: any) {
    alert(err.message || 'Error updating receipt');
  }
}

async function remove() {
  if (!confirm('Delete this receipt?')) return;
  try {
    await receiptService.deleteReceipt(groupId, receiptId);
    router.push(`/groups/${groupId}/receipts`);
  } catch (err: any) {
    alert(err.message || 'Error deleting receipt');
  }
}

function cancelEdit() {
  editMode.value = false;
}

onMounted(() => load());
</script>

<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-xl font-bold">Receipt #{{ receiptId }}</h1>
      <button @click="() => router.back()" class="text-blue-600 underline text-sm">
        ← Back
      </button>
    </div>

    <div v-if="loading">Loading…</div>
    <div v-if="error" class="text-red-600">{{ error }}</div>

    <div v-if="receipt && !editMode" class="space-y-4">
      <div class="grid grid-cols-2 gap-4">
        <div>
          <p class="text-gray-600 text-sm">Payment Date</p>
          <p class="font-semibold">{{ new Date(receipt.paymentDate).toLocaleDateString() }}</p>
        </div>
        <div>
          <p class="text-gray-600 text-sm">Total Amount</p>
          <p class="font-semibold text-lg">{{ receipt.totalAmount.toFixed(2) }}</p>
        </div>
        <div>
          <p class="text-gray-600 text-sm">Seller</p>
          <p class="font-semibold">
            {{ receipt.sellerName ? receipt.sellerName : (receipt.sellerId ?? 'Not specified') }}
          </p>
        </div>
        <div>
          <p class="text-gray-600 text-sm">Created</p>
          <p class="font-semibold">{{ new Date(receipt.createdDate).toLocaleDateString() }}</p>
        </div>
      </div>

      <div class="border-t pt-4">
        <h2 class="font-semibold mb-3">Products ({{ receipt.products.length }})</h2>
        <div v-if="receipt.products.length === 0" class="text-gray-500">No products</div>
        <table v-else class="w-full border-collapse">
          <thead>
            <tr>
              <th class="border px-2 py-1 text-left">Name</th>
              <th class="border px-2 py-1 text-right">Price</th>
              <th class="border px-2 py-1 text-right">Qty</th>
              <th class="border px-2 py-1 text-right">Subtotal</th>
              <th class="border px-2 py-1">Categories</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in receipt.products" :key="p.id">
              <td class="border px-2 py-1">{{ p.name }}</td>
              <td class="border px-2 py-1 text-right">{{ p.price.toFixed(2) }}</td>
              <td class="border px-2 py-1 text-right">{{ p.quantity }}</td>
              <td class="border px-2 py-1 text-right">{{ (p.price * p.quantity).toFixed(2) }}</td>
              <td class="border px-2 py-1">
                <span v-for="cat in p.categories" :key="cat" class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded mr-1 inline-block">
                  {{ cat }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="border-t pt-4 space-y-2">
        <button @click="startEdit" class="bg-green-600 text-white px-3 py-2 rounded mr-2">
          Edit
        </button>
        <button @click="remove" class="bg-red-600 text-white px-3 py-2 rounded">
          Delete
        </button>
      </div>
    </div>

    <div v-if="receipt && editMode" class="space-y-4">
      <div class="flex flex-col gap-4 max-w-2xl">
        <label class="flex flex-col">
          Payment Date
          <input type="date" v-model="editDto.paymentDate" class="border p-1" />
        </label>

        <label class="flex flex-col relative">
          Seller
          <SellerPicker :sellers="sellers" v-model="editDto.sellerId!" />
        </label>

        <div class="border rounded p-4 bg-gray-50">
          <h3 class="font-semibold mb-3">Products</h3>
          
          <div v-for="product in editDto.products" :key="product._uid" class="mb-4 border-b pb-4 last:border-b-0">
            <div class="grid grid-cols-2 gap-3">
              <label class="flex flex-col">
                Product Name
                <input type="text" v-model="product.name" class="border p-1" />
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
              v-if="editDto.products.length > 1"
              type="button"
              @click="removeProduct(product._uid)"
              class="text-red-600 text-sm mt-3 underline"
            >
              Remove product
            </button>
          </div>

          <button type="button" @click="addProduct" class="text-blue-600 underline text-sm mt-2">
            + Add product
          </button>
        </div>

        <div class="flex gap-2">
          <button @click="save" class="bg-green-600 text-white px-3 py-2 rounded">
            Save
          </button>
          <button @click="cancelEdit" class="bg-gray-600 text-white px-3 py-2 rounded">
            Cancel
          </button>
        </div>
      </div>
    </div>
  </div>
</template>