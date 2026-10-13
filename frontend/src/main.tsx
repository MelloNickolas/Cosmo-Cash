import '@fontsource/ibm-plex-sans/latin-300.css'
import '@fontsource/ibm-plex-sans/latin-400.css'
import '@fontsource/ibm-plex-sans/latin-600.css'
import '@fontsource/ibm-plex-sans/latin-ext-400.css'
import '@carbon/charts-react/styles.css'
import './styles/global.scss'

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App'
import { AuthProvider } from './auth/AuthContext'
import { AvisosProvider } from './components/Avisos'
import { NOME_LOJA } from './config'

document.title = NOME_LOJA

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AvisosProvider>
      <AuthProvider>
        <App />
      </AuthProvider>
    </AvisosProvider>
  </StrictMode>,
)
