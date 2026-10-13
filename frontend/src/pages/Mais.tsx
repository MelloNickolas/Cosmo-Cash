import { ChevronRight, Logout, Settings, UserAvatar, UserMultiple } from '@carbon/icons-react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { PaginaCabecalho } from '../components/Comuns'

// Menu "Mais" do celular: o que não cabe na barra inferior.
export default function Mais() {
  const { ehAdmin, sair, usuario } = useAuth()

  const itens = [
    { para: '/clientes', rotulo: 'Clientes', icone: UserMultiple },
    ...(ehAdmin ? [{ para: '/usuarios', rotulo: 'Usuários', icone: UserAvatar }] : []),
    { para: '/conta', rotulo: 'Minha conta', icone: Settings },
  ]

  return (
    <>
      <PaginaCabecalho titulo="Mais" subtitulo={usuario?.nome} />

      <nav className="lista-cartoes" aria-label="Mais opções">
        {itens.map((item) => {
          const Icone = item.icone
          return (
            <Link key={item.para} to={item.para} className="item-cartao linha-menu">
              <span className="linha">
                <span style={{ display: 'inline-flex', alignItems: 'center', gap: '0.75rem' }}>
                  <Icone size={20} />
                  <span className="titulo">{item.rotulo}</span>
                </span>
                <ChevronRight size={16} />
              </span>
            </Link>
          )
        })}

        <button type="button" className="item-cartao linha-menu" onClick={sair}>
          <span className="linha">
            <span style={{ display: 'inline-flex', alignItems: 'center', gap: '0.75rem' }}>
              <Logout size={20} />
              <span className="titulo">Sair</span>
            </span>
          </span>
        </button>
      </nav>
    </>
  )
}
