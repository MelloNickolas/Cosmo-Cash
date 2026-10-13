import { Button, Modal, Table, TableBody, TableCell, TableHead, TableHeader, TableRow, Tag, TextInput } from '@carbon/react'
import { Add, Edit, Renew, TrashCan } from '@carbon/icons-react'
import { useState } from 'react'
import { useAvisos } from '../components/Avisos'
import { ConfirmarModal, ErroCarregar, PaginaCabecalho, Vazio } from '../components/Comuns'
import { CampoBusca, EsqueletoLista, Paginacao, SeletorAtivo } from '../components/ListaUtils'
import { ativoDoFiltro, type FiltroAtivo } from '../utils/filtros'
import { clienteApi } from '../services/api'
import { mensagemDeErro } from '../services/client'
import type { ClienteForm, ClienteResumo } from '../services/types'
import { useCarregar, useDebounce } from '../utils/hooks'
import { formatarTelefone, mascararCep, mascararCpf, mascararTelefone } from '../utils/format'

const FORM_VAZIO: ClienteForm = { nome: '', cpf: '', telefone: '', cep: '', endereco: '' }

export default function Clientes() {
  const avisos = useAvisos()
  const [busca, setBusca] = useState('')
  const [filtro, setFiltro] = useState<FiltroAtivo>('ativos')
  const [pagina, setPagina] = useState(1)
  const [tamanho, setTamanho] = useState(10)
  const buscaAtrasada = useDebounce(busca)

  const [editando, setEditando] = useState<number | 'novo' | null>(null)
  const [abrindo, setAbrindo] = useState(false)
  const [form, setForm] = useState<ClienteForm>(FORM_VAZIO)
  const [erroForm, setErroForm] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [alvo, setAlvo] = useState<{ cliente: ClienteResumo; acao: 'desativar' | 'reativar' } | null>(null)
  const [processando, setProcessando] = useState(false)

  const lista = useCarregar(
    () => clienteApi.listar({ pagina, tamanhoPagina: tamanho, busca: buscaAtrasada, ativo: ativoDoFiltro(filtro) }),
    [pagina, tamanho, buscaAtrasada, filtro],
  )

  function abrirNovo() {
    setForm(FORM_VAZIO)
    setErroForm('')
    setEditando('novo')
  }

  // O CPF completo só vem no detalhe; a listagem mostra mascarado (LGPD).
  async function abrirEdicao(resumo: ClienteResumo) {
    setAbrindo(true)
    try {
      const c = await clienteApi.obter(resumo.id)
      setForm({
        nome: c.nome,
        cpf: mascararCpf(c.cpf ?? ''),
        telefone: mascararTelefone(c.telefone ?? ''),
        cep: mascararCep(c.cep ?? ''),
        endereco: c.endereco ?? '',
      })
      setErroForm('')
      setEditando(c.id)
    } catch (e) {
      avisos.erro('Não foi possível abrir o cliente', mensagemDeErro(e))
    } finally {
      setAbrindo(false)
    }
  }

  async function salvar() {
    if (!form.nome.trim()) {
      setErroForm('Informe o nome do cliente.')
      return
    }

    setSalvando(true)
    setErroForm('')
    try {
      if (editando === 'novo') await clienteApi.criar(form)
      else if (typeof editando === 'number') await clienteApi.atualizar(editando, form)

      avisos.sucesso(editando === 'novo' ? 'Cliente cadastrado' : 'Cliente atualizado')
      setEditando(null)
      lista.recarregar()
    } catch (e) {
      setErroForm(mensagemDeErro(e))
    } finally {
      setSalvando(false)
    }
  }

  async function confirmarAlvo() {
    if (!alvo) return
    setProcessando(true)
    try {
      if (alvo.acao === 'desativar') await clienteApi.desativar(alvo.cliente.id)
      else await clienteApi.reativar(alvo.cliente.id)

      avisos.sucesso(alvo.acao === 'desativar' ? 'Cliente desativado' : 'Cliente reativado')
      setAlvo(null)
      lista.recarregar()
    } catch (e) {
      avisos.erro('Não foi possível concluir', mensagemDeErro(e))
    } finally {
      setProcessando(false)
    }
  }

  const itens = lista.dados?.itens ?? []

  return (
    <>
      <PaginaCabecalho titulo="Clientes" subtitulo="Só o nome é obrigatório. O CPF aparece mascarado na lista.">
        <Button renderIcon={Add} onClick={abrirNovo}>
          Novo cliente
        </Button>
      </PaginaCabecalho>

      <div className="filtros">
        <CampoBusca
          valor={busca}
          aoMudar={(v) => {
            setBusca(v)
            setPagina(1)
          }} placeholder="Buscar por nome, CPF ou telefone" />
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
        <Vazio
          titulo={buscaAtrasada || filtro !== 'ativos' ? 'Nenhum cliente encontrado' : 'Nenhum cliente cadastrado'}
          texto={buscaAtrasada ? 'Tente outra busca.' : 'Clientes são opcionais nas vendas, mas ajudam a manter o histórico.'}
        >
          {!buscaAtrasada && filtro === 'ativos' && (
            <Button renderIcon={Add} onClick={abrirNovo}>
              Cadastrar cliente
            </Button>
          )}
        </Vazio>
      )}

      {itens.length > 0 && (
        <>
          <div className="so-desktop">
            <Table size="lg" aria-label="Lista de clientes">
              <TableHead>
                <TableRow>
                  <TableHeader>Nome</TableHeader>
                  <TableHeader>CPF</TableHeader>
                  <TableHeader>Telefone</TableHeader>
                  <TableHeader>Situação</TableHeader>
                  <TableHeader>
                    <span className="cds--visually-hidden">Ações</span>
                  </TableHeader>
                </TableRow>
              </TableHead>
              <TableBody>
                {itens.map((c) => (
                  <TableRow key={c.id}>
                    <TableCell>{c.nome}</TableCell>
                    <TableCell>{c.cpfMascarado ?? '—'}</TableCell>
                    <TableCell>{c.telefone ? formatarTelefone(c.telefone) : '—'}</TableCell>
                    <TableCell>
                      <Tag type={c.ativo ? 'green' : 'gray'} size="sm">
                        {c.ativo ? 'Ativo' : 'Desativado'}
                      </Tag>
                    </TableCell>
                    <TableCell className="num">
                      <Button
                        kind="ghost"
                        size="sm"
                        hasIconOnly
                        renderIcon={Edit}
                        iconDescription="Editar"
                        tooltipPosition="left"
                        disabled={abrindo}
                        onClick={() => abrirEdicao(c)}
                      />
                      <Button
                        kind="ghost"
                        size="sm"
                        hasIconOnly
                        renderIcon={c.ativo ? TrashCan : Renew}
                        iconDescription={c.ativo ? 'Desativar' : 'Reativar'}
                        tooltipPosition="left"
                        onClick={() => setAlvo({ cliente: c, acao: c.ativo ? 'desativar' : 'reativar' })}
                      />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          <div className="so-mobile lista-cartoes">
            {itens.map((c) => (
              <article key={c.id} className={`item-cartao${c.ativo ? '' : ' inativo'}`}>
                <div className="linha">
                  <span className="titulo">{c.nome}</span>
                  {!c.ativo && (
                    <Tag type="gray" size="sm">
                      Desativado
                    </Tag>
                  )}
                </div>
                <div className="secundario">
                  {c.cpfMascarado ?? 'Sem CPF'}
                  {c.telefone ? ` · ${formatarTelefone(c.telefone)}` : ''}
                </div>
                <div className="acoes-cartao">
                  <Button kind="tertiary" size="sm" renderIcon={Edit} disabled={abrindo} onClick={() => abrirEdicao(c)}>
                    Editar
                  </Button>
                  <Button
                    kind="ghost"
                    size="sm"
                    renderIcon={c.ativo ? TrashCan : Renew}
                    onClick={() => setAlvo({ cliente: c, acao: c.ativo ? 'desativar' : 'reativar' })}
                  >
                    {c.ativo ? 'Desativar' : 'Reativar'}
                  </Button>
                </div>
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
        modalHeading={editando === 'novo' ? 'Novo cliente' : 'Editar cliente'}
        primaryButtonText={salvando ? 'Salvando…' : 'Salvar'}
        secondaryButtonText="Cancelar"
        primaryButtonDisabled={salvando}
        onRequestSubmit={salvar}
        onRequestClose={() => !salvando && setEditando(null)}
      >
        <div className="formulario">
          <TextInput
            id="c-nome"
            labelText="Nome"
            value={form.nome}
            onChange={(e) => setForm({ ...form, nome: e.target.value })}
            maxLength={120}
          />
          <div className="linha-campos">
            <TextInput
              id="c-cpf"
              labelText="CPF (opcional)"
              inputMode="numeric"
              placeholder="000.000.000-00"
              value={form.cpf}
              onChange={(e) => setForm({ ...form, cpf: mascararCpf(e.target.value) })}
            />
            <TextInput
              id="c-telefone"
              labelText="Telefone (opcional)"
              inputMode="tel"
              placeholder="(00) 00000-0000"
              value={form.telefone}
              onChange={(e) => setForm({ ...form, telefone: mascararTelefone(e.target.value) })}
            />
          </div>
          <TextInput
            id="c-cep"
            labelText="CEP (opcional)"
            inputMode="numeric"
            placeholder="00000-000"
            value={form.cep}
            onChange={(e) => setForm({ ...form, cep: mascararCep(e.target.value) })}
          />
          <TextInput
            id="c-endereco"
            labelText="Endereço completo (opcional)"
            value={form.endereco}
            onChange={(e) => setForm({ ...form, endereco: e.target.value })}
            maxLength={300}
          />
          {erroForm && (
            <p role="alert" className="negativo">
              {erroForm}
            </p>
          )}
        </div>
      </Modal>

      <ConfirmarModal
        aberto={alvo !== null}
        perigo={alvo?.acao === 'desativar'}
        carregando={processando}
        titulo={alvo?.acao === 'desativar' ? 'Desativar cliente?' : 'Reativar cliente?'}
        texto={
          alvo?.acao === 'desativar'
            ? `${alvo.cliente.nome} deixa de aparecer em novas vendas. O histórico de vendas dele continua.`
            : `${alvo?.cliente.nome} volta a poder ser usado em novas vendas.`
        }
        rotuloConfirmar={alvo?.acao === 'desativar' ? 'Desativar' : 'Reativar'}
        aoConfirmar={confirmarAlvo}
        aoCancelar={() => setAlvo(null)}
      />
    </>
  )
}
