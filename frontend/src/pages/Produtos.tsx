import { Button, Modal, Table, TableBody, TableCell, TableHead, TableHeader, TableRow, Tag, TextInput } from '@carbon/react'
import { Add, Edit, Renew, TrashCan } from '@carbon/icons-react'
import { useState } from 'react'
import { useAvisos } from '../components/Avisos'
import { ConfirmarModal, ErroCarregar, PaginaCabecalho, Vazio } from '../components/Comuns'
import { CampoBusca, EsqueletoLista, Paginacao, SeletorAtivo } from '../components/ListaUtils'
import { ativoDoFiltro, type FiltroAtivo } from '../utils/filtros'
import { produtoApi } from '../services/api'
import { mensagemDeErro } from '../services/client'
import type { Produto, ProdutoForm } from '../services/types'
import { useCarregar, useDebounce } from '../utils/hooks'

const VARIACOES_PADRAO = ['Eau de Parfum', 'Eau de Toilette', 'Parfum', 'Extrait de Parfum']
const FORM_VAZIO = { marca: '', descricao: '', volumeMl: '', variacao: '' }

export default function Produtos() {
  const avisos = useAvisos()
  const [busca, setBusca] = useState('')
  const [filtro, setFiltro] = useState<FiltroAtivo>('ativos')
  const [pagina, setPagina] = useState(1)
  const [tamanho, setTamanho] = useState(10)
  const buscaAtrasada = useDebounce(busca)

  const [editando, setEditando] = useState<Produto | 'novo' | null>(null)
  const [form, setForm] = useState(FORM_VAZIO)
  const [erroForm, setErroForm] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [alvo, setAlvo] = useState<{ produto: Produto; acao: 'desativar' | 'reativar' } | null>(null)
  const [processando, setProcessando] = useState(false)

  const lista = useCarregar(
    () => produtoApi.listar({ pagina, tamanhoPagina: tamanho, busca: buscaAtrasada, ativo: ativoDoFiltro(filtro) }),
    [pagina, tamanho, buscaAtrasada, filtro],
  )
  const marcas = useCarregar(() => produtoApi.marcas(), [lista.dados?.total])
  const variacoes = useCarregar(() => produtoApi.variacoes(), [lista.dados?.total])

  const sugestoesVariacao = Array.from(new Set([...(variacoes.dados ?? []), ...VARIACOES_PADRAO]))

  function abrirNovo() {
    setForm(FORM_VAZIO)
    setErroForm('')
    setEditando('novo')
  }

  function abrirEdicao(produto: Produto) {
    setForm({
      marca: produto.marca,
      descricao: produto.descricao,
      volumeMl: String(produto.volumeMl),
      variacao: produto.variacao,
    })
    setErroForm('')
    setEditando(produto)
  }

  async function salvar() {
    const volume = Number(form.volumeMl)
    if (!form.marca.trim() || !form.descricao.trim() || !form.variacao.trim()) {
      setErroForm('Preencha marca, descrição e variação.')
      return
    }
    if (!Number.isInteger(volume) || volume <= 0) {
      setErroForm('Informe o volume em ml (número inteiro maior que zero).')
      return
    }

    const dados: ProdutoForm = {
      marca: form.marca.trim(),
      descricao: form.descricao.trim(),
      volumeMl: volume,
      variacao: form.variacao.trim(),
    }

    setSalvando(true)
    setErroForm('')
    try {
      if (editando === 'novo') await produtoApi.criar(dados)
      else if (editando) await produtoApi.atualizar(editando.id, dados)

      avisos.sucesso(editando === 'novo' ? 'Produto cadastrado' : 'Produto atualizado')
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
      if (alvo.acao === 'desativar') await produtoApi.desativar(alvo.produto.id)
      else await produtoApi.reativar(alvo.produto.id)

      avisos.sucesso(alvo.acao === 'desativar' ? 'Produto desativado' : 'Produto reativado')
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
      <PaginaCabecalho titulo="Produtos" subtitulo="Perfumes cadastrados. O preço é informado a cada venda.">
        <Button renderIcon={Add} onClick={abrirNovo}>
          Novo produto
        </Button>
      </PaginaCabecalho>

      <div className="filtros">
        <CampoBusca
          valor={busca}
          aoMudar={(v) => {
            setBusca(v)
            setPagina(1)
          }} placeholder="Buscar por marca ou descrição" />
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
          titulo={buscaAtrasada || filtro !== 'ativos' ? 'Nenhum produto encontrado' : 'Nenhum produto cadastrado'}
          texto={buscaAtrasada ? 'Tente outra busca.' : 'Cadastre o primeiro perfume para começar a registrar vendas.'}
        >
          {!buscaAtrasada && filtro === 'ativos' && (
            <Button renderIcon={Add} onClick={abrirNovo}>
              Cadastrar produto
            </Button>
          )}
        </Vazio>
      )}

      {itens.length > 0 && (
        <>
          <div className="so-desktop">
            <Table size="lg" aria-label="Lista de produtos">
              <TableHead>
                <TableRow>
                  <TableHeader>Marca</TableHeader>
                  <TableHeader>Descrição</TableHeader>
                  <TableHeader>Variação</TableHeader>
                  <TableHeader className="num">Volume</TableHeader>
                  <TableHeader>Situação</TableHeader>
                  <TableHeader>
                    <span className="cds--visually-hidden">Ações</span>
                  </TableHeader>
                </TableRow>
              </TableHead>
              <TableBody>
                {itens.map((p) => (
                  <TableRow key={p.id}>
                    <TableCell>{p.marca}</TableCell>
                    <TableCell>{p.descricao}</TableCell>
                    <TableCell>{p.variacao}</TableCell>
                    <TableCell className="num">{p.volumeMl} ml</TableCell>
                    <TableCell>
                      <Tag type={p.ativo ? 'green' : 'gray'} size="sm">
                        {p.ativo ? 'Ativo' : 'Desativado'}
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
                        onClick={() => abrirEdicao(p)}
                      />
                      <Button
                        kind="ghost"
                        size="sm"
                        hasIconOnly
                        renderIcon={p.ativo ? TrashCan : Renew}
                        iconDescription={p.ativo ? 'Desativar' : 'Reativar'}
                        tooltipPosition="left"
                        onClick={() => setAlvo({ produto: p, acao: p.ativo ? 'desativar' : 'reativar' })}
                      />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          <div className="so-mobile lista-cartoes">
            {itens.map((p) => (
              <article key={p.id} className={`item-cartao${p.ativo ? '' : ' inativo'}`}>
                <div className="linha">
                  <span className="titulo">{p.marca}</span>
                  <span className="secundario">{p.volumeMl} ml</span>
                </div>
                <div>{p.descricao}</div>
                <div className="linha">
                  <span className="secundario">{p.variacao}</span>
                  {!p.ativo && (
                    <Tag type="gray" size="sm">
                      Desativado
                    </Tag>
                  )}
                </div>
                <div className="acoes-cartao">
                  <Button kind="tertiary" size="sm" renderIcon={Edit} onClick={() => abrirEdicao(p)}>
                    Editar
                  </Button>
                  <Button
                    kind="ghost"
                    size="sm"
                    renderIcon={p.ativo ? TrashCan : Renew}
                    onClick={() => setAlvo({ produto: p, acao: p.ativo ? 'desativar' : 'reativar' })}
                  >
                    {p.ativo ? 'Desativar' : 'Reativar'}
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
        modalHeading={editando === 'novo' ? 'Novo produto' : 'Editar produto'}
        primaryButtonText={salvando ? 'Salvando…' : 'Salvar'}
        secondaryButtonText="Cancelar"
        primaryButtonDisabled={salvando}
        onRequestSubmit={salvar}
        onRequestClose={() => !salvando && setEditando(null)}
      >
        <div className="formulario">
          <TextInput
            id="p-marca"
            labelText="Marca"
            list="lista-marcas"
            autoComplete="off"
            value={form.marca}
            onChange={(e) => setForm({ ...form, marca: e.target.value })}
            maxLength={80}
          />
          <datalist id="lista-marcas">
            {(marcas.dados ?? []).map((m) => (
              <option key={m} value={m} />
            ))}
          </datalist>

          <TextInput
            id="p-descricao"
            labelText="Descrição"
            placeholder="Ex.: Sauvage"
            value={form.descricao}
            onChange={(e) => setForm({ ...form, descricao: e.target.value })}
            maxLength={200}
          />

          <div className="linha-campos">
            <TextInput
              id="p-volume"
              labelText="Volume (ml)"
              type="number"
              inputMode="numeric"
              min={1}
              value={form.volumeMl}
              onChange={(e) => setForm({ ...form, volumeMl: e.target.value })}
            />
            <TextInput
              id="p-variacao"
              labelText="Variação"
              list="lista-variacoes"
              autoComplete="off"
              placeholder="Ex.: Eau de Parfum"
              value={form.variacao}
              onChange={(e) => setForm({ ...form, variacao: e.target.value })}
              maxLength={60}
            />
            <datalist id="lista-variacoes">
              {sugestoesVariacao.map((v) => (
                <option key={v} value={v} />
              ))}
            </datalist>
          </div>

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
        titulo={alvo?.acao === 'desativar' ? 'Desativar produto?' : 'Reativar produto?'}
        texto={
          alvo?.acao === 'desativar'
            ? `"${alvo.produto.marca} ${alvo.produto.descricao}" deixa de aparecer em novas vendas. As vendas já feitas continuam como estão.`
            : `"${alvo?.produto.marca} ${alvo?.produto.descricao}" volta a poder ser usado em novas vendas.`
        }
        rotuloConfirmar={alvo?.acao === 'desativar' ? 'Desativar' : 'Reativar'}
        aoConfirmar={confirmarAlvo}
        aoCancelar={() => setAlvo(null)}
      />
    </>
  )
}
