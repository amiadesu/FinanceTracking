import { joinURL } from 'ufo'
import { getUserSession } from 'nuxt-oidc-auth/runtime/server/utils/session.js'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  let backendBaseUrl = config.public.apiBase || 'http://api_server:8080'
  backendBaseUrl = backendBaseUrl.replace(/\/api\/?$/, '')

  let accessToken = null

  const cookie = getHeader(event, 'cookie')
  if (cookie) {
    try {
      const session = await getUserSession(event)
      accessToken = session.accessToken
    } catch (err) {
      console.error("[API Proxy] Error retrieving session:", err)
    }
  }

  console.log("Access token:", accessToken);

  const path = event.path.replace(/^\/api\/proxy\//, '')

  console.log('[API Proxy Debug] Path:', path, 'Original:', event.path);

  const target = joinURL(backendBaseUrl, path)

  const headers: Record<string, string> = {}
  if (accessToken) {
    headers.Authorization = `Bearer ${accessToken}`
  }

  return proxyRequest(event, target, {
    headers
  })
})