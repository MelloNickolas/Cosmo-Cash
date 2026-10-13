import { http } from './client'
import type {
  Cliente,
  ClienteForm,
  ClienteResumo,
  FormaPagamento,
  LoginResponse,
  OrdenacaoRanking,
  Painel,
  Paginado,
  PeriodoDashboard,
  Produto,
  ProdutoForm,
  StatusVenda,
  Usuario,
  UsuarioForm,
  UsuarioLogado,
  Venda,
  VendaForm,
  VendaResumo,
} from './types'

// Remove chaves sem valor para não poluir a query string.
function limpar<T extends Record<string, unknown>>(params: T): Partial<T> {
  return Object.fromEntries(
    Object.entries(params).filter(([, valor]) => valor !== undefined && valor !== null && valor !== ''),
  ) as Partial<T>
}

export const authApi = {
  async login(email: string, senha: string): Promise<LoginResponse> {
    return (await http.post('/Auth/Login', { email, senha })).data
  },
  async eu(): Promise<UsuarioLogado> {
    return (await http.get('/Auth/Me')).data
  },
  async alterarMinhaSenha(senhaAtual: string, novaSenha: string): Promise<void> {
    await http.put('/Auth/AlterarMinhaSenha', { senhaAtual, novaSenha })
  },
}

export const usuarioApi = {
  async listar(p: { pagina?: number; tamanhoPagina?: number; busca?: string; ativo?: boolean }) {
    return (await http.get<Paginado<Usuario>>('/Usuario/ListarUsuarios', { params: limpar(p) })).data
  },
  async criar(dados: UsuarioForm): Promise<Usuario> {
    return (await http.post('/Usuario/CriarUsuario', dados)).data
  },
  async atualizar(id: number, dados: Omit<UsuarioForm, 'senha'>): Promise<Usuario> {
    return (await http.put(`/Usuario/AtualizarUsuario/${id}`, dados)).data
  },
  async alterarSenha(id: number, novaSenha: string): Promise<void> {
    await http.put(`/Usuario/AlterarSenha/${id}`, { novaSenha })
  },
  async desativar(id: number): Promise<void> {
    await http.delete(`/Usuario/DesativarUsuario/${id}`)
  },
  async reativar(id: number): Promise<Usuario> {
    return (await http.patch(`/Usuario/ReativarUsuario/${id}`)).data
  },
}

export const produtoApi = {
  async listar(p: {
    pagina?: number
    tamanhoPagina?: number
    busca?: string
    ativo?: boolean
    marca?: string
    variacao?: string
  }) {
    return (await http.get<Paginado<Produto>>('/Produto/ListarProdutos', { params: limpar(p) })).data
  },
  // Todos os ativos (para a tela de venda), paginando de 100 em 100.
  async listarAtivos(): Promise<Produto[]> {
    const todos: Produto[] = []
    for (let pagina = 1; pagina <= 50; pagina++) {
      const r = await produtoApi.listar({ pagina, tamanhoPagina: 100, ativo: true })
      todos.push(...r.itens)
      if (pagina >= r.totalPaginas) break
    }
    return todos
  },
  async marcas(): Promise<string[]> {
    return (await http.get('/Produto/ListarMarcas')).data
  },
  async variacoes(): Promise<string[]> {
    return (await http.get('/Produto/ListarVariacoes')).data
  },
  async criar(dados: ProdutoForm): Promise<Produto> {
    return (await http.post('/Produto/CriarProduto', dados)).data
  },
  async atualizar(id: number, dados: ProdutoForm): Promise<Produto> {
    return (await http.put(`/Produto/AtualizarProduto/${id}`, dados)).data
  },
  async desativar(id: number): Promise<void> {
    await http.delete(`/Produto/DesativarProduto/${id}`)
  },
  async reativar(id: number): Promise<Produto> {
    return (await http.patch(`/Produto/ReativarProduto/${id}`)).data
  },
}

export const clienteApi = {
  async listar(p: { pagina?: number; tamanhoPagina?: number; busca?: string; ativo?: boolean }) {
    return (await http.get<Paginado<ClienteResumo>>('/Cliente/ListarClientes', { params: limpar(p) })).data
  },
  async obter(id: number): Promise<Cliente> {
    return (await http.get(`/Cliente/ObterCliente/${id}`)).data
  },
  async criar(dados: ClienteForm): Promise<Cliente> {
    return (await http.post('/Cliente/CriarCliente', dados)).data
  },
  async atualizar(id: number, dados: ClienteForm): Promise<Cliente> {
    return (await http.put(`/Cliente/AtualizarCliente/${id}`, dados)).data
  },
  async desativar(id: number): Promise<void> {
    await http.delete(`/Cliente/DesativarCliente/${id}`)
  },
  async reativar(id: number): Promise<Cliente> {
    return (await http.patch(`/Cliente/ReativarCliente/${id}`)).data
  },
}

export const vendaApi = {
  async listar(p: {
    pagina?: number
    tamanhoPagina?: number
    dataInicio?: string
    dataFim?: string
    status?: StatusVenda
    formaPagamento?: FormaPagamento
    clienteID?: number
    usuarioID?: number
  }) {
    return (await http.get<Paginado<VendaResumo>>('/Venda/ListarVendas', { params: limpar(p) })).data
  },
  async obter(id: number): Promise<Venda> {
    return (await http.get(`/Venda/ObterVenda/${id}`)).data
  },
  async criar(dados: VendaForm): Promise<Venda> {
    return (await http.post('/Venda/CriarVenda', dados)).data
  },
  async atualizar(id: number, dados: VendaForm): Promise<Venda> {
    return (await http.put(`/Venda/AtualizarVenda/${id}`, dados)).data
  },
  async cancelar(id: number): Promise<void> {
    await http.delete(`/Venda/CancelarVenda/${id}`)
  },
  async reativar(id: number): Promise<Venda> {
    return (await http.patch(`/Venda/ReativarVenda/${id}`)).data
  },
}

export const dashboardApi = {
  async painel(p: {
    periodo?: PeriodoDashboard
    dataInicio?: string
    dataFim?: string
    ordenarPor?: OrdenacaoRanking
    limite?: number
  }): Promise<Painel> {
    return (await http.get('/Dashboard/Painel', { params: limpar(p) })).data
  },
}
