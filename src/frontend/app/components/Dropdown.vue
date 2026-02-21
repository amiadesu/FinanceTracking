<template>
  <div ref="container" class="relative">
    <!-- Custom button slot -->
    <slot name="button" :opened="opened" :toggle="toggle">
      <!-- Default button -->
      <button @click="toggle" class="inline-flex items-center px-4 py-2 border rounded">
        Options
        <svg class="w-4 h-4 ml-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M19 9l-7 7-7-7" />
        </svg>
      </button>
    </slot>

    <!-- Menu slot -->
    <slot name="menu" v-if="opened" :close="close"></slot>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";

const opened = ref(false);

function toggle() {
  opened.value = !opened.value;
}

function close() {
  opened.value = false;
}

const container = ref<HTMLElement | null>(null);
const buttonRef = ref<HTMLElement | null>(null);

// Handle clicks outside, ignore button
function handleClickOutside(event: MouseEvent) {
  const target = event.target as Node;
  if (container.value && !container.value.contains(target)) {
    close();
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>