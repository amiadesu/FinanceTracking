<script setup>
const route = useRoute();

const groupId = route.params.id;

const { data, error, pending } = await useFetch(`/api/proxy/api/groups/${groupId}`, {
    method: 'get'
});

console.log(data.value, error.value);
</script>

<template>
  <div>
    <div v-if="pending">Checking token via BFF proxy...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <pre v-else>
        <div>
            {{ data.name }}
        </div>
        <NuxtLink :to="`/groups/${groupId}/members`">
            Group members
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/history`">
            Group history
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/categories`">
            Categories
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/goals`">
            Budget Goals
        </NuxtLink>
        <NuxtLink :to="`/groups/${groupId}/receipts`">
            Receipts
        </NuxtLink>
        {{ data }}
    </pre>
  </div>
</template>