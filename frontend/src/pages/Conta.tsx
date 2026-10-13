import { Button, InlineNotification, PasswordInput, Tag } from '@carbon/react'
import { useState, type FormEvent } from 'react'
import { useAuth } from '../auth/AuthContext'
import { useAvisos } from '../components/Avisos'
import { PaginaCabecalho } from '../components/Comuns'
import { authApi } from '../services/api'
import { mensagemDeErro } from '../services/client'

export default function Conta() {
  const { usuario, sair } = useAuth()
  const avisos = useAvisos()
  const [atual, setAtual] = useState('')
  const [nova, setNova] = useState('')
  const [confirmacao, setConfirmacao] = useState('')
  const [erro, setErro] = useState('')
  const [salvando, setSalvando] = useState(false)

  async function trocar(evento: FormEvent) {
    evento.preventDefault()
    setErro('')

    if (!atual || !nova) {
      setErro('Preencha a senha atual e a nova senha.')
      return
    }
    if (nova.length < 8) {
      setErro('A nova senha deve ter pelo menos 8 caracteres.')
      return
    }
    if (nova !== confirmacao) {
      setErro('A confirmação não é igual à nova senha.')
      return
    }

    setSalvando(true)
    try {
      await authApi.alterarMinhaSenha(atual, nova)
      avisos.sucesso('Senha alterada')
      setAtual('')
      setNova('')
      setConfirmacao('')
    } catch (e) {
      setErro(mensagemDeErro(e))
    } finally {
      setSalvando(false)
    }
  }

  return (
    <>
      <PaginaCabecalho titulo="Minha conta" />

      <div className="cartao" style={{ maxWidth: '32rem' }}>
        <p>
          <strong>{usuario?.nome}</strong>
          <br />
          <span className="suave">{usuario?.email}</span>
        </p>
        <Tag type={usuario?.tipo === 'AdminTotal' ? 'cyan' : 'gray'} size="sm">
          {usuario?.tipo === 'AdminTotal' ? 'Administrador' : 'Sócio'}
        </Tag>
      </div>

      <section className="secao" style={{ maxWidth: '32rem' }}>
        <h2>Trocar senha</h2>
        <form className="formulario" onSubmit={trocar} noValidate>
          {erro && <InlineNotification kind="error" lowContrast hideCloseButton title="Não foi possível trocar" subtitle={erro} />}
          <PasswordInput
            id="senha-atual"
            labelText="Senha atual"
            autoComplete="current-password"
            value={atual}
            onChange={(e) => setAtual(e.target.value)}
            hidePasswordLabel="Mostrar senha"
            showPasswordLabel="Ocultar senha"
          />
          <PasswordInput
            id="senha-nova"
            labelText="Nova senha"
            helperText="Mínimo de 8 caracteres."
            autoComplete="new-password"
            value={nova}
            onChange={(e) => setNova(e.target.value)}
            hidePasswordLabel="Mostrar senha"
            showPasswordLabel="Ocultar senha"
          />
          <PasswordInput
            id="senha-confirmacao"
            labelText="Confirmar nova senha"
            autoComplete="new-password"
            value={confirmacao}
            onChange={(e) => setConfirmacao(e.target.value)}
            hidePasswordLabel="Mostrar senha"
            showPasswordLabel="Ocultar senha"
          />
          <div className="acoes">
            <Button type="submit" disabled={salvando}>
              {salvando ? 'Salvando…' : 'Trocar senha'}
            </Button>
            <Button kind="tertiary" type="button" onClick={sair}>
              Sair
            </Button>
          </div>
        </form>
      </section>
    </>
  )
}
