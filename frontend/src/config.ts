// Configuração por instância (uma instância por cliente). Vem das variáveis VITE_ do build.
export const API_URL: string = (import.meta.env.VITE_API_URL ?? 'http://localhost:5136').replace(/\/+$/, '')
export const NOME_LOJA: string = (import.meta.env.VITE_NOME_LOJA ?? '').trim() || 'Cosmo Cash'
