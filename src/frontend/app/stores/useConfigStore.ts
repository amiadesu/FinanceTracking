import { defineStore } from 'pinia';
import { ref } from 'vue';
import { configService, type SystemConfigDto } from '~/services/configService';

export const useConfigStore = defineStore('config', () => {
  const config = ref<SystemConfigDto | null>(null);
  const isLoading = ref(false);

  async function fetchConfig() {
    if (config.value) return;
    
    isLoading.value = true;
    try {
      config.value = await configService.getSystemConfiguration();
    } catch (error) {
      console.error('Failed to load system config', error);
    } finally {
      isLoading.value = false;
    }
  }

  return { config, isLoading, fetchConfig };
});