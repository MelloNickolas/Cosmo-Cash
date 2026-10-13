import {
  Button,
  Modal,
  PasswordInput,
  Select,
  SelectItem,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Tag,
  TextInput,
} from '@carbon/react'
import { Add, Edit, Password, Renew, TrashCan } from '@carbon/icons-react'
import { useState } from 'react'
import { useAuth } from '../auth/AuthContext'
import { useAvisos } from '../components/Avisos'
import { ConfirmarModal, ErroCarregar, PaginaCabecalho, Vazio } from '../components/Comuns'
import { CampoBusca, EsqueletoLista, Paginacao, SeletorAtivo } from '../components/ListaUtils'
import { ativoDoFiltro, type FiltroAtivo } from '../utils/filtros'
import { usuarioApi } from '../services/api'
import { mensagemDeErro } from '../services/client'
import type { TipoUsuario, Usuario } from '../services/types'
import { useCarregar, useDebounce } from '../utils/hooks'

const rotuloTipo = (tipo: TipoUsuario) => (tipo === 'AdminTotal' ? 'Administrador' : 'Sócio')
const FORM_VAZIO = { nome: '', email: '', senha: '', tipo: 'Socio' as TipoUsuario }

export default function Usuarios() {
  const { usuario: eu } = useAuth()
  const avisos = useAvisos()
  const [busca, setBusca] = useState('')
  const [filtro, setFiltro] = useState<FiltroAtivo>('ativos')
  const [pagina, setPagina] = useState(1)
  const [tamanho, setTamanho] = useState(10)
  const buscaAtrasada = useDebounce(busca)

  const [editando, setEditando] = useState<Usuario | 'novo' | null>(null)
  const [form, setForm] = useState(FORM_VAZIO)
  const [erroForm, setErroForm] = useState('')
  const [salvando, setSalvando] = useState(false)

  const [senhaDe, setSenhaDe] = useState<Usuario | null>(null)
  const [novaSenha, setNovaSenha] = useState('')
  const [erroSenha, setErroSenha] = useState('')

  const [alvo, setAlvo] = useState<{ usuario: Usuario; acao: 'desativar' | 'reativar' } | null>(null)
  const [processando, setProcessando] = useState(false)

  const lista = useCarregar(
    () => usuarioApi.listar({ pagina, tamanhoPagina: tamanho, busca: buscaAtrasada, ativo: ativoDoFiltro(filtro) }),
    [pagina, tamanho, buscaAtrasada, filtro],
  )

  function abrirNovo() {
    setForm(FORM_VAZIO)
    setErroForm('')
    setEditando('novo')
  }

  function abrirEdicao(u: Usuario) {
    setForm({ nome: u.nome, email: u.email, senha: '', tipo: u.tipo })
    setErroForm('')
    setEditando(u)
  }

  async function salvar() {
    if (!form.nome.trim() || !form.email.trim()) {
      setErroForm('Informe nome e e-mail.')
      return
    }
    if (editando === 'novo' && form.senha.length < 8) {
      setErroForm('A senha deve ter pelo menos 8 caracteres.')
      return
    }

    setSalvando(true)
    setErroForm('')
    try {
      if (editando === 'novo') {
        await usuarioApi.criar({ nome: form.nome, email: form.email, senha: form.senha, tipo: form.tipo })
      } else if (editando) {
        await usuarioApi.atualizar(editando.id, { nome: form.nome, email: form.email, tipo: form.tipo })
      }

      avisos.sucesso(editando === 'novo' ? 'Usuário criado' : 'Usuário atualizado')
      setEditando(null)
      lista.recarregar()
    } catch (e) {
      setErroForm(mensagemDeErro(e))
    } finally {
      setSalvando(false)
    }
  }

  async function redefinirSenha() {
    if (!senhaDe) return
    if (novaSenha.length < 8) {
      setErroSenha('A senha deve ter pelo menos 8 caracteres.')
      return
    }

    setSalvando(true)
    setErroSenha('')
    try {
      await usuarioApi.alterarSenha(senhaDe.id, novaSenha)
      avisos.sucesso('Senha redefinida', `A nova senha de ${senhaDe.nome} já vale.`)
      setSenhaDe(null)
      setNovaSenha('')
    } catch (e) {
      setErroSenha(mensagemDeErro(e))
    } finally {
      setSalvando(false)
    }
  }

  async function confirmarAlvo() {
    if (!alvo) return
    setProcessando(true)
    try {
      if (alvo.acao === 'desativar') await usuarioApi.desativar(alvo.usuario.id)
      else await usuarioApi.reativar(alvo.usuario.id)

      avisos.sucesso(alvo.acao === 'desativar' ? 'Usuário desativado' : 'Usuário reativado')
      setAlvo(null)
      lista.recarregar()
    } catch (e) {
      avisos.erro('Não foi possível concluir', mensagemDeErro(e))
      setAlvo(null)
    } finally {
      setProcessando(false)
    }
  }

  const itens = lista.dados?.itens ?? []
  const souEu = (u: Usuario) => u.id === eu?.usuarioID

  const acoes = (u: Usuario) => (
    <>
      <Button kind="ghost" size="sm" hasIconOnly renderIcon={Edit} iconDescription="Editar" tooltipPosition="left" onClick={() => abrirEdicao(u)} />
      <Button
        kind="ghost"
        size="sm"
        hasIconOnly
        renderIcon={Password}
        iconDescription="Redefinir senha"
        tooltipPosition="left"
        onClick={() => {
          setNovaSenha('')
          setErroSenha('')
          setSenhaDe(u)
        }}
      />
      <Button
        kind="ghost"
        size="sm"
        hasIconOnly
        renderIcon={u.ativo ? TrashCan : Renew}
        iconDescription={souEu(u) ? 'Você não pode desativar a si mesmo' : u.ativo ? 'Desativar' : 'Reativar'}
        tooltipPosition="left"
        disabled={souEu(u)}
        onClick={() => setAlvo({ usuario: u, acao: u.ativo ? 'desativar' : 'reativar' })}
      />
    </>
  )

  return (
    <>
      <PaginaCabecalho titulo="Usuários" subtitulo="Quem pode entrar no sistema. Só o administrador vê esta página.">
        <Button renderIcon={Add} onClick={abrirNovo}>
          Novo usuário
        </Button>
      </PaginaCabecalho>

      <div className="filtros">
        <CampoBusca
          valor={busca}
          aoMudar={(v) => {
            setBusca(v)
            setPagina(1)
          }} placeholder="Buscar por nome ou e-mail" />
        <SeletorAtivo
          valor={filtro}
          aoMudar={(v) => {
            setFiltro(v)
            setPagina(1)
          }}
        />
      </div>

      {lista.erro && <ErroCarregar mensagem={lista.erro} aoTentar={lista.recarregar} />}
      {!lista.erro && lista.carregando && !lista.dados && <EsqueletoLista />}

      {!lista.erro && lista.dados && itens.length === 0 && (
        <Vazio titulo="Nenhum usuário encontrado" texto="Tente outra busca ou outro filtro." />
      )}

      {itens.length > 0 && (
        <>
          <div className="so-desktop">
            <Table size="lg" aria-label="Lista de usuários">
              <TableHead>
                <TableRow>
                  <TableHeader>Nome</TableHeader>
                  <TableHeader>E-mail</TableHeader>
                  <TableHeader>Perfil</TableHeader>
                  <TableHeader>Situação</TableHeader>
                  <TableHeader>
                    <span className="cds--visually-hidden">Ações</span>
                  </TableHeader>
                </TableRow>
              </TableHead>
              <TableBody>
                {itens.map((u) => (
                  <TableRow key={u.id}>
                    <TableCell>
                      {u.nome} {souEu(u) && <span className="suave">(você)</span>}
                    </TableCell>
                    <TableCell>{u.email}</TableCell>
                    <TableCell>
                      <Tag type={u.tipo === 'AdminTotal' ? 'cyan' : 'gray'} size="sm">
                        {rotuloTipo(u.tipo)}
                      </Tag>
                    </TableCell>
                    <TableCell>
                      <Tag type={u.ativo ? 'green' : 'gray'} size="sm">
                        {u.ativo ? 'Ativo' : 'Desativado'}
                      </Tag>
                    </TableCell>
                    <TableCell className="num">{acoes(u)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          <div className="so-mobile lista-cartoes">
            {itens.map((u) => (
              <article key={u.id} className={`item-cartao${u.ativo ? '' : ' inativo'}`}>
                <div className="linha">
                  <span className="titulo">
                    {u.nome} {souEu(u) && <span className="suave">(você)</span>}
                  </span>
                  <Tag type={u.tipo === 'AdminTotal' ? 'cyan' : 'gray'} size="sm">
                    {rotuloTipo(u.tipo)}
                  </Tag>
                </div>
                <div className="secundario">{u.email}</div>
                {!u.ativo && (
                  <div>
                    <Tag type="gray" size="sm">
                      Desativado
                    </Tag>
                  </div>
                )}
                <div className="acoes-cartao">{acoes(u)}</div>
              </article>
            ))}
          </div>

          <Paginacao
            pagina={pagina}
            tamanho={tamanho}
            total={lista.dados?.total ?? 0}
            aoMudar={(p, t) => {
              setTamanho(t)
              setPagina(p)
            }}
          />
        </>
      )}

      <Modal
        open={editando !== null}
        size="sm"
        modalHeading={editando === 'novo' ? 'Novo usuário' : 'Editar usuário'}
        primaryButtonText={salvando ? 'Salvando…' : 'Salvar'}
        secondaryButtonText="Cancelar"
        primaryButtonDisabled={salvando}
        onRequestSubmit={salvar}
        onRequestClose={() => !salvando && setEditando(null)}
      >
        <div className="formulario">
          <TextInput
            id="u-nome"
            labelText="Nome"
            value={form.nome}
            onChange={(e) => setForm({ ...form, nome: e.target.value })}
            maxLength={120}
          />
          <TextInput
            id="u-email"
            labelText="E-mail"
            type="email"
            inputMode="email"
            autoCapitalize="none"
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
          />
          {editando === 'novo' && (
            <PasswordInput
              id="u-senha"
              labelText="Senha inicial"
              helperText="Mínimo de 8 caracteres. O usuário pode trocá-la depois em Minha conta."
              autoComplete="new-password"
              value={form.senha}
              onChange={(e) => setForm({ ...form, senha: e.target.value })}
              hidePasswordLabel="Mostrar senha"
              showPasswordLabel="Ocultar senha"
            />
          )}
          <Select
            id="u-tipo"
            labelText="Perfil"
            helperText="O sócio faz tudo, menos gerenciar usuários."
            value={form.tipo}
            onChange={(e: React.ChangeEvent<HTMLSelectElement>) =>
              setForm({ ...form, tipo: e.target.value as TipoUsuario })
            }
          >
            <SelectItem value="Socio" text="Sócio" />
            <SelectItem value="AdminTotal" text="Administrador" />
          </Select>
          {erroForm && (
            <p role="alert" className="negativo">
              {erroForm}
            </p>
          )}
        </div>
      </Modal>

      <Modal
        open={senhaDe !== null}
        size="xs"
        modalHeading={`Redefinir senha de ${senhaDe?.nome ?? ''}`}
        primaryButtonText={salvando ? 'Salvando…' : 'Redefinir'}
        secondaryButtonText="Cancelar"
        primaryButtonDisabled={salvando}
        onRequestSubmit={redefinirSenha}
        onRequestClose={() => !salvando && setSenhaDe(null)}
      >
        <div className="formulario">
          <PasswordInput
            id="u-nova-senha"
            labelText="Nova senha"
            helperText="Mínimo de 8 caracteres."
            autoComplete="new-password"
            value={novaSenha}
            onChange={(e) => setNovaSenha(e.target.value)}
            hidePasswordLabel="Mostrar senha"
            showPasswordLabel="Ocultar senha"
          />
          {erroSenha && (
            <p role="alert" className="negativo">
              {erroSenha}
            </p>
          )}
        </div>
      </Modal>

      <ConfirmarModal
        aberto={alvo !== null}
        perigo={alvo?.acao === 'desativar'}
        carregando={processando}
        titulo={alvo?.acao === 'desativar' ? 'Desativar usuário?' : 'Reativar usuário?'}
        texto={
          alvo?.acao === 'desativar'
            ? `${alvo.usuario.nome} perde o acesso na hora. As vendas registradas por ele continuam no histórico.`
            : `${alvo?.usuario.nome} volta a poder entrar no sistema.`
        }
        rotuloConfirmar={alvo?.acao === 'desativar' ? 'Desativar' : 'Reativar'}
        aoConfirmar={confirmarAlvo}
        aoCancelar={() => setAlvo(null)}
      />
    </>
  )
}
