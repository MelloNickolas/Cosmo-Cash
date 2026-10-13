import { Button, ComboBox, InlineNotification, Loading, TextInput } from '@carbon/react'
import { Add, TrashCan } from '@carbon/icons-react'
import { useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAvisos } from '../components/Avisos'
import { ConfirmarModal, ErroCarregar, PaginaCabecalho } from '../components/Comuns'
import { clienteApi, produtoApi, vendaApi } from '../services/api'
import { mensagemDeErro } from '../services/client'
import type { FormaPagamento, Produto, Venda, VendaForm as VendaFormDados } from '../services/types'
import { dataHora, diaISO, hojeISO, lerValor, moeda, valorParaCampo } from '../utils/format'
import { useCarregar, useDebounce } from '../utils/hooks'

interface ItemEstado {
  chave: number
  produto: Produto | null
  quantidade: string
  precoVenda: string
  precoCusto: string
}

interface OpcaoCliente {
  id: number
  nome: string
}

let proximaChave = 1
const novoItem = (): ItemEstado => ({ chave: proximaChave++, produto: null, quantidade: '1', precoVenda: '', precoCusto: '' })

const rotuloProduto = (p: Produto) => `${p.marca} · ${p.descricao} · ${p.volumeMl} ml · ${p.variacao}`

// Produto "reconstruído" a partir de um item de venda (o produto pode estar desativado hoje).
const produtoDoItem = (item: Venda['itens'][number]): Produto => ({
  id: item.produtoID,
  marca: item.marca,
  descricao: item.descricao,
  volumeMl: item.volumeMl,
  variacao: item.variacao,
  ativo: true,
  criadoEm: '',
})

// Carrega a venda (quando é edição) e só então monta o formulário, que já nasce com os valores iniciais.
export default function VendaForm() {
  const { id } = useParams()
  const editandoId = id ? Number(id) : null

  const venda = useCarregar(() => (editandoId ? vendaApi.obter(editandoId) : Promise.resolve(null)), [editandoId])

  if (editandoId && venda.carregando && !venda.dados) {
    return <Loading withOverlay={false} small description="Carregando venda" />
  }
  if (editandoId && venda.erro) return <ErroCarregar mensagem={venda.erro} aoTentar={venda.recarregar} />

  // A key refaz o formulário do zero quando a venda muda de situação (ex.: reativada).
  return (
    <FormularioVenda
      key={`${editandoId ?? 'nova'}-${venda.dados?.status ?? ''}`}
      vendaInicial={venda.dados}
      editandoId={editandoId}
      recarregarVenda={venda.recarregar}
    />
  )
}

function FormularioVenda({
  vendaInicial,
  editandoId,
  recarregarVenda,
}: {
  vendaInicial: Venda | null
  editandoId: number | null
  recarregarVenda: () => void
}) {
  const navegar = useNavigate()
  const avisos = useAvisos()

  const produtos = useCarregar(() => produtoApi.listarAtivos(), [])

  const [itens, setItens] = useState<ItemEstado[]>(() =>
    vendaInicial
      ? vendaInicial.itens.map((item) => ({
          chave: proximaChave++,
          produto: produtoDoItem(item),
          quantidade: String(item.quantidade),
          precoVenda: valorParaCampo(item.precoVenda),
          precoCusto: valorParaCampo(item.precoCusto),
        }))
      : [novoItem()],
  )
  const [forma, setForma] = useState<FormaPagamento>(vendaInicial?.formaPagamento ?? 'Pix')
  const [data, setData] = useState(vendaInicial ? diaISO(vendaInicial.dataVenda) : hojeISO())
  const [cliente, setCliente] = useState<OpcaoCliente | null>(
    vendaInicial?.cliente ? { id: vendaInicial.cliente.id, nome: vendaInicial.cliente.nome } : null,
  )
  const [buscaCliente, setBuscaCliente] = useState('')
  const buscaClienteAtrasada = useDebounce(buscaCliente, 300)
  const [tentou, setTentou] = useState(false)
  const [erroGeral, setErroGeral] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [confirmarCancelar, setConfirmarCancelar] = useState(false)

  const clientes = useCarregar(
    () => clienteApi.listar({ busca: buscaClienteAtrasada, ativo: true, tamanhoPagina: 20 }),
    [buscaClienteAtrasada],
  )

  // Opções do seletor de produto: ativos + os que já estão na venda (mesmo se desativados hoje).
  const opcoesProduto = useMemo(() => {
    const mapa = new Map<number, Produto>()
    for (const p of produtos.dados ?? []) mapa.set(p.id, p)
    for (const item of itens) if (item.produto && !mapa.has(item.produto.id)) mapa.set(item.produto.id, item.produto)
    return Array.from(mapa.values())
  }, [produtos.dados, itens])

  const opcoesCliente = useMemo(() => {
    const lista: OpcaoCliente[] = (clientes.dados?.itens ?? []).map((c) => ({ id: c.id, nome: c.nome }))
    if (cliente && !lista.some((c) => c.id === cliente.id)) lista.unshift(cliente)
    return lista
  }, [clientes.dados, cliente])

  const cancelada = vendaInicial?.status === 'Cancelada'

  /* ---------- cálculos ao vivo ---------- */

  const linhas = itens.map((item) => {
    const q = Number(item.quantidade)
    const pv = lerValor(item.precoVenda)
    const pc = lerValor(item.precoCusto)
    const quantidadeOk = Number.isInteger(q) && q >= 1 && q <= 999
    return {
      quantidadeOk,
      precoVendaOk: pv !== null && pv > 0,
      precoCustoOk: pc !== null && pc >= 0,
      q: quantidadeOk ? q : 0,
      pv: pv ?? 0,
      pc: pc ?? 0,
    }
  })

  const total = linhas.reduce((soma, l) => soma + l.pv * l.q, 0)
  const custo = linhas.reduce((soma, l) => soma + l.pc * l.q, 0)
  const lucro = total - custo

  function validar(): string | null {
    if (itens.length === 0) return 'Adicione pelo menos um item.'
    for (let i = 0; i < itens.length; i++) {
      const l = linhas[i]
      if (!itens[i].produto) return `Item ${i + 1}: escolha o produto.`
      if (!l.quantidadeOk) return `Item ${i + 1}: a quantidade deve estar entre 1 e 999.`
      if (!l.precoVendaOk) return `Item ${i + 1}: informe o preço de venda (maior que zero).`
      if (!l.precoCustoOk) return `Item ${i + 1}: informe o preço de custo (use 0 se não houver).`
    }
    return null
  }

  async function salvar() {
    setTentou(true)
    setErroGeral('')

    const problema = validar()
    if (problema) {
      setErroGeral(problema)
      return
    }

    const dados: VendaFormDados = {
      formaPagamento: forma,
      clienteID: cliente?.id ?? null,
      itens: itens.map((item, i) => ({
        produtoID: item.produto!.id,
        quantidade: linhas[i].q,
        precoVenda: linhas[i].pv,
        precoCusto: linhas[i].pc,
      })),
    }

    // "Sempre hoje": sem data, o servidor usa o instante atual. Só mandamos quando o dia foi alterado.
    const diaOriginal = vendaInicial ? diaISO(vendaInicial.dataVenda) : hojeISO()
    if (data && data !== diaOriginal) dados.dataVenda = data

    setSalvando(true)
    try {
      if (editandoId) {
        await vendaApi.atualizar(editandoId, dados)
        avisos.sucesso('Venda atualizada')
      } else {
        await vendaApi.criar(dados)
        avisos.sucesso('Venda registrada', moeda(total))
      }
      navegar('/vendas')
    } catch (e) {
      setErroGeral(mensagemDeErro(e))
      setSalvando(false)
    }
  }

  async function cancelarVenda() {
    if (!editandoId) return
    setSalvando(true)
    try {
      await vendaApi.cancelar(editandoId)
      avisos.sucesso('Venda cancelada')
      navegar('/vendas')
    } catch (e) {
      avisos.erro('Não foi possível cancelar', mensagemDeErro(e))
      setConfirmarCancelar(false)
      setSalvando(false)
    }
  }

  async function reativarVenda() {
    if (!editandoId) return
    setSalvando(true)
    try {
      await vendaApi.reativar(editandoId)
      avisos.sucesso('Venda reativada')
      recarregarVenda()
    } catch (e) {
      avisos.erro('Não foi possível reativar', mensagemDeErro(e))
    } finally {
      setSalvando(false)
    }
  }

  const atualizarItem = (chave: number, parcial: Partial<ItemEstado>) =>
    setItens((lista) => lista.map((i) => (i.chave === chave ? { ...i, ...parcial } : i)))

  const bloqueado = cancelada || salvando

  return (
    <>
      <PaginaCabecalho
        titulo={editandoId ? `Venda #${editandoId}` : 'Nova venda'}
        subtitulo={
          vendaInicial
            ? `${dataHora(vendaInicial.dataVenda)} · registrada por ${vendaInicial.usuario.nome}`
            : 'Informe os itens, o pagamento e confirme.'
        }
      >
        {editandoId && cancelada && (
          <Button kind="tertiary" onClick={reativarVenda} disabled={salvando}>
            Reativar venda
          </Button>
        )}
      </PaginaCabecalho>

      {cancelada && (
        <InlineNotification
          kind="warning"
          lowContrast
          hideCloseButton
          title="Venda cancelada"
          subtitle="Ela não entra nos totais do painel. Reative para poder editar."
        />
      )}

      {produtos.erro && <ErroCarregar mensagem={produtos.erro} aoTentar={produtos.recarregar} />}

      <form
        className="formulario"
        onSubmit={(e) => {
          e.preventDefault()
          void salvar()
        }}
        noValidate
      >
        <section aria-labelledby="titulo-itens">
          <h2 id="titulo-itens" className="secao-titulo">
            Itens
          </h2>

          <div className="itens-venda">
            {itens.map((item, i) => {
              const l = linhas[i]
              return (
                <div key={item.chave} className="item-venda">
                  <div className="produto">
                    <ComboBox
                      id={`produto-${item.chave}`}
                      titleText={`Produto ${itens.length > 1 ? i + 1 : ''}`.trim()}
                      placeholder={produtos.carregando ? 'Carregando produtos…' : 'Busque por marca, nome ou variação'}
                      items={opcoesProduto}
                      itemToString={(p: Produto | null) => (p ? rotuloProduto(p) : '')}
                      selectedItem={item.produto}
                      disabled={bloqueado}
                      invalid={tentou && !item.produto}
                      invalidText="Escolha o produto."
                      shouldFilterItem={({ item: p, inputValue }: { item: Produto; inputValue: string | null }) => {
                        const termo = (inputValue ?? '').toLowerCase().trim()
                        if (!termo) return true
                        return termo.split(/\s+/).every((parte) => rotuloProduto(p).toLowerCase().includes(parte))
                      }}
                      onChange={({ selectedItem }: { selectedItem: Produto | null | undefined }) =>
                        atualizarItem(item.chave, { produto: selectedItem ?? null })
                      }
                    />
                  </div>

                  <TextInput
                    id={`qtd-${item.chave}`}
                    labelText="Quantidade"
                    type="number"
                    inputMode="numeric"
                    min={1}
                    max={999}
                    value={item.quantidade}
                    disabled={bloqueado}
                    invalid={tentou && !l.quantidadeOk}
                    invalidText="1 a 999"
                    onChange={(e) => atualizarItem(item.chave, { quantidade: e.target.value })}
                  />
                  <TextInput
                    id={`venda-${item.chave}`}
                    labelText="Preço de venda (R$)"
                    inputMode="decimal"
                    placeholder="0,00"
                    value={item.precoVenda}
                    disabled={bloqueado}
                    invalid={tentou && !l.precoVendaOk}
                    invalidText="Maior que zero"
                    onChange={(e) => atualizarItem(item.chave, { precoVenda: e.target.value })}
                  />
                  <TextInput
                    id={`custo-${item.chave}`}
                    labelText="Preço de custo (R$)"
                    inputMode="decimal"
                    placeholder="0,00"
                    value={item.precoCusto}
                    disabled={bloqueado}
                    invalid={tentou && !l.precoCustoOk}
                    invalidText="Use 0 se não houver"
                    onChange={(e) => atualizarItem(item.chave, { precoCusto: e.target.value })}
                  />

                  <div className="rodape-item">
                    <span>
                      {l.q > 0 && l.pv > 0 ? (
                        <>
                          Subtotal <strong>{moeda(l.pv * l.q)}</strong> · lucro{' '}
                          <span className={l.pv - l.pc >= 0 ? 'positivo' : 'negativo'}>{moeda((l.pv - l.pc) * l.q)}</span>
                        </>
                      ) : (
                        'Preencha quantidade e preços'
                      )}
                    </span>
                    {itens.length > 1 && (
                      <Button
                        kind="ghost"
                        size="sm"
                        renderIcon={TrashCan}
                        disabled={bloqueado}
                        onClick={() => setItens((lista) => lista.filter((x) => x.chave !== item.chave))}
                      >
                        Remover
                      </Button>
                    )}
                  </div>
                </div>
              )
            })}
          </div>

          <div style={{ marginTop: '0.75rem' }}>
            <Button
              kind="tertiary"
              renderIcon={Add}
              disabled={bloqueado || itens.length >= 50}
              onClick={() => setItens((lista) => [...lista, novoItem()])}
            >
              Adicionar item
            </Button>
          </div>
        </section>

        <section aria-labelledby="titulo-pagamento">
          <h2 id="titulo-pagamento" className="secao-titulo">
            Pagamento
          </h2>
          <div className="formas" role="group" aria-label="Forma de pagamento">
            {(['Pix', 'Cartao'] as const).map((f) => (
              <button key={f} type="button" aria-pressed={forma === f} disabled={bloqueado} onClick={() => setForma(f)}>
                {f === 'Cartao' ? 'Cartão' : 'Pix'}
              </button>
            ))}
          </div>
        </section>

        <section className="linha-campos">
          <ComboBox
            id="cliente"
            titleText="Cliente (opcional)"
            placeholder="Buscar cliente pelo nome"
            helperText="Deixe em branco para venda sem cliente cadastrado."
            items={opcoesCliente}
            itemToString={(c: OpcaoCliente | null) => (c ? c.nome : '')}
            selectedItem={cliente}
            disabled={bloqueado}
            // A busca é feita no servidor; o filtro local não pode esconder o que ele devolveu.
            shouldFilterItem={() => true}
            onInputChange={(texto: string) => setBuscaCliente(texto ?? '')}
            onChange={({ selectedItem }: { selectedItem: OpcaoCliente | null | undefined }) =>
              setCliente(selectedItem ?? null)
            }
          />
          <TextInput
            id="data-venda"
            labelText="Data da venda"
            type="date"
            value={data}
            max={hojeISO()}
            disabled={bloqueado}
            helperText={data === hojeISO() ? 'Hoje' : 'Data retroativa'}
            onChange={(e) => setData(e.target.value || hojeISO())}
          />
        </section>

        {erroGeral && (
          <InlineNotification kind="error" lowContrast hideCloseButton title="Revise a venda" subtitle={erroGeral} />
        )}

        <div className="resumo-venda" aria-live="polite">
          <div>
            <div className="rotulo">Total</div>
            <div className="valor neon">{moeda(total)}</div>
          </div>
          <div>
            <div className="rotulo">Custo</div>
            <div className="valor">{moeda(custo)}</div>
          </div>
          <div>
            <div className="rotulo">Lucro</div>
            <div className={`valor ${lucro >= 0 ? 'positivo' : 'negativo'}`}>{moeda(lucro)}</div>
          </div>
        </div>

        <div className="acoes">
          <Button type="submit" disabled={bloqueado}>
            {salvando ? 'Salvando…' : editandoId ? 'Salvar alterações' : 'Registrar venda'}
          </Button>
          <Button kind="ghost" type="button" disabled={salvando} onClick={() => navegar('/vendas')}>
            Voltar
          </Button>
          {editandoId && !cancelada && (
            <Button kind="danger--ghost" type="button" disabled={salvando} onClick={() => setConfirmarCancelar(true)}>
              Cancelar venda
            </Button>
          )}
        </div>
      </form>

      <ConfirmarModal
        aberto={confirmarCancelar}
        perigo
        carregando={salvando}
        titulo="Cancelar esta venda?"
        texto="Ela sai dos totais do painel, mas continua no histórico e pode ser reativada depois."
        rotuloConfirmar="Cancelar venda"
        aoConfirmar={cancelarVenda}
        aoCancelar={() => setConfirmarCancelar(false)}
      />
    </>
  )
}
