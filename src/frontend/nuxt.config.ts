import tailwindcss from '@tailwindcss/vite';

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  runtimeConfig: {
    apiSecret: process.env.PRIVATE_API_KEY,
    public: {
      apiBase: process.env.PRIVATE_API_BASE_URL
    }
  },
  oidc: {
    defaultProvider: 'oidc',
    session: {
      automaticRefresh: true,
      expirationCheck: true,
      maxAge: 60 * 60 * 24,
    },
    middleware: {
      globalMiddlewareEnabled: false,
      customLoginPage: false,
    },
    providers: {
      oidc: {
        clientId: process.env.OIDC_AUTH_SERVER_CLIENT_ID,
        clientSecret: process.env.OIDC_AUTH_SERVER_CLIENT_SECRET,

        // baseUrl: process.env.OIDC_AUTH_SERVER_ISSUER_URL,
        authorizationUrl: process.env.OIDC_AUTH_SERVER_AUTHORIZATION_URL,
        tokenUrl: process.env.OIDC_AUTH_SERVER_TOKEN_URL,
        userInfoUrl: process.env.OIDC_AUTH_SERVER_USER_INFO_URL,
        logoutUrl: process.env.OIDC_AUTH_SERVER_LOGOUT_URL,

        responseType: 'code',
        authenticationScheme: 'header', 
        scope: ['openid', 'profile', 'email', 'offline_access', 'financetracking.api'],
        pkce: true,

        redirectUri: 'http://localhost:3000/auth/oidc/callback',
        callbackRedirectUrl: '/',
        logoutRedirectUri: 'http://localhost:3000',
        logoutRedirectParameterName: 'post_logout_redirect_uri',
        
        openIdConfiguration: 'https://localhost:5001/.well-known/openid-configuration',
        allowedClientAuthParameters: ['action'],
        exposeAccessToken: true,
        exposeIdToken: true,
        tokenRequestType: 'form-urlencoded'
      }
    }
  },
  pages: true,
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  modules: [
    '@nuxt/icon',
    '@nuxtjs/color-mode',
    'nuxt-oidc-auth'
  ],
  css: ['~/assets/css/main.css'],
  vite: {
    plugins: [
      tailwindcss(),
    ],
  },
  colorMode: {
    classPrefix: '',
    classSuffix: '',
    preference: 'system',
    fallback: 'light',
    storage: 'localStorage',
    storageKey: 'nuxt-color-mode'
  }
})
