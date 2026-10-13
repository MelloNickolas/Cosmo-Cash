import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'
import { VitePWA } from 'vite-plugin-pwa'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // O nome da loja entra no manifest do PWA em tempo de build (uma instância por cliente).
  const env = loadEnv(mode, process.cwd(), '')
  const nome = env.VITE_NOME_LOJA?.trim() || 'Cosmo Cash'

  return {
    plugins: [
      react(),
      VitePWA({
        registerType: 'autoUpdate',
        includeAssets: ['favicon.svg', 'apple-touch-icon.png'],
        manifest: {
          name: nome,
          short_name: nome.length > 12 ? nome.slice(0, 12) : nome,
          description: 'Registro de vendas, clientes e painel de resultados.',
          lang: 'pt-BR',
          start_url: '/',
          scope: '/',
          display: 'standalone',
          orientation: 'portrait',
          background_color: '#0B1320',
          theme_color: '#0B1320',
          icons: [
            { src: 'pwa-192.png', sizes: '192x192', type: 'image/png' },
            { src: 'pwa-512.png', sizes: '512x512', type: 'image/png' },
            { src: 'pwa-maskable-512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
          ],
        },
        workbox: {
          // Só o "casco" do app é cacheado. As chamadas à API não passam pelo service worker
          // (outro domínio e sem runtimeCaching), então os números nunca ficam desatualizados.
          globPatterns: ['**/*.{js,css,html,svg,png,woff2}'],
          navigateFallback: '/index.html',
          maximumFileSizeToCacheInBytes: 4 * 1024 * 1024,
        },
      }),
    ],
    css: {
      preprocessorOptions: {
        scss: {
          // O Carbon usa a API moderna do Sass; silencia avisos de dependências do próprio Carbon.
          quietDeps: true,
          silenceDeprecations: ['import', 'global-builtin', 'legacy-js-api'],
        },
      },
    },
  }
})
