// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui'
  ],

  ssr: false,

  devtools: {
    enabled: true
  },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    public: {
      apiPort: '5081',
      // Empty by default -- local/LAN dev derives the API host from
      // window.location.hostname (see useApi.ts). Production (Netlify) sets
      // this via the NUXT_PUBLIC_API_BASE env var to a full URL, since
      // frontend and backend live on different domains there.
      apiBase: ''
    }
  },

  devServer: {
    host: '0.0.0.0',
    port: 3030
  },

  compatibilityDate: '2026-06-30',

  eslint: {
    config: {
      stylistic: {
        commaDangle: 'never',
        braceStyle: '1tbs'
      }
    }
  }
})
