<script setup lang="ts">
import { useHead } from '#imports'

const { loggedIn, login } = useOidcAuth();

useHead({
  title: 'FinanceTracker - Shared Finance Management',
})
</script>

<template>
  <div class="flex flex-col items-center max-w-4xl mx-auto py-12 px-4 text-center">
    <section class="mb-16">      
      <h1 class="text-5xl sm:text-6xl font-extrabold tracking-tight mb-6 bg-linear-to-b from-gray-900 to-gray-600 dark:from-white dark:to-gray-400 bg-clip-text text-transparent">
        FinanceTracker
      </h1>
      
      <p class="text-xl text-gray-600 dark:text-gray-400 max-w-2xl mx-auto mb-10 leading-relaxed">
        The ultimate way to track your group finances. Upload receipts, categorize spending, and stay on budget - all in one place.
      </p>

      <div class="flex flex-wrap justify-center gap-4">
        <template v-if="loggedIn">
          <UButton 
            to="/groups" 
            size="xl" 
            color="primary" 
            icon="i-heroicons-arrow-right" 
            trailing
          >
            Go to My Groups
          </UButton>
        </template>
        <template v-else>
          <UButton 
            @click="login()" 
            size="xl" 
            color="primary"
          >
            Get Started for Free
          </UButton>
          <UButton 
            variant="ghost" 
            color="neutral" 
            size="xl"
            @click="login(undefined, { action: 'register' })"
          >
            Create Account
          </UButton>
        </template>
      </div>
    </section>

    <section class="grid grid-cols-1 md:grid-cols-3 gap-8 text-left w-full">
      <UCard class="hover:ring-1 hover:ring-blue-500 transition-all">
        <div class="h-12 w-12 rounded-lg bg-blue-100 dark:bg-blue-900/50 flex items-center justify-center mb-4">
          <UIcon name="i-heroicons-document-text" class="h-6 w-6 text-blue-600 dark:text-blue-400" />
        </div>
        <h3 class="font-bold text-lg mb-2">Receipt Parsing</h3>
        <p class="text-sm text-gray-500">Upload XML fiscal receipts to automatically extract product data and amounts.</p>
      </UCard>

      <UCard class="hover:ring-1 hover:ring-indigo-500 transition-all">
        <div class="h-12 w-12 rounded-lg bg-indigo-100 dark:bg-indigo-900/50 flex items-center justify-center mb-4">
          <UIcon name="i-heroicons-users" class="h-6 w-6 text-indigo-600 dark:text-indigo-400" />
        </div>
        <h3 class="font-bold text-lg mb-2">Group Sharing</h3>
        <p class="text-sm text-gray-500">Collaborate with family and friends. Manage shared budgets and track group history.</p>
      </UCard>

      <UCard class="hover:ring-1 hover:ring-purple-500 transition-all">
        <div class="h-12 w-12 rounded-lg bg-purple-100 dark:bg-purple-900/50 flex items-center justify-center mb-4">
          <UIcon name="i-heroicons-chart-pie" class="h-6 w-6 text-purple-600 dark:text-purple-400" />
        </div>
        <h3 class="font-bold text-lg mb-2">Budget Goals</h3>
        <p class="text-sm text-gray-500">Set monthly targets for categories and visualize your progress effortlessly.</p>
      </UCard>
    </section>
  </div>
</template>