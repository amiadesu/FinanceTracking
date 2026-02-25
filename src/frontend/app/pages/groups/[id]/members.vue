<script setup>
const route = useRoute();

const groupId = route.params.id;

const { data, error, pending } = await useFetch(`/api/proxy/api/groups/${groupId}/members`, {
    method: 'get'
});

console.log(data.value, error.value);
</script>

<template>
  <div>
    <div v-if="pending">Checking token via BFF proxy...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <pre v-else>
        {{ data }}
    </pre>
  </div>
</template>