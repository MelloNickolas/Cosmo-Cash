import axios, { AxiosError } from 'axios'
import { API_URL } from '../config'

const CHAVE_TOKEN = 'cosmocash:token'
const CHAVE_USUARIO = 'cosmocash:usuario'

// localStorage pode lançar exceção (navegação privada, dados bloqueados); o app não pode quebrar por isso.
export const sessaoStorage = {
  lerToken(): string | null {
    try {
      return localStorage.getItem(CHAVE_TOKEN)
    } catch {
      return null
    }
  },
  lerUsuario<T>(): T | null {
    try {
      const bruto = localStorage.getItem(CHAVE_USUARIO)
      return bruto ? (JSON.parse(bruto) as T) : null
    } catch {
      return null
    }
  },
  salvar(token: string, usuario: unknown) {
    try {
      localStorage.setItem(CHAVE_TOKEN, token)
      localStorage.setItem(CHAVE_USUARIO, JSON.stringify(usuario))
    } catch {
      /* sem armazenamento: a sessão vale só nesta aba */
    }
  },
  limpar() {
    try {
      localStorage.removeItem(CHAVE_TOKEN)
      localStorage.removeItem(CHAVE_USUARIO)
    } catch {
      /* ignora */
    }
  },
}

// Token em memória: garante a sessão mesmo se o armazenamento estiver indisponível.
let tokenEmMemoria: string | null = sessaoStorage.lerToken()
let aoExpirarSessao: (() => void) | null = null

export function definirToken(token: string | null) {
  tokenEmMemoria = token
}

export function aoSessaoExpirar(callback: (() => void) | null) {
  aoExpirarSessao = callback
}

export const http = axios.create({ baseURL: API_URL, timeout: 20000 })

http.interceptors.request.use((config) => {
  if (tokenEmMemoria) config.headers.Authorization = `Bearer ${tokenEmMemoria}`
  return config
})

http.interceptors.response.use(
  (resposta) => resposta,
  (erro: AxiosError) => {
    const ehLogin = erro.config?.url?.includes('/Auth/Login')
    if (erro.response?.status === 401 && !ehLogin && tokenEmMemoria) aoExpirarSessao?.()
    return Promise.reject(erro)
  },
)

// Mensagem amigável a partir de qualquer erro da API.
export function mensagemDeErro(erro: unknown): string {
  if (axios.isAxiosError(erro)) {
    if (!erro.response) return 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.'

    const dados = erro.response.data as { mensagem?: string } | undefined
    if (dados?.mensagem) return dados.mensagem
    if (erro.response.status === 429) return 'Muitas tentativas seguidas. Aguarde um minuto e tente de novo.'
    if (erro.response.status === 403) return 'Você não tem permissão para fazer isso.'
    if (erro.response.status === 401) return 'Sua sessão expirou. Entre novamente.'
    if (erro.response.status >= 500) return 'O servidor teve um problema. Tente novamente em instantes.'
  }

  return 'Ocorreu um erro inesperado. Tente novamente.'
}
