// Tipos que espelham as respostas da API (JSON em camelCase, enums como texto).

export type TipoUsuario = 'AdminTotal' | 'Socio'
export type FormaPagamento = 'Cartao' | 'Pix'
export type StatusVenda = 'Concluida' | 'Cancelada'

export interface Paginado<T> {
  itens: T[]
  pagina: number
  tamanhoPagina: number
  total: number
  totalPaginas: number
}

/* ---------- Auth / Usuários ---------- */

export interface UsuarioLogado {
  usuarioID: number
  nome: string
  email: string
  tipo: TipoUsuario
}

export interface LoginResponse extends UsuarioLogado {
  token: string
  expiraEm: string
}

export interface Usuario {
  id: number
  nome: string
  email: string
  tipo: TipoUsuario
  ativo: boolean
  criadoEm: string
}

export interface UsuarioForm {
  nome: string
  email: string
  senha?: string
  tipo: TipoUsuario
}

/* ---------- Produtos ---------- */

export interface Produto {
  id: number
  marca: string
  descricao: string
  volumeMl: number
  variacao: string
  ativo: boolean
  criadoEm: string
}

export interface ProdutoForm {
  marca: string
  descricao: string
  volumeMl: number
  variacao: string
}

/* ---------- Clientes ---------- */

export interface ClienteResumo {
  id: number
  nome: string
  cpfMascarado: string | null
  telefone: string | null
  ativo: boolean
}

export interface Cliente {
  id: number
  nome: string
  cpf: string | null
  telefone: string | null
  cep: string | null
  endereco: string | null
  ativo: boolean
  criadoEm: string
}

export interface ClienteForm {
  nome: string
  cpf: string
  telefone: string
  cep: string
  endereco: string
}

/* ---------- Vendas ---------- */

export interface VendaItem {
  id: number
  produtoID: number
  marca: string
  descricao: string
  volumeMl: number
  variacao: string
  quantidade: number
  precoVenda: number
  precoCusto: number
  subtotal: number
  lucro: number
}

export interface Venda {
  id: number
  dataVenda: string
  formaPagamento: FormaPagamento
  status: StatusVenda
  canceladaEm: string | null
  cliente: { id: number; nome: string } | null
  usuario: { id: number; nome: string }
  itens: VendaItem[]
  valorTotal: number
  custoTotal: number
  lucro: number
}

export interface VendaResumo {
  id: number
  dataVenda: string
  formaPagamento: FormaPagamento
  status: StatusVenda
  clienteID: number | null
  clienteNome: string | null
  usuarioID: number
  usuarioNome: string
  quantidadeItens: number
  valorTotal: number
  custoTotal: number
  lucro: number
}

export interface VendaItemForm {
  produtoID: number
  quantidade: number
  precoVenda: number
  precoCusto: number
}

export interface VendaForm {
  dataVenda?: string
  formaPagamento: FormaPagamento
  clienteID?: number | null
  itens: VendaItemForm[]
}

/* ---------- Dashboard ---------- */

export type PeriodoDashboard = 'Hoje' | 'Semana' | 'Mes' | 'Ano' | 'Personalizado'
export type OrdenacaoRanking = 'Quantidade' | 'Faturamento' | 'Lucro'

export interface Totais {
  vendas: number
  itens: number
  faturamento: number
  custo: number
  lucro: number
  ticketMedio: number
}

export interface Variacao {
  vendas: number | null
  faturamento: number | null
  custo: number | null
  lucro: number | null
  ticketMedio: number | null
}

export interface Painel {
  periodo: {
    tipo: PeriodoDashboard
    inicio: string
    fim: string
    fimEfetivo: string
    anteriorInicio: string
    anteriorFim: string
  }
  agrupamento: 'Dia' | 'Mes'
  resumo: { atual: Totais; anterior: Totais; variacao: Variacao }
  serie: { periodo: string; vendas: number; faturamento: number; custo: number; lucro: number }[]
  porFormaPagamento: { formaPagamento: FormaPagamento; vendas: number; faturamento: number; lucro: number }[]
  topProdutos: {
    produtoID: number
    marca: string
    descricao: string
    volumeMl: number
    variacao: string
    quantidade: number
    faturamento: number
    lucro: number
  }[]
  topMarcas: { marca: string; quantidade: number; faturamento: number; lucro: number }[]
}
