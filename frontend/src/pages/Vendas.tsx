import { Button, Select, SelectItem, Table, TableBody, TableCell, TableHead, TableHeader, TableRow, Tag, TextInput } from '@carbon/react'
import { Add } from '@carbon/icons-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ErroCarregar, PaginaCabecalho, Vazio } from '../components/Comuns'
import { EsqueletoLista, Paginacao } from '../components/ListaUtils'
import { vendaApi } from '../services/api'
import type { FormaPagamento, StatusVenda, VendaResumo } from '../services/types'
import { dataHora, hojeISO, moeda, rotuloForma } from '../utils/format'
import { useCarregar } from '../utils/hooks'

// Primeiro dia do mês corrente (calendário de São Paulo).
const inicioDoMes = () => `${hojeISO().slice(0, 7)}-01`

function EtiquetaStatus({ status }: { status: StatusVenda }) {
  return (
    <Tag type={status === 'Concluida' ? 'green' : 'red'} size="sm">
      {status === 'Concluida' ? 'Concluída' : 'Cancelada'}
    </Tag>
  )
}

export default function Vendas() {
  const navegar = useNavigate()
  const [dataInicio, setDataInicio] = useState(inicioDoMes)
  const [dataFim, setDataFim] = useState(hojeISO)
  const [status, setStatus] = useState<'' | StatusVenda>('')
  const [forma, setForma] = useState<'' | FormaPagamento>('')
  const [pagina, setPagina] = useState(1)
  const [tamanho, setTamanho] = useState(10)

  const periodoInvalido = Boolean(dataInicio && dataFim && dataFim < dataInicio)

  const lista = useCarregar(
    () =>
      periodoInvalido
        ? Promise.resolve(null)
        : vendaApi.listar({
            pagina,
            tamanhoPagina: tamanho,
            dataInicio: dataInicio || undefined,
            dataFim: dataFim || undefined,
            status: status || undefined,
            formaPagamento: forma || undefined,
          }),
    [pagina, tamanho, dataInicio, dataFim, status, forma, periodoInvalido],
  )

  const itens: VendaResumo[] = lista.dados?.itens ?? []
  const abrir = (venda: VendaResumo) => navegar(`/vendas/${venda.id}`)

  return (
    <>
      <PaginaCabecalho titulo="Vendas" subtitulo="Toque em uma venda para ver os itens, editar ou cancelar.">
        <Button renderIcon={Add} onClick={() => navegar('/vendas/nova')}>
          Nova venda
        </Button>
      </PaginaCabecalho>

      <div className="filtros">
        <TextInput
          id="v-inicio"
          type="date"
          labelText="De"
          value={dataInicio}
          max={dataFim || undefined}
          onChange={(e) => {
            setDataInicio(e.target.value)
            setPagina(1)
          }}
        />
        <TextInput
          id="v-fim"
          type="date"
          labelText="Até"
          value={dataFim}
          min={dataInicio || undefined}
          invalid={periodoInvalido}
          invalidText="A data final não pode ser anterior à inicial."
          onChange={(e) => {
            setDataFim(e.target.value)
            setPagina(1)
          }}
        />
        <Select
          id="v-status"
          labelText="Situação"
          value={status}
          onChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
            setStatus(e.target.value as '' | StatusVenda)
            setPagina(1)
          }}
        >
          <SelectItem value="" text="Todas" />
          <SelectItem value="Concluida" text="Concluídas" />
          <SelectItem value="Cancelada" text="Canceladas" />
        </Select>
        <Select
          id="v-forma"
          labelText="Pagamento"
          value={forma}
          onChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
            setForma(e.target.value as '' | FormaPagamento)
            setPagina(1)
          }}
        >
          <SelectItem value="" text="Todos" />
          <SelectItem value="Pix" text="Pix" />
          <SelectItem value="Cartao" text="Cartão" />
        </Select>
      </div>

      {lista.erro && <ErroCarregar mensagem={lista.erro} aoTentar={lista.recarregar} />}
      {!lista.erro && lista.carregando && !lista.dados && <EsqueletoLista />}

      {!lista.erro && lista.dados && itens.length === 0 && (
        <Vazio titulo="Nenhuma venda neste período" texto="Ajuste os filtros ou registre uma nova venda.">
          <Button renderIcon={Add} onClick={() => navegar('/vendas/nova')}>
            Registrar venda
          </Button>
        </Vazio>
      )}

      {itens.length > 0 && (
        <>
          <div className="so-desktop">
            <Table size="lg" aria-label="Lista de vendas">
              <TableHead>
                <TableRow>
                  <TableHeader>Data</TableHeader>
                  <TableHeader>Cliente</TableHeader>
                  <TableHeader>Pagamento</TableHeader>
                  <TableHeader className="num">Itens</TableHeader>
                  <TableHeader className="num">Total</TableHeader>
                  <TableHeader className="num">Lucro</TableHeader>
                  <TableHeader>Situação</TableHeader>
                </TableRow>
              </TableHead>
              <TableBody>
                {itens.map((v) => (
                  <TableRow
                    key={v.id}
                    className="linha-clicavel"
                    tabIndex={0}
                    onClick={() => abrir(v)}
                    onKeyDown={(e) => e.key === 'Enter' && abrir(v)}
                  >
                    <TableCell>{dataHora(v.dataVenda)}</TableCell>
                    <TableCell>{v.clienteNome ?? <span className="suave">—</span>}</TableCell>
                    <TableCell>{rotuloForma(v.formaPagamento)}</TableCell>
                    <TableCell className="num">{v.quantidadeItens}</TableCell>
                    <TableCell className="num">{moeda(v.valorTotal)}</TableCell>
                    <TableCell className="num positivo">{moeda(v.lucro)}</TableCell>
                    <TableCell>
                      <EtiquetaStatus status={v.status} />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          <div className="so-mobile lista-cartoes">
            {itens.map((v) => (
              <button
                key={v.id}
                type="button"
                className={`item-cartao linha-menu${v.status === 'Cancelada' ? ' cancelada' : ''}`}
                onClick={() => abrir(v)}
              >
                <span className="linha">
                  <span className="titulo">{moeda(v.valorTotal)}</span>
                  <EtiquetaStatus status={v.status} />
                </span>
                <span className="secundario">
                  {dataHora(v.dataVenda)} · {rotuloForma(v.formaPagamento)} · {v.quantidadeItens}{' '}
                  {v.quantidadeItens === 1 ? 'item' : 'itens'}
                </span>
                <span className="linha">
                  <span className="secundario">{v.clienteNome ?? 'Sem cliente'}</span>
                  <span className="positivo">Lucro {moeda(v.lucro)}</span>
                </span>
              </button>
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
    </>
  )
}
