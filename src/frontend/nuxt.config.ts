import tailwindcss from '@tailwindcss/vite';

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  runtimeConfig: {
    apiSecret: '',
    public: {
      apiBase: ''
    },
    oidc: {
      providers: {
        oidc: {
          clientId: '',
          clientSecret: '',

          // baseUrl: '',
          authorizationUrl: '',
          tokenUrl: '',
          userInfoUrl: '',
          logoutUrl: '',

          openIdConfiguration: '',

          redirectUri: '',
          logoutRedirectUri: '',

          scope: [],
        }
      }
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
        responseType: 'code',
        authenticationScheme: 'header', 
        pkce: true,

        callbackRedirectUrl: '/',
        logoutRedirectParameterName: 'post_logout_redirect_uri',
        
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
