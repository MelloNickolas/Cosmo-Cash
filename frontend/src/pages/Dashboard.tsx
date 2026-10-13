import { ContentSwitcher, Switch, Table, TableBody, TableCell, TableHead, TableHeader, TableRow, TextInput } from '@carbon/react'
import { ArrowDown, ArrowUp, Subtract } from '@carbon/icons-react'
import { DonutChart, GroupedBarChart, LineChart } from '@carbon/charts-react'
import { useMemo, useState } from 'react'
import { ErroCarregar, PaginaCabecalho, Vazio } from '../components/Comuns'
import { dashboardApi } from '../services/api'
import type { OrdenacaoRanking, Painel, PeriodoDashboard } from '../services/types'
import { diaMes, hojeISO, mesAno, moeda, numero, rotuloForma, ymdParaBR } from '../utils/format'
import { useCarregar } from '../utils/hooks'

const PERIODOS: { valor: PeriodoDashboard; rotulo: string }[] = [
  { valor: 'Hoje', rotulo: 'Hoje' },
  { valor: 'Semana', rotulo: 'Semana' },
  { valor: 'Mes', rotulo: 'Mês' },
  { valor: 'Ano', rotulo: 'Ano' },
  { valor: 'Personalizado', rotulo: 'Datas' },
]

const ORDENACOES: { valor: OrdenacaoRanking; rotulo: string }[] = [
  { valor: 'Quantidade', rotulo: 'Quantidade' },
  { valor: 'Faturamento', rotulo: 'Faturamento' },
  { valor: 'Lucro', rotulo: 'Lucro' },
]

const COMPARACAO: Record<PeriodoDashboard, string> = {
  Hoje: 'ontem',
  Semana: 'semana passada',
  Mes: 'mês passado',
  Ano: 'ano passado',
  Personalizado: 'período anterior',
}

const TEMA_GRAFICO = {
  theme: 'g100',
  toolbar: { enabled: false },
  legend: { alignment: 'center' },
  resizable: true,
} as const

// Tooltip do gráfico: valores numéricos viram moeda; rótulos (datas, meses) ficam como texto.
function formatarValorGrafico(valor: unknown): string {
  return typeof valor === 'number' && Number.isFinite(valor) ? moeda(valor) : String(valor ?? '')
}

function dataIso(deslocamentoDias: number): string {
  const [a, m, d] = hojeISO().split('-').map(Number)
  const dt = new Date(Date.UTC(a, m - 1, d + deslocamentoDias))
  return dt.toISOString().slice(0, 10)
}

function Variacao({ valor, referencia }: { valor: number | null; referencia: string }) {
  if (valor === null) {
    return (
      <div className="variacao" title={`Sem vendas em ${referencia} para comparar`}>
        <Subtract size={14} /> sem comparação
      </div>
    )
  }
  const sobe = valor > 0
  const desce = valor < 0
  const Icone = sobe ? ArrowUp : desce ? ArrowDown : Subtract

  return (
    <div className={`variacao ${sobe ? 'sobe' : desce ? 'desce' : ''}`}>
      <Icone size={14} />
      {`${valor > 0 ? '+' : ''}${valor.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}% vs ${referencia}`}
    </div>
  )
}

function Indicador({
  rotulo,
  valor,
  variacao,
  referencia,
  destaque = false,
}: {
  rotulo: string
  valor: string
  variacao: number | null
  referencia: string
  destaque?: boolean
}) {
  return (
    <div className={`kpi${destaque ? ' destaque' : ''}`}>
      <div className="rotulo">{rotulo}</div>
      <div className="valor">{valor}</div>
      <Variacao valor={variacao} referencia={referencia} />
    </div>
  )
}

export default function Dashboard() {
  const [periodo, setPeriodo] = useState<PeriodoDashboard>('Mes')
  const [inicio, setInicio] = useState(() => dataIso(-29))
  const [fim, setFim] = useState(hojeISO)
  const [ordenarPor, setOrdenarPor] = useState<OrdenacaoRanking>('Quantidade')

  const personalizadoInvalido = periodo === 'Personalizado' && (!inicio || !fim || fim < inicio)

  const painel = useCarregar<Painel | null>(
    () =>
      personalizadoInvalido
        ? Promise.resolve(null)
        : dashboardApi.painel({
            periodo,
            dataInicio: periodo === 'Personalizado' ? inicio : undefined,
            dataFim: periodo === 'Personalizado' ? fim : undefined,
            ordenarPor,
            limite: 5,
          }),
    [periodo, inicio, fim, ordenarPor, personalizadoInvalido],
  )

  const p = painel.dados
  const referencia = COMPARACAO[periodo]
  const semVendas = p !== null && p.resumo.atual.vendas === 0

  const dadosSerie = useMemo(() => {
    if (!p) return []
    const rotulo = (periodoStr: string) => (p.agrupamento === 'Mes' ? mesAno(periodoStr) : diaMes(periodoStr))
    return p.serie.flatMap((ponto) => [
      { group: 'Faturamento', key: rotulo(ponto.periodo), value: ponto.faturamento },
      { group: 'Lucro', key: rotulo(ponto.periodo), value: ponto.lucro },
    ])
  }, [p])

  const dadosForma = useMemo(
    () => (p?.porFormaPagamento ?? []).map((f) => ({ group: rotuloForma(f.formaPagamento), value: f.faturamento })),
    [p],
  )

  const opcoesSerie = {
    ...TEMA_GRAFICO,
    title: '',
    height: '320px',
    axes: {
      // O título do eixo é o que o tooltip usa no cabeçalho (sem ele aparecia "x-value").
      bottom: { mapsTo: 'key', scaleType: 'labels', title: p?.agrupamento === 'Mes' ? 'Mês' : 'Dia' },
      left: { mapsTo: 'value', ticks: { formatter: (v: number | string) => moeda(Number(v)) } },
    },
    color: { scale: { Faturamento: '#39FF14', Lucro: '#5BC0BE' } },
    tooltip: {
      // Faturamento + lucro não é uma soma que faça sentido (o lucro já está no faturamento).
      showTotal: false,
      // O formatador também recebe o rótulo do eixo (texto, ex.: "12/10"): só números viram moeda.
      valueFormatter: formatarValorGrafico,
    },
    ...(p && p.serie.length > 1 ? { curve: 'curveMonotoneX', points: { enabled: p.serie.length <= 31 } } : {}),
  }

  const opcoesForma = {
    ...TEMA_GRAFICO,
    title: '',
    height: '320px',
    resizable: true,
    donut: {
      alignment: 'center',
      center: { label: 'Faturamento', numberFormatter: (v: number) => moeda(v) },
    },
    pie: { labels: { enabled: false } },
    color: { scale: { Pix: '#39FF14', Cartão: '#5BC0BE' } },
    tooltip: { valueFormatter: formatarValorGrafico },
  }

  const indice = PERIODOS.findIndex((x) => x.valor === periodo)

  return (
    <>
      <PaginaCabecalho
        titulo="Painel"
        subtitulo={
          p
            ? `${ymdParaBR(p.periodo.inicio)} a ${ymdParaBR(p.periodo.fimEfetivo)} · comparado com ${referencia} (${ymdParaBR(p.periodo.anteriorInicio)} a ${ymdParaBR(p.periodo.anteriorFim)})`
            : 'Resultados das vendas concluídas.'
        }
      />

      <div className="filtros">
        <ContentSwitcher
          size="md"
          selectedIndex={indice}
          onChange={({ index }: { index?: number }) => {
            if (index !== undefined) setPeriodo(PERIODOS[index].valor)
          }}
        >
          {PERIODOS.map((x) => (
            <Switch key={x.valor} name={x.valor} text={x.rotulo} />
          ))}
        </ContentSwitcher>
      </div>

      {periodo === 'Personalizado' && (
        <div className="filtros">
          <TextInput id="d-inicio" type="date" labelText="De" value={inicio} max={fim} onChange={(e) => setInicio(e.target.value)} />
          <TextInput
            id="d-fim"
            type="date"
            labelText="Até"
            value={fim}
            min={inicio}
            max={hojeISO()}
            invalid={personalizadoInvalido}
            invalidText="Informe um período válido."
            onChange={(e) => setFim(e.target.value)}
          />
        </div>
      )}

      {painel.erro && <ErroCarregar mensagem={painel.erro} aoTentar={painel.recarregar} />}

      {!painel.erro && !p && painel.carregando && (
        <div className="grade-kpi" aria-busy="true" aria-label="Carregando painel">
          {Array.from({ length: 5 }, (_, i) => (
            <div key={i} className="kpi skeleton-bloco" />
          ))}
        </div>
      )}

      {p && (
        <div style={{ opacity: painel.carregando ? 0.6 : 1, transition: 'opacity 120ms' }}>
          <div className="grade-kpi">
            <Indicador
              destaque
              rotulo="Lucro"
              valor={moeda(p.resumo.atual.lucro)}
              variacao={p.resumo.variacao.lucro}
              referencia={referencia}
            />
            <Indicador
              rotulo="Faturamento"
              valor={moeda(p.resumo.atual.faturamento)}
              variacao={p.resumo.variacao.faturamento}
              referencia={referencia}
            />
            <Indicador
              rotulo="Vendas"
              valor={numero(p.resumo.atual.vendas)}
              variacao={p.resumo.variacao.vendas}
              referencia={referencia}
            />
            <Indicador
              rotulo="Ticket médio"
              valor={moeda(p.resumo.atual.ticketMedio)}
              variacao={p.resumo.variacao.ticketMedio}
              referencia={referencia}
            />
            <Indicador
              rotulo="Custo"
              valor={moeda(p.resumo.atual.custo)}
              variacao={p.resumo.variacao.custo}
              referencia={referencia}
            />
          </div>

          {semVendas ? (
            <section className="secao">
              <Vazio titulo="Nenhuma venda neste período" texto="Quando houver vendas concluídas, os gráficos e rankings aparecem aqui." />
            </section>
          ) : (
            <>
              <section className="secao grade-2">
                <div className="grafico">
                  <h3>Faturamento e lucro por {p.agrupamento === 'Mes' ? 'mês' : 'dia'}</h3>
                  {p.serie.length > 1 ? (
                    <LineChart data={dadosSerie} options={opcoesSerie as never} />
                  ) : (
                    <GroupedBarChart data={dadosSerie} options={opcoesSerie as never} />
                  )}
                </div>

                <div className="grafico">
                  <h3>Por forma de pagamento</h3>
                  <DonutChart data={dadosForma} options={opcoesForma as never} />
                </div>
              </section>

              <section className="secao">
                <div className="filtros" style={{ alignItems: 'center', justifyContent: 'space-between' }}>
                  <h2 style={{ margin: 0, flex: '1 1 auto' }}>Mais vendidos</h2>
                  <div style={{ flex: '0 1 22rem' }}>
                    <ContentSwitcher
                      size="sm"
                      selectedIndex={ORDENACOES.findIndex((o) => o.valor === ordenarPor)}
                      onChange={({ index }: { index?: number }) => {
                        if (index !== undefined) setOrdenarPor(ORDENACOES[index].valor)
                      }}
                    >
                      {ORDENACOES.map((o) => (
                        <Switch key={o.valor} name={o.valor} text={o.rotulo} />
                      ))}
                    </ContentSwitcher>
                  </div>
                </div>

                <div className="grade-iguais">
                  <div>
                    <h3 className="sub-titulo">Produtos</h3>
                    <Table size="md" aria-label="Produtos mais vendidos">
                      <TableHead>
                        <TableRow>
                          <TableHeader>Produto</TableHeader>
                          <TableHeader className="num">Qtd.</TableHeader>
                          <TableHeader className="num">Lucro</TableHeader>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {p.topProdutos.map((t) => (
                          <TableRow key={t.produtoID}>
                            <TableCell>
                              <div>
                                {t.marca} {t.descricao}
                              </div>
                              <div className="suave" style={{ fontSize: '0.75rem' }}>
                                {t.volumeMl} ml · {t.variacao}
                              </div>
                            </TableCell>
                            <TableCell className="num">{numero(t.quantidade)}</TableCell>
                            <TableCell className="num positivo">{moeda(t.lucro)}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>

                  <div>
                    <h3 className="sub-titulo">Marcas</h3>
                    <Table size="md" aria-label="Marcas mais vendidas">
                      <TableHead>
                        <TableRow>
                          <TableHeader>Marca</TableHeader>
                          <TableHeader className="num">Qtd.</TableHeader>
                          <TableHeader className="num">Faturamento</TableHeader>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {p.topMarcas.map((m) => (
                          <TableRow key={m.marca}>
                            <TableCell>{m.marca}</TableCell>
                            <TableCell className="num">{numero(m.quantidade)}</TableCell>
                            <TableCell className="num">{moeda(m.faturamento)}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                </div>
              </section>
            </>
          )}
        </div>
      )}
    </>
  )
}
