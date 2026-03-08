<template>
  <UApp :toaster="{ position: 'top-center', max: 3, expand: false, duration: 3500, portal: true }">
    <div class="min-h-screen flex flex-col bg-slate-50 dark:bg-slate-950 text-gray-900 dark:text-gray-100 transition-colors duration-300">
      <NavBar class="h-16" />

      <main class="grow flex flex-col items-center justify-center px-4 py-8">
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