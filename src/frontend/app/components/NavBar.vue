<template>
    <nav
      class="w-full border-b border-gray-200 bg-white text-gray-900 sticky top-0 z-50 shadow-md shadow-blue-200/30"
      aria-label="Main navigation"
    >
        <div class="flex flex-row max-w-6xl mx-auto px-4 py-2.5 justify-between items-center">
            <!-- Left: Catalog link -->
            <div class="flex items-center gap-6">
                <NuxtLink to="/" class="group flex items-center gap-2">
                    <div class="h-10 w-10 rounded-xl mb-1.5 bg-white border border-gray-800 group-hover:border-gray-200 grid place-items-center transition duration-200">
                        <span class="p-1 bg-zinc-900 group-hover:bg-gradient-to-br from-blue-600 to-indigo-500 bg-clip-text text-transparent text-2xl font-black select-none transition duration-200">
                            F
                        </span>
                    </div>
                    <div class="flex text-2xl mb-1.5 px-1 py-1 items-center font-semibold bg-zinc-900 group-hover:bg-gradient-to-r from-blue-600 to-indigo-500 bg-clip-text text-transparent transition duration-200">
                        FinanceTracker
                    </div>
                </NuxtLink>

                <!-- Token Info Button -->
                <NuxtLink v-if="loggedIn" to="/token-info">
                <button
                        id="main"
                        class="inline-flex items-center gap-x-1.5 px-2 py-1 rounded-lg text-sm font-medium text-gray-500 hover:border-blue-500 hover:bg-blue-50 hover:text-blue-700 border-gray-200 hover:shadow-md shadow-blue-500/10 transition"
                    >
                        View token info
                    </button>
                </NuxtLink>
            </div>

            <!-- Right: Controls -->
            <div class="flex items-center gap-3">
                <!-- Language Switcher -->
                <!-- <label class="sr-only" for="lang">Language</label> -->
                <!-- <Dropdown id="lang" class="inline-block"> -->
                    <!-- Custom button -->
                    <!-- <template #button="{ opened, toggle }">
                        <button 
                            @click="handleLogIn"
                            class="inline-flex w-full items-center justify-center gap-x-1.5 rounded-md border border-gray-300 bg-transparent px-2 py-1 text-sm font-medium hover:bg-gray-200 focus:outline-none focus:ring focus:ring-blue-500"
                        >
                            {{ language.toUpperCase() }}
                            <svg viewBox="0 0 20 20" fill="currentColor" data-slot="icon" aria-hidden="true" class="-mr-1 size-5 text-gray-400">
                                <path d="M5.22 8.22a.75.75 0 0 1 1.06 0L10 11.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 9.28a.75.75 0 0 1 0-1.06Z" clip-rule="evenodd" fill-rule="evenodd" />
                            </svg>
                        </button>
                    </template> -->

                    <!-- Menu items -->
                    <!-- <template #menu="{ close }">
                        <Menu 
                            @close="close" 
                            class="absolute left-0 mt-1 w-full bg-white border rounded-lg shadow-lg z-50"
                        >
                            <div class="py-1">
                                <p
                                    class="block w-full py-1 text-center text-sm text-gray-700 hover:bg-blue-600 hover:text-white cursor-pointer select-none"
                                    v-for="lang in languages" 
                                    :key="lang" 
                                    @click="setLanguage(lang), close()"
                                >
                                    {{ lang.toUpperCase() }}
                                </p>
                            </div>
                        </Menu>
                    </template> -->
                <!-- </Dropdown> -->

                <!-- Theme Switcher -->
                <button
                    type="button"
                    @click="toggleTheme"
                    class="p-2 rounded-full hover:bg-gray-200 dark:hover:bg-gray-800 transition"
                    :aria-label="isDark ? 'Switch to light theme' : 'Switch to dark theme'"
                    :title="isDark ? 'Light' : 'Dark'"
                >
                    <span v-if="isDark">🌙</span>
                    <span v-else>☀️</span>
                </button>



                <!-- Account Menu -->
                <label class="sr-only" for="account">Account</label>
                <Dropdown id="account" class="inline-block">
                    <!-- Custom button -->
                    <template #button="{ opened, toggle }">
                        <button
                            v-if="!loggedIn"
                            @click="toggle"
                            class="inline-flex items-center gap-x-1.5 px-1 py-1 rounded-lg text-sm text-center font-medium hover:bg-gray-200 transition"
                        >
                            Account
                            <Icon 
                                name="mdi-light:account"
                                class="text-xl align-middle"
                            />
                        </button>
                        <button
                            v-else
                            @click="toggle"
                            class="inline-flex items-center gap-x-1.5 px-1 py-1 rounded-lg text-sm text-center font-medium hover:bg-gray-200 transition"
                        >
                            {{ user?.userInfo?.name }}
                            <Icon 
                                name="mdi-light:account"
                                class="text-xl align-middle"
                            />
                        </button>
                    </template>

                    <!-- Menu items -->
                    <template #menu="{ close }">
                        <Menu 
                            @close="close" 
                            class="absolute right-0 mt-1 w-full bg-white border rounded-lg shadow-lg z-50 overflow-hidden"
                        >
                            <div v-if="!loggedIn" class="py-1">
                                <div
                                    @click="handleLogIn"
                                    class="block w-full py-1 text-center text-sm text-gray-700 hover:bg-blue-600 hover:text-white cursor-pointer select-none"
                                >
                                    Log in
                                </div>
                                <div
                                    @click="handleRegister"
                                    class="block w-full py-1 text-center text-sm text-gray-700 hover:bg-blue-600 hover:text-white cursor-pointer select-none"
                                >
                                    Register
                                </div>
                            </div>
                            <div v-else class="py-1">
                                <div
                                    @click="handleLogout"
                                    class="block w-full py-1 text-center text-sm text-gray-700 hover:bg-blue-600 hover:text-white cursor-pointer select-none"
                                >
                                    Log out
                                </div>
                            </div>
                        </Menu>
                    </template>
                </Dropdown>
            </div>
        </div>
    </nav>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useColorMode } from '#imports'

const languages = ['en', 'ua'];

const language = ref<string>('en')

const colorMode = useColorMode()
const isDark = computed(() => colorMode.value === 'dark');

const { loggedIn, user, login, logout } = useOidcAuth();

function toggleTheme() {
  colorMode.preference = isDark.value ? 'light' : 'dark'
}

function setLanguage(lang: string) {
    language.value = lang;
}

function handleLogIn() {
    login();
}

function handleRegister() {
    login(undefined, {
        action: 'register'
    });
}

function handleLogout() {
    logout();
}
</script>