import { Pagination, Search, Select, SelectItem, SkeletonText } from '@carbon/react'
import type { FiltroAtivo } from '../utils/filtros'

export function CampoBusca({
  valor,
  aoMudar,
  rotulo = 'Buscar',
  placeholder = 'Buscar',
}: {
  valor: string
  aoMudar: (valor: string) => void
  rotulo?: string
  placeholder?: string
}) {
  return (
    <div className="filtro-largo">
      <Search
        size="lg"
        labelText={rotulo}
        placeholder={placeholder}
        closeButtonLabelText="Limpar busca"
        value={valor}
        onChange={(e: { target: { value: string } }) => aoMudar(e.target.value)}
      />
    </div>
  )
}

export function SeletorAtivo({ valor, aoMudar }: { valor: FiltroAtivo; aoMudar: (v: FiltroAtivo) => void }) {
  return (
    <Select
      id="filtro-ativo"
      labelText="Situação"
      value={valor}
      onChange={(e: React.ChangeEvent<HTMLSelectElement>) => aoMudar(e.target.value as FiltroAtivo)}
    >
      <SelectItem value="ativos" text="Ativos" />
      <SelectItem value="inativos" text="Desativados" />
      <SelectItem value="todos" text="Todos" />
    </Select>
  )
}

export function Paginacao({
  pagina,
  tamanho,
  total,
  aoMudar,
}: {
  pagina: number
  tamanho: number
  total: number
  aoMudar: (pagina: number, tamanho: number) => void
}) {
  if (total <= 0) return null

  return (
    <Pagination
      page={pagina}
      pageSize={tamanho}
      pageSizes={[10, 20, 50]}
      totalItems={total}
      itemsPerPageText="Itens por página:"
      pageNumberText="Página"
      backwardText="Página anterior"
      forwardText="Próxima página"
      itemRangeText={(min: number, max: number, totalItens: number) => `${min}–${max} de ${totalItens}`}
      pageRangeText={(_atual: number, totalPaginas: number) => `de ${totalPaginas}`}
      onChange={({ page, pageSize }: { page: number; pageSize: number }) => aoMudar(page, pageSize)}
    />
  )
}

export function EsqueletoLista({ linhas = 4 }: { linhas?: number }) {
  return (
    <div className="lista-cartoes" aria-busy="true" aria-label="Carregando">
      {Array.from({ length: linhas }, (_, i) => (
        <div key={i} className="item-cartao">
          <SkeletonText heading width="40%" />
          <SkeletonText width="70%" />
        </div>
      ))}
    </div>
  )
}
