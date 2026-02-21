<template>
    <div class="min-h-screen flex flex-col bg-white text-gray-900">
        <NavBar class="h-16" />

        <main class="flex-grow flex flex-col items-center justify-center px-4 py-16 text-center">
            <div class="mb-8">
                <!-- <div class="mx-auto h-14 w-14 rounded-2xl bg-gradient-to-br from-blue-600 to-indigo-500 shadow-lg grid place-items-center">
                    <span class="text-white text-2xl font-black select-none">S</span>
                </div> -->
                <h1 class="mt-4 text-4xl sm:text-5xl font-extrabold tracking-tight">
                    FinanceTracker
                </h1>
                <p class="mt-2 text-gray-600 max-w-md mx-auto">
                    Easily track you finances using fiscal receipts
                </p>
            </div>
        </main>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useHead, useRouter } from '#imports'

const router = useRouter();

useHead({
  title: 'FinanceTracker',
})
</script>

<style scoped>
input::placeholder { opacity: 0.7; transition: opacity .2s ease; }
input:focus::placeholder { opacity: 0.4; }
</style>