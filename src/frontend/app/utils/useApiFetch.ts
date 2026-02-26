import { defu } from 'defu'
import { useRequestHeaders } from '#app'

export function useApiFetch<T>(url: string, options: any = {}) {
  const headers = import.meta.server ? useRequestHeaders(['cookie']) : {}

  const defaults = {
    baseURL: '/api/proxy', 
    headers,
  };

  const params = defu(options, defaults);

  return $fetch<T>(url, params);
}