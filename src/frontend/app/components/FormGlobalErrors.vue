<script setup lang="ts">
defineProps<{
  errors: string[]
}>();
</script>

<template>
  <div v-if="errors && errors.length > 0">
    <p 
      v-for="(error, index) in errors" 
      :key="index" 
      class="text-sm text-red-500 dark:text-red-400 mt-2 font-normal"
    >
      {{ error }}
    </p>
  </div>
</template>