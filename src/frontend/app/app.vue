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
import { onMounted } from 'vue';
import { useRoute, useRouter } from '#imports';

const route = useRoute();
const router = useRouter();

const configStore = useConfigStore();
configStore.fetchConfig();

const { loggedIn, login, logout, refresh } = useOidcAuth();

onMounted(async () => {
  if (loggedIn.value) {
      if (route.query.refreshAuth === 'true') {
        router.replace({ query: { ...route.query, refreshAuth: undefined } });
        
        try {
          await login();
        } catch (error) {
          console.error('Failed to refresh session after profile update:', error);
        }
      }

      if (route.query.accountDeleted === 'true') {
        router.replace({ query: { ...route.query, accountDeleted: undefined } });
        
        await logout();
      }
  }
})
</script>