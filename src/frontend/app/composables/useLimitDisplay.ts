import { computed, toValue, type MaybeRefOrGetter } from 'vue';
import { useConfigStore } from '~/stores/useConfigStore';

export function useLimitDisplay(
  current: MaybeRefOrGetter<number>, 
  max: MaybeRefOrGetter<number>
) {
  const configStore = useConfigStore();

  return computed(() => {
    const currentValue = toValue(current);
    const maxValue = toValue(max);
    
    if (!configStore.config) return `${currentValue} / ${maxValue}`;
    
    const isInfinite = maxValue === configStore.config.infiniteLimit;
    return `${currentValue} / ${isInfinite ? '∞' : maxValue}`;
  });
}