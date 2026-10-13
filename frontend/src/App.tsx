import { lazy, Suspense, useEffect } from 'react'
import { BrowserRouter, Navigate, Route, Routes, useLocation } from 'react-router-dom'
import { Loading } from '@carbon/react'
import { RotaProtegida } from './auth/RotaProtegida'
import { AppLayout } from './components/AppLayout'
import Login from './pages/Login'

// As telas pesadas (gráficos) carregam sob demanda, para o primeiro acesso no celular ser rápido.
const Dashboard = lazy(() => import('./pages/Dashboard'))
const Vendas = lazy(() => import('./pages/Vendas'))
const VendaForm = lazy(() => import('./pages/VendaForm'))
const Produtos = lazy(() => import('./pages/Produtos'))
const Clientes = lazy(() => import('./pages/Clientes'))
const Usuarios = lazy(() => import('./pages/Usuarios'))
const Conta = lazy(() => import('./pages/Conta'))
const Mais = lazy(() => import('./pages/Mais'))

// Ao trocar de tela, volta ao topo (o navegador, num SPA, manteria a rolagem anterior).
function VoltarAoTopo() {
  const { pathname } = useLocation()
  useEffect(() => {
    window.scrollTo(0, 0)
  }, [pathname])
  return null
}

export default function App() {
  return (
    <BrowserRouter>
      <VoltarAoTopo />
      <Suspense fallback={<Loading withOverlay={false} small description="Carregando" />}>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route element={<RotaProtegida />}>
            <Route element={<AppLayout />}>
              <Route index element={<Dashboard />} />
              <Route path="vendas" element={<Vendas />} />
              <Route path="vendas/nova" element={<VendaForm />} />
              <Route path="vendas/:id" element={<VendaForm />} />
              <Route path="produtos" element={<Produtos />} />
              <Route path="clientes" element={<Clientes />} />
              <Route path="conta" element={<Conta />} />
              <Route path="mais" element={<Mais />} />

              <Route element={<RotaProtegida somenteAdmin />}>
                <Route path="usuarios" element={<Usuarios />} />
              </Route>
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  )
}
