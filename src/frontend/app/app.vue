<template>
  <UApp>
    <div class="min-h-screen flex flex-col bg-white text-gray-900">
      <NavBar class="h-16" />

      <main class="flex-grow flex flex-col items-center justify-center px-4 py-4 text-center">
        <NuxtLayout>
          <NuxtPage />
        </NuxtLayout>
      </main>
    </div>
  </UApp>
</template>

<script setup lang="ts">
const configStore = useConfigStore();
configStore.fetchConfig();
</script>