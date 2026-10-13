import { useCallback, useEffect, useState } from 'react'
import { mensagemDeErro } from '../services/client'

// true quando a media query casa (ex.: '(min-width: 1056px)').
export function useMediaQuery(consulta: string): boolean {
  const [casa, setCasa] = useState(() => window.matchMedia(consulta).matches)

  useEffect(() => {
    const lista = window.matchMedia(consulta)
    const atualizar = () => setCasa(lista.matches)
    atualizar()
    lista.addEventListener('change', atualizar)
    return () => lista.removeEventListener('change', atualizar)
  }, [consulta])

  return casa
}

// Atrasa a propagação de um valor (busca digitada, por exemplo).
export function useDebounce<T>(valor: T, atrasoMs = 350): T {
  const [atrasado, setAtrasado] = useState(valor)

  useEffect(() => {
    const id = window.setTimeout(() => setAtrasado(valor), atrasoMs)
    return () => window.clearTimeout(id)
  }, [valor, atrasoMs])

  return atrasado
}

// Carrega dados assíncronos reagindo a dependências. Mantém o dado anterior enquanto recarrega
// (a tela não "pisca" vazia) e ignora respostas de requisições que já ficaram para trás.
export function useCarregar<T>(buscar: () => Promise<T>, dependencias: readonly unknown[]) {
  const [dados, setDados] = useState<T | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [rodada, setRodada] = useState(0)

  useEffect(() => {
    let ativo = true
    setCarregando(true)
    setErro('')

    buscar()
      .then((resultado) => {
        if (ativo) setDados(resultado)
      })
      .catch((e: unknown) => {
        if (ativo) setErro(mensagemDeErro(e))
      })
      .finally(() => {
        if (ativo) setCarregando(false)
      })

    return () => {
      ativo = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencias, rodada])

  const recarregar = useCallback(() => setRodada((r) => r + 1), [])

  return { dados, carregando, erro, recarregar }
}
