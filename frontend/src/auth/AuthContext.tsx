import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '../services/api'
import { aoSessaoExpirar, definirToken, sessaoStorage } from '../services/client'
import type { UsuarioLogado } from '../services/types'

interface AuthContexto {
  usuario: UsuarioLogado | null
  carregando: boolean
  ehAdmin: boolean
  sessaoExpirou: boolean
  entrar: (email: string, senha: string) => Promise<void>
  sair: () => void
}

const Contexto = createContext<AuthContexto | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioLogado | null>(() => sessaoStorage.lerUsuario<UsuarioLogado>())
  const [carregando, setCarregando] = useState<boolean>(() => sessaoStorage.lerToken() !== null)
  const [sessaoExpirou, setSessaoExpirou] = useState(false)

  const sair = useCallback(() => {
    definirToken(null)
    sessaoStorage.limpar()
    setUsuario(null)
  }, [])

  // Qualquer 401 fora do login derruba a sessão e leva para a tela de entrada.
  useEffect(() => {
    aoSessaoExpirar(() => {
      sair()
      setSessaoExpirou(true)
    })
    return () => aoSessaoExpirar(null)
  }, [sair])

  // Ao abrir o app com token guardado, confirma no servidor que ele ainda vale
  // (pode ter expirado ou o usuário pode ter sido desativado).
  useEffect(() => {
    if (sessaoStorage.lerToken() === null) return

    let ativo = true
    authApi
      .eu()
      .then((eu) => {
        if (!ativo) return
        setUsuario(eu)
        sessaoStorage.salvar(sessaoStorage.lerToken() ?? '', eu)
      })
      .catch(() => {
        // 401 já foi tratado pelo interceptor; erro de rede mantém a sessão em cache.
      })
      .finally(() => {
        if (ativo) setCarregando(false)
      })

    return () => {
      ativo = false
    }
  }, [])

  const entrar = useCallback(async (email: string, senha: string) => {
    const resposta = await authApi.login(email, senha)
    const eu: UsuarioLogado = {
      usuarioID: resposta.usuarioID,
      nome: resposta.nome,
      email: resposta.email,
      tipo: resposta.tipo,
    }
    definirToken(resposta.token)
    sessaoStorage.salvar(resposta.token, eu)
    setSessaoExpirou(false)
    setUsuario(eu)
  }, [])

  const valor = useMemo<AuthContexto>(
    () => ({ usuario, carregando, ehAdmin: usuario?.tipo === 'AdminTotal', sessaoExpirou, entrar, sair }),
    [usuario, carregando, sessaoExpirou, entrar, sair],
  )

  return <Contexto.Provider value={valor}>{children}</Contexto.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContexto {
  const contexto = useContext(Contexto)
  if (!contexto) throw new Error('useAuth precisa estar dentro de <AuthProvider>.')
  return contexto
}
