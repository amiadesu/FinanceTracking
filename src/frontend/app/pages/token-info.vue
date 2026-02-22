<script setup>
// Simply call the proxy path. 
// Any path after '/api/proxy/' will be forwarded to your API server.
const { data, error, pending } = await useFetch('/api/proxy/api/auth/token-info')

console.log(data.value, error.value);

// Example of a POST request:
const saveTransaction = async (formData) => {
  await $fetch('/api/proxy/api/transactions', {
    method: 'POST',
    body: formData
  })
}
</script>

<template>
  <div>
    <div v-if="pending">Checking token via BFF proxy...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <pre v-else>{{ data }}</pre>
  </div>
</template>