import { Loading } from '@carbon/react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from './AuthContext'

// Exige login. Com `somenteAdmin`, exige também o perfil AdminTotal.
export function RotaProtegida({ somenteAdmin = false }: { somenteAdmin?: boolean }) {
  const { usuario, carregando, ehAdmin } = useAuth()
  const local = useLocation()

  if (carregando) return <Loading withOverlay description="Carregando" />
  if (!usuario) return <Navigate to="/login" replace state={{ de: local.pathname }} />
  if (somenteAdmin && !ehAdmin) return <Navigate to="/" replace />

  return <Outlet />
}
