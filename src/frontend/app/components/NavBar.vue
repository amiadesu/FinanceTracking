<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useColorMode } from '#imports';
import { invitationService } from '~/services/invitationService';

const colorMode = useColorMode();
const isDark = computed(() => colorMode.value === 'dark');

const { loggedIn, user, login, logout } = useOidcAuth();
const pendingCount = ref<number>(0);

async function loadPendingCount() {
    if (!loggedIn.value) {
        pendingCount.value = 0;
        return;
    }
    try {
        const res = await invitationService.getAvailablePendingCount();
        pendingCount.value = res.count;
    } catch {
        pendingCount.value = 0;
    }
}

onMounted(loadPendingCount);
watch(loggedIn, (val) => {
    if (val) loadPendingCount();
    else pendingCount.value = 0;
});

function toggleTheme() {
    colorMode.preference = isDark.value ? 'light' : 'dark';
}

const accountItems = computed(() => {
    if (!loggedIn.value) {
        return [
            [
                { 
                    label: 'Log in', 
                    icon: 'i-heroicons-arrow-right-on-rectangle', 
                    onSelect: () => login(), 
                    badge: undefined
                },
                { 
                    label: 'Register', 
                    icon: 'i-heroicons-user-plus', 
                    onSelect: () => login(undefined, { action: 'register' }), 
                    badge: undefined
                }
            ]
        ];
    }
    return [
        [
            { 
                label: 'Invitations', 
                to: '/invitations', 
                icon: 'i-heroicons-envelope',
                badge: pendingCount.value > 0 ? pendingCount.value : undefined
            }
        ],
        [
            { 
                label: 'Log out', 
                icon: 'i-heroicons-arrow-left-on-rectangle', 
                onSelect: () => logout(), 
                badge: undefined
            }
        ]
    ]
});
</script>

<template>
    <nav class="w-full border-b border-gray-200 dark:border-gray-800 bg-white/80 dark:bg-gray-900/80 backdrop-blur sticky top-0 z-50 shadow-sm" aria-label="Main navigation">
        <div class="flex flex-row max-w-6xl mx-auto px-4 py-2.5 justify-between items-center">
            <div class="flex items-center gap-6">
                <NuxtLink to="/" class="group flex items-center gap-2">
                    <div class="h-10 w-10 rounded-xl bg-white dark:bg-gray-800 border border-gray-800 dark:border-gray-400 group-hover:border-gray-400 dark:group-hover:border-gray-200 grid place-items-center transition duration-200">
                        <span class="p-1 bg-zinc-900 dark:bg-white group-hover:bg-linear-to-br from-blue-600 to-indigo-500 bg-clip-text text-transparent text-2xl font-black select-none transition duration-200">
                            F
                        </span>
                    </div>
                    <div class="flex text-2xl px-1 items-center font-semibold bg-zinc-900 dark:bg-white group-hover:bg-linear-to-br from-blue-600 to-indigo-500 bg-clip-text text-transparent transition duration-200">
                        FinanceTracker
                    </div>
                </NuxtLink>

                <UButton 
                    v-if="loggedIn" 
                    to="/groups" 
                    variant="ghost" 
                    color="neutral"
                    icon="i-heroicons-user-group"
                    class="hidden sm:flex"
                >
                    Groups
                </UButton>
            </div>

            <div class="flex items-center gap-2 sm:gap-3">
                <UButton
                    :icon="isDark ? 'i-heroicons-moon' : 'i-heroicons-sun'"
                    color="neutral"
                    variant="ghost"
                    aria-label="Toggle Theme"
                    @click="toggleTheme"
                />

                <UDropdownMenu :items="accountItems" :popper="{ placement: 'bottom-end' }">
                    <UButton 
                        color="neutral" 
                        variant="ghost"
                        :label="loggedIn ? user?.userInfo?.name as string : 'Account'"
                        icon="i-heroicons-user"
                    >
                        <template #trailing>
                            <UBadge 
                                v-if="loggedIn && pendingCount > 0" 
                                color="error" 
                                variant="solid" 
                                size="xs" 
                                class="rounded-full"
                            >
                                {{ pendingCount }}
                            </UBadge>
                            <UIcon name="i-heroicons-chevron-down-20-solid" class="w-4 h-4 text-gray-500" />
                        </template>
                    </UButton>

                    <template #item="{ item }">
                        <span class="truncate">{{ item.label }}</span>
                        <UIcon :name="item.icon" class="shrink-0 h-4 w-4 text-gray-400 dark:text-gray-500 ms-auto" />
                        <UBadge v-if="item.badge" color="error" size="xs" class="ms-2 rounded-full">
                            {{ item.badge }}
                        </UBadge>
                    </template>
                </UDropdownMenu>
            </div>
        </div>
    </nav>
</template>