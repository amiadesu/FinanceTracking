<script setup>
const { data, error, pending } = await useFetch('/api/proxy/api/groups', {
    method: 'get'
});

console.log(data.value, error.value);
</script>

<template>
  <div>
    <div v-if="pending">Checking token via BFF proxy...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <pre v-else>
        <div v-for="group in data" :key="group.id">
            <NuxtLink :to="`/groups/${group.id}`">
                {{ group.name }}
            </NuxtLink>
            <div>
                {{ group }}
            </div>
        </div>
    </pre>
  </div>
</template>