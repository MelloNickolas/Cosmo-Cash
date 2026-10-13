import {
  Header,
  HeaderGlobalAction,
  HeaderGlobalBar,
  HeaderName,
  SideNav,
  SideNavItems,
  SideNavLink,
} from '@carbon/react'
import {
  Add,
  Catalog,
  Dashboard as IconeDashboard,
  Logout,
  Menu as IconeMais,
  Receipt,
  Settings,
  UserAvatar,
  UserMultiple,
} from '@carbon/icons-react'
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { NOME_LOJA } from '../config'
import { useMediaQuery } from '../utils/hooks'

interface ItemMenu {
  rotulo: string
  para: string
  icone: typeof Receipt
  somenteAdmin?: boolean
  exato?: boolean
}

const MENU_DESKTOP: ItemMenu[] = [
  { rotulo: 'Painel', para: '/', icone: IconeDashboard, exato: true },
  { rotulo: 'Vendas', para: '/vendas', icone: Receipt },
  { rotulo: 'Produtos', para: '/produtos', icone: Catalog },
  { rotulo: 'Clientes', para: '/clientes', icone: UserMultiple },
  { rotulo: 'Usuários', para: '/usuarios', icone: UserAvatar, somenteAdmin: true },
  { rotulo: 'Minha conta', para: '/conta', icone: Settings },
]

function ativo(caminho: string, item: ItemMenu): boolean {
  return item.exato ? caminho === item.para : caminho === item.para || caminho.startsWith(`${item.para}/`)
}

export function AppLayout() {
  const { usuario, ehAdmin, sair } = useAuth()
  const navegar = useNavigate()
  const { pathname } = useLocation()
  const desktop = useMediaQuery('(min-width: 1056px)')

  const itens = MENU_DESKTOP.filter((item) => !item.somenteAdmin || ehAdmin)

  return (
    <div className="app">
      <Header aria-label={NOME_LOJA}>
        <HeaderName href="/" prefix="" onClick={(e: React.MouseEvent) => { e.preventDefault(); navegar('/') }}>
          <span className="marca" aria-hidden="true">🪐</span> {NOME_LOJA}
        </HeaderName>
        <HeaderGlobalBar>
          <span className="header-usuario" title={usuario?.email}>
            {usuario?.nome}
          </span>
          <HeaderGlobalAction aria-label="Sair" tooltipAlignment="end" onClick={sair}>
            <Logout size={20} />
          </HeaderGlobalAction>
        </HeaderGlobalBar>
      </Header>

      {desktop && (
        <SideNav aria-label="Menu principal" expanded isFixedNav isChildOfHeader={false}>
          <SideNavItems>
            {itens.map((item) => {
              const Icone = item.icone
              return (
                <SideNavLink
                  key={item.para}
                  href={item.para}
                  renderIcon={Icone}
                  isActive={ativo(pathname, item)}
                  onClick={(e: React.MouseEvent) => {
                    e.preventDefault()
                    navegar(item.para)
                  }}
                >
                  {item.rotulo}
                </SideNavLink>
              )
            })}
          </SideNavItems>
        </SideNav>
      )}

      <main className="conteudo" id="conteudo">
        <Outlet />
      </main>

      {!desktop && (
        <nav className="bottom-nav" aria-label="Navegação principal">
          <NavLink to="/" end className={({ isActive }) => `bottom-item${isActive ? ' ativo' : ''}`}>
            <IconeDashboard size={22} />
            <span>Painel</span>
          </NavLink>
          <NavLink to="/vendas" end className={({ isActive }) => `bottom-item${isActive ? ' ativo' : ''}`}>
            <Receipt size={22} />
            <span>Vendas</span>
          </NavLink>
          <NavLink to="/vendas/nova" className="bottom-nova" aria-label="Nova venda">
            <Add size={28} />
          </NavLink>
          <NavLink to="/produtos" className={({ isActive }) => `bottom-item${isActive ? ' ativo' : ''}`}>
            <Catalog size={22} />
            <span>Produtos</span>
          </NavLink>
          <NavLink
            to="/mais"
            className={() =>
              `bottom-item${['/mais', '/clientes', '/usuarios', '/conta'].some((p) => pathname.startsWith(p)) ? ' ativo' : ''}`
            }
          >
            <IconeMais size={22} />
            <span>Mais</span>
          </NavLink>
        </nav>
      )}
    </div>
  )
}
