import { Button, InlineNotification, PasswordInput, TextInput } from '@carbon/react'
import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { NOME_LOJA } from '../config'
import { mensagemDeErro } from '../services/client'

export default function Login() {
  const { usuario, entrar, sessaoExpirou } = useAuth()
  const navegar = useNavigate()
  const local = useLocation()
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [erro, setErro] = useState('')
  const [enviando, setEnviando] = useState(false)

  const destino = (local.state as { de?: string } | null)?.de ?? '/'
  if (usuario) return <Navigate to={destino} replace />

  async function enviar(evento: FormEvent) {
    evento.preventDefault()
    if (enviando) return

    setErro('')
    if (!email.trim() || !senha) {
      setErro('Informe seu e-mail e sua senha.')
      return
    }

    setEnviando(true)
    try {
      await entrar(email.trim(), senha)
      navegar(destino, { replace: true })
    } catch (e) {
      setErro(mensagemDeErro(e))
      setEnviando(false)
    }
  }

  return (
    <div className="login">
      <form className="login-cartao" onSubmit={enviar} noValidate>
        <div className="logo" aria-hidden="true">
          🪐
        </div>
        <h1>{NOME_LOJA}</h1>
        <p>Entre para acessar o painel e as vendas.</p>

        {sessaoExpirou && !erro && (
          <InlineNotification
            kind="info"
            lowContrast
            hideCloseButton
            title="Sessão encerrada"
            subtitle="Entre novamente para continuar."
          />
        )}
        {erro && <InlineNotification kind="error" lowContrast hideCloseButton title="Não foi possível entrar" subtitle={erro} />}

        <div className="formulario">
          <TextInput
            id="email"
            labelText="E-mail"
            type="email"
            autoComplete="username"
            inputMode="email"
            autoCapitalize="none"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoFocus
          />
          <PasswordInput
            id="senha"
            labelText="Senha"
            autoComplete="current-password"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            hidePasswordLabel="Mostrar senha"
            showPasswordLabel="Ocultar senha"
          />
          <Button type="submit" disabled={enviando} style={{ width: '100%', maxWidth: '100%' }}>
            {enviando ? 'Entrando…' : 'Entrar'}
          </Button>
        </div>
      </form>
    </div>
  )
}
