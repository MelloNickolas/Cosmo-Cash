export type FiltroAtivo = 'ativos' | 'inativos' | 'todos'

export const ativoDoFiltro = (filtro: FiltroAtivo): boolean | undefined =>
  filtro === 'todos' ? undefined : filtro === 'ativos'
