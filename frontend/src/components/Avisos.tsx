import { ToastNotification } from '@carbon/react'
import { createContext, useCallback, useContext, useMemo, useRef, useState, type ReactNode } from 'react'

type Tipo = 'success' | 'error' | 'info'
interface Aviso {
  id: number
  tipo: Tipo
  titulo: string
  texto?: string
}

interface AvisosContexto {
  sucesso: (titulo: string, texto?: string) => void
  erro: (titulo: string, texto?: string) => void
  info: (titulo: string, texto?: string) => void
}

const Contexto = createContext<AvisosContexto | null>(null)

export function AvisosProvider({ children }: { children: ReactNode }) {
  const [avisos, setAvisos] = useState<Aviso[]>([])
  const proximoId = useRef(1)

  const remover = useCallback((id: number) => setAvisos((lista) => lista.filter((a) => a.id !== id)), [])

  const adicionar = useCallback(
    (tipo: Tipo, titulo: string, texto?: string) => {
      const id = proximoId.current++
      setAvisos((lista) => [...lista.slice(-2), { id, tipo, titulo, texto }])
      // Erros ficam mais tempo na tela para dar para ler.
      window.setTimeout(() => remover(id), tipo === 'error' ? 9000 : 4500)
    },
    [remover],
  )

  const valor = useMemo<AvisosContexto>(
    () => ({
      sucesso: (titulo, texto) => adicionar('success', titulo, texto),
      erro: (titulo, texto) => adicionar('error', titulo, texto),
      info: (titulo, texto) => adicionar('info', titulo, texto),
    }),
    [adicionar],
  )

  return (
    <Contexto.Provider value={valor}>
      {children}
      <div className="avisos" role="region" aria-label="Notificações" aria-live="polite">
        {avisos.map((aviso) => (
          <ToastNotification
            key={aviso.id}
            kind={aviso.tipo}
            title={aviso.titulo}
            subtitle={aviso.texto}
            timeout={0}
            lowContrast
            onClose={() => {
              remover(aviso.id)
              return false
            }}
          />
        ))}
      </div>
    </Contexto.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAvisos(): AvisosContexto {
  const contexto = useContext(Contexto)
  if (!contexto) throw new Error('useAvisos precisa estar dentro de <AvisosProvider>.')
  return contexto
}
